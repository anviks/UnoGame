import { type Moment } from 'moment';

export interface Card {
  id?: number;
  color: number;
  value: number;
}

export interface DrawnCard {
  index: number;
  card: Card;
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
  includedCards: Card[];
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
