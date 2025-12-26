import {
  type RouteLocationNormalizedLoadedGeneric,
  type Router,
} from 'vue-router';

export async function removeQueryParameter(
  route: RouteLocationNormalizedLoadedGeneric,
  router: Router,
  paramName: string
) {
  const query = Object.assign({}, route.query);
  delete query[paramName];
  await router.replace({ query });
}

