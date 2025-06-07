<template>
  <div class="d-flex ga-4 align-center pb-3">
    <v-combobox
      :modelValue="player.name"
      @update:modelValue="updatePlayer"
      :label="'Player ' + (index + 1)"
      style="flex-basis: 60%"
      :items="users"
      item-value="id"
      item-title="name"
      hide-details
      return-object
    />
    <v-select
      v-model="player.type"
      :items="playerTypeSelection"
      label="Player type"
      style="flex-basis: 40%"
      hide-details
      :disabled="!player.id"
    />
    <v-icon
      v-if="canDelete"
      class="remove-player-button"
      @click="$emit('delete-player')"
    >
      mdi-window-close
    </v-icon>
  </div>
</template>

<script
  setup
  lang="ts"
>
import type { PropType } from 'vue';
import type { PlayerField, User } from '@/types.ts';
import { playerType } from '@/constants.ts';
import { capitalize } from 'lodash-es';

const emits = defineEmits(['delete-player']);

const props = defineProps({
  player: {
    type: Object as PropType<PlayerField>,
    required: true,
  },
  index: {
    type: Number,
    required: true,
  },
  canDelete: {
    type: Boolean,
    default: false,
  },
  users: {
    type: Array as PropType<User[]>,
    default: [],
  },
});

const playerTypeSelection = Object.entries(playerType).map(([title, value]) => ({ title: capitalize(title), value }));

const updatePlayer = (newValue: string | PlayerField) => {
  if (typeof newValue === 'string') {
    props.player.id = undefined;
    props.player.name = newValue;
    props.player.type = playerType.COMPUTER;
  } else {
    props.player.id = newValue.id;
    props.player.name = newValue.name;
    props.player.type = playerType.HUMAN;
  }
};
</script>

<style scoped>

</style>