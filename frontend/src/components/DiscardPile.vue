<template>
  <div style="position: relative; height: 150px">
    <uno-card
      v-for="(card, i) in reversedCards"
      :color="card.color"
      :value="card.value"
      :rotation-jitter="80"
      :horizontal-jitter="30"
      :vertical-jitter="30"
      :chosen-color="i === reversedCards.length - 1 ? currentColor : null"
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

/*
 The random offsets and rotations are cached by the card's index.
 Since new cards get inserted to the index 0, if the array isn't reversed here,
 the new card inserted to index 0 takes the previous index-0 card's position and so on throughout the array.
 The new offset and rotation would be generated for the bottom card.
*/
const reversedCards = computed(() => {
  return [...props.cards].reverse();
});
</script>

<style scoped>

</style>