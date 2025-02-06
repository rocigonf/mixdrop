import { ActionType } from "./actionType";
import { CardToPlay } from "./CardToPlay";

export interface Action {
    cards : CardToPlay[],
    type : ActionType[]
}