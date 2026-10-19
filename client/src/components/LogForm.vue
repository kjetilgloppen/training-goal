<script setup lang="ts">
import { ref } from 'vue'
import type { CreateLogRequest } from '../types'
import { toIso, toSeconds, splitDuration } from '../pace'

// With `initial` the form edits an existing entry and shows a Cancel button. Without it,
// it is the "add entry" form; the parent calls `reset()` once the entry has been saved,
// so a failed save keeps what the user typed.
const props = defineProps<{ initial?: CreateLogRequest; submitLabel: string }>()
const emit = defineEmits<{
  (e: 'submit', log: CreateLogRequest): void
  (e: 'cancel'): void
}>()

const { h, m, s } = splitDuration(props.initial?.durationSeconds ?? 0)

const date = ref(props.initial?.date ?? toIso(new Date()))
const hours = ref<number | null>(h || null)
const minutes = ref<number | null>(m || null)
const seconds = ref<number | null>(s || null)
const note = ref(props.initial?.note ?? '')

function submit() {
  emit('submit', {
    date: date.value,
    durationSeconds: toSeconds(hours.value, minutes.value, seconds.value),
    note: note.value.trim() || null,
  })
}

// Clears everything but the date, so several entries for one day are quick to add.
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
    <div :class="{ row: initial }">
      <button type="submit" class="btn btn-primary">{{ submitLabel }}</button>
      <button v-if="initial" type="button" class="btn" @click="emit('cancel')">Cancel</button>
    </div>
  </form>
</template>
