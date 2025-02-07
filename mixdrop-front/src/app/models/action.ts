import { ActionType } from "./actionType";
import { CardToPlay } from "./cardToPlay";

export interface Action {
    cards : CardToPlay[],
    actionsType : ActionType[]
}