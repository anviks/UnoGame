<template>
  <v-container
    max-width="50%"
  >
    <v-form
      ref="form"
      @submit.prevent
    >
      <v-card :title="!token ? 'Register' : 'Please choose a name to continue'">
        <v-card-text>
          <v-text-field
            v-model="email"
            label="Email"
            autofocus
            :rules="[(value) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value) || 'Invalid email']"
            :disabled="!!token"
          ></v-text-field>

          <template v-if="token">
            <v-text-field
              v-model="username"
              label="Username"
              :error="isAvailable === false"
              :rules="[(value) => value.length > 2 || 'Username must have at least 3 characters', () => isAvailable !== false || 'Username is already taken']"
              outlined
              dense
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

<!--            <small-->
<!--              :style="isAvailable === null ? 'visibility: hidden' : ''"-->
<!--              :class="{-->
<!--              'text-green': isAvailable,-->
<!--              'text-red': !isAvailable,-->
<!--            }"-->
<!--            >-->
<!--              {{ isAvailable ? '✔ Available' : '✘ Taken' }}-->
<!--            </small>-->
          </template>

          <v-card-actions class="d-flex justify-end">
            <v-btn
              v-if="token"
              @click="register"
            >
              Register
            </v-btn>
            <v-btn
              v-else
              @click="sendMagicLink"
            >
              Send registration link
            </v-btn>
          </v-card-actions>
        </v-card-text>
      </v-card>
    </v-form>
  </v-container>
</template>

<script
  setup
  lang="ts"
>
import { useAuthStore } from '@/stores/authStore.ts';
import { computed, onMounted, ref, watch } from 'vue';
import { debounce } from 'lodash-es';
import { AuthApi } from '@/api/AuthApi.ts';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from 'vue-toastification';
import { removeQueryParameter } from '@/helpers.ts';

const toast = useToast();
const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const email = ref('');
const username = ref('');
const isAvailable = ref<boolean | null>(null);
const checking = ref(false);
const magicToken = ref();
const form = ref();

const token = computed(() => route.query.token?.toString());

const checkUsername = debounce(async (name: string) => {
  if (!name || name.length < 3) {
    isAvailable.value = null;
    checking.value = false;
    return;
  }

  checking.value = true;

  isAvailable.value = await AuthApi.isUsernameAvailable(name);

  checking.value = false;
}, 500);

const sendMagicLink = async () => {
  const validationResult = await form.value?.validate();
  if (!validationResult.valid) return;

  await AuthApi.sendMagicLink(email.value);
  toast.success('Registration link sent.');
};

const register = async () => {
  const validationResult = await form.value?.validate();
  if (!validationResult.valid) return;

  await AuthApi.register(username.value, magicToken.value.token);
  authStore.username = username.value;

  toast.success('Registration successful.');
  await router.push({ name: 'home' });
};

watch(username, (newVal) => {
  isAvailable.value = null;
  checkUsername(newVal);
});

onMounted(async () => {
  if (token.value != null) {
    try {
      magicToken.value = await AuthApi.getMagicToken(token.value);
      email.value = magicToken.value.email;
    } catch (e) {
      // const query = Object.assign({}, route.query);
      // delete query.token;
      // await router.replace({ query });
      await removeQueryParameter(route, router, 'token');
    }
  }
});
</script>

<style scoped>

</style>