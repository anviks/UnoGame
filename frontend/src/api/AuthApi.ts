import { ApiClient } from '@/api/ApiClient.ts';

export class AuthApi {
  static async sendMagicLink(email: string) {
    return ApiClient.post('/auth/request-magic-link', { email });
  }

  static async register(username: string, token: string) {
    return ApiClient.post('/auth/register', {
      username,
      token,
    }, {
      errorMessage: 'Error while registering.',
    });
  }

  static async isUsernameAvailable(username: string) {
    return ApiClient.get('/auth/is-username-available', {
      params: {
        username,
      },
      errorMessage: 'Error fetching user info.',
    });
  }

  static async whoAmI() {
    return ApiClient.get('/auth/whoami', {
      errorMessage: null,
    });
  }

  static async getMagicToken(token: string) {
    return ApiClient.get(`/auth/magic-token/${token}`);
  }
}
