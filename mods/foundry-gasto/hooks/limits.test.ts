import { expect, test } from 'claude-code/testing'

import {
  defaultLimits,
  endpointOrigins,
  limitColor,
  mergeMeterFile,
  parseCommand,
  parseLimit,
  personalSpend,
  ring,
} from './register'

const day = (eur: number) => ({ eur, calls: 1, unpriced: 0, estimated: 0, rejected: 0 })

test('my spend adds today, the week from Monday and the month', async () => {
  // Wednesday 18 November 2026: the week started on Monday 16.
  const now = new Date(2026, 10, 18, 12)
  const spend = personalSpend(
    { '2026-11-18': day(1), '2026-11-16': day(2), '2026-11-15': day(4), '2026-11-02': day(8), '2026-10-31': day(16) },
    now,
  )
  expect(spend.day).toBe(1)
  expect(spend.week).toBe(3)
  expect(spend.month).toBe(15)
})

test('a week that starts in the previous month still counts those days', async () => {
  // Tuesday 1 December 2026: Monday was 30 November.
  const spend = personalSpend({ '2026-12-01': day(1), '2026-11-30': day(2) }, new Date(2026, 11, 1, 9))
  expect(spend.week).toBe(3)
  expect(spend.month).toBe(1)
})

test('a limit is read with comma or dot, and anything else is no limit', async () => {
  expect(parseLimit('7,5')).toBe(7.5)
  expect(parseLimit(' 10 € ')).toBe(10)
  expect(parseLimit('')).toBe(0)
  expect(parseLimit('-3')).toBe(0)
  expect(parseLimit('mucho')).toBe(0)
})

test('limits are on by default', async () => {
  expect(defaultLimits.day > 0 && defaultLimits.week > 0 && defaultLimits.month > 0).toBe(true)
})

test('the ring is green, yellow from the warning and red at the limit', async () => {
  expect(limitColor(1, 5, 80)).toBe('green')
  expect(limitColor(4, 5, 80)).toBe('yellow')
  expect(limitColor(5, 5, 80)).toBe('red')
  expect(limitColor(9, 0, 80)).toBe('blue')
  expect(ring(2.5, 5, 'green').includes('>50%<')).toBe(true)
  expect(ring(9, 0, 'blue').includes('stroke-dasharray')).toBe(false)
})

test('the meter file gives each address of the resource a port and keeps other projects', async () => {
  const other = { port: 8787, name: 'otro', upstream: 'https://otro.openai.azure.com' }
  const file = mergeMeterFile(
    { routes: [other], prices: { 'otro|gpt': { input: 1, output: 2 } }, holds: { otro: 'grupo' } },
    {
      name: 'aif-x',
      resourceId: '/subscriptions/s/x',
      endpoints: ['https://aif-x.cognitiveservices.azure.com', 'https://aif-x.openai.azure.com'],
      prices: { 'aif-x|gpt-4.1': { input: 3, output: 9, cached: 1 } },
      limits: { ...defaultLimits, day: 7 },
      hold: '',
    },
  )
  expect(file.routes.map(r => r.port).join()).toBe('8787,8788,8789')
  expect(file.routes[0].name).toBe('otro')
  expect(file.limits.day).toBe(7)
  expect(file.holds.otro).toBe('grupo')
  expect(file.fallback?.output).toBe(9)
  expect(file.onLimit).toBe('stop')
  expect(file.routes[1].resourceId).toBe('/subscriptions/s/x')
  expect(file.routes[0].resourceId).toBeUndefined()

  const again = mergeMeterFile(file, {
    name: 'aif-x',
    endpoints: ['https://aif-x.openai.azure.com'],
    prices: {},
    limits: { ...defaultLimits, isPaused: true },
    hold: 'grupo',
  })
  expect(again.routes.length).toBe(3)
  expect(again.prices['aif-x|gpt-4.1'].input).toBe(3)
  expect(again.hold).toBe('pausa')
  expect(again.holds['aif-x']).toBe('grupo')
})

test('a hold not known yet leaves the one in the file', async () => {
  const file = mergeMeterFile({ holds: { 'aif-x': 'grupo' } }, { name: 'aif-x', endpoints: [], prices: {}, limits: defaultLimits })
  expect(file.holds['aif-x']).toBe('grupo')
})

test('the addresses of an account come once each, the main one first', async () => {
  const origins = endpointOrigins({
    endpoint: 'https://aif-x.cognitiveservices.azure.com/',
    endpoints: {
      'OpenAI Language Model Instance API': 'https://aif-x.openai.azure.com/',
      'Content Safety': 'https://aif-x.cognitiveservices.azure.com/',
      'Speech (shared by the region)': 'https://westeurope.stt.speech.microsoft.com',
    },
  })
  expect(origins.join()).toBe('https://aif-x.cognitiveservices.azure.com,https://aif-x.openai.azure.com')
})

test('/foundry reads a limit, a pause and a resume', async () => {
  expect(parseCommand('limite dia 7,5')?.day).toBe(7.5)
  expect(parseCommand('límite semana 30')?.week).toBe(30)
  expect(parseCommand('limite mes 0')?.month).toBe(0)
  expect(parseCommand('pausa')?.isPaused).toBe(true)
  expect(parseCommand('reanudar')?.isPaused).toBe(false)
  expect(parseCommand('')).toBeUndefined()
})
