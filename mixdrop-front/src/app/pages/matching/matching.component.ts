import { Component } from '@angular/core';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { FriendshipService } from '../../services/friendship.service';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { BattleService } from '../../services/battle.service';
import { Friend } from '../../models/friend';
import { Subscription } from 'rxjs';
import { Battle } from '../../models/battle';
import { MessageType } from '../../models/message-type';

@Component({
  selector: 'app-matching',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './matching.component.html',
  styleUrl: './matching.component.css'
})
export class MatchingComponent {

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

  battleId: number = 0

  askedForFriend: boolean = false


  ngOnInit(): void {
    this.user = this.authService.getUser();

    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))

    // pide info de amigos 
    this.askForInfo(MessageType.Friend)
    this.processFriends()

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



  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    if (jsonResponse.messageType == MessageType.Friend) {
      // Es posible que haya que hacer JSON.parse() otra vez
      this.askedForFriend = true
      this.friendsRaw = jsonResponse.friends
      this.processFriends()
    }
    if (!this.askedForFriend) {
      this.askForInfo(MessageType.Friend)
    }


  }


  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
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
