<template>
  <v-container max-width="80%">
    <v-data-table
      :headers="headers"
      :items="items"
    >
      <template #top>
        <v-toolbar flat>
          <v-toolbar-title>My Data Table</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-btn
            text
            :to="{ name: 'new-game' }"
          >
            Add Item
          </v-btn>
        </v-toolbar>
      </template>

      <template #item="{ item }: { item: Game }">
        <tr :class="getRowClass(item)">
          <td>{{ item.name }}</td>
          <td>{{ item.createdAt }}</td>
          <td>{{ item.updatedAt }}</td>
          <td>
            <div v-for="player in item.state.players">
              {{ player.name }}
            </div>
          </td>
          <td>
            <v-btn
              v-if="
                item.state.players.some(
                  (player) => player.userId === authStore.userId
                )
              "
              :to="{ name: 'game', params: { gameId: item.id } }"
            >
              Join Game
            </v-btn>
          </td>
        </tr>
      </template>
    </v-data-table>
  </v-container>
</template>

<script setup lang="ts">
import { useApiRequest } from '@/composables/useApiRequest';
import { gameDtoToGame } from '@/helpers/mappers';
import { useAuthStore } from '@/stores/authStore.ts';
import type { Game, GameDto } from '@/types.ts';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const router = useRouter();
const { request } = useApiRequest();
const items = ref<Game[]>([]);
const page = ref(1);
const itemsPerPage = 10;
const highlightedRows = ref(new Set());

const headers = [
  { title: 'Game Name', key: 'gameName' },
  { title: 'Created At', key: 'createdAt' },
  { title: 'Updated At', key: 'updatedAt' },
  { title: 'Players', key: 'players' },
  { title: '', key: 'actions' },
];

const highlightRow = (id: number) => {
  highlightedRows.value.add(id);
  requestAnimationFrame(() => {
    highlightedRows.value.delete(id);
  });
};

const getRowClass = (game: Game) => {
  let isHighlighted = highlightedRows.value.has(game.id);
  return {
    'highlighted-game-row': isHighlighted,
    'game-row': !isHighlighted,
  };
};

onMounted(async () => {
  const { success, data } = await request<GameDto[]>('/games', {
    method: 'GET',
    errorMessage: 'Error fetching games.',
  });

  if (success) {
    items.value = data.map(gameDtoToGame);
  }

  const { highlightId, ...others } = window.history.state;
  if (highlightId) {
    highlightRow(highlightId);
    window.history.replaceState(others, '', window.location.href);
  }
});
</script>

<style scoped lang="scss">
.highlighted-game-row {
  background-color: rgba(59, 129, 237, 0.4) !important;
}

.game-row {
  background-color: transparent;
  transition: background-color 2s ease-in-out;
}
</style>
