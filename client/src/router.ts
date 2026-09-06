import { createRouter, createWebHistory } from 'vue-router'
import { api, UnauthorizedError } from './api'
import GoalsView from './views/GoalsView.vue'
import GoalDetailView from './views/GoalDetailView.vue'
import LoginView from './views/LoginView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', name: 'goals', component: GoalsView, meta: { requiresAuth: true } },
    { path: '/goals/:id', name: 'goal', component: GoalDetailView, meta: { requiresAuth: true } },
    { path: '/login', name: 'login', component: LoginView },
  ],
})

// Simple auth guard: check the cookie session before entering protected routes.
router.beforeEach(async (to) => {
  if (!to.meta.requiresAuth) return true
  try {
    const authed = await api.me()
    return authed ? true : { name: 'login' }
  } catch (e) {
    if (e instanceof UnauthorizedError) return { name: 'login' }
    return true
  }
})

export default router
