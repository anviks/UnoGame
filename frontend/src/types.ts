import { type Moment } from 'moment';

export interface Card {
  id?: number;
  color: number;
  value: number;
}

export interface Game {
  id: number;
  name: string;
  currentColor: number;
  currentValue: number | null;
  players: Player[];
  discardPile: Card[];
  drawPile: Card[];
  createdAt: Moment;
  updatedAt: Moment;
}

export interface Player {
  id?: number;
  name: string;
  type: number;
  saidUno: boolean;
  position: number;
  userId?: number | null;
  cards: Card[];
}

export interface PlayerField {
  id?: number;
  name: string;
  type: number;
}

export interface GameForm {
  gameName: string;
  players: PlayerField[];
  deck: any;
}

export interface User {
  id: number;
  fingerprint: string;
  name: string;
}
