// TODO: Habrá que agregar los resultados

import { Battle } from "./battle";
import { User } from "./user";

export interface UserBattle {
    id : number,
    punctuation : number,
    timePlayed : number,
    userId : number,
    battleId: number,
    receiver : boolean,
    user : User,
    battle : Battle
}