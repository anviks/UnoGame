<template>
  <v-app>
    <v-app-bar>
      <v-toolbar-title>UNO</v-toolbar-title>
      <v-spacer></v-spacer>

      <template v-if="authStore.username">
        <v-toolbar-title>{{ authStore.username }}</v-toolbar-title>
        <v-btn
          text
          to="/"
        >
          Home
        </v-btn>
        <v-btn
          text
          @click="handleLogout"
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

const authStore = useAuthStore();

const handleLogout = async () => {
  await authStore.logout();
};
</script>

<style></style>
