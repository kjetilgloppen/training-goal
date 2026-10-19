<script setup lang="ts">
import { ref } from 'vue'
import type { CreateGoalRequest } from '../types'
import { toIso } from '../pace'

// With `initial` the form is an always-open edit form that emits `update`/`cancel`;
// without it, it is the collapsible "new goal" form that emits `create`.
const props = defineProps<{ initial?: CreateGoalRequest }>()
const emit = defineEmits<{
  (e: 'create', goal: CreateGoalRequest): void
  (e: 'update', goal: CreateGoalRequest): void
  (e: 'cancel'): void
}>()

const editing = !!props.initial
const open = ref(editing)
const thisYear = new Date().getFullYear()

const name = ref(props.initial?.name ?? '')
const targetCount = ref<number | null>(props.initial?.targetCount ?? 100)
const unit = ref(props.initial?.unit ?? '')
const periodStart = ref(props.initial?.periodStart ?? toIso(new Date(thisYear, 0, 1)))
const periodEnd = ref(props.initial?.periodEnd ?? toIso(new Date(thisYear, 11, 31)))

function submit() {
  if (!name.value || !targetCount.value) return
  const goal: CreateGoalRequest = {
    name: name.value.trim(),
    targetCount: targetCount.value,
    unit: unit.value.trim() || null,
    periodStart: periodStart.value,
    periodEnd: periodEnd.value,
  }
  if (editing) {
    emit('update', goal)
  } else {
    emit('create', goal)
    reset()
  }
}

function cancel() {
  if (editing) emit('cancel')
  else reset()
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
      <h2>{{ editing ? 'Edit goal' : 'New goal' }}</h2>
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
            <input v-model="unit" maxlength="50" placeholder="hikes" />
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
          <button type="submit" class="btn btn-primary" :disabled="!name || !targetCount">
            {{ editing ? 'Save' : 'Create' }}
          </button>
          <button type="button" class="btn" @click="cancel">Cancel</button>
        </div>
      </form>
    </div>
  </div>
</template>
