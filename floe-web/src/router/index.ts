import { useAuthStore } from '@/stores/auth';
import { createRouter, createWebHistory } from 'vue-router'

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', component: () => import('@/views/LandingView.vue') },
    { path: '/login', component: () => import('@/views/LoginView.vue'), meta: { guestOnly: true }},
    { path: '/register', component: () => import('@/views/RegisterView.vue'), meta: { guestOnly: true }},
  ],
});

// Redirects for admin and guest pages.
router.beforeEach(to => {
  const auth = useAuthStore();
  if (to.meta.requiresAdmin && !auth.isAdmin) {
    return '/login';
  }
  if (to.meta.guestOnly && auth.isAuthenticated) {
    return '/';
  }
});

declare module 'vue-router' {
  interface RouteMeta {
    guestOnly?: boolean;
    requiresAdmin?: boolean;
  }
}