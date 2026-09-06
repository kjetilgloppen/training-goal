<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { api, UnauthorizedError } from '../api'
import type { CreateGoalRequest, Goal } from '../types'
import { toIso } from '../pace'
import GoalCard from '../components/GoalCard.vue'
import NewGoalForm from '../components/NewGoalForm.vue'

const router = useRouter()
const goals = ref<Goal[]>([])
const loading = ref(true)
const error = ref('')

async function load() {
  loading.value = true
  error.value = ''
  try {
    goals.value = await api.getGoals()
  } catch (e) {
    if (e instanceof UnauthorizedError) return router.push({ name: 'login' })
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}

async function createGoal(body: CreateGoalRequest) {
  try {
    await api.createGoal(body)
    await load()
  } catch (e) {
    error.value = (e as Error).message
  }
}

async function logToday(goal: Goal) {
  try {
    await api.addLog(goal.id, { date: toIso(new Date()), durationSeconds: null, note: null })
    await load()
  } catch (e) {
    error.value = (e as Error).message
  }
}

onMounted(load)
</script>

<template>
  <div class="spread" style="margin-bottom: 16px">
    <h1>Your goals</h1>
  </div>

  <NewGoalForm @create="createGoal" />

  <p v-if="error" class="error">{{ error }}</p>
  <p v-if="loading" class="muted" style="margin-top: 20px">Loading…</p>

  <div v-else style="margin-top: 20px">
    <p v-if="goals.length === 0" class="muted">No goals yet — create your first one above.</p>
    <GoalCard v-for="g in goals" :key="g.id" :goal="g" @log-today="logToday" />
  </div>
</template>
