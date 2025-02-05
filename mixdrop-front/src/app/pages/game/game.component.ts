import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { BattleService } from '../../services/battle.service';
import { Subscription } from 'rxjs';
import { WebsocketService } from '../../services/websocket.service';
import { MessageType } from '../../models/message-type';
import { Card } from '../../models/card';

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
  cards: Card[] = []

  myTurn: Boolean = false;

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
        this.cards = jsonResponse.cards
        this.myTurn = jsonResponse.cards.isTheirTurn
        break
      case MessageType.TurnPlayed:
        this.myTurn = jsonResponse.turn.isTheirTurn
        break;

    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }

  // nose porque solo funciona la primera vez q lo pulso :(
  playTurn() {
    this.askForInfo(MessageType.TurnPlayed)
  }
}
