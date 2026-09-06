<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api'

const router = useRouter()
const passcode = ref('')
const error = ref('')
const busy = ref(false)

async function submit() {
  error.value = ''
  busy.value = true
  try {
    await api.login(passcode.value)
    router.push({ name: 'goals' })
  } catch (e) {
    error.value = 'Incorrect passcode.'
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div class="card" style="max-width: 360px; margin: 40px auto">
    <h2>Sign in</h2>
    <p class="muted" style="font-size: 14px">Enter your passcode to access your goals.</p>
    <form @submit.prevent="submit">
      <div class="field">
        <label for="pc">Passcode</label>
        <input id="pc" v-model="passcode" type="password" autocomplete="current-password" autofocus />
      </div>
      <p v-if="error" class="error">{{ error }}</p>
      <button class="btn btn-primary btn-lg" style="width: 100%" :disabled="busy || !passcode">
        {{ busy ? 'Signing in…' : 'Sign in' }}
      </button>
    </form>
  </div>
</template>
