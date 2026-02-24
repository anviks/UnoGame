<template>
  <v-container
    v-if="items.length === 0"
    max-width="1000px"
    class="h-full flex justify-center items-center"
  >
    <div class="flex flex-col items-center text-center">
      <v-icon
        size="80"
        class="mb-5 opacity-15!"
      >
        mdi-cards-playing-outline
      </v-icon>

      <p class="text-lg mb-6 opacity-40!">No games yet.</p>

      <v-btn
        :to="{ name: 'new-game' }"
        prepend-icon="mdi-plus"
      >
        Create your first game
      </v-btn>
    </div>
  </v-container>

  <v-container
    v-else
    max-width="1000px"
    class="h-full"
  >
    <div class="flex items-center justify-between mb-8">
      <h1 class="text-2xl">Games</h1>
      <v-btn
        color="primary"
        :to="{ name: 'new-game' }"
        prepend-icon="mdi-plus"
      >
        New Game
      </v-btn>
    </div>
    <div class="games-grid">
      <v-card
        v-for="game in items"
        :key="game.id"
        class="game-card pa-4"
        :class="{ 'game-card--highlighted': highlightedRows.has(game.id) }"
        rounded="lg"
        elevation="1"
      >
        <div class="flex items-start gap-3 mb-4">
          <div class="flex-1 min-w-0">
            <div class="game-name">{{ game.name }}</div>
            <v-chip
              :color="isFinished(game) ? 'success' : 'primary'"
              size="x-small"
              variant="tonal"
              class="mt-1.5"
            >
              {{ isFinished(game) ? 'Finished' : 'In Progress' }}
            </v-chip>
          </div>

          <uno-card
            v-if="getTopCard(game)"
            :color="getTopCard(game)!.color"
            :value="getTopCard(game)!.value"
            :size="65"
            shadowed
          />
        </div>

        <div class="flex flex-col gap-0.5 mb-4">
          <div
            v-for="(player, i) in game.state.players"
            :key="i"
            class="flex items-center text-[0.8125rem] py-0.5!"
            :class="{
              'player-row--current': isCurrentPlayer(game, i),
              'player-row--winner': isWinner(game, i),
            }"
          >
            <v-icon
              v-if="isWinner(game, i)"
              color="amber-darken-1"
              size="13"
            >
              mdi-trophy
            </v-icon>
            <v-icon
              v-else-if="isCurrentPlayer(game, i)"
              color="primary"
              size="13"
            >
              mdi-play
            </v-icon>
            <span
              v-else
              class="inline-block w-3.25"
            />

            <v-icon
              size="13"
              class="player-name ml-1 opacity-35"
            >
              {{
                player.type === playerType.COMPUTER
                  ? 'mdi-robot-outline'
                  : 'mdi-account-outline'
              }}
            </v-icon>
            <span class="player-name ml-1.5 truncate">{{ player.name }}</span>
            <span class="player-cards ml-auto">{{ player.handSize }}</span>
          </div>
        </div>

        <div class="flex items-center justify-between pt-3 game-card-footer">
          <span class="text-xs opacity-40">{{ game.updatedAt.fromNow() }}</span>
          <v-btn
            v-if="canJoin(game)"
            :to="{ name: 'game', params: { gameId: game.id } }"
            size="small"
            variant="tonal"
          >
            Join
          </v-btn>
        </div>
      </v-card>
    </div>
  </v-container>
</template>

<script setup lang="ts">
import { UnoCard } from '@/components';
import { useApiRequest } from '@/composables/useApiRequest';
import { playerType } from '@/constants.ts';
import { gameDtoToGame } from '@/helpers/mappers';
import { useAuthStore } from '@/stores/authStore.ts';
import type { Card, Game, GameDto } from '@/types.ts';
import { onMounted, ref } from 'vue';

const authStore = useAuthStore();
const { request } = useApiRequest<GameDto[]>('/games');
const items = ref<Game[]>([]);
const highlightedRows = ref(new Set<number>());

const getTopCard = (game: Game): Card | null => {
  const pile = game.state.discardPile;
  return pile.length > 0 ? pile[pile.length - 1]! : null;
};

const isFinished = (game: Game) => game.state.winnerIndex !== null;

const isCurrentPlayer = (game: Game, index: number) =>
  !isFinished(game) && game.state.currentPlayerIndex === index;

const isWinner = (game: Game, index: number) =>
  game.state.winnerIndex === index;

const canJoin = (game: Game) =>
  game.state.players.some((player) => player.userId === authStore.userId);

const highlightRow = (id: number) => {
  highlightedRows.value.add(id);
  requestAnimationFrame(() => {
    highlightedRows.value.delete(id);
  });
};

onMounted(async () => {
  const { success, data } = await request({
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
.player-row {
  &--current .player-name {
    font-weight: 600;
    color: rgb(var(--v-theme-primary));
  }

  &--winner .player-name {
    font-weight: 600;
    color: rgb(var(--v-theme-success));
  }
}

.games-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(270px, 1fr));
  gap: 16px;
}

.game-card {
  transition:
    box-shadow 0.2s ease,
    background-color 2s ease-in-out !important;

  &:hover {
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12) !important;
  }

  &--highlighted {
    background-color: rgba(59, 129, 237, 0.18) !important;
  }
}

.game-name {
  font-size: 1rem;
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.player-cards {
  font-size: 0.75rem;
  opacity: 0.5;
  background: rgba(0, 0, 0, 0.06);
  padding: 1px 7px;
  border-radius: 10px;
  min-width: 28px;
  text-align: center;
}

.game-card-footer {
  border-top: 1px solid rgba(0, 0, 0, 0.07);
}
</style>
