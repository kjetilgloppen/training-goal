<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { ref, watchEffect } from 'vue'
import { api } from './api'

const router = useRouter()
const route = useRoute()
const loggedIn = ref(false)

// Keep the header's logout button in sync with the current route/session.
watchEffect(async () => {
  if (route.name === 'login') {
    loggedIn.value = false
    return
  }
  try {
    loggedIn.value = await api.me()
  } catch {
    loggedIn.value = false
  }
})

async function logout() {
  await api.logout()
  loggedIn.value = false
  router.push({ name: 'login' })
}
</script>

<template>
  <header class="app-header">
    <RouterLink to="/" class="brand">🏔️ Training Goals</RouterLink>
    <button v-if="loggedIn" class="link-btn" @click="logout">Log out</button>
  </header>
  <main class="container">
    <RouterView />
  </main>
</template>
