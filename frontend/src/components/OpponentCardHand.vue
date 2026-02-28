<template>
  <div class="flex relative w-11.25 h-[calc(1.5*45px)]">
    <CardBack
      v-for="i in cardCount"
      :key="i"
      :size="45"
      class="absolute"
      :style="{ left: i * (50 / cardCount) + 'px' }"
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
