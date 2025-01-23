import { Friend } from "./friend";

export interface User {
    Id : number,
    Nickname : string,
    Email : string,
    AvatarPath : string,
    Role: string,
    IsInQueue: boolean,
    StateId: number,
    Friend: Friend[],
    Banned : boolean
}
