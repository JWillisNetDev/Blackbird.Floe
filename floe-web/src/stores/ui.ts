import { defineStore } from 'pinia';
import { useStorage } from '@vueuse/core';

export const useUiStore = defineStore('ui', () => {
    const sidebarCollapsed = useStorage('sidebarCollapsed', false);
    const toggleSidebar = () => (sidebarCollapsed.value = !sidebarCollapsed.value);
    return { sidebarCollapsed, toggleSidebar };
})