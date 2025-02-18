import { Component, ElementRef, Input, Signal, ViewChild } from '@angular/core';
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

  @ViewChild("card", { static: false }) carta!: ElementRef<HTMLDivElement> 

  @Input() currentCard: Card | null = null

  turn: boolean = false;
  part: string = "Bass"
  artist: string = "Lorde"
  level: number = 3;
  name: string = "Green li asd ad asd asght";
  img: string = this.IMG_URL + ""
  color: string = "";
  gris : string = "#3a3a3a"


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
    }
  }

  ngAfterViewInit() {
    if (this.carta) {
      this.carta.nativeElement.style.setProperty('--color-level', this.color);
    }
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
  }
}
