<script setup lang="ts">
import { ref } from 'vue'
import type { CreateGoalRequest } from '../types'
import { toIso } from '../pace'

const emit = defineEmits<{ (e: 'create', goal: CreateGoalRequest): void }>()

const open = ref(false)
const thisYear = new Date().getFullYear()

const name = ref('')
const targetCount = ref<number | null>(100)
const unit = ref('')
const periodStart = ref(toIso(new Date(thisYear, 0, 1)))
const periodEnd = ref(toIso(new Date(thisYear, 11, 31)))

function submit() {
  if (!name.value || !targetCount.value) return
  emit('create', {
    name: name.value.trim(),
    targetCount: targetCount.value,
    unit: unit.value.trim() || null,
    periodStart: periodStart.value,
    periodEnd: periodEnd.value,
  })
  reset()
}

function reset() {
  open.value = false
  name.value = ''
  targetCount.value = 100
  unit.value = ''
}
</script>

<template>
  <div>
    <button v-if="!open" class="btn btn-primary btn-lg" @click="open = true">+ New goal</button>
    <div v-else class="card">
      <h2>New goal</h2>
      <form @submit.prevent="submit">
        <div class="field">
          <label>Name</label>
          <input v-model="name" placeholder="e.g. Hike a mountain" autofocus />
        </div>
        <div class="row">
          <div class="field">
            <label>Target count</label>
            <input v-model.number="targetCount" type="number" min="1" />
          </div>
          <div class="field">
            <label>Unit (optional)</label>
            <input v-model="unit" placeholder="hikes" />
          </div>
        </div>
        <div class="row">
          <div class="field">
            <label>Period start</label>
            <input v-model="periodStart" type="date" />
          </div>
          <div class="field">
            <label>Period end</label>
            <input v-model="periodEnd" type="date" />
          </div>
        </div>
        <div class="row" style="margin-top: 4px">
          <button type="submit" class="btn btn-primary" :disabled="!name || !targetCount">Create</button>
          <button type="button" class="btn" @click="reset">Cancel</button>
        </div>
      </form>
    </div>
  </div>
</template>
