import { Component, ElementRef, Input, Signal, viewChild } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Card } from '../../models/card';
import { canHaveDecorators } from 'typescript';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {
  readonly MAX_LEVELS = new Array(3);


  public readonly IMG_URL = environment.apiImg;

  // @viewChild() carta: ElementRef<HTMLDivElement>

  @Input() currentCard: Card | null = null

  turn: boolean = false;
  part: string = "Bass"
  artist: string = "Lorde"
  level: number = 3;
  name: string = "Green li asd ad asd asght";
  img: string = this.IMG_URL + ""
  color: string = "";


  setCart(card: Card) {
    this.part = card.track.part.name;
    this.level = card.level;
    this.img = this.IMG_URL + card.imagePath;
    this.name = card.track.song.name;
  }



  ngOnInit() {
    console.log("carta en padre:", this.currentCard);

    if (this.currentCard) {
      this.setCart(this.currentCard);
      this.setColor(this.currentCard.track.part.name);
      this.setLevel(this.currentCard.level);
      console.log("la parte", this.part)
      console.log("el nivel", this.level)
    }

    //this.carta.nativeElement.style = "--color-level: red"

  }


  setColor(part: string) {

    switch (part) {
      case "Vocal": this.color = "#fee710"
        break;
      case "Main": this.color = "#e50061"
        break;
      case "Bass": this.color = "#49af38"
        break;
      case "Drums": this.color = "#02a7e9"
        break;
    }
    document.documentElement.style.setProperty('--color-trapecio', this.color);
  }

  setLevel(level: number) {
    var color: string = "#3a3a3a";
    switch (level) {
      case 1: document.documentElement.style.setProperty('--level2-color', color);
        document.documentElement.style.setProperty('--level3-color', color);
        break;
      case 2: document.documentElement.style.setProperty('--level2-color', this.color);
        document.documentElement.style.setProperty('--level3-color', color);
        break;
      case 3: document.documentElement.style.setProperty('--level2-color', this.color);
        document.documentElement.style.setProperty('--level3-color', this.color);
        break;
    }
  }


}
