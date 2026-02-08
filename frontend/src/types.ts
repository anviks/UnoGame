import { type Moment } from 'moment';
import type { GameErrorCode } from './constants';

export type HubResponse =
  | {
      accepted: true;
    }
  | { accepted: false; error: GameErrorCode };

export interface Card {
  id: number;
  color: number;
  value: number;
}

export type CardPayload = Omit<Card, 'id'>;

export interface DrawnCard {
  index: number;
  card: Card;
}

export interface PublicDrawResult {
  requested: number;
  drawn: number;
  completed: boolean;
  reshuffleIndex: number | null;
}

export interface DrawResult extends PublicDrawResult {
  drawnCards: DrawnCard[];
}

export interface Game {
  id: number;
  name: string;
  createdAt: Moment;
  updatedAt: Moment;
  state: GameState;
}

export type GameDto = Omit<Game, 'createdAt' | 'updatedAt'> & {
  createdAt: string;
  updatedAt: string;
};

export interface GameState {
  currentColor: number;
  currentValue: number | null;
  currentPlayerIndex: number;
  isReversed: boolean;
  winnerIndex: number | null;
  pendingPenalty: PendingPenalty | null;
  players: Player[];
  drawPileSize: number;
  discardPile: Card[];
}

export interface PendingPenalty {
  playerName: string;
  cardCount: number;
}

export interface Player {
  name: string;
  type: number;
  saidUno: boolean;
  userId?: number | null;
  cards: Card[] | null;
  handSize: number;
  pendingDrawnCard: Card | null;
}

export interface PlayerField {
  username: string;
  type: number;
}

export interface GameForm {
  gameName: string;
  players: PlayerField[];
  includedCards?: CardPayload[];
}

export interface User {
  id: number;
  username: string;
  createdAt?: Moment;
}

export interface ApiError {
  message: string;
  reasons: string[];
  metadata: Record<string, any>;
}
