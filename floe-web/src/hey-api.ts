import type { CreateClientConfig } from "./api/client.gen";
import { useAuthStore } from '@/stores/auth';

export const createClientConfig: CreateClientConfig = config => ({
    ...config,
    baseUrl: import.meta.env.VITE_API_URL, // TODO ensure this is correct for deployment?
    auth: () => useAuthStore().token ?? undefined,
});