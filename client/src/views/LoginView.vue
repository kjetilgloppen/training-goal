<script setup lang="ts">
import { useRoute } from 'vue-router'

const route = useRoute()
const messages: Record<string, string> = {
  denied: 'That Google account is not on the tester list.',
  failed: 'Google sign-in was cancelled or failed. Please try again.',
}
const error = typeof route.query.error === 'string' ? messages[route.query.error] : ''
</script>

<template>
  <div class="card" style="max-width: 360px; margin: 40px auto">
    <h2>Sign in</h2>
    <p class="muted" style="font-size: 14px">Sign in with Google to access your goals.</p>
    <p v-if="error" class="error">{{ error }}</p>
    <!-- Full page navigation: the server redirects to Google and back. -->
    <a class="btn btn-primary btn-lg" style="width: 100%; text-align: center" href="/api/auth/google">
      Sign in with Google
    </a>
  </div>
</template>
