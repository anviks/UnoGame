<template>
  <v-container v-if="state">
    <discard-pile
      ref="discardPileRef"
      :cards="state.discardPile"
      :current-color="state.currentColor"
    />
    <div
      class="d-flex ga-3"
      style="user-select: none"
    >
      <draw-pile
        ref="drawPileRef"
        :amount="state.drawPileSize"
        @click="drawCard"
      />
      <span class="card-count">{{ state.drawPileSize }}</span>
    </div>
    <v-btn @click="endTurn">End turn</v-btn>

    <div
      class="uno-hand-wrapper"
      ref="handScrollRef"
    >
      <transition-group
        name="hand-change"
        tag="div"
        class="uno-card-hand"
      >
        <card-choice
          v-for="(card, index) in thisPlayer?.cards"
          :key="card.id"
          :color="card.color"
          :value="card.value"
          :ref="(el) => (cardRefs[index] = el as any)"
          @card-chosen="(chosenColor) => playCard(index, card, chosenColor)"
        />
      </transition-group>
    </div>

    <transition
      v-for="(transitioningCard, i) in transitioningCards"
      :key="transitioningCard.value.id"
    >
      <uno-card
        class="flying-card"
        :color="transitioningCard.value.color"
        :value="transitioningCard.value.value"
        :style="flyCardStyles[i]!.value"
        shadowed
      />
    </transition>
  </v-container>
</template>

<script setup lang="ts">
import { CardChoice, DiscardPile, DrawPile, UnoCard } from '@/components';
import { useApiRequest } from '@/composables/useApiRequest';
import {
  errorMessages,
  type GameErrorCode,
  GameErrorCodes,
} from '@/constants.ts';
import { animateCardMove, getElementSnapshot } from '@/helpers/ui';
import { useAuthStore } from '@/stores/authStore.ts';
import type {
  Card,
  DrawnCard,
  DrawResult,
  GameDto,
  GameState,
  HubResponse,
  Player,
  PublicDrawResult,
} from '@/types.ts';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import _ from 'lodash';
import {
  type ComponentPublicInstance,
  computed,
  nextTick,
  onMounted,
  onUnmounted,
  type Ref,
  ref,
  useTemplateRef,
  watch,
} from 'vue';
import { useToast } from 'vue-toastification';

const props = defineProps({
  gameId: {
    type: Number,
    required: true,
  },
});

const authStore = useAuthStore();
const toast = useToast();
const { request } = useApiRequest<GameDto>(`/games/${props.gameId}`);
const cardRefs = ref<ComponentPublicInstance<typeof CardChoice>[]>([]);

const connected = ref(false);
const connection = ref<HubConnection>();

const sendMessage = (user: string, message: string) => {
  connection
    .value!.send('SendMessage', user, message)
    .catch((err) => console.error(err));
};

const playCard = async (index: number, card: Card, chosenColor?: number) => {
  const response = await connection.value!.invoke<HubResponse>(
    'PlayCard',
    card,
    chosenColor
  );

  if (!response.accepted) {
    const errorCode = response.error;

    if (errorCode === GameErrorCodes.INVALID_CARD) {
      cardRefs.value[index]!.triggerShake();
    } else {
      toast.error(errorMessages[errorCode] ?? errorCode);
    }
  }
};

const drawPileRef = useTemplateRef('drawPileRef');
const discardPileRef = useTemplateRef('discardPileRef');

const transitioningCards = ref<Ref<Card>[]>([]);
const flyCardStyles = ref<Ref<Record<string, string>>[]>([]);

const drawCard = async () => {
  const response = await connection.value!.invoke<HubResponse>('DrawCard');

  if (!response.accepted) {
    const errorCode = response.error;
    toast.error(errorMessages[errorCode] ?? errorCode);
  }
};

const endTurn = async () => {
  const response = await connection.value!.invoke<HubResponse>('EndTurn');

  if (!response.accepted) {
    const errorCode = response.error;
    toast.error(errorMessages[errorCode] ?? errorCode);
  }
};

const animateDrawnCards = async (
  drawnCards: DrawnCard[],
  sequential = false
) => {
  for (const drawnCard of drawnCards) {
    state.value!.drawPileSize--;

    thisPlayer.value!.cards!.splice(drawnCard.index, 0, drawnCard.card);
    await nextTick();

    let cardRef = ref<Card>(drawnCard.card);
    transitioningCards.value.push(cardRef);

    let styleRef = ref();
    flyCardStyles.value.push(styleRef);

    const animation = animateCardMove({
      fromElement: getElementSnapshot(drawPileRef.value?.topCardRef.$el),
      toElement: cardRefs.value[drawnCard.index]!.$el,
      styleRef: styleRef,
    }).then(() => {
      const index = transitioningCards.value.findIndex(
        (cr) => cr.value.id === drawnCard.card.id
      );
      transitioningCards.value.splice(index, 1);
      flyCardStyles.value.splice(index, 1);
    });

    if (sequential) {
      await animation;
    } else {
      await new Promise((resolve) => setTimeout(resolve, 100));
    }
  }
};

