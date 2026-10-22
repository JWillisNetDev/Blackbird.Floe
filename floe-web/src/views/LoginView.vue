<template>
    <section class="mx-auto w-80 pt-12">
        <h1 class="mb-6 text-2xl font-semibold text-slate-900">Log in</h1>
        <form class="space-y-4" @submit.prevent="submit">
            <input v-model.trim="form.email" type="email" autocomplete="email" required placeholder="Email"
                class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2" />
            <input v-model.trim="form.password" type="password" autocomplete="password" required placeholder="Password"
                class="w-full rounded-lg border border-slate-300 bg-white px-3 py-2" />
            <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
            <button :disabled="submitting" class="w-full rounded-lg bg-slate-900 py-2.5 text-white disabled:opacity-50">Log in</button>
        </form>
    </section>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore, AuthError } from '@/stores/auth';

const auth = useAuthStore();
const router = useRouter();
const form = reactive({ email: '', password: '' });
const error = ref('');
const submitting = ref(false);

async function submit() {
    submitting.value = true;
    error.value = '';
    try {
        await auth.login(form);
        await router.push('/');
    } catch (e) {
        error.value = e instanceof AuthError && e.status === 401 ? 'Invalid email or password.' : 'Something went wrong.';
    } finally {
        submitting.value = false;
    }
}

</script>

<style scoped>

</style>