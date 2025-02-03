import { Part } from "./part";
import { Song } from "./song";

export interface Track {
    id: number,
    part: Part,
    song: Song
}
