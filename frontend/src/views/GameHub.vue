<template>
  <v-container>
    <div
      style="position: absolute; bottom: 100px; left: 50%; transform: translateX(-50%)"
      class="uno-card-hand d-flex ga-2"
    >
      <uno-card-choice
        v-for="(card, index) in game?.players?.find((player: Player) => player.userId === authStore.userId)?.cards"
        :color="card.color"
        :value="card.value"
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
import { onMounted, onUnmounted, ref } from 'vue';
import { GameApi } from '@/api/GameApi.ts';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore.ts';
import UnoCardChoice from '@/components/UnoCardChoice.vue';
import type { Card, Player } from '@/types.ts';
import { useToast } from 'vue-toastification';

const props = defineProps({
  gameId: {
    type: Number,
    required: true,
  },
});

const authStore = useAuthStore();
const toast = useToast();
const cardRefs = ref<any>({});

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
    let valueElement = cardRefs.value[index];
    triggerShake(valueElement.$el);
  }
};

function triggerShake(refEl: HTMLElement) {
  refEl.classList.add('shake');
  setTimeout(() => refEl.classList.remove('shake'), 500);
}

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

  connection.value
    .start()
    .then(() => {
      connected.value = true;
      sendMessage('System', `${authStore.username} joined the game.`);
    })
    .catch((err) => console.error('Connection failed: ', err));
};

const game = ref();

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

@keyframes shake {
  0%   { transform: translateX(0); }
  15%  { transform: translateX(-6px); }
  30%  { transform: translateX(6px); }
  45%  { transform: translateX(-4px); }
  60%  { transform: translateX(4px); }
  75%  { transform: translateX(-2px); }
  90%  { transform: translateX(2px); }
  100% { transform: translateX(0); }
}

.shake {
  animation: shake 0.5s ease;
}

</style>