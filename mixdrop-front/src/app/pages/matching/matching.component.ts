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
import { Battle } from '../../models/battle';
import { MessageType } from '../../models/message-type';
import { Friend } from '../../models/friend';

@Component({
  selector: 'app-matching',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './matching.component.html',
  styleUrl: './matching.component.css'
})
export class MatchingComponent implements OnInit {

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


  ngOnInit(): void {
    this.user = this.authService.getUser();

    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))

    // pide info de amigos 
    this.askForInfo(MessageType.Friend)
    this.processFriends()
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    switch (jsonResponse.messageType) {
      case MessageType.Play:
        // TODO: Redirigir a la vista (por ruta se pasa el id de la batalla)
        alert("Partida encontrada :3")
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
    this.battleService.createBattle(null, true)

  }



  processFriends() {
    this.myFriends = []
    for (const friend of this.friendsRaw) {
      if (friend.accepted) {
        this.myFriends.push(friend)

        if (friend.receiverUser?.stateId == 2 || friend.senderUser?.stateId == 2) {

          // esto coge 2 veces a cada usuario nose pq
          if (friend.receiverUser !== null) {
            this.conenctedFriends.push(friend.receiverUser);
          }
          if (friend.senderUser !== null){
            this.conenctedFriends.push(friend.senderUser);
        }

        console.log(" amigos conectados : ", this.conenctedFriends)
      }
    }
  }
    console.log("amigos: ", this.myFriends)
}
}
