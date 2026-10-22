<template>
    <section class="mx-auto max-w-3xl space-y-2 pt-12 text-center">
        <h1 class="text-4xl font-bold text-slate-900">Floe</h1>
        <p v-if="auth.isAdmin" class="text-slate-600">Signed in as admin.</p>
    </section>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth';
const auth = useAuthStore();
</script>

<style scoped>

</style>