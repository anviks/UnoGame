<template>
  <div class="flex relative">
    <CardBack
      v-for="i in cardCount"
      :key="i"
      :size="45"
      class="not-first:absolute"
      :style="{ left: (i - 1) * (50 / cardCount) + 'px' }"
      :ref="(el: any) => { if (i === cardCount) topCardRef = el; }"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import CardBack from './CardBack.vue';

defineProps<{ cardCount: number }>();

const topCardRef = ref<InstanceType<typeof CardBack>>();
defineExpose({ topCardRef });
</script>
