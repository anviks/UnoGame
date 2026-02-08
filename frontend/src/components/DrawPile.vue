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
      :ref="(el) => { if (i === amountOfCards) topCardRef = el }"
    />
    <card-back
      :style="{
        'position': 'absolute',
        'left': 0,
        'visibility': 'hidden'
      }"
      ref="fakeCard"
    />
  </div>
</template>

<script
  setup
  lang="ts"
>
import { CardBack } from '@/components';
import { computed, ref } from 'vue';
import _ from 'lodash';

interface Props {
  amount: number;
}

const props = defineProps<Props>();

const topCardRef = ref();
const fakeCard = ref();
defineExpose({ topCardRef, fakeCard });

const amountOfCards = computed(() => _.clamp(props.amount, 0, 15));
</script>

<style scoped>

</style>