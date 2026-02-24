<template>
  <v-app>
    <v-app-bar>
      <v-toolbar-title>
        <router-link :to="{ name: 'home' }"> UNO </router-link>
      </v-toolbar-title>

      <v-spacer></v-spacer>

      <template v-if="authStore.username">
        <v-toolbar-title>{{ authStore.username }}</v-toolbar-title>
        <v-btn
          text
          @click="logout"
        >
          Logout
        </v-btn>
      </template>

      <template v-else>
        <v-btn
          text
          :to="{ name: 'login' }"
        >
          Login
        </v-btn>
        <v-btn
          text
          :to="{ name: 'register' }"
        >
          Register
        </v-btn>
      </template>
    </v-app-bar>

    <v-main class="overflow-auto">
      <router-view />
    </v-main>

    <v-footer app>
      <span>&copy; 2025 UNO</span>
    </v-footer>
  </v-app>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/authStore.ts';
import { RouterView } from 'vue-router';
import { useApiRequest } from './composables/useApiRequest';

const authStore = useAuthStore();
const { request } = useApiRequest('/auth/logout');

const logout = async (): Promise<void> => {
  await request({
    method: 'POST',
    showErrorToast: false,
  });

  await authStore.authenticate();
};
</script>

<style></style>
