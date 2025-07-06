<template>
  <div
    :style="{
      'position': 'relative',
      'width': 100 + amountOfCards * 5 + 'px',
      'height': '150px',
      'cursor': 'pointer'
    }"
  >
    <card-back
      v-for="i in amountOfCards"
      :style="{
        'position': 'absolute',
        'left': i * 5 + 'px',
      }"
    />
  </div>
</template>

<script
  setup
  lang="ts"
>
import CardBack from '@/components/CardBack.vue';
import { computed } from 'vue';
import _ from 'lodash-es';

const props = defineProps({
  amount: {
    type: Number,
    required: true,
  },
});

const amountOfCards = computed(() => _.clamp(props.amount, 0, 15));
</script>

<style scoped>

</style>