import { Slot } from "./slot";
import { Track } from "./track";

export interface Board {
    slots: Slot[]
    playing: Track | null
}
