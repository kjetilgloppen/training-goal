<script setup lang="ts">
import { ref } from 'vue'
import type { CreateLogRequest } from '../types'
import { toIso, toSeconds } from '../pace'

defineProps<{ submitLabel: string }>()
const emit = defineEmits<{ (e: 'submit', log: CreateLogRequest): void }>()

const date = ref(toIso(new Date()))
const hours = ref<number | null>(null)
const minutes = ref<number | null>(null)
const seconds = ref<number | null>(null)
const note = ref('')

function submit() {
  emit('submit', {
    date: date.value,
    durationSeconds: toSeconds(hours.value, minutes.value, seconds.value),
    note: note.value.trim() || null,
  })
}

// Called by the parent once the entry has been saved, so a failed save keeps what the user
// typed. Clears everything but the date, so several entries for one day are quick to add.
function reset() {
  hours.value = null
  minutes.value = null
  seconds.value = null
  note.value = ''
}

defineExpose({ reset })
</script>

<template>
  <form @submit.prevent="submit">
    <div class="field">
      <label>Date</label>
      <input v-model="date" type="date" />
    </div>
    <div class="field">
      <label>Duration (optional)</label>
      <div class="row">
        <input v-model.number="hours" type="number" min="0" placeholder="h" aria-label="hours" />
        <input v-model.number="minutes" type="number" min="0" max="59" placeholder="m" aria-label="minutes" />
        <input v-model.number="seconds" type="number" min="0" max="59" placeholder="s" aria-label="seconds" />
      </div>
    </div>
    <div class="field">
      <label>Note (optional)</label>
      <input v-model="note" placeholder="e.g. Mt. Rainier, great weather" />
    </div>
    <button type="submit" class="btn btn-primary">{{ submitLabel }}</button>
  </form>
</template>
