<template>
    <div class="grid h-screen grid-rows-[auto_1fr] bg-slate-50">
        <TopNav />
        <div class="flex min-h-0">
            <SideNav />
            <main class="flex-1 overflow-y-auto p-6">
                <RouterView />
            </main>
        </div>
    </div>
</template>

<script setup lang="ts">
import TopNav from '@/components/nav/TopNav.vue';
import SideNav from '@/components/nav/SideNav.vue';
</script>

<style scoped></style>