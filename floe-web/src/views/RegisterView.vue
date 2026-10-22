<template>
    <section class="mx-auto w-80 pt-12">
        <h1 class="mb-6 text-2xl font-semibold text-slate-900">Create account</h1>
        <form class="space-y-4" @submit.prevent="submit">
            <input v-model.trim="form.email" type="email" autocomplete="email" required placeholder="Email"
                class="w-full rounded-lg border border-slate300 bg-white px-3 py-2" />
            <input v-model.trim="form.password" type="password" autocomplete="new-password" required placeholder="Password"
                class="w-full rounded-lg border border-slate300 bg-white px-3 py-2" />
            <input v-model.trim="form.confirm" type="password" autocomplete="new-password" required placeholder="Confirm password"
                class="w-full rounded-lg border border-slate300 bg-white px-3 py-2" />
            <p v-if="mismatch" class="text-sm text-red-600">Passwords do not match.</p>
            <p v-if="error" class="text-sm text-red-600">{{ error }}</p>
            <button :disabled="submitting || mismatch" class="w-full rounded-lg bg-slate-900 py-2.5 text-white disabled:opacity-50">
                Create Account
            </button>
        </form>
    </section>
</template>

<script setup lang="ts">
import { AuthError, useAuthStore } from '@/stores/auth';
import { computed, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

const auth = useAuthStore();
const router = useRouter();
const form = reactive({ email: '', password: '', confirm: '' });
const error = ref('');
const submitting = ref(false);

const mismatch = computed(() => !!form.confirm && form.confirm !== form.password);

async function submit() {
    if (mismatch.value) {
        return;
    }

    submitting.value = true;
    error.value = '';
    try {
        await auth.register({ email: form.email, password: form.password });
        await router.push('/login');
    } catch (e) {
        error.value = e instanceof AuthError && e.status === 409 ? 'That email has already been registered.'
            : e instanceof AuthError && e.status === 400 ? 'Ensure that the email you entered is correct.'
            : 'Something went wrong.'
    } finally {
        submitting.value = false;
    }
}

</script>

<style scoped>

</style>