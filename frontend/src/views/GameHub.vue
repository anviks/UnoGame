<template>
  <v-container v-if="state">
    <div
      class="flex gap-3 select-none absolute top-2/5 left-1/3 -translate-1/2"
    >
      <draw-pile
        ref="drawPileRef"
        :amount="state.drawPileSize"
        @click="drawCard"
      />
      <span class="card-count">{{ state.drawPileSize }}</span>
    </div>

    <div class="absolute top-2/5 left-2/3 -translate-1/2">
      <discard-pile
        ref="discardPileRef"
        :cards="state.discardPile"
        :current-color="state.currentColor"
      />
    </div>

    <v-btn @click="endTurn">End turn</v-btn>
    <div
      class="absolute bottom-25 left-1/2 -translate-x-1/2 w-full overflow-x-hidden overflow-y-visible px-15 py-0"
      ref="handScrollRef"
    >
      <transition-group
        name="hand-change"
        tag="div"
        class="inline-flex items-center pt-10 pb-4 min-h-40 overflow-visible!"
      >
        <card-choice
          v-for="(card, index) in thisPlayer?.cards"
          :key="card.id"
          :color="card.color"
          :value="card.value"
          :ref="(el) => (cardRefs[index] = el as any)"
          class="-ml-5! first:ml-0!"
          @card-chosen="(chosenColor) => playCard(index, card, chosenColor)"
        />
      </transition-group>
    </div>

    <uno-card
      v-for="(transitioningCard, i) in transitioningCards"
      :key="transitioningCard.value.id"
      :ref="(el: any) => setFlyCardRef(transitioningCard.value.id!, el)"
      :color="transitioningCard.value.color"
      :value="transitioningCard.value.value"
      :style="flyCardStyles[i]!.value"
      shadowed
    />
  </v-container>
</template>

<script setup lang="ts">
import { CardChoice, DiscardPile, DrawPile, UnoCard } from '@/components';
import { useApiRequest } from '@/composables/useApiRequest';
import { useCardTransitions } from '@/composables/useCardTransitions';
import { errorMessages, GameErrorCodes } from '@/constants.ts';
import { getElementSnapshot } from '@/helpers/ui';
import { sleep } from '@/helpers/general';
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
  ref,
  useTemplateRef,
  watch,
} from 'vue';
import { useToast } from 'vue-toastification';

interface Props {
  gameId: number;
}

const props = defineProps<Props>();

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

const { transitioningCards, flyCardStyles, setFlyCardRef, animate } = useCardTransitions();

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

    const animation = animate(
      drawnCard.card,
      getElementSnapshot(drawPileRef.value?.topCardRef.$el),
      cardRefs.value[drawnCard.index]!.$el
    );

    if (sequential) {
      await animation;
    } else {
      await sleep(100);
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

        await animate(
          card,
          fromElement,
          discardPileRef.value!.cardRefs![0]!.$el
        );
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

      const discardPileSize = state.value!.discardPile.length;

      for (let i = 1; i < discardPileSize; i++) {
        const card = state.value!.discardPile[1]!;
        const cardEl = discardPileRef.value!.cardRefs[i]!;
        animate(
          card,
          getElementSnapshot(cardEl.$el),
          drawPileRef.value!.fakeCard.$el,
          1000
        ).then(() => state.value!.drawPileSize++);
        state.value!.discardPile.splice(1, 1);
        await sleep(100);
      }

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
