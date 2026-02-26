import { animateCardMove, type AnimateCardOptions } from '@/helpers/ui';
import type { Card } from '@/types';
import { nextTick, ref, type Ref } from 'vue';

export function useCardTransitions() {
  const transitioningCards = ref<Ref<Card>[]>([]);
  const flyCardElMap = new Map<number, HTMLElement>();

  function setFlyCardRef(cardId: number, el: any) {
    if (el) {
      flyCardElMap.set(cardId, el.$el ?? el);
    } else {
      flyCardElMap.delete(cardId);
    }
  }

  async function animate(
    card: Card,
    animationOptions: Omit<AnimateCardOptions, 'animatedEl'>
  ) {
    const cardRef = ref<Card>(card);
    transitioningCards.value.push(cardRef);

    await nextTick();

    const animatedEl = flyCardElMap.get(card.id!)!;

    await animateCardMove({
      ...animationOptions,
      animatedEl,
    });

    const index = transitioningCards.value.findIndex(
      (cr) => cr.value.id === card.id
    );
    transitioningCards.value.splice(index, 1);
    flyCardElMap.delete(card.id!);
  }

  return { transitioningCards, setFlyCardRef, animate };
}