const connectToGame = async () => {
  connection.value = new HubConnectionBuilder()
    .withUrl(
      `${import.meta.env.VITE_BACKEND_URL}/gamehub?gameId=${props.gameId}`
    )
    .build();

  connection.value.on('Error', (message: string) => {
    toast.error(message);
  });

  connection.value.on(
    'CardPlayed',
    async (player: Player, card: Card, chosenColor: number | null) => {
      state.value!.discardPile.unshift(card);
      state.value!.currentColor = chosenColor ?? card.color;
      state.value!.currentValue = chosenColor === null ? card.value : null;

      if (player.name === thisPlayer.value?.name) {
        const playedCardIndex = thisPlayer.value.cards!.findIndex((c) =>
          _.isEqual(c, card)
        );
        const fromElement = getElementSnapshot(
          cardRefs.value[playedCardIndex]!.$el
        );
        thisPlayer.value!.cards!.splice(playedCardIndex, 1);
        await nextTick();

        let cardRef = ref<Card>(card);
        transitioningCards.value.push(cardRef);

        let styleRef = ref();
        flyCardStyles.value.push(styleRef);

        await animateCardMove({
          fromElement,
          toElement: discardPileRef.value!.topCardRef!.$el,
          styleRef: styleRef,
        });

        const index = transitioningCards.value.findIndex(
          (cr) => cr.value.id === card.id
        );
        transitioningCards.value.splice(index, 1);
        flyCardStyles.value.splice(index, 1);
      }
    }
  );

  connection.value.on(
    'CardDrawnOpponent',
    async (player: Player, drawResult: PublicDrawResult) => {
      state.value!.drawPileSize -= drawResult.drawn;

      if (drawResult.reshuffleIndex !== null) {
        state.value!.drawPileSize += state.value!.discardPile.length - 1;
        state.value!.discardPile.splice(1);
      }
    }
  );

  connection.value.on('CardDrawnSelf', async (drawResult: DrawResult) => {
    if (drawResult.reshuffleIndex == null) {
      await animateDrawnCards(drawResult.drawnCards, false);
    } else {
      const firstBatch = drawResult.drawnCards.slice(
        0,
        drawResult.reshuffleIndex
      );
      const secondBatch = drawResult.drawnCards.slice(
        drawResult.reshuffleIndex
      );
      await animateDrawnCards(firstBatch, false);
      state.value!.drawPileSize += state.value!.discardPile.length - 1;
      state.value!.discardPile.splice(1);
      await animateDrawnCards(secondBatch, false);
    }
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

const thisPlayer = computed(() =>
  state?.value?.players?.find(
    (player: Player) => player.userId === authStore.userId
  )
);
const lastCardRef = computed(
  () => cardRefs.value[thisPlayer.value!.cards!.length - 1]
);

const handScrollRef = ref<HTMLElement | null>(null);

let autoScrollInterval: number | null = null;
const EDGE_ZONE_WIDTH = 120; // Width of the edge zone in pixels
const SCROLL_SPEED = 10; // Pixels per frame

watch(handScrollRef, (value) => {
  if (value == null) return;

  const el = handScrollRef.value;

  const stopAutoScroll = () => {
    if (autoScrollInterval !== null) {
      clearInterval(autoScrollInterval);
      autoScrollInterval = null;
    }
    if (el) {
      el.classList.remove('cursor-moving-left');
      el.classList.remove('cursor-moving-right');
    }
  };

  const startAutoScroll = (direction: 'left' | 'right') => {
    if (!el) return;
    stopAutoScroll();
    el.classList.add(`cursor-moving-${direction}`);

    autoScrollInterval = window.setInterval(() => {
      if (!el) return;
      if (direction === 'left') {
        el.scrollLeft -= SCROLL_SPEED;
      } else {
        el.scrollLeft += SCROLL_SPEED;
      }
    }, 16); // ~60fps
  };

  el?.addEventListener('mousemove', (e) => {
    // Check if cursor is in edge zones
    const rect = el.getBoundingClientRect();
    const mouseX = e.clientX - rect.left;
    const isInLeftZone = mouseX < EDGE_ZONE_WIDTH;
    const isInRightZone = mouseX > rect.width - EDGE_ZONE_WIDTH;
    const canScrollLeft = el.scrollLeft > 0;
    const canScrollRight = el.scrollLeft < el.scrollWidth - el.clientWidth;

    if (isInLeftZone && canScrollLeft) {
      startAutoScroll('left');
    } else if (isInRightZone && canScrollRight) {
      startAutoScroll('right');
    } else {
      stopAutoScroll();
    }
  });
});

onMounted(async () => {
  await connectToGame();
  const { success, data } = await request({
    method: 'GET',
  });
  if (success) {
    state.value = data.state;
  }
});

onUnmounted(() => {
  sendMessage('System', `${authStore.username} left the game.`);
});
</script>

<style scoped lang="scss">
.uno-hand-wrapper {
  position: absolute;
  bottom: 100px;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  overflow-x: hidden;
  overflow-y: visible;
  padding: 0 60px;
  // -webkit-overflow-scrolling: touch;
}

.uno-card-hand {
  display: inline-flex;
  align-items: center;
  padding: 2.5rem 0 1rem 0; /* Extra top padding for hover translateY(-20px) */
  min-height: 160px;
  overflow: visible !important;
}

.uno-card-hand > * {
  margin-left: -20px;

  &:first-child {
    margin-left: 0;
  }
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

.hand-change-move {
  transition: all 0.5s ease;
}

.hand-change-leave-active {
  display: none;
}
</style>
