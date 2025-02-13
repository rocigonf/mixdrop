import { ActionType } from "./actionType";
import { CardToPlay } from "./cardToPlay";

export interface Action {
    card : CardToPlay | null,
    actionType : ActionType | null
}