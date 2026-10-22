<template>
    <div class="py-2">
        <button v-if="!collapsed" :aria-expanded="open"
            class="flex w-full items-center justify-between px-4 py-1 text-xs font-semibold uppercase text-slate-500"
            @click="open = !open">
            {{ label }} <span>{{ open ? '-' : '+' }}</span>
        </button>
        <ul v-show="open || collapsed">
            <li v-for="item in items" :key="item.label">
                <RouterLink :to="item.to" :title="collapsed ? item.label : undefined"
                    class="block truncate px-4 py-2 text-sm text-slate-700 hover:bg-slate-100"
                    exact-active-class="bg-slate-100 font-medium text-slate-900">
                    {{ collapsed ? item.label[0] : item.label }}
                </RouterLink>
            </li>
        </ul>
    </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { RouteLocationRaw } from 'vue-router';

export interface NavItem { label: string; to: RouteLocationRaw; }

defineProps<{ label: string; items: NavItem[]; collapsed: boolean }>();
const open = ref(true);
</script>

<style scoped>

</style>