<template>
  <component
    :is="cardComponent"
    :class="[
      {'uno-card-shadow': shadowed},
      {'greyed': disabled},
      chosenColor != null ? `chosen-${colorAsString(chosenColor)}` : '',
    ]"
    :style="cardStyle"
  />
</template>

<script
  lang="ts"
  setup
>
import { computed } from 'vue';
import { cardColor, cardValue } from '@/constants.ts';

interface Props {
  color: number;
  value: number;
  size?: number;
  disabled?: boolean;
  shadowed?: boolean;
  rotationJitter?: number;
  horizontalJitter?: number;
  verticalJitter?: number;
  chosenColor?: number | null;
}

const props = withDefaults(defineProps<Props>(), {
  size: 100,
  disabled: false,
  shadowed: false,
  rotationJitter: 0,
  horizontalJitter: 0,
  verticalJitter: 0,
  chosenColor: null,
});

const randomJitter = (range: number) => {
  return (Math.random() - 0.5) * range;
};

const cardStyle = computed(() => ({
  width: `${props.size}px`,
  height: `${props.size * 1.5}px`,
  borderRadius: `${props.size * 0.16}px`,
  rotate: `${randomJitter(props.rotationJitter)}deg`,
  left: `${randomJitter(props.horizontalJitter)}px`,
  top: `${randomJitter(props.verticalJitter)}px`,
}));

const colorAsString = (color: number | null) => {
  if (color === null) return '';
  return Object.keys(cardColor)[color]?.toLowerCase() ?? '';
};

const color = computed(() => colorAsString(props.color));

const value = computed(() => {
  if (props.value < 10) {
    return props.value;
  } else {
    return Object.keys(cardValue)[props.value].toLowerCase().replaceAll('_', '-');
  }
});

const cards = import.meta.glob(`./cards/**/*.vue`, { eager: true, import: 'default' });

const cardComponent = computed(() => {
  let cardPath = `./cards/${color.value}/${value.value}.vue`;
  const matchedSvg = cards[cardPath];
  if (!matchedSvg) {
    console.warn(`Missing card at ${cardPath}`);
  }
  return matchedSvg;
});

</script>

<style scoped>
.uno-card-shadow {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.greyed {
  filter: grayscale(100%);
}
</style>
