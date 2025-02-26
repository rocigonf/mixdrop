import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { Observable, map, Subscription, takeWhile, timer } from 'rxjs';
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
import { AsyncPipe, DatePipe } from '@angular/common';
import { CardComponent } from "../../components/card/card.component";
import { RouletteComponent } from "../../components/roulette/roulette.component";
import { UserBattle } from '../../models/user-battle';
import { ChangeDetectorRef } from '@angular/core';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [NavbarComponent, ChatComponent, DatePipe, AsyncPipe, CardComponent, CommonModule, RouletteComponent],
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


  @ViewChild(RouletteComponent) roulette!: RouletteComponent;

  public readonly IMG_URL = environment.apiImg;

  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  seconds = 120

  timeRemaining$: Observable<number> | null = null

  userBattle: UserBattleDto | null = null
  gameEnded: boolean = false

  board: Board = {
    playing: null,
    slots: [
      new Slot(), new Slot(), new Slot(), new Slot(), new Slot()
    ]
  }
  cardToUse: Card | null = null
  bonus: string = ""

  otherPlayerPunct: number = 0
  otherUserId: number = 0
  enemyUser: UserBattle | undefined = undefined;

  //private indexToDelete = -1

  private isProcessingAudio: boolean = false;

  currentBattle: Battle | null = null;

  position: number = 0;

  spinRouletteLevel: number = -1;
  whoSpinRoulette: string = ""

  private canReceive = true

  showRoulette: boolean = false;
  isRouletteSpinning: boolean = false;


  private audioContext: AudioContext = new AudioContext();
  private activeSources: Map<string, AudioBufferSourceNode> = new Map;

  constructor(private webSocketService: WebsocketService,
    public battleService: BattleService,
    public authService: AuthService,
    public router: Router,
    private cdr: ChangeDetectorRef) {
  }


  async ngOnInit(): Promise<void> {

    console.log(this.authService.isAuthenticated())
    if (!this.authService.isAuthenticated()) {
      this.navigateToUrl("login");
    } else {
      this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
      this.askForInfo(MessageType.ShuffleDeckStart)
    }
  }

  ngAfterViewInit(): void {
    console.log('Ruleta cargada:', this.roulette);
  }

  startRoulette(level: number) {
    this.isRouletteSpinning = true;
    if (this.roulette && this.roulette.wheel) {
      this.roulette.spinRoulette(level);
      console.log("Ruleta girada por " , this.whoSpinRoulette);

      setTimeout(() => {
        this.showRoulette = false;
        this.isRouletteSpinning = false;
      }, 4000);
    }
  }

  showAndHideRoulette(level: number) {
    if (this.isRouletteSpinning) {
      console.log("esta girando ya")
    } else {
      this.showRoulette = true;
      this.spinRouletteLevel = level;

      this.startRoulette(level);

      setTimeout(() => {
        this.startRoulette(level);
        setTimeout(() => {
          this.showRoulette = false;
        }, 4000);
      }, 100);
    }
  }

  async ngOnDestroy(): Promise<void> {
    this.messageReceived$?.unsubscribe()
    this.audioContext.close()
    if(!this.gameEnded)
    {
      if(this.currentBattle?.isAgainstBot)
      {
        await this.battleService.deleteBotBattle()
      }
      else
      {
        await this.battleService.forfeitBattle()
      }
    }
  }

  navigateToUrl(url: string) {
    this.router.navigateByUrl(url);
  }

  async processMessage(message: any) {
    // Esto es porque parece que recibe los mensajes dos veces
    if (!this.canReceive) {
      this.canReceive = true
      return
    }

    console.warn("Entrando al semáforo...")
    await this.waitForAudioProcessing()
    console.warn("Saliendo...")

    if (message instanceof Blob) {
      const data = await message.arrayBuffer()
      await this.processAudio(data)
      return
    }


      this.serverResponse = message
      const jsonResponse = JSON.parse(this.serverResponse)
      let positions: number[] = []

      switch (jsonResponse.messageType) {
        case MessageType.ShuffleDeckStart:
          this.userBattle = jsonResponse.userBattleDto
          this.currentBattle = jsonResponse.currentBattle
          this.enemyUser = this.currentBattle?.battleUsers.find(u => u.userId != this.userBattle?.userId)
          this.activateTimer()
          break;
          
         case MessageType.TurnResult:
          this.board = jsonResponse.board

          const newPlayer: UserBattleDto = jsonResponse.player
          //const cards = this.userBattle!!.cards

          this.userBattle = newPlayer
          /*const newCard = jsonResponse.card
          this.userBattle.cards = cards

          // Por si la carta no se puede jugar
          if(newCard && this.indexToDelete != -1)
          {
            console.error("BORRADO EN ", this.indexToDelete)
            this.userBattle.cards.splice(this.indexToDelete, 1)
            this.userBattle.cards.push(newCard)
            this.indexToDelete = -1
            console.error("BORRADO")
          }*/
      
          this.spinRouletteLevel = jsonResponse.levelRoulette

        // RULETA
        if (this.spinRouletteLevel > -1) {
          this.whoSpinRoulette = jsonResponse.whoSpinTheWheel
          this.showRoulette = true;
          console.log("NIVEL RULETA", this.spinRouletteLevel)
          this.showAndHideRoulette(this.spinRouletteLevel);
        }
          
          this.bonus = jsonResponse.bonus
          this.otherPlayerPunct = jsonResponse.otherplayer

          positions = jsonResponse.position
          this.playAudio(positions, jsonResponse.wheel); 

          this.activateTimer()
          break
   
      case MessageType.EndGame:
          this.gameEnded = true
          this.otherUserId = jsonResponse.otherUserId

          this.otherPlayerPunct = jsonResponse.otherplayer

          this.board = jsonResponse.board
          this.userBattle = jsonResponse.player
          positions = jsonResponse.position
          this.playAudio(positions, jsonResponse.wheel); 

          if (this.userBattle?.battleResultId == 1) {
            this.showAlert("Ganaste", "Ganaste :D")
          }
          else {
            this.showAlert("Perdiste", "Perdiste :(")
          }

          this.timeRemaining$ = null
          break;
          
          case MessageType.AskForBattle:
            this.showAlert("Revancha solicitada", "Revancha solicitada")
            this.router.navigateByUrl("menu")
            break
          
        case MessageType.DisconnectedFromBattle:
          if(jsonResponse.reported == false)
          {
            alert("El otro usuario se ha desconectado, por lo que has ganado")
          }
          else
          {
            alert("Se te acabó el tiempo, por lo que has perdido")
          }
          this.router.navigateByUrl("menu")
          break
      }
      console.log("Respuesta del socket en JSON: ", jsonResponse)
  }


  activateTimer() {
    if (this.currentBattle?.isAgainstBot == false && this.userBattle!!.isTheirTurn) {
      this.timeRemaining$ = timer(0, 1000).pipe(
        map(n => (this.seconds - n) * 1000),
        takeWhile(n => n >= 0),
      );
    }
  }

  private showAlert(title: string, message: string) {
    Swal.fire({
      title: title,
      text: message,
      showConfirmButton: false,
      timer: 2000
    })
  }


  // reproduce el mix en byte que le envia al jugar una carta
  async playAudio(positions: number[], spinTheWheel: boolean) {
    this.isProcessingAudio = true
    if (spinTheWheel) {
      console.log("Se ha girado la ruleta. El resultado ha sido: ", positions)
      for (let i = 0; i < positions.length; i++) {
        this.stopTrack(positions[i])
      }
      this.isProcessingAudio = false
      return;
    }

    this.position = positions[0]

    const slut = this.board.slots[this.position]
    if (slut?.card != null) {
      console.log("Borrando posición indicada: ", this.position)
      this.stopTrack(this.position)
    }

    this.isProcessingAudio = false
  }

  private async processAudio(audio: ArrayBuffer) {
    this.isProcessingAudio = true

    const audioBuffer = await this.audioContext.decodeAudioData(audio);
    const source = this.audioContext.createBufferSource()

    source.buffer = audioBuffer
    source.loop = true

    source.start(undefined, this.audioContext.currentTime)
    source.connect(this.audioContext.destination)

    this.activeSources[this.position.toString()] = source

    this.isProcessingAudio = false
  }

  private stopTrack(position: number) {
    console.log("Posición a borrar: ", position)
    //const source = this.activeSources.get(position)
    this.activeSources[position.toString()]?.stop()
    this.activeSources.delete(position.toString())
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
          resolve();
        } else {
          setTimeout(checkProcessing, 50); // Se llama a la verificación cada 50 milisegundos
        }
      };
      checkProcessing();
    });
  }

  selectCard(card: Card) {
    this.cardToUse = card
  }

  useCard(desiredPosition: number) {
    if (this.cardToUse && this.userBattle) {
      const cardToPlay: CardToPlay = {
        cardId: this.cardToUse.id,
        position: desiredPosition,
      }
      const action: Action = {
        card: cardToPlay,
        actionType: null
      }

      /*for(let i = 0; i < this.userBattle?.cards.length; i++)
      {
        if(this.userBattle.cards[i].id == this.cardToUse.id)
        {
          console.error("BORRAR EN ", i)
          this.indexToDelete = i
          break
        }
      }*/
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

  revenge() {
    sessionStorage.setItem("revenge", "true")
    sessionStorage.setItem("otherUserId", this.otherUserId.toString())
    this.navigateToUrl("matchmaking")
  }


  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendNative(messageType.toString())
  }

  // Envía la acción del usuario al servidor
  sendAction(action: Action) {
    const data = {
      "action": action,
      "messageType": MessageType.PlayCard
    }
    const message = JSON.stringify(data)
    this.webSocketService.sendNative(message)
  }


  // DRAG AND DROP --------------------------------

  onDragStart(event: DragEvent, card: Card) {
    event.dataTransfer?.setData('text/plain', JSON.stringify(card));
    event.dataTransfer!.effectAllowed = "move";
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.dataTransfer!.dropEffect = "move";
  }


  onDragEnter(event: DragEvent) {
    event.preventDefault();
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
  }

  onDrop(event: DragEvent, slotIndex: number) {
    event.preventDefault();
    if (!this.cardToUse) return;

    // Verificar si se puede colocar la carta en ese slot
    if (this.userBattle?.isTheirTurn) {
      this.useCard(slotIndex); // Función que coloca la carta en el tablero
    }

  }

  shouldHighlightSlot(slotIndex: number, allowedTypes: string[]): boolean {
    if (!this.userBattle?.isTheirTurn || !this.cardToUse) {
      return false;
    }
    const slotCard = this.board?.slots?.[slotIndex]?.card;
    return (this.checkType(allowedTypes, this.cardToUse.track.part.name) &&
      (!slotCard || slotCard.level <= this.cardToUse.level));
  }

}
