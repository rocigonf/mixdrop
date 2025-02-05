import { Track } from "./track";

export interface Card {
    id: number,
    imagePath: string,
    level: number,
    track: Track
}
