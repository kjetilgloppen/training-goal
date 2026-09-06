import type { Goal } from './types'

const MS_PER_DAY = 24 * 60 * 60 * 1000

/** Parse a yyyy-mm-dd string into a local Date at midnight. */
export function parseDate(iso: string): Date {
  const [y, m, d] = iso.split('-').map(Number)
  return new Date(y, m - 1, d)
}

/** Format a Date as yyyy-mm-dd (local). */
export function toIso(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

/** Whole days from a→b (b - a), based on local midnight. */
export function daysBetween(a: Date, b: Date): number {
  return Math.round((startOfDay(b).getTime() - startOfDay(a).getTime()) / MS_PER_DAY)
}

function startOfDay(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate())
}

function clamp(n: number, lo: number, hi: number): number {
  return Math.min(hi, Math.max(lo, n))
}

export interface GoalStats {
  actualCount: number
  expectedCount: number // where you "should" be today
  delta: number // actual - expected; positive = ahead
  onTrack: boolean
  elapsedFraction: number
  percentComplete: number // actual / target
  projectedTotal: number | null // run-rate projection for period end
  remaining: number
  daysRemaining: number
  neededPerWeek: number // to still hit target in the remaining time
  totalDurationSeconds: number
  avgDurationSeconds: number | null
  hasStarted: boolean
  hasEnded: boolean
}

/** Split a total number of seconds into { h, m, s }. */
export function splitDuration(totalSeconds: number): { h: number; m: number; s: number } {
  const t = Math.max(0, Math.round(totalSeconds))
  return { h: Math.floor(t / 3600), m: Math.floor((t % 3600) / 60), s: t % 60 }
}

/** Combine hours/minutes/seconds into total seconds (null if all empty). */
export function toSeconds(h: number | null, m: number | null, s: number | null): number | null {
  const total = (h || 0) * 3600 + (m || 0) * 60 + (s || 0)
  return total > 0 ? total : null
}

/** Human-readable duration, e.g. "1h 30m 20s". Omits zero parts; "0s" when empty. */
export function formatDuration(totalSeconds: number | null | undefined): string {
  if (!totalSeconds) return '0s'
  const { h, m, s } = splitDuration(totalSeconds)
  const parts: string[] = []
  if (h) parts.push(`${h}h`)
  if (m) parts.push(`${m}m`)
  if (s) parts.push(`${s}s`)
  return parts.join(' ')
}

export function computeStats(goal: Goal, today: Date = new Date()): GoalStats {
  const start = parseDate(goal.periodStart)
  const end = parseDate(goal.periodEnd)
  const now = startOfDay(today)

  const totalDays = Math.max(daysBetween(start, end), 0)
  const elapsedDays = clamp(daysBetween(start, now), 0, totalDays || daysBetween(start, now))
  const elapsedFraction = totalDays === 0 ? 1 : clamp(elapsedDays / totalDays, 0, 1)

  // Count logs dated on or before today.
  const countedLogs = goal.logs.filter((l) => parseDate(l.date) <= now)
  const actualCount = countedLogs.length
  const expectedCount = goal.targetCount * elapsedFraction
  const delta = actualCount - expectedCount

  const remaining = Math.max(goal.targetCount - actualCount, 0)
  const daysRemaining = Math.max(daysBetween(now, end), 0)
  const neededPerWeek = daysRemaining > 0 ? remaining / (daysRemaining / 7) : remaining

  const durations = goal.logs
    .map((l) => l.durationSeconds)
    .filter((d): d is number => d != null)
  const totalDurationSeconds = durations.reduce((a, b) => a + b, 0)
  const avgDurationSeconds =
    durations.length > 0 ? totalDurationSeconds / durations.length : null

  return {
    actualCount,
    expectedCount,
    delta,
    onTrack: delta >= 0,
    elapsedFraction,
    percentComplete: goal.targetCount > 0 ? actualCount / goal.targetCount : 0,
    projectedTotal: elapsedFraction > 0 ? actualCount / elapsedFraction : null,
    remaining,
    daysRemaining,
    neededPerWeek,
    totalDurationSeconds,
    avgDurationSeconds,
    hasStarted: now >= start,
    hasEnded: now > end,
  }
}

export interface ChartSeries {
  totalDays: number
  target: number
  todayIndex: number
  expected: { x: number; y: number }[]
  actual: { x: number; y: number }[]
  labelForDay: (dayIndex: number) => string
}

/** Build data for the "cumulative actual vs expected" line chart (x = day index from start). */
export function buildChartSeries(goal: Goal, today: Date = new Date()): ChartSeries {
  const start = parseDate(goal.periodStart)
  const end = parseDate(goal.periodEnd)
  const totalDays = Math.max(daysBetween(start, end), 1)
  const todayIndex = clamp(daysBetween(start, startOfDay(today)), 0, totalDays)

  const expected = [
    { x: 0, y: 0 },
    { x: totalDays, y: goal.targetCount },
  ]

  // Cumulative actual, stepped, only up to today.
  const sorted = [...goal.logs]
    .map((l) => ({ dayIndex: clamp(daysBetween(start, parseDate(l.date)), 0, totalDays) }))
    .filter((l) => l.dayIndex <= todayIndex)
    .sort((a, b) => a.dayIndex - b.dayIndex)

  const actual: { x: number; y: number }[] = [{ x: 0, y: 0 }]
  let cumulative = 0
  for (const l of sorted) {
    actual.push({ x: l.dayIndex, y: cumulative })
    cumulative += 1
    actual.push({ x: l.dayIndex, y: cumulative })
  }
  // Extend the line flat to "today".
  actual.push({ x: todayIndex, y: cumulative })

  const labelForDay = (dayIndex: number) => {
    const d = new Date(start)
    d.setDate(d.getDate() + Math.round(dayIndex))
    return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
  }

  return { totalDays, target: goal.targetCount, todayIndex, expected, actual, labelForDay }
}
