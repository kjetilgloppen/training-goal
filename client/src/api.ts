import type { CreateGoalRequest, CreateLogRequest, Goal, LogEntry } from './types'

// All requests share the auth cookie. A 401 means "not logged in".
async function request<T>(url: string, options: RequestInit = {}): Promise<T> {
  const res = await fetch(url, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })

  if (res.status === 401) throw new UnauthorizedError()
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.message ?? `Request failed (${res.status})`)
  }
  // 204 No Content
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export class UnauthorizedError extends Error {
  constructor() {
    super('Unauthorized')
    this.name = 'UnauthorizedError'
  }
}

export const api = {
  // --- auth ---
  async me(): Promise<boolean> {
    const r = await request<{ authenticated: boolean }>('/api/auth/me')
    return r.authenticated
  },
  async login(passcode: string): Promise<void> {
    await request('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ passcode }),
    })
  },
  async logout(): Promise<void> {
    await request('/api/auth/logout', { method: 'POST' })
  },

  // --- goals ---
  getGoals: () => request<Goal[]>('/api/goals'),
  getGoal: (id: number) => request<Goal>(`/api/goals/${id}`),
  createGoal: (body: CreateGoalRequest) =>
    request<Goal>('/api/goals', { method: 'POST', body: JSON.stringify(body) }),
  deleteGoal: (id: number) =>
    request<void>(`/api/goals/${id}`, { method: 'DELETE' }),

  // --- logs ---
  addLog: (goalId: number, body: CreateLogRequest) =>
    request<LogEntry>(`/api/goals/${goalId}/logs`, {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  deleteLog: (id: number) =>
    request<void>(`/api/logs/${id}`, { method: 'DELETE' }),
}
