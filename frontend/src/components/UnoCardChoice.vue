<template>
  <uno-card
    v-if="color !== cardColor.WILD"
    :color="color"
    :value="value"
    class="card-choice"
    @click="$emit('card-chosen')"
  />
  <div
    v-else
    class="wild-card-div"
    @mouseenter="startedHovering"
    @mouseleave="stoppedHovering"
  >
    <uno-card
      :color="cardColor.WILD"
      :value="value"
      class="card-choice wild-image"
      ref="wildCard"
    />
    <div
      class="uno-color-choice card-choice"
      ref="colorChoices"
    >
      <span
        class="quarter top-left uno-red"
        @click="$emit('card-chosen', cardColor.RED)"
      />
      <span
        class="quarter top-right uno-blue"
        @click="$emit('card-chosen', cardColor.BLUE)"
      />
      <span
        class="quarter bottom-left uno-yellow"
        @click="$emit('card-chosen', cardColor.YELLOW)"
      />
      <span
        class="quarter bottom-right uno-green"
        @click="$emit('card-chosen', cardColor.GREEN)"
      />
    </div>
  </div>
</template>

<script
  setup
  lang="ts"
>
import { cardColor } from '@/constants.ts';
import UnoCard from '@/components/UnoCard.vue';
import { useTemplateRef } from 'vue';

const props = defineProps({
  color: {
    type: Number,
    required: true,
  },
  value: {
    type: Number,
    required: true,
  },
  size: {
    type: Number,
    default: 100,
  },
});

const emits = defineEmits<{
  (e: 'card-chosen', color?: number): void
}>();

const wildCard = useTemplateRef('wildCard');
const colorChoices = useTemplateRef('colorChoices');
let timeoutId: number;

function startedHovering() {
  clearTimeout(timeoutId);
  if (wildCard.value == null || colorChoices.value == null) return;
  wildCard.value.$el.style.display = 'none';
  Array.from(colorChoices.value.children).forEach(child => (child as HTMLSpanElement).style.display = '');
}

function stoppedHovering() {
  timeoutId = setTimeout(() => {
    if (wildCard.value == null || colorChoices.value == null) return;
    wildCard.value.$el.style.display = 'block';
    Array.from(colorChoices.value.children).forEach(child => (child as HTMLSpanElement).style.display = 'none');
  }, 300);
}
</script>

<style
  scoped
  lang="scss"
>
.uno-card, .card-choice {
  height: 150px;
  width: 100px;
  border-radius: 17px;
}

.card-choice {
  transition: transform 0.3s, box-shadow 0.3s;

  &:hover {
    transform: scale(1.1) translateZ(10px);
    box-shadow: 2px 2px 4px 2px rgba(0, 0, 0, 0.3);
    cursor: pointer;
  }
}

.wild-card-div {
  display: inline-block;
  position: relative;
  margin: 0 0 5px;

  & > .wild-image {
    position: absolute;
    z-index: 2;
  }

  &:hover > .wild-image {
    z-index: 1;
    display: none;
  }

  & > .uno-color-choice {
    position: relative;
    z-index: 1;
    transition: z-index 0s 0.3s, transform 0.3s;
  }

  &:hover > .uno-color-choice {
    z-index: 2;
    transition: transform 0.3s;
  }
}

.uno-color-choice {
  grid-template-columns: repeat(2, 1fr);
  grid-template-rows: repeat(2, 1fr);
  display: grid;
}

.quarter {
  width: 100%;
  height: 100%;
  transform: scale(1);
  z-index: 1;
  transition: transform 0.3s ease, box-shadow 0.3s ease, z-index 0.3s;
  will-change: transform;

  &:hover {
    transform: scale(1.1);
    box-shadow: 2px 2px 4px 2px rgba(0, 0, 0, 0.3);
    z-index: 100;
  }
}

.top-left {
  border-top-left-radius: 17px;
}

.top-right {
  border-top-right-radius: 17px;
}

.bottom-left {
  border-bottom-left-radius: 17px;
}

.bottom-right {
  border-bottom-right-radius: 17px;
}

.uno-red {
  background-color: var(--uno-red);
}

.uno-yellow {
  background-color: var(--uno-yellow);
}

.uno-green {
  background-color: var(--uno-green);
}

.uno-blue {
  background-color: var(--uno-blue);
}
</style>