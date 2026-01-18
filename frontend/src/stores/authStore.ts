import { useApiRequest } from '@/composables/useApiRequest';
import type { User } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  const userId = ref<number>();
  const username = ref<string>();
  const { request } = useApiRequest();
  let initPromise: Promise<void> | null = null;

  const authenticate = async (): Promise<void> => {
    const { data } = await request<User>('/auth/whoami', {
      method: 'GET',
      showErrorToast: false,
    });

    if (data) {
      userId.value = data.id;
      username.value = data.username;
    }
  };

  const initialize = async (): Promise<void> => {
    if (initPromise) {
      return initPromise;
    }

    initPromise = authenticate();

    return initPromise;
  };

  // Initialize immediately when store is created
  initialize();

  const logout = async (): Promise<void> => {
    await request('/auth/logout', {
      method: 'POST',
      showErrorToast: false,
    });

    userId.value = undefined;
    username.value = undefined;
  };

  return { authenticate, userId, username, initialize, logout };
});
