<template>
  <div class="flex gap-4 items-center pb-3">
      <v-combobox
        :modelValue="player"
        @update:modelValue="updatePlayer"
        :label="'Player ' + (index + 1)"
        :items="users"
        item-value="id"
        variant="outlined"
        class="flex-6!"
        item-title="username"
        hide-details
        return-object
      />
      <v-select
        v-model="player.type"
        :items="playerTypeSelection"
        label="Player type"
        variant="outlined"
        hide-details
        class="flex-4!"
        :disabled="!userExists"
      />
      <v-icon
        v-if="canDelete"
        class="opacity-50 hover:opacity-100! flex-0.5!"
        @click="$emit('delete-player')"
      >
        mdi-window-close
      </v-icon>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { PlayerField, User } from '@/types.ts';
import { playerType } from '@/constants.ts';
import _ from 'lodash';

const emits = defineEmits<{ 'delete-player': [] }>();

interface Props {
  player: PlayerField;
  index: number;
  canDelete?: boolean;
  users?: User[];
}

const props = withDefaults(defineProps<Props>(), {
  canDelete: false,
  users: () => [],
});

const playerTypeSelection = Object.entries(playerType).map(
  ([title, value]) => ({ title: _.capitalize(title), value })
);
const userExists = ref(false);

const updatePlayer = (newValue: string | PlayerField) => {
  if (newValue == null || typeof newValue === 'string') {
    userExists.value = false;
    props.player.username = newValue;
    props.player.type = playerType.COMPUTER;
  } else {
    userExists.value = true;
    props.player.username = newValue.username;
    props.player.type = playerType.HUMAN;
  }
};
</script>

<style scoped></style>
