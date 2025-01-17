import { Friend } from "./friend";
import { User } from "./user";

export interface UserFriend {
    id : number,
    user : User,
    friend : Friend
}
