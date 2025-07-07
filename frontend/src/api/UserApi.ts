import ApiClient from './ApiClient.ts';
import type { User } from '@/types.ts';

export default class UserApi {
  static async getAllUsers(): Promise<User[]> {
    return ApiClient.get('/users');
  }
}