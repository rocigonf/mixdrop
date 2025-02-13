import { Component } from '@angular/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {

  public readonly IMG_URL = environment.apiImg;

  level : number = 1;
  name : string = "prueba";
  img : string = this.IMG_URL + ""

}
