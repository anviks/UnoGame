<template>
  <div class="flex relative">
    <CardBack
      v-for="i in cardCount"
      :key="i"
      :size="45"
      class="not-first:absolute"
      :style="{ left: (i - 1) * (50 / cardCount) + 'px' }"
    />
  </div>
</template>

<script setup lang="ts">
import { watch } from 'vue';
import CardBack from './CardBack.vue';

const props = defineProps<{ cardCount: number }>();

watch(() => props.cardCount, (val) => {
  console.log(val);
});
</script>

<style scoped></style>
