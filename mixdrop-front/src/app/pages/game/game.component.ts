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
import { ChatComponent } from "../../components/chat/chat.component";
import { Battle } from '../../models/battle';
import { CardComponent } from "../../components/card/card.component";

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [NavbarComponent, ChatComponent, CardComponent],
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

  time: number = 120;

  board: Board = {
    playing: null,
    slots: [
      new Slot(), new Slot(), new Slot(), new Slot(), new Slot()
    ]
  }
  cardToUse: Card | null = null

  mix: string = ""
  bonus: string = ""

  otherPlayerPunct: number = 0

  private isProcessingAudio: boolean = false;

  currentBattle: Battle | null = null;

  private audioContext: AudioContext = new AudioContext();
  private activeSources: Map<number, AudioBufferSourceNode> = new Map<number, AudioBufferSourceNode>;

  constructor(private webSocketService: WebsocketService,
    public battleService: BattleService,
    public authService: AuthService,
    public router: Router) {
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
  }

  ngOnDestroy(): void {
    this.audioContext.close()
  }

  navigateToUrl(url: string) {
    this.router.navigateByUrl(url);
  }

  async processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)
    let positions: number[] = []

    switch (jsonResponse.messageType) {
      case MessageType.ShuffleDeckStart:
        this.userBattle = jsonResponse.userBattleDto
        this.currentBattle = jsonResponse.currentBattle

        break;
      case MessageType.TurnResult:
        console.warn("Entrando al semáforo...")
        await this.waitForAudioProcessing()
        console.warn("Saliendo...")


        this.board = jsonResponse.board
        this.userBattle = jsonResponse.player
        this.bonus = jsonResponse.bonus
        this.otherPlayerPunct = jsonResponse.otherplayer

        this.mix = jsonResponse.filepath
        positions = jsonResponse.position
        this.playAudio(this.mix, positions, jsonResponse.wheel); 
      
        break

      case MessageType.EndGame:
        this.gameEnded = true

        this.board = jsonResponse.board
        this.userBattle = jsonResponse.player
        this.mix = jsonResponse.filepath
        positions = jsonResponse.position
        this.playAudio(this.mix, positions, jsonResponse.wheel); 

        if (this.userBattle?.battleResultId == 1) {
          alert("Ganaste :D")
        }
        else {
          alert("Perdiste :(")
        }
        break;
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  // reproduce el mix en byte que le envia al jugar una carta
  async playAudio(encodedAudio: string, positions: number[], spinTheWheel : boolean) 
  {
  
    this.isProcessingAudio = true
    if(spinTheWheel)
    {
      console.log("Se ha girado la ruleta. El resultado ha sido: ", positions)
      for(let i = 0; i < positions.length; i++)
      {
        this.stopTrack(positions[i])
      }
      this.isProcessingAudio = false
      return;
    }

    const position = positions[0]

    const slut = this.board.slots[position]
    if (slut?.card != null) {
      console.log("Borrando posición indicada: ", position)
      this.stopTrack(position)
    }

    const audioBuffer = await this.audioContext.decodeAudioData(this.base64ToArrayBuffer(encodedAudio));
    const source = this.audioContext.createBufferSource()

    source.buffer = audioBuffer
    source.loop = true

    source.start(undefined, this.audioContext.currentTime)
    source.connect(this.audioContext.destination)

    this.activeSources.set(position, source)

    this.isProcessingAudio = false
  }

  private stopTrack(position: number)
  {
    console.log("Posición a borrar: ", position)
    //const source = this.activeSources.get(position)
    this.activeSources.get(position)?.stop()
    this.activeSources.delete(position)
    /*if(source)
      {
        console.error("Borrando...")
        source.stop()
        this.activeSources.delete(position)
      }
      else
      {console.error("NO EXISTE LA POSICIÓN")}*/    
  }

  // Semaforeame esta mister
  // Igual al antiguo método que teníamos pero de forma recursiva
  private async waitForAudioProcessing() {
    return new Promise<void>((resolve) => {
      const checkProcessing = () => {
        if (!this.isProcessingAudio) {
          resolve(); // S
        } else {
          setTimeout(checkProcessing, 50); // Se llama a la verificación cada 50 milisegundos
        }
      };
      checkProcessing();
    });
  }

  // Larga vida a StackOverflow xD (https://stackoverflow.com/questions/21797299/how-can-i-convert-a-base64-string-to-arraybuffer)
  private base64ToArrayBuffer(base64: string) {
    const binaryString = atob(base64);
    const bytes = new Uint8Array(binaryString.length);
    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes.buffer;
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

  useButton() {
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
