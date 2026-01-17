<template>
  <v-container max-width="50%">
    <v-form
      ref="form"
      @submit.prevent
    >
      <v-card :title="!token ? 'Register' : 'Please choose a name to continue'">
        <v-card-text class="mt-3">
          <div class="flex flex-col gap-2">
            <v-text-field
              v-model="formValues.username"
              label="Username"
              :error="isAvailable === false"
              :rules="[
                (value) =>
                  value.length > 2 ||
                  'Username must have at least 3 characters',
                () => isAvailable !== false || 'Username is already taken',
              ]"
              variant="outlined"
              autofocus
            >
              <template #append-inner>
                <v-progress-circular
                  v-if="checking"
                  indeterminate
                  size="20"
                  width="2"
                  color="primary"
                />
                <v-icon
                  v-else-if="isAvailable === true"
                  color="green"
                >
                  mdi-check
                </v-icon>
                <v-icon
                  v-else-if="isAvailable === false"
                  color="red"
                >
                  mdi-close
                </v-icon>
              </template>
            </v-text-field>

            <v-text-field
              v-model="formValues.password"
              label="Password"
              variant="outlined"
              type="password"
            />

            <v-text-field
              v-model="formValues.passwordConfirmation"
              label="Confirm password"
              variant="outlined"
              type="password"
              @keydown.enter="register"
            />
          </div>

          <v-card-actions class="d-flex justify-end">
            <v-btn @click="register">Register</v-btn>
          </v-card-actions>
        </v-card-text>
      </v-card>
    </v-form>
  </v-container>
</template>

<script setup lang="ts">
import { useApiRequest } from '@/composables/useApiRequest';
import { useAuthStore } from '@/stores/authStore.ts';
import _ from 'lodash';
import { computed, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const fetcher = useApiRequest();

const formValues = reactive({
  username: '',
  password: '',
  passwordConfirmation: '',
});

const isAvailable = ref<boolean | null>(null);
const checking = ref(false);
const form = ref();

const token = computed(() => route.query.token?.toString());

const checkUsername = _.debounce(async (name: string) => {
  if (!name || name.length < 3) {
    isAvailable.value = null;
    checking.value = false;
    return;
  }

  checking.value = true;

  const { data } = await fetcher<boolean>('/auth/is-username-available', {
    params: { username: name },
    errorMessage: 'Error fetching user info.',
  });

  isAvailable.value = data;
  checking.value = false;
}, 500);

const register = async () => {
  const validationResult = await form.value?.validate();
  if (!validationResult.valid) return;

  const { success } = await fetcher<boolean>('/auth/register', {
    method: 'POST',
    data: {
      username: formValues.username,
      password: formValues.password,
    },
    errorMessage: 'Error while registering.',
    successMessage: 'Registration successful.',
  });

  if (!success) return;

  authStore.username = formValues.username;
  await router.push({ name: 'home' });
};

watch(
  () => formValues.username,
  (newVal) => {
    isAvailable.value = null;
    checkUsername(newVal);
  }
);
</script>

<style scoped></style>
