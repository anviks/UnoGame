<template>
  <v-container max-width="800px">
    <v-row>
      <v-col cols="12">
        <h1>Create a new game</h1>
      </v-col>
    </v-row>

    <v-form
      ref="form"
      @submit.prevent
    >
      <v-row>
        <v-col cols="12">
          <v-text-field
            v-model="game.gameName"
            label="Game name"
            :rules="rules.gameName"
          />
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <template
            v-for="(player, index) in game.players"
            :key="index"
          >
            <game-form-player-row
              :player="player"
              :index="index"
              :users="users"
              :can-delete="game.players.length > 2"
              @delete-player="game.players.splice(index, 1)"
            />
          </template>
        </v-col>
      </v-row>

      <v-row>
        <v-col cols="12">
          <v-btn
            @click="game.players.push(getDefaultPlayer())"
            :disabled="game.players.length >= 10"
          >
            Add player
          </v-btn>
        </v-col>
      </v-row>

      <div class="mt-8 mb-8">
        <div class="flex items-center justify-between mb-3">
          <h2>Cards in the game</h2>
          <div class="flex gap-2">
            <v-btn
              size="small"
              variant="tonal"
              @click="setAllCards(true)"
              >Select all</v-btn
            >
            <v-btn
              size="small"
              variant="outlined"
              @click="setAllCards(false)"
              >Deselect all</v-btn
            >
          </div>
        </div>

        <div class="flex flex-wrap gap-1 mb-5">
          <v-chip
            v-for="(_, valueIndex) in getAllValues(true)"
            :key="valueIndex"
            size="small"
            :variant="
              ({ true: 'flat', false: 'outlined', null: 'tonal' } as const)[
                String(getColumnState(valueIndex))
              ]
            "
            @click="toggleColumn(valueIndex)"
            class="cursor-pointer"
          >
            {{ valueLabels[valueIndex] }}
          </v-chip>
        </div>

        <div class="flex flex-col gap-5">
          <div
            v-for="(color, colorIndex) in getAllColors(true)"
            :key="colorIndex"
            class="color-section"
            :class="`color-section--${colorKeys[colorIndex]}`"
          >
            <div class="flex items-center mb-2">
              <span
                class="text-sm tracking-[0.04em] font-semibold capitalize"
                >{{ colorKeys[colorIndex] }}</span
              >
              <div class="flex items-center gap-0 ml-auto">
                <v-btn
                  density="compact"
                  variant="text"
                  size="small"
                  @click="setRowCards(colorIndex, true)"
                  >All</v-btn
                >
                <span class="opacity-25 select-none">|</span>
                <v-btn
                  density="compact"
                  variant="text"
                  size="small"
                  @click="setRowCards(colorIndex, false)"
                  >None</v-btn
                >
              </div>
            </div>
            <div class="flex gap-1">
              <uno-card
                v-for="(value, valueIndex) in getAllValues(true)"
                :key="valueIndex"
                :value="value"
                :color="color"
                :disabled="!enabledCards[colorIndex]?.[valueIndex]"
                @click="
                  enabledCards[colorIndex]![valueIndex] =
                    !enabledCards[colorIndex]![valueIndex]
                "
                :size="55"
                class="cursor-pointer"
              />
            </div>
          </div>
        </div>
      </div>

      <v-row>
        <v-col cols="12">
          <v-btn @click="createGame">Create game</v-btn>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script setup lang="ts">
import { GameFormPlayerRow, UnoCard } from '@/components';
import { useApiRequest } from '@/composables/useApiRequest';
import { cardColor, cardValue, playerType } from '@/constants.ts';
import { getAllColors, getAllValues } from '@/helpers/cards';
import { useAuthStore } from '@/stores/authStore.ts';
import type { GameDto } from '@/types.ts';
import type { CardPayload, GameForm, PlayerField, User } from '@/types.ts';
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const rules = {
  gameName: [(value: string) => !!value || 'Game name is required'],
};

const getDefaultPlayer = (): PlayerField => ({
  username: '',
  type: playerType.COMPUTER,
});

const form = ref();
const router = useRouter();
const route = useRoute();
const { request: createGameRequest } = useApiRequest<GameDto>('/games');
const { request: fetchUsersRequest } = useApiRequest<User[]>('/users');

const game = ref<GameForm>({
  gameName: '',
  players: [getDefaultPlayer(), getDefaultPlayer()],
  includedCards: undefined,
});

const enabledCards = ref<boolean[][]>(
  Array(4)
    .fill(null)
    .map(() => Array(13).fill(true))
);

const colorKeys = ['red', 'yellow', 'green', 'blue'];
const valueLabels = [
  '0',
  '1',
  '2',
  '3',
  '4',
  '5',
  '6',
  '7',
  '8',
  '9',
  'Skip',
  'Rev',
  '+2',
];

const setAllCards = (value: boolean) => {
  for (let i = 0; i < 4; i++) {
    for (let j = 0; j < 13; j++) {
      enabledCards.value[i]![j] = value;
    }
  }
};

const setRowCards = (row: number, value: boolean) => {
  for (let j = 0; j < 13; j++) {
    enabledCards.value[row]![j] = value;
  }
};

const getColumnState = (column: number) => {
  if (enabledCards.value.every((row) => row[column] === true)) return true;
  if (enabledCards.value.every((row) => row[column] === false)) return false;
  return null;
};

const toggleColumn = (column: number) => {
  const newValue = getColumnState(column) !== true;
  for (let i = 0; i < 4; i++) {
    enabledCards.value[i]![column] = newValue;
  }
};

const createGame = async () => {
  const validationResult = await form.value?.validate();
  if (!validationResult.valid) return;

  const includedCards: CardPayload[] = [];
  for (let i = 0; i < enabledCards.value.length; i++) {
    for (let j = 0; j < enabledCards.value[i]!.length; j++) {
      if (enabledCards.value[i]![j]) {
        includedCards.push({
          color: getAllColors()[i]!,
          value: getAllValues()[j]!,
        });
      }
    }
  }

  for (let i = 0; i < 2; i++) {
    includedCards.push({
      color: cardColor.WILD,
      value: cardValue.WILD,
    });

    includedCards.push({
      color: cardColor.WILD,
      value: cardValue.WILD_DRAW_FOUR,
    });
  }

  game.value.includedCards = includedCards;

  const { success, data } = await createGameRequest({
    method: 'POST',
    data: game.value,
    errorMessage: 'Error creating game: {error}',
    successMessage: 'Game created!',
  });

  if (!success) return;

  await router.push({ name: 'home', state: { highlightId: data.id } });
};

const users = ref<User[]>([]);
const authStore = useAuthStore();

onMounted(async () => {
  await authStore.initialize();

  if (!authStore.username) {
    await router.push({ name: 'register', query: { return: route.path } });
  }

  const { success, data } = await fetchUsersRequest({
    method: 'GET',
    errorMessage: 'Error fetching users.',
  });

  if (!success) return;

  users.value = data;
});
</script>

<style lang="scss" scoped>
.color-section {
  border-left: 4px solid;
  padding-left: 14px;

  &--red {
    border-color: #ef5350;
  }
  &--yellow {
    border-color: #fdd835;
  }
  &--green {
    border-color: #66bb6a;
  }
  &--blue {
    border-color: #42a5f5;
  }
}
</style>
