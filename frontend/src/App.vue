<template>
  <v-app>
    <v-app-bar>
      <v-toolbar-title>UNO</v-toolbar-title>
      <v-spacer></v-spacer>
      <v-toolbar-title v-if="authStore.username">{{ authStore.username }}</v-toolbar-title>
      <v-btn
        text
        to="/"
      >
        Home
      </v-btn>
    </v-app-bar>

    <v-main class="overflow-auto">
      <router-view />
    </v-main>

    <v-footer app>
      <span>&copy; 2025 UNO</span>
    </v-footer>
  </v-app>
</template>

<script
  setup
  lang="ts"
>
import { RouterView } from 'vue-router';
import { useAuthStore } from '@/stores/authStore.ts';

const authStore = useAuthStore();
</script>

<style>
:root {
  --uno-red: #ff5555;
  --uno-yellow: #ffaa00;
  --uno-green: #55aa55;
  --uno-blue: #5555ff;
}

@font-face {
  font-family: 'Bungee';
  src: url('@/assets/fonts/Bungee-Regular.ttf') format('truetype');
  font-display: swap;
}
</style>
