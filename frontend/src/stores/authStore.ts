import { ref } from 'vue';
import { defineStore } from 'pinia';
import { AuthApi } from '@/api';

export const useAuthStore = defineStore('auth', () => {
  const userId = ref<number>();
  const username = ref<string>();

  AuthApi.whoAmI().then((user) => {
    if (user) {
      userId.value = user.id;
      username.value = user.name;
    }
  });

  return { userId, username };
});


