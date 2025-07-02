<template>
  <v-container>
    <uno-card
      v-if="game?.discardPile[0]"
      :color="game?.discardPile[0].color"
      :value="game?.discardPile[0].value"
    ></uno-card>
    <div
      style="position: absolute; bottom: 100px; left: 50%; transform: translateX(-50%)"
      class="uno-card-hand d-flex ga-2"
    >
      <uno-card-choice
        v-for="(card, index) in thisPlayer?.cards"
        :color="card.color"
        :value="card.value"
        :key="index"
        :ref="el => cardRefs[index] = el"
        @card-chosen="(chosenColor) => playCard(index, card, chosenColor)"
      ></uno-card-choice>
    </div>
  </v-container>
</template>

<script
  setup
  lang="ts"
>
import { type ComponentPublicInstance, computed, onMounted, onUnmounted, ref } from 'vue';
import { GameApi } from '@/api/GameApi.ts';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore.ts';
import UnoCardChoice from '@/components/UnoCardChoice.vue';
import type { Card, Game, Player } from '@/types.ts';
import { useToast } from 'vue-toastification';
import UnoCard from '@/components/UnoCard.vue';
import _ from 'lodash-es';

const props = defineProps({
  gameId: {
    type: Number,
    required: true,
  },
});

const authStore = useAuthStore();
const toast = useToast();
const cardRefs = ref<ComponentPublicInstance<any | typeof UnoCardChoice>[]>([]);

const connected = ref(false);
const connection = ref<HubConnection>();
const messages = ref<any[]>([]);

const sendMessage = (user: string, message: string) => {
  connection.value!
    .send('SendMessage', user, message)
    .catch((err) => console.error(err));
};

const playCard = async (index: number, card: Card, chosenColor?: number) => {
  try {
    await connection.value!.invoke('PlayCard', card, chosenColor);
  } catch (e) {
    cardRefs.value[index].triggerShake();
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

  connection.value.on('CardPlayed', async (player: Player, card: Card, chosenColor: number | null) => {
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