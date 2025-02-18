import { Card } from "./card";

export interface UserBattleDto {
    userId: number,
    cards: Card[],
    isTheirTurn: boolean,
    punctuation: number,
    battleResultId: number,
    userName: string
}
