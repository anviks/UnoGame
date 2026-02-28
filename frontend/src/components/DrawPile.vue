<template>
  <transition-group
    tag="div"
    :style="{
      'padding-right': amountOfCards * 5 + 'px',
    }"
    class="flex relative cursor-pointer"
  >
    <card-back
      v-for="i in amountOfCards"
      :key="i"
      :style="{
        left: (i - 1) * 5 + 'px',
      }"
      class="absolute"
      :ref="
        (el) => {
          if (i === amountOfCards) topCardRef = el;
        }
      "
    />
    <card-back
      :key="-1"
      class="left-0 invisible"
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
