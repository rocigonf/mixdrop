import { Component, OnDestroy, OnInit } from '@angular/core';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MessageType } from '../../models/message-type';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { UserService } from '../../services/user.service';
import { UserFriend } from '../../models/user-friend';
import { User } from '../../models/user';
import { Battle } from '../../models/battle';
import { UserFriendService } from '../../services/user-friend.service';
import { BattleService } from '../../services/battle.service';

import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';


@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [NavbarComponent, FormsModule],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent implements OnInit, OnDestroy {
  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  totalPlayers = 0;
  friends: UserFriend[] = []
  pendingBattles: Battle[] = []
  battleId : number = 0

  searchedUsers!: User[];
  query: string = '';
  public readonly IMG_URL = environment.apiImg;
  
  constructor (private webSocketService : WebsocketService, 
    private router: Router, private userService: UserService, 
    private userFriendService : UserFriendService,
    private battleService : BattleService
  ){}

  // TODO: Redirigir al login si no ha iniciado sesión
  ngOnInit(): void 
  {
    // Procesa la respuesta
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
    this.askForInfo(1)
  }

  processMessage(message : any)
  {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)
    // En función del tipo de mensaje que he recibido, sé que me han enviado unos datos u otros
    
    // Es posible que haya que hacer JSON.parse() otra vez en alguno de los casos
    switch(jsonResponse.messageType)
    {
      case MessageType.Friend:
        // Es posible que haya que hacer JSON.parse() otra vez
        this.friends = jsonResponse.friends
        break
      case MessageType.Stats:
        this.totalPlayers = jsonResponse.total
        break
      case MessageType.PendingBattle:
        this.pendingBattles = jsonResponse.pendingBattles
        break
      case MessageType.Play:
        // TODO: Redirigir a la vista (por ruta se pasa el id de la batalla)
        alert("*voz de narrador del valorant*")
        this.battleId = jsonResponse.battleId
        break
    }
    console.log(jsonResponse)
  }

  async removeFriend(userFriend : UserFriend)
  {
    // En el servidor se llamaría a un método para borrar la amistad, el cual llamaría al socket del otro usuario para notificarle
    // Para recibir la notificación ya se encarga "processMesage", y de actualizar la lista
    await this.userFriendService.removeFriendById(userFriend.id)
  }

  async addFriend(user : User)
  {
    // Hago una petición para que cree el amigo, y el back el servidor debería notificar a ambos usuarios enviando la lista de amigos
    const response = await this.userFriendService.addFriend(user)
    console.log("Respuesta de agregar al amigo: ", response)
  }

  async acceptBattle(battle: Battle)
  {
    // Aquí actualizaría el estado de la batalla con una petición, que notificaría a todos los usuarios y los llevaría a ambos a la vista de batalla
    battle.accepted = true
    const response = await this.battleService.acceptBattle(battle)
    console.log("Respuesta de aceptar la batalla: ", response)
  }

  askForInfo(messageType : MessageType)
  {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }

  ngOnDestroy(): void {
    this.messageReceived$?.unsubscribe();
    this.webSocketService.disconnectRxjs();
  }


  async getSearchedUsers(query: string) : Promise<User[]> {
    const result = await this.userService.searchUser(query);
    console.log(result)
    this.searchedUsers = result;
    return result;
  }

}
