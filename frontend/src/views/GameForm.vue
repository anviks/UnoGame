<template>
  <v-container
    max-width="800px"
  >
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
        <h2>Cards in the game</h2>
        <div>
          <table class="bordered-table">
            <tbody>
            <tr>
              <td></td>
              <td
                v-for="i in 13"
                :key="i"
              >
                <v-checkbox
                  :indeterminate="getCheckboxValue(null, i - 1) === null"
                  :model-value="getCheckboxValue(null, i - 1)"
                  @update:model-value="setCheckboxValue(null, i - 1, $event!)"
                  hide-details
                  style="justify-items: center"
                />
              </td>
            </tr>

            <tr
              v-for="(color, i) in getAllColors(true)"
              :key="i"
            >
              <template
                v-for="(value, j) in getAllValues(true)"
                :key="j"
              >
                <td v-if="j === 0">
                  <v-checkbox
                    :indeterminate="getCheckboxValue(i, null) === null"
                    :model-value="getCheckboxValue(i, null)"
                    @update:model-value="setCheckboxValue(i, null, $event!)"
                    hide-details
                  />
                </td>

                <td>
                  <uno-card
                    v-if="enabledCards[i] && enabledCards[i][j] != null"
                    :value="value"
                    :color="color"
                    :disabled="!enabledCards[i][j]"
                    @click="enabledCards[i][j] = !enabledCards[i][j]"
                    :size="50"
                    style="vertical-align: middle"
                  />
                </td>
              </template>
            </tr>
            </tbody>
          </table>
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

<script
  setup
  lang="ts"
>
import { onMounted, onUnmounted, ref } from 'vue';
import { type Card, type GameForm, type PlayerField, type User } from '@/types.ts';
import { cardColor, cardValue, playerType } from '@/constants.ts';
import { getAllColors, getAllValues } from '@/helpers.ts';
import { UnoCard, GameFormPlayerRow } from '@/components';
import { useAuthStore } from '@/stores/authStore.ts';
import { GameApi, UserApi } from '@/api';
import { useToast } from 'vue-toastification';
import { useRouter } from 'vue-router';

const rules = {
  gameName: [
    (value: string) => !!value || 'Game name is required',
  ],
};

const getDefaultPlayer = (): PlayerField => ({
  name: '',
  type: playerType.COMPUTER,
});

const form = ref();
const toast = useToast();
const router = useRouter();

const game = ref<GameForm>({
  gameName: '',
  players: [
    getDefaultPlayer(),
    getDefaultPlayer(),
  ],
  deck: undefined,
});

const enabledCards = ref<boolean[][]>(Array(4).fill(null).map(() => Array(13).fill(true)));

const getCheckboxValue = (row: number | null, column: number | null) => {
  if (row != null) {
    if (enabledCards.value[row].every((col) => col === true)) {
      return true;
    } else if (enabledCards.value[row].every((col) => col === false)) {
      return false;
    } else {
      return null;
    }
  } else {
    if (enabledCards.value.every((row) => row[column!] === true)) {
      return true;
    } else if (enabledCards.value.every((row) => row[column!] === false)) {
      return false;
    } else {
      return null;
    }
  }
};

const setCheckboxValue = (row: number | null, column: number | null, value: boolean) => {
  if (row != null) {
    for (let j = 0; j < 13; j++) {
      enabledCards.value[row][j] = value;
    }
  } else {
    for (let i = 0; i < 4; i++) {
      enabledCards.value[i][column!] = value;
    }
  }
};

const createGame = async () => {
  const validationResult = await form.value?.validate();
  if (!validationResult.valid) return;

  const deck: Card[] = [];
  for (let i = 0; i < enabledCards.value.length; i++) {
    for (let j = 0; j < enabledCards.value[i].length; j++) {
      if (enabledCards.value[i][j]) {
        deck.push({
          color: getAllColors()[i],
          value: getAllValues()[j],
        });
      }
    }
  }

  for (let i = 0; i < 2; i++) {
    deck.push({
      color: cardColor.WILD,
      value: cardValue.WILD,
    });

    deck.push({
      color: cardColor.WILD,
      value: cardValue.WILD_DRAW_FOUR,
    });
  }

  game.value.deck = deck;

  const result = await GameApi.createGame(game.value);

  toast.success('Game created!');

  await router.push({ name: 'home', state: { highlightId: result.id } });
};

const users = ref<User[]>([]);
const authStore = useAuthStore();

onMounted(async () => {
  users.value = await UserApi.getAllUsers();
  if (!authStore.username) {
    // authStore.isRegisterDialogOpen = true;
  }
});

onUnmounted(() => {
  // authStore.isRegisterDialogOpen = false;
});

</script>

<style
  lang="scss"
  scoped
>
.remove-player-button {
  opacity: 0.5;

  &:hover {
    opacity: 1;
  }
}

.grid {
  display: grid;
  grid-template-columns: repeat(13, 1fr);
  grid-template-rows: repeat(4, 1fr);
  gap: 10px;
  padding: 20px;
}

.cell {
  background-color: #f0f0f0;
  border: 1px solid #ccc;
  padding: 20px;
  text-align: center;
  border-radius: 4px;
  font-family: sans-serif;
}

.bordered-table {
  border-width: 1px;
  border-style: solid;
  border-color: rgb(0, 0, 0, 0.5);
  border-radius: 5px;

  & td {
    border-width: 1px;
    border-style: solid;
    border-color: rgb(0, 0, 0, 0.3);
    border-radius: 5px;
    min-width: 48px;
    justify-items: center;
  }
}
</style>