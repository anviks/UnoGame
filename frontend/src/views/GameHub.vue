<template>
  <v-container v-if="state">
    <discard-pile
      :cards="state.discardPile"
      :current-color="state.currentColor"
    />
    <div class="d-flex ga-3">
      <draw-pile
        ref="drawPileRef"
        :amount="state.drawPileSize"
        @click="drawCard"
      />
      <span class="card-count">{{ state.drawPileSize }}</span>
    </div>
    <v-btn @click="endTurn">End turn</v-btn>
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

    <transition name="fly-card">
      <uno-card
        v-if="transitioningCard"
        class="flying-card"
        :color="transitioningCard.color"
        :value="transitioningCard.value"
        :style="flyCardStyle"
        shadowed
      />
    </transition>
  </v-container>
</template>

<script
  setup
  lang="ts"
>
import { type ComponentPublicInstance, computed, nextTick, onMounted, onUnmounted, ref, useTemplateRef } from 'vue';
import { GameApi } from '@/api';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore.ts';
import { CardChoice, DiscardPile, DrawPile, UnoCard } from '@/components';
import type { Card, GameState, Player } from '@/types.ts';
import { useToast } from 'vue-toastification';
import { errorMessages, type GameErrorCode, GameErrorCodes } from '@/constants.ts';

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

const playCard = async (index: number, card: Card, chosenColor?: number) => {
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

const drawPileRef = useTemplateRef('drawPileRef');

const transitioningCard = ref<Card | null>(null);
const flyCardStyle = ref<Record<string, string>>({});

const animateCardToHand = (card: Card) => {
  transitioningCard.value = card;

  nextTick(() => {
    const from = drawPileRef.value?.topCardRef.$el.getBoundingClientRect();
    const to = lastCardRef.value?.$el.getBoundingClientRect();

    if (from && to) {
      const dx = to.left - from.left;
      const dy = to.top - from.top;

      flyCardStyle.value = {
        position: 'absolute',
        left: `${from.left}px`,
        top: `${from.top}px`,
        transform: `translate(${dx}px, ${dy}px)`,
        transition: 'transform 0.6s ease',
        zIndex: '999',
      };

      setTimeout(() => {
        flyCardStyle.value.visibility = 'hidden';
        nextTick(() => {
          transitioningCard.value = null;
          flyCardStyle.value = {};
          lastCardRef.value.$el.style.visibility = '';
        });
      }, 600);
    }
  });
};

const drawCard = async () => {
  const response = await connection.value!.invoke('DrawCard');

  if (!response.success) {
    const errorCode = response.error as GameErrorCode;
    toast.error(errorMessages[errorCode] ?? errorCode);
  }
};

const endTurn = async () => {
  const response = await connection.value!.invoke('EndTurn');

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

  connection.value.on('CardPlayed', async (player: Player, card: Card, chosenColor: number | null) => {
    state.value?.discardPile.splice(0, 0, card);
    if (player.name === thisPlayer.value?.name) {
      const playedCardIndex = thisPlayer.value.cards!.findIndex(c => c.color === card.color && c.value === card.value);
      thisPlayer.value.cards!.splice(playedCardIndex, 1);
    }
  });

  connection.value.on('CardDrawn', async (player: Player) => {
    state.value!.drawPileSize--;
    // TODO: Add possibility to check if draw pile has reset due to the drawing
  });

  connection.value.on('CardDrawnSelf', async (card: Card) => {
    thisPlayer.value!.cards!.push(card);
    await nextTick(() => {
      lastCardRef.value.$el.style.visibility = 'hidden';
    });
    animateCardToHand(card);
  });

  connection.value
    .start()
    .then(() => {
      connected.value = true;
      sendMessage('System', `${authStore.username} joined the game.`);
    })
    .catch((err) => console.error('Connection failed: ', err));
};

const state = ref<GameState>();

const thisPlayer = computed(() => state?.value?.players?.find((player: Player) => player.userId === authStore.userId));
const lastCardRef = computed(() => cardRefs.value[thisPlayer.value!.cards!.length - 1]);

onMounted(async () => {
  await connectToGame();
  state.value = await GameApi.getGame(props.gameId);
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

.card-count {
  font-family: 'Bungee', sans-serif;
  font-size: 36px;
  color: white;

  /* @formatter:off */
  text-shadow:
          -1px -1px 2px #000,
          1px -1px 2px #000,
          -1px 1px 2px #000,
          1px 1px 2px #000;
  /* @formatter:on */

  align-self: center;
}
</style>