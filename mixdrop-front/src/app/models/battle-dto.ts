import { UserBattleDto } from "./user-battle-dto";

export interface BattleDto {
    id: number,
    isAgainstBot : boolean,
    battleStateId: number
    userId: number,
    usersBattles: UserBattleDto[],
    createdAt: Date,
    finishedAt: Date
}
