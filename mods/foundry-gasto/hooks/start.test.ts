import { expect, test } from 'claude-code/testing'

import { register } from './register'
import type { MeterFile } from './register'

const HOME = 'C:\\Users\\prueba'
const DIR = `${HOME}\\.claude\\foundry-gasto`
const config = {
  isEnabled: true,
  resourceId: '/subscriptions/s/resourceGroups/rg/providers/Microsoft.CognitiveServices/accounts/aif-x',
  name: 'aif-x',
  resourceGroup: 'rg',
  subscription: 's',
  subscriptionName: 'sub',
  meters: { 'gpt-x': { input: 'm-in', output: 'm-out' } },
  configuredAt: '2026-11-01T00:00:00Z',
}

/** A session in memory: every noun the module calls, answered here. Nothing reaches Azure or the disk. */
function session() {
  const files = new Map<string, string>()
  const store = new Map<string, unknown>([['project:c:\\proyecto', config]])
  const state = new Map<string, { value: unknown; version: number }>()
  const runs: string[] = []
  const toasts: string[] = []
  const statuses: (string | undefined)[] = []
  const timers: { ms: number; fn: () => unknown }[] = []
  const $ = {
    command: { register: async () => undefined },
    plugin: { root: 'C:\\mod', name: 'foundry-gasto' },
    session: { id: async () => 's1', root: async () => 'C:\\proyecto' },
    store: {
      get: async (key: string) => store.get(key),
      set: async (key: string, value: unknown) => void (value === null ? store.delete(key) : store.set(key, value)),
    },
    env: { get: async (name: string) => (name === 'USERPROFILE' ? HOME : undefined) },
    fs: {
      read: async (path: string) => {
        const text = files.get(path)
        if (text === undefined) throw new Error('ENOENT')
        return text
      },
      write: async (path: string, text: string) => void files.set(path, text),
      exists: async () => false,
    },
    process: {
      run: async (argv: string[]) => {
        const line = argv.join(' ')
        runs.push(line)
        const stdout = line.includes('account show')
          ? '{"endpoint":"https://aif-x.cognitiveservices.azure.com/","endpoints":{"a":"https://aif-x.openai.azure.com/","b":"https://westeurope.stt.speech.microsoft.com"}}'
          : line.includes('budget')
            ? '[{"name":"b","amount":"150","resourceGroup":"rg","currentSpend":{"amount":"120"}}]'
            : line.includes('metrics')
              ? '{"value":[]}'
              : ''
        return { exitCode: line.includes('where.exe') ? 1 : 0, stdout, stderr: '', isStdoutTruncated: false }
      },
    },
    http: {
      fetch: async () => ({
        status: 200,
        ok: true,
        headers: {},
        text: '{"Items":[{"meterId":"m-in","retailPrice":2,"unitOfMeasure":"1M"},{"meterId":"m-out","retailPrice":8,"unitOfMeasure":"1M"}]}',
      }),
    },
    state: {
      get: async (ref: { key: string }) => state.get(ref.key) ?? { value: undefined, version: 0 },
      set: async (ref: { key: string }, value: unknown) => {
        const version = (state.get(ref.key)?.version ?? 0) + 1
        state.set(ref.key, { value, version })
        return { isSet: true, version }
      },
    },
    clock: { every: (ms: number, fn: () => unknown) => void timers.push({ ms, fn }) },
    ui: {
      open: async () => ({ isPlaced: true }),
      status: (text?: string) => void statuses.push(text),
      toast: (text: string) => void toasts.push(text),
    },
  }
  const hooks = new Map<string, (...args: unknown[]) => unknown>()
  register(((name: string, a: unknown, b: unknown) => {
    hooks.set(name, (b ?? a) as (...args: unknown[]) => unknown)
  }) as never)
  const settle = async () => {
    for (let i = 0; i < 400; i++) await Promise.resolve()
  }
  const tick = async (ms: number) => {
    for (const t of timers.filter(x => x.ms === ms)) await t.fn()
    await settle()
  }
  const meterFile = () => JSON.parse(files.get(`${DIR}\\contador.json`) ?? '{}') as Partial<MeterFile>
  const meterAtom = () => (state.get('meter')?.value as { value: Record<string, unknown> } | undefined)?.value
  return { $, hooks, files, store, runs, toasts, statuses, settle, tick, meterFile, meterAtom }
}

