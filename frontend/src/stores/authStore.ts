import { useApiRequest } from '@/composables/useApiRequest';
import type { User } from '@/types';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  const userId = ref<number>();
  const username = ref<string>();
  const fetcher = useApiRequest();
  let initPromise: Promise<void> | null = null;

  const initialize = async (): Promise<void> => {
    if (initPromise) {
      return initPromise;
    }

    initPromise = fetcher<User>('/auth/whoami', {
      method: 'GET',
      showErrorToast: false,
    }).then(({ data }) => {
      if (data) {
        userId.value = data.id;
        username.value = data.username;
      }
    });

    return initPromise;
  };

  // Initialize immediately when store is created
  initialize();

  const logout = async (): Promise<void> => {
    await fetcher('/auth/logout', {
      method: 'POST',
      showErrorToast: false,
    });

    userId.value = undefined;
    username.value = undefined;
  };

  return { userId, username, initialize, logout };
});
