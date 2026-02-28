<template>
  <transition-group
    tag="div"
    :style="{
      position: 'relative',
      'padding-right': amountOfCards * 5 + 'px',
      cursor: 'pointer',
    }"
    class="flex"
  >
    <card-back
      v-for="i in amountOfCards"
      :key="i"
      :style="{
        position: 'absolute',
        left: (i - 1) * 5 + 'px',
      }"
      :ref="
        (el) => {
          if (i === amountOfCards) topCardRef = el;
        }
      "
    />
    <card-back
      :key="-1"
      :style="{
        left: 0,
        visibility: 'hidden',
      }"
      ref="fakeCard"
    />
  </transition-group>
</template>

<script setup lang="ts">
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

<style scoped></style>