test('a session start reads the addresses, writes the meter file and starts the meter', async () => {
  const s = session()
  await s.hooks.get('session.start')?.(s.$, {}, async () => ({}))
  await s.settle()

  const show = s.runs.find(r => r.includes('account show')) ?? ''
  expect(show.includes('--name aif-x --resource-group rg --subscription s')).toBe(true)
  expect(show.includes('--ids')).toBe(false)

  const file = s.meterFile()
  expect(file.version).toBe('3')
  expect(file.onLimit).toBe('stop')
  expect(JSON.stringify(file.limits)).toBe('{"day":5,"week":20,"month":50}')
  // The region's shared speech host is not this resource's: no route for it.
  expect((file.routes ?? []).map(r => `${r.port} ${r.upstream}`).join(' | ')).toBe(
    '8787 https://aif-x.cognitiveservices.azure.com | 8788 https://aif-x.openai.azure.com',
  )
  expect((file.routes ?? []).every(r => r.resourceId === config.resourceId)).toBe(true)
  expect(JSON.stringify(file.prices?.['aif-x|gpt-x'])).toBe('{"input":0.000002,"output":0.000008}')
  // The group is under its budget: nothing held.
  expect(JSON.stringify(file.holds)).toBe('{}')

  const start = s.runs.find(r => r.includes('arrancar.ps1')) ?? ''
  expect(start.includes(`C:\\mod\\contador\\arrancar.ps1 -Dir ${DIR}`)).toBe(true)
})

test('what the meter reports reaches the panel: the cut, the stopped process and a toast', async () => {
  const s = session()
  await s.hooks.get('session.start')?.(s.$, {}, async () => ({}))
  await s.settle()
  const now = new Date()
  const day = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
  s.files.set(
    `${DIR}\\estado.json`,
    JSON.stringify({
      version: '3',
      beat: now.toISOString(),
      isStopping: false,
      blocked: 'dia',
      held: [],
      errors: [],
      callers: ['python'],
      stopped: [{ t: now.toISOString(), name: 'python', pid: 4242, why: 'dia' }],
      spared: [],
      byDay: { [day]: { eur: 5.2, direct: 5.2, calls: 0, unpriced: 0, estimated: 0, rejected: 1 } },
    }),
  )
  const launches = s.runs.filter(r => r.includes('arrancar.ps1')).length
  await s.tick(3_000)

  const seen = s.meterAtom()
  expect(seen?.isAlive).toBe(true)
  expect(seen?.blocked).toBe('dia')
  expect(JSON.stringify(seen?.routes).includes('8787')).toBe(true)
  expect(s.statuses.at(-1)?.startsWith('⛔ Foundry cortado')).toBe(true)
  expect(s.toasts.some(t => t.includes('Foundry cortado'))).toBe(true)
  expect(s.toasts.some(t => t.includes('he parado «python»'))).toBe(true)
  // A meter that answers is not started again.
  expect(s.runs.filter(r => r.includes('arrancar.ps1')).length).toBe(launches)
})

test('a limit changed in the panel reaches the meter file, and so does a pause', async () => {
  const s = session()
  await s.hooks.get('session.start')?.(s.$, {}, async () => ({}))
  await s.settle()

  s.store.set('action:s1', { kind: 'limits', patch: { day: 9, onLimit: 'warn' } })
  await s.tick(2_000)
  expect(s.meterFile().limits?.day).toBe(9)
  expect(s.meterFile().onLimit).toBe('warn')
  expect(s.meterFile().hold).toBe('')

  s.store.set('action:s1', { kind: 'limits', patch: { isPaused: true } })
  await s.tick(2_000)
  expect(s.meterFile().hold).toBe('pausa')
  expect(s.meterFile().limits?.day).toBe(9)
})

test('with the group over its budget and that cut chosen, the resource is held', async () => {
  const s = session()
  s.store.set('limits', { isGroupCut: true })
  const run = s.$.process.run
  s.$.process.run = async (argv: string[]) => {
    const result = await run(argv)
    return argv.includes('budget') ? { ...result, stdout: result.stdout.replace('"120"', '"180"') } : result
  }
  await s.hooks.get('session.start')?.(s.$, {}, async () => ({}))
  await s.settle()
  expect(s.meterFile().holds?.['aif-x']).toBe('grupo')
})

test('an older mod leaves alone the meter file a newer one wrote, and does not start the meter', async () => {
  const s = session()
  const newer = '{"version":"4","routes":[]}'
  s.files.set(`${DIR}\\contador.json`, newer)
  await s.hooks.get('session.start')?.(s.$, {}, async () => ({}))
  await s.settle()
  await s.tick(3_000)
  expect(s.files.get(`${DIR}\\contador.json`)).toBe(newer)
  expect(s.runs.some(r => r.includes('arrancar.ps1'))).toBe(false)
  expect(JSON.stringify(s.meterAtom()?.errors).includes('versión más nueva')).toBe(true)
})

test('a meter that could not start says why in the panel', async () => {
  const s = session()
  s.files.set(`${DIR}\\contador-error.txt`, '2026-11-18T10:00:00 Cannot add type. Compilation errors occurred.')
  await s.hooks.get('session.start')?.(s.$, {}, async () => ({}))
  await s.settle()
  await s.tick(3_000)
  expect(JSON.stringify(s.meterAtom()?.errors).includes('El contador no arranca')).toBe(true)
})
