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
    <!--    <transition-group-->
    <!--      name="hand-change"-->
    <!--      tag="div"-->
    <!--      style="position: absolute; bottom: 100px; left: 50%; transform: translateX(-50%)"-->
    <!--      class="uno-card-hand d-flex ga-2"-->
    <!--    >-->
    <!--      <card-choice-->
    <!--        v-for="(card, index) in thisPlayer?.cards"-->
    <!--        :color="card.color"-->
    <!--        :value="card.value"-->
    <!--        :key="card.id"-->
    <!--        :ref="el => cardRefs[index] = el"-->
    <!--        @card-chosen="(chosenColor) => playCard(index, card, chosenColor)"-->
    <!--      ></card-choice>-->
    <!--    </transition-group>-->

    <div
      class="uno-hand-wrapper"
      ref="handScrollRef"
    >
      <div class="uno-card-hand">
        <card-choice
          v-for="(card, index) in thisPlayer?.cards"
          :key="card.id"
          :color="card.color"
          :value="card.value"
          :ref="(el) => (cardRefs[index] = el)"
          @card-chosen="(chosenColor) => playCard(index, card, chosenColor)"
        />
      </div>
    </div>

    <transition
      v-for="(transitioningCard, i) in cthTransitioningCards"
      :key="transitioningCard.value.id"
    >
      <uno-card
        class="flying-card"
        :color="transitioningCard.value.color"
        :value="transitioningCard.value.value"
        :style="cthFlyCardStyles[i].value"
        shadowed
      />
    </transition>

    <transition
      v-for="(transitioningCard, i) in ctdTransitioningCards"
      :key="transitioningCard.value.id"
    >
      <uno-card
        class="flying-card"
        :color="transitioningCard.value.color"
        :value="transitioningCard.value.value"
        :style="ctdFlyCardStyles[i].value"
        shadowed
      />
    </transition>
  </v-container>
</template>

<script setup lang="ts">
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
import { useApiRequest } from '@/composables/useApiRequest';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore.ts';
import { CardChoice, DiscardPile, DrawPile, UnoCard } from '@/components';
import type { Card, GameDto, GameState, Player } from '@/types.ts';
import { useToast } from 'vue-toastification';
import {
  errorMessages,
  type GameErrorCode,
  GameErrorCodes,
} from '@/constants.ts';
import { animateCardMove } from '@/helpers/ui';
import _ from 'lodash-es';

const props = defineProps({
  gameId: {
    type: Number,
    required: true,
  },
});

const authStore = useAuthStore();
const toast = useToast();
const apiRequest = useApiRequest();
const cardRefs = ref<ComponentPublicInstance<any | typeof CardChoice>[]>([]);

const connected = ref(false);
const connection = ref<HubConnection>();

const sendMessage = (user: string, message: string) => {
  connection
    .value!.send('SendMessage', user, message)
    .catch((err) => console.error(err));
};

interface PlayCardResponse {
  success: boolean;
  error?: string;
}

