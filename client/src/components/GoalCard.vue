<script setup lang="ts">
import { computed } from 'vue'
import type { Goal } from '../types'
import { computeStats } from '../pace'

const props = defineProps<{ goal: Goal }>()
const emit = defineEmits<{ (e: 'log-today', goal: Goal): void }>()

const stats = computed(() => computeStats(props.goal))
const unit = computed(() => props.goal.unit || 'done')
const pct = computed(() => Math.min(100, Math.round(stats.value.percentComplete * 100)))
const deltaLabel = computed(() => {
  const d = Math.round(Math.abs(stats.value.delta))
  return stats.value.onTrack ? `${d} ahead` : `${d} behind`
})
</script>

<template>
  <div class="card goal-card">
    <RouterLink :to="{ name: 'goal', params: { id: goal.id } }" style="text-decoration: none; color: inherit">
      <div class="spread">
        <h2 style="margin: 0">{{ goal.name }}</h2>
        <span class="badge" :class="stats.onTrack ? 'ahead' : 'behind'">{{ deltaLabel }}</span>
      </div>
      <div class="progress"><span :style="{ width: pct + '%' }"></span></div>
      <div class="spread">
        <span class="muted">
          {{ stats.actualCount }} / {{ goal.targetCount }} {{ unit }}
          &middot; target today {{ Math.round(stats.expectedCount) }}
        </span>
        <span class="muted">{{ pct }}%</span>
      </div>
    </RouterLink>
    <button class="btn btn-primary" style="margin-top: 12px" @click="emit('log-today', goal)">
      + Log today
    </button>
  </div>
</template>
