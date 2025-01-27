import { User } from "./user";

export interface Friend {
    id : number,
    accepted : boolean,
    senderUser : User | null,
    receiverUser: User | null,
    senderUserId: number,
    receiverUserId: number
}
