import { Component } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ChatComponent } from "../chat/chat.component";

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [ChatComponent],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {

  public readonly IMG_URL = environment.apiImg;

  level : number = 1;
  name : string = "prueba";
  img : string = this.IMG_URL + ""

}
