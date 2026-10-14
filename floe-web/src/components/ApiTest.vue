<script setup lang="ts">
import { ref, onMounted } from 'vue';

const weathers = ref<string[]>([]);
const loading = ref<boolean>(true);
const errorMsg = ref<string | null>(null);

async function doFetch() {
    try {
        loading.value = true;
        errorMsg.value = null;
        
        const resp = await fetch('/api/weather');
        if (!resp.ok) {
            throw new Error(`api error: ${resp.status} - ${resp.statusText}`);
        }

        const data = await resp.json();
        weathers.value = data;
    } catch (err) {
        console.error('failed to fetch from the api:', err);
        errorMsg.value = err instanceof Error ? err.message : 'an unknown error has occurred.';
    } finally {
        loading.value = false;
    }
}

onMounted(() => {
    doFetch();
});
</script>

<template>
    <h2>Connection Check</h2>

    <div v-if="loading">
        <span>Loading from the API...</span>
    </div>

    <div v-else-if="errorMsg">
        <p>An error has occurred...</p>
        <small>{{ errorMsg }}</small>
        <button @click="doFetch">Try Again</button>
    </div>

    <div v-else>
        <p>Successfully communicating with API</p>

        <ul>
            <li v-for="(item, index) in weathers" :key="index">
                {{ item }}
            </li>
        </ul>
    </div>
</template>

<style scoped>
</style>