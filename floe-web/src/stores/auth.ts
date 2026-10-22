import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { postApiAuthLogin as loginRequest, postApiAuthRegister as registerRequest, type LoginRequest, type RegisterRequest } from '@/api';

const STORAGE_KEY = 'accessToken';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const ROLE_ADMIN = 'Admin';

interface JwtPayload { exp: number; [ROLE_CLAIM]?: string | string[]; }

export class AuthError extends Error {
    constructor(public status: number, detail?: string | null) {
        super(detail ?? `Request failed with status code ${status}`);
    }
}

function decode(token: string): JwtPayload {
    const base64 = token.split('.')[1]!.replace(/-/g, '+').replace(/_/g, '/');
    return JSON.parse(atob(base64));
}

export const useAuthStore = defineStore('auth', () => {
    const token = ref<string | null>(sessionStorage.getItem(STORAGE_KEY));
    const payload = computed(() => (token.value ? decode(token.value) : null));

    const isAuthenticated = computed(() => !!payload.value && payload.value.exp * 1000 > Date.now());
    const isAdmin = computed(() => {
        const role = payload.value?.[ROLE_CLAIM];
        return isAuthenticated.value && (Array.isArray(role) ? role.includes(ROLE_ADMIN) : role === 'Admin');
    });

    function setToken(value: string | null) {
        token.value = value;
        value ? sessionStorage.setItem(STORAGE_KEY, value) : sessionStorage.removeItem(STORAGE_KEY);
    }

    async function login(body: LoginRequest) {
        const { data, error, response } = await loginRequest({ body });
        if (error || !data) {
            throw new AuthError(response?.status ?? 0, error?.detail ?? 'No data received');
        }
        setToken(data.accessToken);
    }

    async function register(body: RegisterRequest) {
        const { error, response } = await registerRequest({ body });
        if (error) {
            throw new AuthError(response?.status ?? 0, error.detail);
        }
    }

    const logout = () => setToken(null);

    if (!isAuthenticated.value) {
        logout();
    }

    return { token, isAuthenticated, isAdmin, login, register, logout };
});
