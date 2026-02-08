import { animateCardMove, type HTMLSnapshot } from '@/helpers/ui';
import type { Card } from '@/types';
import { nextTick, ref, type Ref } from 'vue';

export function useCardTransitions() {
  const transitioningCards = ref<Ref<Card>[]>([]);
  const flyCardStyles = ref<Ref<Record<string, string>>[]>([]);
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
    from: HTMLSnapshot,
    to: HTMLElement,
    duration?: number
  ) {
    const cardRef = ref<Card>(card);
    transitioningCards.value.push(cardRef);

    const styleRef = ref<Record<string, string>>({});
    flyCardStyles.value.push(styleRef);

    await nextTick();

    const animatedEl = flyCardElMap.get(card.id!)!;

    await animateCardMove({
      fromEl: from,
      toEl: to,
      styleRef,
      animatedEl,
      duration,
    });

    const index = transitioningCards.value.findIndex(
      (cr) => cr.value.id === card.id
    );
    transitioningCards.value.splice(index, 1);
    flyCardStyles.value.splice(index, 1);
    flyCardElMap.delete(card.id!);
  }

  return { transitioningCards, flyCardStyles, setFlyCardRef, animate };
}
