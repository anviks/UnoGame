import { type Moment } from 'moment';

export interface UnoCard {
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
  discardPile: UnoCard[];
  drawPile: UnoCard[];
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
  cards: UnoCard[];
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
