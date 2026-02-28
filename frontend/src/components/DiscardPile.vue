<template>
  <div class="relative">
    <uno-card
      v-for="(card, i) in cards"
      :key="card.id"
      :color="card.color"
      :value="card.value"
      :rotation-jitter="80"
      :horizontal-jitter="30"
      :vertical-jitter="30"
      :chosen-color="i === 0 ? currentColor : null"
      class="not-first:absolute"
      shadowed
      :style="{ zIndex: cards.length - i }"
      :ref="(el: any) => (cardRefs[i] = el)"
    ></uno-card>
  </div>
</template>

<script setup lang="ts">
import { UnoCard } from '@/components';
import type { Card } from '@/types.ts';
import { ref } from 'vue';

interface Props {
  cards: Card[];
  currentColor?: number | null;
}

const props = withDefaults(defineProps<Props>(), { currentColor: null });

const cardRefs = ref<InstanceType<typeof UnoCard>[]>([]);
defineExpose({ cardRefs });
</script>

<style scoped></style>
