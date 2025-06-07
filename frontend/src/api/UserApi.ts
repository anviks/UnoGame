import { ApiClient } from '@/api/ApiClient.ts';
import type { User } from '@/types.ts';

export class UserApi {
  static async getAllUsers(): Promise<User[]> {
    return ApiClient.get('/users');
  }
}