import { Card } from "./card";

export interface UserBattleDto {
    cards: Card[],
    isTheirTurn: boolean,
    punctuation: number,
    battleResult: number
}
