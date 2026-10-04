<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api, UnauthorizedError } from '../api'
import type { Goal } from '../types'
import { computeStats, toIso, parseDate, toSeconds, formatDuration } from '../pace'
import ProgressChart from '../components/ProgressChart.vue'

const route = useRoute()
const router = useRouter()
const id = Number(route.params.id)

const goal = ref<Goal | null>(null)
const loading = ref(true)
const error = ref('')

// add-log form
const logDate = ref(toIso(new Date()))
const logHours = ref<number | null>(null)
const logMinutes = ref<number | null>(null)
const logSeconds = ref<number | null>(null)
const logNote = ref('')

const stats = computed(() => (goal.value ? computeStats(goal.value) : null))
const unit = computed(() => goal.value?.unit || 'done')
const sortedLogs = computed(() =>
  goal.value ? [...goal.value.logs].sort((a, b) => (a.date < b.date ? 1 : -1)) : [],
)

// Same-day occurrence index per log (0-based). Entries within a day are ordered by
// ascending id (insertion order), so the first entry of a day is index 0 (untinted)
// and each subsequent same-day entry escalates.
const dupIndexById = computed(() => {
  const map = new Map<number, number>()
  if (!goal.value) return map
  const byDate = new Map<string, number[]>()
  for (const l of goal.value.logs) {
    const arr = byDate.get(l.date) ?? []
    arr.push(l.id)
    byDate.set(l.date, arr)
  }
  for (const ids of byDate.values()) {
    ids.sort((a, b) => a - b)
    ids.forEach((id, i) => map.set(id, i))
  }
  return map
})

// Derive a tint from the same-day occurrence index: the first entry (index 0) is
// untinted; each subsequent same-day entry keeps the same blue hue but deepens the
// alpha, so repeats read as a progressively darker blue over --card.
function dupStyle(logId: number) {
  const i = dupIndexById.value.get(logId) ?? 0
  if (i === 0) return undefined
  const alpha = Math.min(0.14 + (i - 1) * 0.1, 0.65)
  return { background: `hsl(215 80% 50% / ${alpha})` }
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    goal.value = await api.getGoal(id)
  } catch (e) {
    if (e instanceof UnauthorizedError) return router.push({ name: 'login' })
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}

async function addLog() {
  if (!goal.value) return
  try {
    await api.addLog(goal.value.id, {
      date: logDate.value,
      durationSeconds: toSeconds(logHours.value, logMinutes.value, logSeconds.value),
      note: logNote.value.trim() || null,
    })
    logHours.value = null
    logMinutes.value = null
    logSeconds.value = null
    logNote.value = ''
    await load()
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function deleteLog(logId: number) {
  try {
    await api.deleteLog(logId)
    await load()
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function deleteGoal() {
  if (!goal.value) return
  if (!confirm(`Delete "${goal.value.name}" and all its logs?`)) return
  try {
    await api.deleteGoal(goal.value.id)
    router.push({ name: 'goals' })
  } catch (e) {
    error.value = (e as Error).message
  }
}

function fmtDate(iso: string) {
  return parseDate(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
}
function round(n: number) {
  return Math.round(n)
}

onMounted(load)
</script>

<template>
  <RouterLink to="/" class="muted" style="text-decoration: none">← All goals</RouterLink>

  <p v-if="loading" class="muted">Loading…</p>
  <p v-if="error" class="error">{{ error }}</p>

  <div v-if="goal && stats">
    <div class="spread" style="margin: 12px 0">
      <div>
        <h1 style="margin: 0">{{ goal.name }}</h1>
        <span class="muted">
          {{ goal.targetCount }} {{ unit }} · {{ fmtDate(goal.periodStart) }} – {{ fmtDate(goal.periodEnd) }}
        </span>
      </div>
      <span class="badge" :class="stats.onTrack ? 'ahead' : 'behind'">
        {{ round(Math.abs(stats.delta)) }} {{ stats.onTrack ? 'ahead' : 'behind' }}
      </span>
    </div>

    <div class="card">
      <ProgressChart :goal="goal" />
    </div>

    <div class="stats">
      <div class="stat">
        <div class="value">{{ stats.actualCount }} / {{ goal.targetCount }}</div>
        <div class="label">Done</div>
      </div>
      <div class="stat">
        <div class="value">{{ round(stats.expectedCount) }}</div>
        <div class="label">Target today</div>
      </div>
      <div class="stat">
        <div class="value">{{ stats.projectedTotal != null ? round(stats.projectedTotal) : '—' }}</div>
        <div class="label">Projected total</div>
      </div>
      <div class="stat">
        <div class="value">{{ stats.neededPerWeek.toFixed(1) }}</div>
        <div class="label">Needed / week</div>
      </div>
      <div class="stat">
        <div class="value">{{ stats.daysRemaining }}</div>
        <div class="label">Days left</div>
      </div>
      <div class="stat" v-if="stats.totalDurationSeconds > 0">
        <div class="value">{{ formatDuration(stats.totalDurationSeconds) }}</div>
        <div class="label">
          Total time<template v-if="stats.avgDurationSeconds">
            · {{ formatDuration(stats.avgDurationSeconds) }} avg</template
          >
        </div>
      </div>
    </div>

    <div class="card">
      <h2>Log an entry</h2>
      <form @submit.prevent="addLog">
        <div class="field">
          <label>Date</label>
          <input v-model="logDate" type="date" />
        </div>
        <div class="field">
          <label>Duration (optional)</label>
          <div class="row">
            <input v-model.number="logHours" type="number" min="0" placeholder="h" aria-label="hours" />
            <input v-model.number="logMinutes" type="number" min="0" max="59" placeholder="m" aria-label="minutes" />
            <input v-model.number="logSeconds" type="number" min="0" max="59" placeholder="s" aria-label="seconds" />
          </div>
        </div>
        <div class="field">
          <label>Note (optional)</label>
          <input v-model="logNote" placeholder="e.g. Mt. Rainier, great weather" />
        </div>
        <button type="submit" class="btn btn-primary">Add entry</button>
      </form>
    </div>

    <div class="card">
      <h2>History ({{ goal.logs.length }})</h2>
      <p v-if="goal.logs.length === 0" class="muted">No entries yet.</p>
      <ul class="log-list">
        <li v-for="l in sortedLogs" :key="l.id" :style="dupStyle(l.id)">
          <span>
            <strong>{{ fmtDate(l.date) }}</strong>
            <span v-if="l.durationSeconds" class="muted"> · {{ formatDuration(l.durationSeconds) }}</span>
            <span v-if="l.note" class="muted"> · {{ l.note }}</span>
          </span>
          <button class="danger" @click="deleteLog(l.id)">Delete</button>
        </li>
      </ul>
    </div>

    <button class="danger" style="margin-top: 8px" @click="deleteGoal">Delete this goal</button>
  </div>
</template>
