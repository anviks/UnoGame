<template>
  <v-container v-if="game">
    <card
      v-if="game.discardPile[0]"
      :color="game.discardPile[0].color"
      :value="game.discardPile[0].value"
    ></card>
    <draw-pile
      :amount="drawPileSize"
      @click="drawCard"
    />
    <div
      style="position: absolute; bottom: 100px; left: 50%; transform: translateX(-50%)"
      class="uno-card-hand d-flex ga-2"
    >
      <card-choice
        v-for="(card, index) in thisPlayer?.cards"
        :color="card.color"
        :value="card.value"
        :key="index"
        :ref="el => cardRefs[index] = el"
        @card-chosen="(chosenColor) => playCard(index, card, chosenColor)"
      ></card-choice>
    </div>
  </v-container>
</template>

<script
  setup
  lang="ts"
>
import { type ComponentPublicInstance, computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { GameApi } from '@/api/GameApi.ts';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore.ts';
import CardChoice from '@/components/CardChoice.vue';
import type { UnoCard, Game, Player } from '@/types.ts';
import { useToast } from 'vue-toastification';
import Card from '@/components/Card.vue';
import { errorMessages, type GameErrorCode, GameErrorCodes } from '@/constants.ts';
import DrawPile from '@/components/DrawPile.vue';

const props = defineProps({
  gameId: {
    type: Number,
    required: true,
  },
});

const authStore = useAuthStore();
const toast = useToast();
const cardRefs = ref<ComponentPublicInstance<any | typeof CardChoice>[]>([]);

const connected = ref(false);
const connection = ref<HubConnection>();
const messages = ref<any[]>([]);

const sendMessage = (user: string, message: string) => {
  connection.value!
    .send('SendMessage', user, message)
    .catch((err) => console.error(err));
};

interface PlayCardResponse {
  success: boolean;
  error?: string;
}

const playCard = async (index: number, card: UnoCard, chosenColor?: number) => {
  const response: PlayCardResponse = await connection.value!.invoke('PlayCard', card, chosenColor);

  if (!response.success) {
    const errorCode = response.error as GameErrorCode;

    if (errorCode === GameErrorCodes.INVALID_CARD) {
      cardRefs.value[index].triggerShake();
    } else {
      toast.error(errorMessages[errorCode] ?? errorCode);
    }
  }
};

const drawCard = async () => {
  const response = await connection.value!.invoke('DrawCard');

  if (!response.success) {
    const errorCode = response.error as GameErrorCode;
    toast.error(errorMessages[errorCode] ?? errorCode);
  }
};

const connectToGame = async () => {
  connection.value = new HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_BACKEND_URL}/gamehub?gameId=${props.gameId}`)
    .build();

  connection.value.on('ReceiveMessage', (user: string, message: string) => {
    messages.value.push({ user, text: message, timestamp: new Date() });
  });

  connection.value.on('ReceiveMove', (player, move) => {
    messages.value.push({
      user: player,
      text: `Made a move: ${move}`,
      timestamp: new Date(),
    });
  });

  connection.value.on('Error', (message: string) => {
    toast.error(message);
  });

  connection.value.on('CardPlayed', async (player: Player, card: UnoCard, chosenColor: number | null) => {
    game.value = await GameApi.getGame(props.gameId);
  });

  connection.value.on('CardDrawn', async (player: Player, card: UnoCard, chosenColor: number | null) => {
    game.value = await GameApi.getGame(props.gameId);
  });

  connection.value
    .start()
    .then(() => {
      connected.value = true;
      sendMessage('System', `${authStore.username} joined the game.`);
    })
    .catch((err) => console.error('Connection failed: ', err));
};

const game = ref<Game>();
const drawPileSize = ref();

watch(game, (value, oldValue, onCleanup) => {
  if (value) {
    drawPileSize.value = value.drawPile.length;
  }
});

const thisPlayer = computed(() => game?.value?.players?.find((player: Player) => player.userId === authStore.userId));

onMounted(async () => {
  await connectToGame();
  game.value = await GameApi.getGame(props.gameId);
});

onUnmounted(() => {
  sendMessage('System', `${authStore.username} left the game.`);
});
</script>

<style
  scoped
  lang="scss"
>
.uno-card-hand {
  display: flex;
  flex-direction: row;
  align-items: center;
  flex-wrap: wrap;
}
</style>