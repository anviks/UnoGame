import axios from 'axios';
import type { Game } from '@/types.ts';
import moment from 'moment';
import { ApiClient } from '@/api/ApiClient.ts';

export class GameApi {
  static async getAllGames() {
    const games: Game[] = await ApiClient.get('/games', {
      errorMessage: 'Error fetching games.',
    });

    for (const game of games) {
      game.createdAt = moment(game.createdAt);
      game.updatedAt = moment(game.updatedAt);
    }

    return games;
  }

  static async getGame(gameId: number) {
    return ApiClient.get(`/games/${gameId}`);
  }

  static async createGame(payload: object) {
    return ApiClient.post('/games', payload);
  }

  static async joinGame() {}
  static async leaveGame() {}
  static async startGame() {}
}
