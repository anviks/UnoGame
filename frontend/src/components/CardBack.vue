<template>
  <img
    :src="cardBack"
    alt="UNO Card Back"
    :class="{'uno-card-shadow': true, 'greyed': disabled}"
    :style="{
      width: `${size}px`,
      height: `${size * 1.5}px`,
      borderRadius: `${size * 0.16}px`
    }"
  />
</template>

<script
  setup
  lang="ts"
>
import cardBack from '@/assets/images/uno-card-back.png';

const props = defineProps({
  size: {
    type: Number,
    default: 100,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
});
</script>

<style scoped>
.uno-card-shadow {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.2);
}

.greyed {
  filter: grayscale(100%);
}
</style>
