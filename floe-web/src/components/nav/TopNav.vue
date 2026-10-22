<template>
    <header class="flex h-14 items-center gap-4 border-b border-slate-200 bg-white px-4">
        <button aria-label="Toggle sidebar" class="rounded p-2 hover:bg-slate-100" @click="ui.toggleSidebar()">#</button>
        <RouterLink to="/" class="font-semibold text-slate-900">Floe</RouterLink>
        <div class="ml-auto flex items-center gap-3">
            <button v-if="auth.isAuthenticated" class="text-sm text-slate-600 hover:text-slate-900" @click="logout()">Log out</button>
            <template v-else>
                <RouterLink to="/login" class="text-sm text-slate-600 hover:text-slate-900"
                    active-class="font-medium text-slate-900">Log in</RouterLink>
                <RouterLink to="/register" class="rounded-lg bg-slate-900 px-3 py-1.5 text-sm text-white hover:bg-slate-700">Register</RouterLink>
            </template>
        </div>
    </header>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';

const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();

async function logout() {
    auth.logout();
    await router.push('/');
}
</script>

<style scoped>

</style>