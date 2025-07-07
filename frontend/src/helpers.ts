import { type Card } from '@/types.ts';
import { cardColor, cardValue } from '@/constants.ts';
import { type RouteLocationNormalizedLoadedGeneric, type Router } from 'vue-router';

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

export async function removeQueryParameter(route: RouteLocationNormalizedLoadedGeneric, router: Router, paramName: string) {
  const query = Object.assign({}, route.query);
  delete query[paramName];
  await router.replace({ query });
}
