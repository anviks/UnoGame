import { type Card } from '@/types.ts';
import { cardColor, cardValue } from '@/constants.ts';
import { type RouteLocationNormalizedLoadedGeneric, type Router } from 'vue-router';
import { nextTick, type Ref } from 'vue';

export function getAllCards(excludeWilds: boolean = false): Card[] {
  const colors = Object.values(cardColor);
  const values = Object.values(cardValue);
  const cards: Card[] = [];

  // Create cards for each color and value
  for (const color of colors) {
    if (color === cardColor.WILD) continue;
    for (const value of values) {
      cards.push({ color, value });
    }
  }

  if (!excludeWilds) {
    cards.push({ color: cardColor.WILD, value: cardValue.WILD });
    cards.push({ color: cardColor.WILD, value: cardValue.WILD_DRAW_FOUR });
  }

  return cards;
}

export function getAllColors(excludeWilds: boolean = false): number[] {
  const colors = Object.values(cardColor);
  return excludeWilds ? colors.filter(color => color !== cardColor.WILD) : colors;
}

export function getAllValues(excludeWilds: boolean = false): number[] {
  const values = Object.values(cardValue);
  return excludeWilds ? values.filter(value => value !== cardValue.WILD && value !== cardValue.WILD_DRAW_FOUR) : values;
}

export function capitalizeFirstLetter(string: string) {
  return String(string).charAt(0).toUpperCase() + String(string).slice(1);
}

export async function removeQueryParameter(
  route: RouteLocationNormalizedLoadedGeneric,
  router: Router,
  paramName: string,
) {
  const query = Object.assign({}, route.query);
  delete query[paramName];
  await router.replace({ query });
}

interface AnimateCardOptions {
  fromRect: DOMRect;
  toElement: HTMLElement;
  styleRef: Ref<Record<string, string>>;
  duration?: number;
}

export const animateCardMove = async (
  {
    fromRect,
    toElement,
    styleRef,
    duration = 600,
  }: AnimateCardOptions,
): Promise<void> => {
  return new Promise(async (resolve, reject) => {
    toElement.style.visibility = 'hidden';
    await nextTick();

    const toRect = toElement.getBoundingClientRect();

    const fromCenterX = fromRect.left + fromRect.width / 2;
    const fromCenterY = fromRect.top + fromRect.height / 2;

    const toCenterX = toRect.left + toRect.width / 2;
    const toCenterY = toRect.top + toRect.height / 2;

    const dx = toCenterX - fromCenterX;
    const dy = toCenterY - fromCenterY;

    const rotation = toElement.style.rotate || '0deg';

    styleRef.value = {
      position: 'absolute',
      left: `${fromRect.left}px`,
      top: `${fromRect.top}px`,
      transform: `translate(${dx}px, ${dy}px) rotate(${rotation})`,
      transition: `transform ${duration}ms ease`,
      zIndex: '999',
    };

    /*
     Setting visibility to hidden and waiting for the next tick before cleaning up seems to be
     one of the few ways to get rid of the transitioning card exactly when the transition ends.
     Without it, the transitioning card sticks around for an additional second or two.
    */
    setTimeout(async () => {
      styleRef.value.visibility = 'hidden';
      await nextTick();
      toElement.style.removeProperty('visibility');
      styleRef.value = {};
      resolve();
    }, duration);
  });
};
