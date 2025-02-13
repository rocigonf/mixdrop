import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { Subscription } from 'rxjs';
import { WebsocketService } from '../../services/websocket.service';
import { MessageType } from '../../models/message-type';
import { Card } from '../../models/card';
import { Action } from '../../models/action';
import { CardToPlay } from '../../models/cardToPlay';
import { ActionType } from '../../models/actionType';
import { UserBattleDto } from '../../models/user-battle-dto';
import { Board } from '../../models/board';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Slot } from '../../models/slot';
import { AuthService } from '../../services/auth.service';
import { BattleService } from '../../services/battle.service';

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
  gameEnded: boolean = false

  audio: HTMLAudioElement | null = null;

  board: Board = {
    playing: null,
    slots: [
      new Slot(), new Slot(), new Slot(), new Slot()
    ]
  }
  cardToUse: Card | null = null

  mix: string = ""
  bonus: string = ""

  private audioContext: AudioContext = new AudioContext();
  private tracks: Map<number, AudioBuffer> = new Map;
  private activeSources: Map<number, AudioBufferSourceNode> = new Map;

  constructor(private webSocketService: WebsocketService,
    public battleService : BattleService,
    public authService: AuthService,
    private router: Router) {
  }

  async ngOnInit(): Promise<void> {

    console.log(this.authService.isAuthenticated())
    if (!this.authService.isAuthenticated()) {
      console.log("no esta autenticfado")
      this.navigateToUrl("login");
    } else {
      this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
      this.askForInfo(MessageType.ShuffleDeckStart)
    }

    this.audioContext.audioWorklet
      .addModule("./js/soundtouch-worklet.js")

  }

  ngOnDestroy(): void {
    this.audio?.pause()
  }

  navigateToUrl(url: string) {
    this.router.navigateByUrl(url);
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
        this.bonus = jsonResponse.bonus

        if(jsonResponse.filepath != "" ||jsonResponse.mix != "")
        {
          this.mix = jsonResponse.filepath
          this.playAudio(this.mix); 
        }
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

  // reproduce el mix en byte que le envia al jugar una carta
  async playAudio(encodedAudio: string) {
    return await new Promise<void>((resolve) => {
      /*const source = this.audioContext.createBufferSource()
      source.buffer = encodedAudio*/
      const audio = new Audio("data:audio/wav;base64," + encodedAudio);
      audio.onended = () => resolve();
      audio.loop = true
      audio.play();
    })
  }

  selectCard(card: Card) {
    this.cardToUse = card
  }

  useCard(desiredPosition: number) {
    if (this.cardToUse) {
      const cardToPlay: CardToPlay = {
        cardId: this.cardToUse.id,
        position: desiredPosition,
      }
      const action: Action = {
        card: cardToPlay,
        actionType: null
      }
      this.sendAction(action)

      this.cardToUse = null
    }
  }

  useButton()
  {
    const actionType: ActionType = {
      name: "button"
    }
    const action: Action = {
      card: null,
      actionType: actionType
    }
    this.sendAction(action)
  }

  checkType(posibleType: string[], actualType: string) {
    // Si no devuelve -1 significa que está en la lista
    return posibleType.indexOf(actualType) != -1
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
