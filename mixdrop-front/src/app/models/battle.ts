import { User } from "./user";
import { UserBattle } from "./user-battle";

export interface Battle {
    id: number,
    accepted : boolean,
    isAgainstBot : boolean,
    isPlaying : boolean,
    battleUsers : UserBattle[],
    user : User,
    createdAt: Date,
    finishedAt: Date
}
