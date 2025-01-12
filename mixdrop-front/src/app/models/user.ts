export interface User {
    id : number,
    nickname : string,
    email : string,
    avatarPath : string,
    role: string,
    isInQueue: boolean,
    stateId: number
}
