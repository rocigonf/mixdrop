import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user';
import { BattleService } from '../../services/battle.service';
import { Subscription } from 'rxjs';
import { MessageType } from '../../models/message-type';
import { Friend } from '../../models/friend';
import { Battle } from '../../models/battle';
import Swal, { SweetAlertIcon } from 'sweetalert2';

@Component({
  selector: 'app-matchmaking',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './matchmaking.component.html',
  styleUrl: './matchmaking.component.css'
})
export class MatchmakingComponent implements OnInit, OnDestroy {

  user: User | null = null;
  public readonly IMG_URL = environment.apiImg;

  constructor(private webSocketService: WebsocketService,
    private router: Router,
    public authService: AuthService,
    public battleService: BattleService
  ) { }


  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  friendsRaw: Friend[] = []
  myFriends: Friend[] = []
  conenctedFriends: User[] = []

  pendingBattles: Battle[] = []

  readyForBattle = false
  battle: Battle | null = null
  battleId: number = 0

  loading: boolean = false

  async ngOnInit(): Promise<void> {

    if (!this.authService.isAuthenticated()) {
      this.router.navigateByUrl("login");
      return
    } else {

      this.user = this.authService.getUser();

      this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))

      const battle = sessionStorage.getItem("battle")
      if (battle) {
        this.readyForBattle = true
        if (battle != "null") {
          this.battle = JSON.parse(battle)
        }
      }
      this.battleId = parseInt(sessionStorage.getItem("battleId")!!)
    }

    if(sessionStorage.getItem("revenge") == "true")
    {
      const idRaw = sessionStorage.getItem("otherUserId")
      if(idRaw)
      {
        const id = parseInt(idRaw)
        await this.battleService.createBattle(id, false)
        sessionStorage.removeItem("revenge")
      }
    }

    // pide info de amigos 
    this.askForInfo(MessageType.Friend)
    this.askForInfo(MessageType.PendingBattle)
    this.processFriends()
  }

  async ngOnDestroy(): Promise<void> {
      this.messageReceived$?.unsubscribe()
      if((this.battle || this.battleId != 0) && this.readyForBattle)
      {
        await this.deleteBattleBydId(this.battleId, true)
        this.resetData()
      }
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    switch (jsonResponse.messageType) {
      case MessageType.Play:
        this.showAlert("Partida encontrada", "Partida encontrada :3", 'info')
        this.readyForBattle = true
        if (jsonResponse.battle) {
          this.battle = jsonResponse.battle
        }
        break
      case MessageType.Friend:
        this.friendsRaw = jsonResponse.friends
        this.processFriends()
        break
      case MessageType.PendingBattle:
        this.pendingBattles = jsonResponse.battles
        break
      case MessageType.AskForFriend:
        this.askForInfo(MessageType.Friend)
        break
      case MessageType.DisconnectedFromBattle:
        alert("La batalla se ha cancelado")
        this.resetData()
        //this.router.navigateByUrl("menu")
        //window.location.reload()
        break
      case MessageType.StartBattle:
        this.resetData()
        this.router.navigateByUrl("game")
        break
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  resetData()
  {
    this.battle = null
    this.readyForBattle = false
    this.loading = false
    this.battleId = 0
    sessionStorage.removeItem("battle")
    sessionStorage.removeItem("battleId")
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendNative(messageType.toString())
  }

  async startBattle() {
    if (!this.battle) return
    await this.battleService.startBattle(this.battle.id)
  }


  async gameWithBot() {
    // el user 2 es nulo y el false de que no es random
    await this.battleService.createBattle(0, false)
  }

  async gameWithFriend(friend: User) {
    await this.battleService.createBattle(friend.id, false)
  }

  async gameRandom() {
    await this.battleService.randomBattle()
    console.log("mortadela");
    this.loading = true
  }

  async deleteRandomBattle() {
    await this.battleService.deleteFromQueue()
    this.loading = false
  }

  // comprueba q el usuario ya tiene una solicitud de batalla pendiente con otro
  hasBattle(user: User | null): boolean {
    const has: boolean = this.pendingBattles.some(battle =>
      (battle.user.id === user?.id) || (battle.user.id === this.user?.id)
      || (battle.battleUsers[0].id === this.user?.id) || (battle.battleUsers[1].id === this.user?.id)
    );
    return has;
  }

  async acceptBattle(friendId: number) {

    const pendingBattle: Battle | undefined = this.pendingBattles.find(battle =>
      (battle.user.id === friendId) &&
      (battle.battleUsers[0].id === this.user?.id) || (battle.battleUsers[1].id === this.user?.id)
    )

    if (pendingBattle != undefined) {
      const response = await this.battleService.acceptBattleById(pendingBattle.id)
      console.log("Respuesta de aceptar la batalla: ", response)
    } else console.log("no se encuentra la batalla")
  }

  async deleteBattleByFriendId(friendId: number) {
    const pendingBattle: Battle | undefined = this.pendingBattles.find(battle =>
      (battle.user.id === friendId) &&
      (battle.battleUsers[0].id === this.user?.id) || (battle.battleUsers[1].id === this.user?.id)
    )

    if (pendingBattle != undefined) {
      await this.deleteBattleBydId(pendingBattle.id, false) // Si es byFriend no tengo que notificar
    } else console.log("no se encuentra la batalla")
  }

  async deleteBattleBydId(battleId: number, notify: boolean) {
      const response = await this.battleService.removeBattleById(battleId, notify)
      console.log("Respuesta de borrar la batalla: ", response)
  }

  processFriends() {

    this.myFriends = []
    this.conenctedFriends = []

    for (const friend of this.friendsRaw) {
      if (friend.accepted) {
        this.myFriends.push(friend)

        if (friend.senderUser?.stateId == 2 || friend.receiverUser?.stateId == 2) {

          if (friend.senderUser) this.conenctedFriends.push(friend.senderUser)
          else if (friend.receiverUser) this.conenctedFriends.push(friend.receiverUser)
        }
      }
    }
  }

  private showAlert(title: string, message: string, icon: SweetAlertIcon) {
        Swal.fire({
          title: title,
          text: message,
          showConfirmButton: false,
          icon: icon,
          timer: 2000
        })
      }
}
