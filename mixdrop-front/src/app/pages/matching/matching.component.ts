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


  ngOnInit(): void {
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
    this.user = this.authService.getUser();
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

}
