import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { BattleService } from '../../services/battle.service';
import { Subscription } from 'rxjs';
import { MessageType } from '../../models/message-type';
import { Friend } from '../../models/friend';
import { Battle } from '../../models/battle';

@Component({
  selector: 'app-matchmaking',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './matchmaking.component.html',
  styleUrl: './matchmaking.component.css'
})
export class MatchmakingComponent implements OnInit {

  user: User | null = null;
  public readonly IMG_URL = environment.apiImg;

  constructor(private webSocketService: WebsocketService,
    private router: Router,
    private userService: UserService,
    public authService: AuthService,
    public battleService: BattleService
  ) { }


  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  friendsRaw: Friend[] = []
  myFriends: Friend[] = []
  conenctedFriends: User[] = []

  pendingBattles: Battle[] = []

  ngOnInit(): void {
    this.user = this.authService.getUser();

    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))

    // pide info de amigos 
    this.askForInfo(MessageType.Friend)
    this.askForInfo(MessageType.PendingBattle)
    this.processFriends()
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    switch (jsonResponse.messageType) {
      case MessageType.Play:
        // TODO: Redirigir a la vista (por ruta se pasa el id de la batalla)
        alert("Partida encontrada :3")
        this.router.navigateByUrl("game") // Redirigir a la vista juego
        break
      case MessageType.Friend:
        this.friendsRaw = jsonResponse.friends
        this.processFriends()
        break
      case MessageType.PendingBattle:
        this.pendingBattles = jsonResponse.battles
        break
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }


  gameWithBot() {
    // el user 2 es nulo y el false de que no es random
    this.battleService.createBattle(null, false)
  }

  gameWithFriend(friend: User) {
    this.battleService.createBattle(friend, false)
  }

  gameRandom() {
    this.battleService.randomBattle()
    console.log("mortadela");
  }

  // comprueba q el usuario ya tiene una solicitud de batalla pendiente con otro
  hasBattle(user: User | null): boolean {
    const has: boolean = this.pendingBattles.some(battle =>
      (battle.user.id === user?.id) || (battle.user.id === this.user?.id)
      || (battle.battleUsers[0].id === this.user?.id)  || (battle.battleUsers[1].id === this.user?.id) 
    );
    return has;
  }


  processFriends() {

    this.myFriends = []
    this.conenctedFriends = []

    console.log(this.friendsRaw)

    for (const friend of this.friendsRaw) {
      if (friend.accepted) {
        this.myFriends.push(friend)

        if (friend.senderUser?.stateId == 2 || friend.receiverUser?.stateId == 2) {

          if (friend.senderUser) this.conenctedFriends.push(friend.senderUser)
          else if (friend.receiverUser) this.conenctedFriends.push(friend.receiverUser)
        }
      }
    }
    console.log("amigos: ", this.myFriends)
  }
}
