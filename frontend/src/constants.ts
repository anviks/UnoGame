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
  NOT_ALLOWED_TO_DRAW_TWICE: 'NOT_ALLOWED_TO_DRAW_TWICE',
  INVALID_CARD_AFTER_DRAW: 'INVALID_CARD_AFTER_DRAW',
  NO_CARDS_TO_DRAW: 'NO_CARDS_TO_DRAW',
  GAME_ALREADY_ENDED: 'GAME_ALREADY_ENDED',
} as const;

export type GameErrorCode = typeof GameErrorCodes[keyof typeof GameErrorCodes];

export const errorMessages: Record<GameErrorCode, string> = {
  [GameErrorCodes.NOT_YOUR_TURN]: 'It\'s not your turn.',
  [GameErrorCodes.INVALID_CARD]: 'You cannot play this card at the moment.',
  [GameErrorCodes.NOT_ALLOWED_TO_DRAW_TWICE]: 'You are not allowed to draw twice in a row.',
  [GameErrorCodes.INVALID_CARD_AFTER_DRAW]: 'You can only play the drawn card.',
  [GameErrorCodes.NO_CARDS_TO_DRAW]: 'There are no cards left to draw.',
  [GameErrorCodes.GAME_ALREADY_ENDED]: 'The game has already ended.',
};
