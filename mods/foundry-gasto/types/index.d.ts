export type DeploymentUsage = {
  deployment: string
  inToday: number
  outToday: number
  inMonth: number
  outMonth: number
  requestsToday: number
  eurToday: number
  eurMonth: number
  isPriced: boolean
}

export type Budget = {
  name: string
  scope: string
  amount: number
  spent: number
  isProject: boolean
}

/** A Foundry resource the person may pick for a project. */
export type Candidate = {
  id: string
  name: string
  kind: string
  resourceGroup: string
  subscription: string
  subscriptionName: string
  /** The project's own files name this resource: the likely answer. */
  isMentioned: boolean
}

/** The billing meters of one deployment. */
export type DeploymentMeters = { input?: string; output?: string; cached?: string }

/** What the mod keeps in $.store per project root, across sessions. */
export type ProjectConfig =
  | {
      isEnabled: true
      resourceId: string
      name: string
      resourceGroup: string
      subscription: string
      subscriptionName: string
      meters: Record<string, DeploymentMeters>
      /** The resource's own addresses (https origins), the ones the meter stands in front of. */
      endpoints?: string[]
      configuredAt: string
    }
  | { isEnabled: false; configuredAt: string }

export type Setup = {
  /** idle: configured or off; loading: listing resources; choosing: waiting for the person; discovering: reading meters */
  phase: 'idle' | 'loading' | 'choosing' | 'discovering'
  candidates: Candidate[]
  message: string
}

export type Snapshot = {
  rows: DeploymentUsage[]
  budgets: Budget[]
  tokensLastHour: number
  eurLastHour: number
  errorsToday: number
  throttledToday: number
  updatedAt: string
  error: string
  isLoading: boolean
}

/** Which folded sections of the pane are open, for this session. */
export type View = { isModelsOpen: boolean; isErrorsOpen: boolean; isLimitsOpen: boolean; isMeterOpen: boolean }

/**
 * The person's own spending limits, in euros, kept in $.store for every project. 0 is no limit.
 * The meter refuses calls once one is reached.
 */
export type Limits = {
  day: number
  week: number
  month: number
  /** Percent of a limit from which the panel warns. */
  warnPercent: number
  /** Also refuse calls while the resource group is over its Azure budget. */
  isGroupCut: boolean
  /** The person stopped all spending by hand. */
  isPaused: boolean
  /** At a limit: 'stop' also stops the processes of this machine that call the resource directly; 'warn' leaves them. */
  onLimit: 'stop' | 'warn'
}

/** What the meter counted on one local day; `direct` is the part of `eur` estimated from direct calls. */
export type MeterDay = { eur: number; calls: number; unpriced: number; estimated: number; rejected: number; direct?: number }

/** One address of the resource and the local port that stands in front of it. */
export type MeterRoute = { port: number; name: string; upstream: string; resourceId?: string }

/** A process the meter stopped at a limit. */
export type StoppedProcess = { t: string; name: string; pid: number; why: string }

/** The personal meter as last seen: its limits, its routes and what it has counted. */
export type Meter = {
  limits: Limits
  /** The routes of this project's resource. */
  routes: MeterRoute[]
  /** The meter answered within the last seconds. */
  isAlive: boolean
  /** Why calls are refused now: '', 'dia', 'semana', 'mes', 'pausa' or 'grupo'. */
  blocked: string
  errors: string[]
  /** Local day (yyyy-mm-dd) to what was counted. */
  byDay: Record<string, MeterDay>
  /** The processes of this machine seen calling the resource directly in the last minutes. */
  callers: string[]
  /** The processes stopped at a limit, oldest first. */
  stopped: StoppedProcess[]
  /** Processes that call the resource but are never stopped (a browser, an editor). */
  spared: string[]
}

/** One model call pattern a prompt would trigger, priced at list price. */
export type EstimateStep = {
  what: string
  deployment: string
  callsMin: number
  callsMax: number
  inputTokens: number
  outputTokens: number
  eurMin: number
  eurMax: number
  isPriced: boolean
}

/** What the draft in the prompt box would cost, as the estimate button last answered. */
export type Estimate = {
  phase: 'idle' | 'running' | 'done' | 'error'
  prompt: string
  steps: EstimateStep[]
  /** Services outside Foundry's token metrics (Document Intelligence, AI Search...), not priced here. */
  others: { service: string; quantity: string }[]
  assumptions: string[]
  eurMin: number
  eurMax: number
  message: string
}

declare module 'claude-code' {
  interface PluginState {
    'foundry-gasto': {
      snapshot: Shaped<Snapshot>
      setup: Shaped<Setup>
      view: Shaped<View>
      estimate: Shaped<Estimate>
      meter: Shaped<Meter>
    }
  }
}
