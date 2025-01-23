import { User } from "./user";

export interface Friend {
    Id : number,
    Accepted : boolean,
    SenderUser : User | null,
    ReceiverUser: User | null
}
