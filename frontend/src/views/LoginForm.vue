<template>
  <v-container max-width="50%">
    <v-form
      ref="form"
      @submit.prevent
    >
      <v-card title="Login">
        <v-card-text class="mt-3">
          <div class="flex flex-col gap-2">
            <v-text-field
              v-model="formValues.username"
              label="Username"
              :rules="[(value) => !!value || 'Username is required']"
              variant="outlined"
              autofocus
            />

            <v-text-field
              v-model="formValues.password"
              label="Password"
              variant="outlined"
              type="password"
              :rules="[(value) => !!value || 'Password is required']"
              @keydown.enter="login"
            />
          </div>

          <v-card-actions class="d-flex justify-end">
            <v-btn
              @click="login"
              :loading="isLoading"
            >
              Login
            </v-btn>
          </v-card-actions>
        </v-card-text>
      </v-card>
    </v-form>
  </v-container>
</template>

<script setup lang="ts">
import { useApiRequest } from '@/composables/useApiRequest';
import { useAuthStore } from '@/stores/authStore.ts';
import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const authStore = useAuthStore();
const { isLoading, request } = useApiRequest();

const formValues = reactive({
  username: '',
  password: '',
});

const form = ref();

const login = async () => {
  const validationResult = await form.value?.validate();
  if (!validationResult.valid) return;

  const { success } = await request<boolean>('/auth/login', {
    method: 'POST',
    data: {
      username: formValues.username,
      password: formValues.password,
    },
    errorMessage: 'Login failed. Please check your credentials.',
    successMessage: 'Login successful.',
  });

  if (!success) return;

  await authStore.initialize();
  await router.push({ name: 'home' });
};
</script>

<style scoped></style>
