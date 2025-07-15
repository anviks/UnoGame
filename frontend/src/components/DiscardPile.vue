<template>
  <div style="position: relative; height: 150px">
    <uno-card
      v-for="(card, i) in cards"
      :key="card.id"
      :color="card.color"
      :value="card.value"
      :rotation-jitter="80"
      :horizontal-jitter="30"
      :vertical-jitter="30"
      :chosen-color="i === 0 ? currentColor : null"
      class="position-absolute"
      shadowed
      :style="{zIndex: cards.length - i}"
      :ref="(el) => { if (i === 0) topCardRef = el }"
    ></uno-card>
  </div>
</template>

<script
  setup
  lang="ts"
>
import { type PropType, ref } from 'vue';
import type { Card } from '@/types.ts';
import { UnoCard } from '@/components';

const props = defineProps({
  cards: {
    type: Array as PropType<Card[]>,
    required: true,
  },
  currentColor: {
    type: [Number, null],
    default: null,
  },
});

const topCardRef = ref();
defineExpose({ topCardRef });
</script>

<style scoped>

</style>