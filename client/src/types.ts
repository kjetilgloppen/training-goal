export interface LogEntry {
  id: number
  date: string // ISO date (yyyy-mm-dd)
  durationSeconds: number | null
  note: string | null
}

export interface Goal {
  id: number
  name: string
  targetCount: number
  periodStart: string // yyyy-mm-dd
  periodEnd: string // yyyy-mm-dd
  unit: string | null
  logs: LogEntry[]
}

export interface CreateGoalRequest {
  name: string
  targetCount: number
  periodStart: string
  periodEnd: string
  unit: string | null
}

export interface CreateLogRequest {
  date: string
  durationSeconds: number | null
  note: string | null
}
