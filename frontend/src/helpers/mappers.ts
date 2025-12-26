import type { Game, GameDto } from "@/types";
import moment from "moment";

export function gameDtoToGame(game: GameDto): Game {
  return {
    ...game,
    createdAt: moment(game.createdAt),
    updatedAt: moment(game.updatedAt),
  };
}