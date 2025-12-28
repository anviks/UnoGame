<template>
  <div :class="{ 'shake': shouldShake }" style="line-height: 0">
    <uno-card
      v-if="color !== cardColor.WILD"
      :color="color"
      :value="value"
      class="card-choice"
      @click="$emit('card-chosen')"
      shadowed
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
        shadowed
      />
      <div
        class="uno-color-choice card-choice grid grid-cols-2 grid-rows-2"
        ref="colorChoices"
      >
        <span
          class="quarter rounded-tl-[17px] bg-(--uno-red)"
          @click="$emit('card-chosen', cardColor.RED)"
        />
        <span
          class="quarter rounded-tr-[17px] bg-(--uno-blue)"
          @click="$emit('card-chosen', cardColor.BLUE)"
        />
        <span
          class="quarter rounded-bl-[17px] bg-(--uno-yellow)"
          @click="$emit('card-chosen', cardColor.YELLOW)"
        />
        <span
          class="quarter rounded-br-[17px] bg-(--uno-green)"
          @click="$emit('card-chosen', cardColor.GREEN)"
        />
      </div>
    </div>
  </div>
</template>

<script
  setup
  lang="ts"
>
import { cardColor } from '@/constants.ts';
import { UnoCard } from '@/components';
import { onMounted, ref, useTemplateRef } from 'vue';

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
let hoverTimeoutId: number;

const setColorChoiceVisibility = (visibility: string) => {
  if (colorChoices.value == null) return;
  Array.from(colorChoices.value.children)
    .forEach(child => (child as HTMLSpanElement).style.visibility = visibility);
};

const setWildCardVisibility = (visibility: string) => {
  if (wildCard.value == null) return;
  wildCard.value.$el.style.visibility = visibility;
};

const startedHovering = () => {
  clearTimeout(hoverTimeoutId);
  setWildCardVisibility('hidden');
  setColorChoiceVisibility('');
};

const stoppedHovering = () => {
  hoverTimeoutId = setTimeout(() => {
    setWildCardVisibility('');
    setColorChoiceVisibility('hidden');
  }, 300);
};

const shouldShake = ref(false);
let shakeTimeoutId: number;

const triggerShake = () => {
  shouldShake.value = false;
  clearTimeout(shakeTimeoutId);
  setTimeout(() => {
    shouldShake.value = true;
    shakeTimeoutId = setTimeout(() => shouldShake.value = false, 500);
  }, 5);
};

defineExpose({ triggerShake });

onMounted(() => {
  setColorChoiceVisibility('hidden');
});
</script>

<style
  scoped
  lang="scss"
>
.uno-card-shadow, .card-choice {
  height: 150px;
  width: 100px;
  border-radius: 17px;
}

.card-choice {
  --card-scale: 1;
  transition: transform 0.3s, box-shadow 0.3s;

  &:hover {
    --card-scale: 1.1;
    transform: scale(var(--card-scale)) translateZ(10px);
    box-shadow: 2px 2px 4px 2px rgba(0, 0, 0, 0.3);
    cursor: pointer;
  }
}

.wild-card-div {
  display: inline-block;
  position: relative;

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

@keyframes shake {
  0% {
    transform: scale(var(--card-scale, 1)) translateX(0);
  }
  15% {
    transform: scale(var(--card-scale, 1)) translateX(-6px);
  }
  30% {
    transform: scale(var(--card-scale, 1)) translateX(6px);
  }
  45% {
    transform: scale(var(--card-scale, 1)) translateX(-4px);
  }
  60% {
    transform: scale(var(--card-scale, 1)) translateX(4px);
  }
  75% {
    transform: scale(var(--card-scale, 1)) translateX(-2px);
  }
  90% {
    transform: scale(var(--card-scale, 1)) translateX(2px);
  }
  100% {
    transform: scale(var(--card-scale, 1)) translateX(0);
  }
}

.shake {
  animation: shake 0.5s ease;
}
</style>