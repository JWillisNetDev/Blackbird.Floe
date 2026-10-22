<template>
    <nav :class="['shrink-0 overflow-y-auto border-r border-slate-200 bg-white transition-[width] duration-200', ui.sidebarCollapsed ? 'w-16' : 'w-60']">
        <NavGroup v-for="g in groups" :key="g.label" v-bind="g" :collapsed="ui.sidebarCollapsed" />
    </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import NavGroup, { type NavItem } from './NavGroup.vue';

const auth = useAuthStore();
const ui = useUiStore();

const groups = computed<{ label: string; items: NavItem[] }[]>(() => [
    { label: 'General', items: [{ label: 'Home', to: '/' }] },
    ...(auth.isAdmin ? [{ label: 'Admin', items: [{ label: 'Users', to: '/admin/users' }] }] : []),
]);
</script>

<style scoped>

</style>