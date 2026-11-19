import { expect, test } from 'claude-code/testing'

import { pace, parsePlan, pricePlan } from './register'

test('reads the estimator reply even with text around the JSON', async () => {
  const plan = parsePlan(
    'Aquí va:\n```json\n{"steps":[{"what":"Generar examen","deployment":"gpt-4.1","callsMin":5,"callsMax":10,"inputTokens":4000,"outputTokens":1500}],"others":[{"service":"Document Intelligence","quantity":"~30 páginas"}],"assumptions":["10 páginas por PDF"]}\n```',
  )
  expect(plan.steps.length).toBe(1)
  expect(plan.steps[0].callsMax).toBe(10)
  expect(plan.others[0].service).toBe('Document Intelligence')
})

test('prices each step at list price, between its minimum and maximum calls', async () => {
  const plan = parsePlan(
    '{"steps":[{"what":"Examen","deployment":"gpt-4.1","callsMin":5,"callsMax":10,"inputTokens":1000000,"outputTokens":0},{"what":"Otro","deployment":"desconocido","callsMin":1,"callsMax":1,"inputTokens":10,"outputTokens":10}],"others":[],"assumptions":[]}',
  )
  const priced = pricePlan(plan, d => (d === 'gpt-4.1' ? { input: 2e-6, output: 8e-6 } : undefined))
  expect(priced.eurMin).toBe(10)
  expect(priced.eurMax).toBe(20)
  expect(priced.steps[1].isPriced).toBe(false)
})

test('a maximum below the minimum is raised to the minimum', async () => {
  const plan = parsePlan('{"steps":[{"what":"x","deployment":"d","callsMin":4,"callsMax":1,"inputTokens":1,"outputTokens":1}]}')
  expect(plan.steps[0].callsMax).toBe(4)
})

test('the pace compares today with the average day of the month', async () => {
  const day11 = new Date(2026, 10, 11, 12)
  expect(pace(3, 33, day11).label).toBe('ritmo normal')
  expect(pace(9, 39, day11).label).toBe('ritmo alto')
  expect(pace(20, 50, day11).label).toBe('ritmo muy alto')
})
