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

    initPromise = fetcher<User>({
      method: 'GET',
      url: '/auth/whoami',
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

  return { userId, username, initialize };
});
