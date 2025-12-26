// import './assets/main.css'

import 'font-awesome/css/font-awesome.min.css';
import '@mdi/font/css/materialdesignicons.css';
import 'vuetify/styles';
import 'tailwindcss';

import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { createVuetify } from 'vuetify';

import * as components from 'vuetify/components';
import * as directives from 'vuetify/directives';
import App from './App.vue';
import router from './router';
import Toast, { type PluginOptions, POSITION } from 'vue-toastification';
import 'vue-toastification/dist/index.css';

const app = createApp(App);

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    themes: {},
  },
  defaults: {
    VTooltip: {
      openDelay: 100,
    },
  },
});

const options: PluginOptions = {
  position: POSITION.BOTTOM_CENTER,
  newestOnTop: false,
};

app.use(createPinia());
app.use(router);
app.use(vuetify);
app.use(Toast, options);

app.mount('#app');

