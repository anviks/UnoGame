import { animateCardMove, type HTMLSnapshot } from '@/helpers/ui';
import type { Card } from '@/types';
import { ref, type Ref } from 'vue';

export function useCardTransitions() {
  const transitioningCards = ref<Ref<Card>[]>([]);
  const flyCardStyles = ref<Ref<Record<string, string>>[]>([]);

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

    await animateCardMove({
      fromElement: from,
      toElement: to,
      styleRef,
      duration,
    });

    const index = transitioningCards.value.findIndex(
      (cr) => cr.value.id === card.id
    );
    transitioningCards.value.splice(index, 1);
    flyCardStyles.value.splice(index, 1);
  }

  return { transitioningCards, flyCardStyles, animate };
}
