import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { BattleService } from '../../services/battle.service';
import { Subscription } from 'rxjs';
import { WebsocketService } from '../../services/websocket.service';
import { MessageType } from '../../models/message-type';
import { Card } from '../../models/card';
import { Action } from '../../models/action';
import { CardToPlay } from '../../models/CardToPlay';
import { Track } from '../../models/track';
import { Part } from '../../models/part';
import { Song } from '../../models/song';
import { ActionType } from '../../models/actionType';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './game.component.html',
  styleUrl: './game.component.css'
})
export class GameComponent implements OnInit {
  // 1) Me conecto al websocket (automáticamente, aquí no hay que hacer nada)
  // 2) El server le manda la información de la batalla (cartas y demás) y le dice si es su turno o no
  // 3) Si es su turno, realiza cualquier acción (poner alguna carta, por ejemplo)
  // 4) Sea o no su turno, pinta el tablero con la información actualizada
  // 5) Después de realizar la acción, le envía al server lo que ha hecho
  // 6) El server verifica que está todo bien, y SE REPITE EN BUCLE DESDE EL PASO 3
  // 7) El server notifica que se acabó la batalla
  // 8) Se muestran los resultados y se procede

  messageReceived$: Subscription | null = null;
  serverResponse: string = '';
  //cards: Card[] = []

  ///TEST BORRAR ESTO DESPUES
  songTest: Song = {
    name: "socorro"
  }

  partTest: Part = {
  }

  trackTest: Track = {
    id: 1,
    part: this.partTest,
    song: this.songTest
  }

  cartaTest: Card = {
    id : 1,
    imagePath : "mondongo",
    level : 3,
    track: this.trackTest
  }

  cartToPlayTest1: CardToPlay = {
    card: this.cartaTest,
    position: 1
  }

  cartToPlayTest2: CardToPlay = {
    card: this.cartaTest,
    position: 2
  }

  actionTypeTest1: ActionType = {
    name : "playCard",
  }

  actionTypeTest2: ActionType = {
    name : "playCard",
  }

  actionTest: Action = {
    cards : [this.cartToPlayTest1, this.cartToPlayTest2],
    type : [this.actionTypeTest1, this.actionTypeTest2]
  }

  ///TEST BORRAR ESTO DESPUES

  constructor(private webSocketService: WebsocketService) {
  }

  async ngOnInit(): Promise<void> {
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
    this.askForInfo(MessageType.ShuffleDeckStart)
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    switch (jsonResponse.messageType) {
      case MessageType.ShuffleDeckStart:
        //this.cards = jsonResponse.cards
        break
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }

  // Envía la acción del usuario al servidor
  sendAction(action: Action){
    const data = {
      "action" : action, 
      "messageType" : MessageType.PlayCard
    }
    const message = JSON.stringify(data)
    this.webSocketService.sendRxjs(message)
  }  
}
