export const playerType = {
  HUMAN: 0,
  COMPUTER: 1,
};

export const cardColor = {
  RED: 0,
  YELLOW: 1,
  GREEN: 2,
  BLUE: 3,
  WILD: 4,
};

export const cardValue = {
  ZERO: 0,
  ONE: 1,
  TWO: 2,
  THREE: 3,
  FOUR: 4,
  FIVE: 5,
  SIX: 6,
  SEVEN: 7,
  EIGHT: 8,
  NINE: 9,
  SKIP: 10,
  REVERSE: 11,
  DRAW_TWO: 12,
  WILD: 13,
  WILD_DRAW_FOUR: 14,
};

export const GameErrorCodes = {
  NOT_YOUR_TURN: 'NOT_YOUR_TURN',
  INVALID_CARD: 'INVALID_CARD',
} as const;

export type GameErrorCode = typeof GameErrorCodes[keyof typeof GameErrorCodes];

export const errorMessages: Record<GameErrorCode, string> = {
  [GameErrorCodes.NOT_YOUR_TURN]: 'It\'s not your turn.',
  [GameErrorCodes.INVALID_CARD]: 'You cannot play this card at the moment.',
};
