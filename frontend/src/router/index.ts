import { createRouter, createWebHistory } from 'vue-router';
import { GameForm, GameHub, Home, LoginForm, RegisterForm } from '@/views';

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home,
    },
    {
      path: '/new-game',
      name: 'new-game',
      component: GameForm,
    },
    {
      path: '/game/:gameId',
      name: 'game',
      component: GameHub,
      props: (route) => ({
        gameId: parseInt(route.params.gameId?.toString() ?? '0') || undefined,
      }),
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterForm,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginForm,
    },
  ],
});

export default router;
