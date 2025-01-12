import { User } from "./user";

export interface LoginResult {
    accessToken: string;
    user : User;
}