const playCard = async (index: number, card: Card, chosenColor?: number) => {
  const response: PlayCardResponse = await connection.value!.invoke(
    'PlayCard',
    card,
    chosenColor
  );

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
const discardPileRef = useTemplateRef('discardPileRef');

const cthTransitioningCards = ref<Ref<Card>[]>([]);
const ctdTransitioningCards = ref<Ref<Card>[]>([]);

const cthFlyCardStyles = ref<Ref<Record<string, string>>[]>([]);
const ctdFlyCardStyles = ref<Ref<Record<string, string>>[]>([]);

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

const animateDrawnCards = async (cards: Card[], sequential = false) => {
  for (const card of cards) {
    state.value!.drawPileSize--;

    thisPlayer.value!.cards!.push(card);
    await nextTick();

    let cardRef = ref<Card>(card);
    cthTransitioningCards.value.push(cardRef);

    let styleRef = ref();
    cthFlyCardStyles.value.push(styleRef);

    const animation = animateCardMove({
      fromRect: drawPileRef.value?.topCardRef.$el.getBoundingClientRect(),
      toElement: lastCardRef.value.$el,
      styleRef: styleRef,
    }).then(() => {
      const index = cthTransitioningCards.value.findIndex(
        (cr) => cr.value.id === card.id
      );
      cthTransitioningCards.value.splice(index, 1);
      cthFlyCardStyles.value.splice(index, 1);
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
        const fromRect =
          cardRefs.value[playedCardIndex].$el.getBoundingClientRect();
        thisPlayer.value!.cards!.splice(playedCardIndex, 1);
        await nextTick();

        let cardRef = ref<Card>(card);
        ctdTransitioningCards.value.push(cardRef);

        let styleRef = ref();
        ctdFlyCardStyles.value.push(styleRef);

        await animateCardMove({
          fromRect: fromRect,
          toElement: discardPileRef.value!.topCardRef!.$el,
          styleRef: styleRef,
        });

        const index = ctdTransitioningCards.value.findIndex(
          (cr) => cr.value.id === card.id
        );
        ctdTransitioningCards.value.splice(index, 1);
        ctdFlyCardStyles.value.splice(index, 1);
      }
    }
  );

  connection.value.on(
    'CardDrawnOpponent',
    async (player: Player, cardCount: number) => {
      state.value!.drawPileSize -= cardCount;
      // TODO: Add possibility to check if draw pile has reset due to the drawing
    }
  );

  connection.value.on('CardDrawnSelf', async (cards: Card[]) => {
    await animateDrawnCards(cards, true);
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

let isDragging = false;
let startX = 0;
let scrollLeft = 0;

watch(handScrollRef, (value) => {
  if (value == null) return;

  const el = handScrollRef.value;

  el?.addEventListener('mousedown', (e) => {
    isDragging = true;
    startX = e.pageX - el.offsetLeft;
    scrollLeft = el.scrollLeft;
  });

  el?.addEventListener('mouseleave', () => {
    isDragging = false;
  });

  el?.addEventListener('mouseup', () => {
    isDragging = false;
  });

  el?.addEventListener('mousemove', (e) => {
    if (!isDragging) return;
    e.preventDefault();
    const x = e.pageX - el.offsetLeft;
    const walk = (x - startX) * 1.5; // Scroll speed
    el.scrollLeft = scrollLeft - walk;
  });

  // Touch support
  let touchStartX = 0;
  let touchScrollLeft = 0;

  el?.addEventListener('touchstart', (e) => {
    touchStartX = e.touches[0].pageX;
    touchScrollLeft = el.scrollLeft;
  });

  el?.addEventListener('touchmove', (e) => {
    const x = e.touches[0].pageX;
    const walk = (x - touchStartX) * 1.5;
    el.scrollLeft = touchScrollLeft - walk;
  });
});

onMounted(async () => {
  await connectToGame();
  const { success, data } = await apiRequest<GameDto>({
    url: `/games/${props.gameId}`,
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
//.uno-card-hand {
//  display: flex;
//  flex-direction: row;
//  align-items: center;
//  flex-wrap: wrap;
//}

//.uno-card-hand {
//  display: flex;
//  justify-content: center;
//  padding: 1rem;
//  overflow: visible;
//  position: relative;
//}
//
//.uno-card-hand > * {
//  transition: transform 0.2s ease, z-index 0.2s ease;
//  margin-left: -40px; // adjust for overlap density
//  z-index: 1;
//
//  &:first-child {
//    margin-left: 0;
//  }
//
//  &:hover {
//    transform: translateY(-20px) scale(1.1);
//    z-index: 100;
//  }
//}

.uno-hand-wrapper {
  position: absolute;
  bottom: 100px;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 100vw;
  overflow-x: auto;
  overflow-y: visible;
  padding: 0 60px;
  -webkit-overflow-scrolling: touch;
  cursor: grab;
}

.uno-hand-wrapper:active {
  cursor: grabbing;
}

.uno-card-hand {
  display: flex;
  align-items: center;
  padding: 1rem 0;
  height: 160px;
  overflow: visible;
}

.uno-card-hand > * {
  margin-left: -40px;
  z-index: 1;
  transition: transform 0.2s ease, z-index 0.2s ease;

  &:hover {
    transform: translateY(-20px) scale(1.1);
    z-index: 10;
  }

  &:first-child {
    margin-left: 0;
  }
}

.card-count {
  font-family: 'Bungee', sans-serif;
  font-size: 36px;
  color: white;

  /* @formatter:off */
  text-shadow: -1px -1px 2px #000, 1px -1px 2px #000, -1px 1px 2px #000,
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
