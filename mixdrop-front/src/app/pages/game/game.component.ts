import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { Subscription } from 'rxjs';
import { WebsocketService } from '../../services/websocket.service';
import { MessageType } from '../../models/message-type';
import { Card } from '../../models/card';
import { Action } from '../../models/action';
import { CardToPlay } from '../../models/cardToPlay';
import { Track } from '../../models/track';
import { Part } from '../../models/part';
import { Song } from '../../models/song';
import { ActionType } from '../../models/actionType';
import { UserBattleDto } from '../../models/user-battle-dto';
import { Board } from '../../models/board';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Slot } from '../../models/slot';
import { J } from '@angular/cdk/keycodes';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './game.component.html',
  styleUrl: './game.component.css'
})
export class GameComponent implements OnInit, OnDestroy {
  // 1) Me conecto al websocket (automáticamente, aquí no hay que hacer nada)
  // 2) El server le manda la información de la batalla (cartas y demás) y le dice si es su turno o no
  // 3) Si es su turno, realiza cualquier acción (poner alguna carta, por ejemplo)
  // 4) Sea o no su turno, pinta el tablero con la información actualizada
  // 5) Después de realizar la acción, le envía al server lo que ha hecho
  // 6) El server verifica que está todo bien, y SE REPITE EN BUCLE DESDE EL PASO 3
  // 7) El server notifica que se acabó la batalla
  // 8) Se muestran los resultados y se procede

  public readonly IMG_URL = environment.apiImg;

  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  userBattle: UserBattleDto | null = null
  filePath: string = this.IMG_URL + "/songs/input/rickroll_full_loop.mp3"
  gameEnded: boolean = false

  audio = new Audio();

  board: Board = {
    playing : null,
    slots : [
      new Slot(), new Slot(), new Slot(), new Slot()
    ]
  }
  cardToUse : Card | null = null
  
  mix: string = ""


  ///TEST BORRAR ESTO DESPUES
  songTest: Song = {
    name: "socorro"
  }

  partTest: Part = {
    id: 0,
    name: "si"
  }

  trackTest: Track = {
    id: 1,
    part: this.partTest,
    song: this.songTest
  }

  cartaTest: Card = {
    id: 1,
    imagePath: "mondongo",
    level: 3,
    track: this.trackTest
  }

  cartToPlayTest1: CardToPlay = {
    cardId: this.cartaTest.id,
    position: 1
  }

  cartToPlayTest2: CardToPlay = {
    cardId: this.cartaTest.id,
    position: 2
  }

  actionTypeTest1: ActionType = {
    name: "playCard",
  }

  actionTypeTest2: ActionType = {
    name: "playCard",
  }

  actionTest: Action = {
    cards : [this.cartToPlayTest1, this.cartToPlayTest2],
    actionsType : [this.actionTypeTest1, this.actionTypeTest2]
  }

  ///TEST BORRAR ESTO DESPUES

  constructor(private webSocketService: WebsocketService, private route: Router) {
  }

  async ngOnInit(): Promise<void> {
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
    this.askForInfo(MessageType.ShuffleDeckStart)
  }

  ngOnDestroy(): void {
    this.audio.pause()
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    switch (jsonResponse.messageType) {
      case MessageType.ShuffleDeckStart:
        this.userBattle = jsonResponse.userBattleDto
        break
      case MessageType.TurnResult:
        this.board = jsonResponse.board
        this.userBattle = jsonResponse.player
        
        // estaba en develop 
        // this.filePath = this.IMG_URL + jsonResponse.filepath

        this.filePath = jsonResponse.filepath
        this.mix = jsonResponse.mix

        this.reproduceAudio()
        this.playAudio(this.mix)
        break
      case MessageType.EndGame:
        // TODO: Mostrar si ha ganado o perdido en función del userBattle.battleResultId y poner un botón para volver al inicio
        alert("Se acabó el juego :D")
        this.gameEnded = true
        this.board = jsonResponse.board
        this.userBattle = jsonResponse.player
        break
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  // Puede ser que falle
  reproduceAudio()
  {
    this.audio.pause()
    this.audio.currentTime = 0

    this.audio = new Audio(this.filePath);
    this.audio.loop = true
    this.audio.play()
  }


  selectCard(card: Card)
  {
    this.cardToUse = card
  }

  useCard(desiredPosition: number)
  {
    if(this.cardToUse)
    {
      const cardToPlay : CardToPlay = {
        cardId: this.cardToUse.id,
        position: desiredPosition,
      }
      const action : Action = {
        cards : [cardToPlay],
        actionsType : []
      }
      this.sendAction(action)

      this.cardToUse = null
    }
  }

  checkType(posibleType: string[], actualType: string)
  {
    // Si no devuelve -1 significa que está en la lista
    return posibleType.indexOf(actualType) != -1
  }

  // reproduce el mix que le envia al jugar una carta
  async playAudio(encodedAudio: string) {
    return await new Promise<void>((resolve) => {
      const audio = new Audio("data:audio/wav;base64," + encodedAudio);
      audio.onended = () => resolve();
      audio.play();
    })
  }
  
  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }

  // Envía la acción del usuario al servidor
  sendAction(action: Action) {
    const data = {
      "action": action,
      "messageType": MessageType.PlayCard
    }
    const message = JSON.stringify(data)
    this.webSocketService.sendRxjs(message)
  }
}
