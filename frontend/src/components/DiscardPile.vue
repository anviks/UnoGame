<template>
  <div style="position: relative; height: 150px">
    <uno-card
      v-for="(card, i) in reversedCards"
      :color="card.color"
      :value="card.value"
      :rotation-jitter="80"
      :horizontal-jitter="30"
      :vertical-jitter="30"
      :chosen-color="currentColor"
      class="position-absolute"
    ></uno-card>
  </div>
</template>

<script
  setup
  lang="ts"
>
import { computed, type PropType } from 'vue';
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

const reversedCards = computed(() => {
  return [...props.cards].reverse();
});
</script>

<style scoped>

</style>