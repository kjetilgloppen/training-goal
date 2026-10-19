<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api, UnauthorizedError } from '../api'
import type { CreateGoalRequest, CreateLogRequest, Goal } from '../types'
import { computeStats, parseDate, formatDuration } from '../pace'
import ProgressChart from '../components/ProgressChart.vue'
import LogForm from '../components/LogForm.vue'
import NewGoalForm from '../components/NewGoalForm.vue'

const route = useRoute()
const router = useRouter()
const id = Number(route.params.id)

const goal = ref<Goal | null>(null)
const loading = ref(true)
const error = ref('')

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

const addForm = ref<InstanceType<typeof LogForm>>()

async function addLog(body: CreateLogRequest) {
  if (!goal.value) return
  try {
    await api.addLog(goal.value.id, body)
    addForm.value?.reset()
    await load()
  } catch (e) {
    error.value = (e as Error).message
  }
}

// edit-goal form
const editingGoal = ref(false)
// Bumped each time the editor opens, so a save that finishes after the editor was closed
// and reopened does not close the newer editor.
let goalEditSession = 0

function startEditGoal() {
  goalEditSession++
  editingGoal.value = true
}

async function saveGoal(body: CreateGoalRequest) {
  if (!goal.value) return
  const session = goalEditSession
  try {
    await api.updateGoal(goal.value.id, body)
    if (session === goalEditSession) editingGoal.value = false
    await load()
  } catch (e) {
    error.value = (e as Error).message
  }
}

// inline edit of a single log row
const editLogId = ref<number | null>(null)

async function saveLog(body: CreateLogRequest) {
  const logId = editLogId.value
  if (logId == null) return
  try {
    await api.updateLog(logId, body)
    // Another entry may have been opened for editing while this save was pending.
    if (editLogId.value === logId) editLogId.value = null
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

    <div v-show="editLogId === null" class="card">
      <h2>Log an entry</h2>
      <LogForm ref="addForm" submit-label="Add entry" @submit="addLog" />
    </div>

    <div class="card">
      <h2>History ({{ goal.logs.length }})</h2>
      <p v-if="goal.logs.length === 0" class="muted">No entries yet.</p>
      <ul class="log-list">
        <li v-for="l in sortedLogs" :key="l.id" :style="dupStyle(l.id)">
          <div v-if="editLogId === l.id" style="width: 100%">
            <h2>Edit entry · {{ fmtDate(l.date) }}</h2>
            <LogForm
              :initial="{ date: l.date, durationSeconds: l.durationSeconds, note: l.note }"
              submit-label="Save"
              @submit="saveLog"
              @cancel="editLogId = null"
            />
          </div>
          <template v-else>
          <span>
            <strong>{{ fmtDate(l.date) }}</strong>
            <span v-if="l.durationSeconds" class="muted"> · {{ formatDuration(l.durationSeconds) }}</span>
            <span v-if="l.note" class="muted"> · {{ l.note }}</span>
          </span>
          <span class="log-actions">
            <button class="icon-btn" title="Edit entry" aria-label="Edit entry" @click="editLogId = l.id">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <path d="M12 20h9" />
                <path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z" />
              </svg>
            </button>
            <button class="icon-btn danger" title="Delete entry" aria-label="Delete entry" @click="deleteLog(l.id)">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <path d="M3 6h18" />
                <path d="M8 6V4h8v2" />
                <path d="M19 6l-1 14H6L5 6" />
                <path d="M10 11v6M14 11v6" />
              </svg>
            </button>
          </span>
          </template>
        </li>
      </ul>
    </div>

    <NewGoalForm
      v-if="editingGoal"
      :initial="{
        name: goal.name,
        targetCount: goal.targetCount,
        unit: goal.unit,
        periodStart: goal.periodStart,
        periodEnd: goal.periodEnd,
      }"
      @update="saveGoal"
      @cancel="editingGoal = false"
    />
    <div v-else class="goal-links">
      <button class="link-btn" @click="startEditGoal">Edit goal</button>
      <button class="link-btn danger" @click="deleteGoal">Delete goal</button>
    </div>
  </div>
</template>
