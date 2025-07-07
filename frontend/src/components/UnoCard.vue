<template>
  <component
    :is="cardComponent"
    :class="{'uno-card': true, 'greyed': disabled}"
    :style="cardStyle"
  />
</template>

<script
  lang="ts"
  setup
>
import { computed } from 'vue';
import { cardColor, cardValue } from '@/constants.ts';

const props = defineProps({
  color: {
    type: Number,
    required: true,
  },
  value: {
    type: Number,
    required: true,
  },
  size: {
    type: Number,
    default: 100,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  rotationJitter: {
    type: Number,
    default: 0,
  },
  horizontalJitter: {
    type: Number,
    default: 0,
  },
  verticalJitter: {
    type: Number,
    default: 0,
  },
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

const color = computed(() => {
  return Object.keys(cardColor)[props.color].toLowerCase();
});

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
.uno-card {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.greyed {
  filter: grayscale(100%);
}
</style>
