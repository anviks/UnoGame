import { type Moment } from 'moment';

export interface Card {
  color: number;
  value: number;
}

export interface Game {
  id: number;
  gameName: string;
  players: Player[];
  deck: any;
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
