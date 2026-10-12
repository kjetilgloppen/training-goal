<script setup lang="ts">
import { useRouter, useRoute } from 'vue-router'
import { ref, watchEffect } from 'vue'
import { api } from './api'

const router = useRouter()
const route = useRoute()
const loggedIn = ref(false)
const userName = ref('')

// Keep the header's user name and logout button in sync with the current route/session.
watchEffect(async () => {
  if (route.name === 'login') {
    loggedIn.value = false
    return
  }
  try {
    const me = await api.me()
    loggedIn.value = me.authenticated
    userName.value = me.name ?? me.email ?? ''
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
    <span v-if="loggedIn" class="muted" style="font-size: 14px; margin-left: auto">{{ userName }}</span>
    <button v-if="loggedIn" class="link-btn" @click="logout">Log out</button>
  </header>
  <main class="container">
    <RouterView />
  </main>
</template>
