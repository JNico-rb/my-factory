import { expect, test } from 'claude-code/testing'

const PANE = {
  plugin: 'foundry-gasto',
  component: 'Pane',
  requestId: 'foundry-gasto',
  surface: 'terminal',
  props: {
    title: 'Foundry · gasto',
    isFocused: false,
    bodyColumns: 50,
    placement: 'dock',
    scroll: { offset: 0, bodyRows: 40 },
    view: {},
  },
} as const

const config = {
  isEnabled: true,
  resourceId: '/subscriptions/s/resourceGroups/rg/providers/Microsoft.CognitiveServices/accounts/aif-x',
  name: 'aif-x',
  resourceGroup: 'rg',
  subscription: 's',
  subscriptionName: 'sub',
  meters: {},
  endpoints: ['https://aif-x.cognitiveservices.azure.com'],
  configuredAt: '2026-11-01T00:00:00Z',
}

const snapshot = {
  rows: [],
  budgets: [
    { name: 'rg-budget', scope: 'rg', amount: 150, spent: 120, isProject: true },
    { name: 'sub-budget', scope: 'suscripción sub', amount: 2000, spent: 2310, isProject: false },
  ],
  tokensLastHour: 0,
  eurLastHour: 0,
  errorsToday: 0,
  throttledToday: 0,
  updatedAt: new Date().toISOString(),
  error: '',
  isLoading: false,
}

const now = new Date()
const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
const meter = (blocked: string, eur: number) => ({
  limits: { day: 5, week: 20, month: 50, warnPercent: 80, isGroupCut: false, isPaused: false, onLimit: 'stop' },
  callers: ['python'],
  stopped: blocked ? [{ t: '2026-11-18T10:00:00Z', name: 'python', pid: 4242, why: blocked }] : [],
  spared: [],
  routes: [{ port: 8787, name: 'aif-x', upstream: 'https://aif-x.cognitiveservices.azure.com' }],
  isAlive: true,
  blocked,
  errors: [],
  byDay: { [today]: { eur, calls: 12, unpriced: 0, estimated: 0, rejected: blocked ? 3 : 0 } },
})

const mount = async ($: Parameters<Parameters<typeof test>[1]>[0], on: Parameters<Parameters<typeof test>[1]>[1], seen: unknown) => {
  on('session.id', () => ({ value: 'test-session' }))
  on('session.root', () => ({ value: 'C:\\proyecto' }))
  on('store.get', (_, e) => ({ value: String(e.key).startsWith('project:') ? config : undefined }))
  on('state.get', (_, e) => ({
    value: {
      value:
        e.key === 'snapshot'
          ? { shape: 'snapshot-3', value: snapshot }
          : e.key === 'meter'
            ? { shape: 'meter-2', value: seen }
            : undefined,
      version: 1,
    },
  }))
  return $.ui.mount(PANE)
}

test('the pane shows my spend against my limits, the official budgets and every button', async ($, on) => {
  const ui = await mount($, on, meter('', 1.2))

  const missing: string[] = []
  // Ring or bar, the figure and its ceiling are there: "1.20 €" over "de 5.00 €", or both in one line.
  for (const text of [/1\.20 €/, /de 5\.00 €/, /120 €/, /de 150 €/, /pasada en 310 €/])
    if (!(await ui.find({ type: 'Text', text }))) missing.push(String(text))
  for (const key of ['refresh', 'reconfigure', 'estimate', 'pause'])
    if (!(await ui.find({ type: 'Button', key }))) missing.push(key)
  expect(missing.join(' ')).toBe('')
  expect(await ui.find({ type: 'Text', text: /Gasto cortado/ })).toBeUndefined()
  await ui.unmount()
})

test('once the day limit is reached the pane says spending is cut and why', async ($, on) => {
  const ui = await mount($, on, meter('dia', 5.02))

  expect(await ui.find({ type: 'Text', text: /Gasto cortado: has llegado a tu límite de hoy/ })).toBeDefined()
  expect(await ui.find({ type: 'Text', text: /Se reabre mañana/ })).toBeDefined()
  expect(await ui.find({ type: 'Text', text: /Parados: python/ })).toBeDefined()
  await ui.unmount()
})
