import { Component, OnDestroy, OnInit } from '@angular/core';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MessageType } from '../../models/message-type';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { Battle } from '../../models/battle';
import { BattleService } from '../../services/battle.service';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { FriendshipService } from '../../services/friendship.service';
import { Friend } from '../../models/friend';


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

  user: User | null = null;

  totalPlayers = 0;

  friendsRaw: Friend[] = []
  acceptedFriends: Friend[] = []
  pendingFriends: Friend[] = []

  pendingBattles: Battle[] = []
  battleId: number = 0

  searchedUsers!: User[];
  searchedFriends: User[] = [];
  queryuser: string = '';
  queryfriend: string = '';
  askedForFriend: boolean = false


  menuSelector: string = 'myFriends';  // myFriends, searchUsers, friendRequest, battleRequest


  public readonly IMG_URL = environment.apiImg;

  constructor(private webSocketService: WebsocketService,
    private router: Router, private userService: UserService,
    private battleService: BattleService,
    public authService: AuthService,
    private friendshipService: FriendshipService,
  ) { }

  // TODO: Redirigir al login si no ha iniciado sesión
  ngOnInit(): void {
    // Procesa la respuesta
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))

    this.user = this.authService.getUser();

    this.askForInfo(MessageType.Stats)

    
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    // En función del tipo de mensaje que he recibido, sé que me han enviado unos datos u otros

    // Es posible que haya que hacer JSON.parse() otra vez en alguno de los casos
    switch (jsonResponse.messageType) {
      case MessageType.Friend:
        // Es posible que haya que hacer JSON.parse() otra vez
        this.askedForFriend = true
        this.friendsRaw = jsonResponse.friends
        this.processFriends()
        break
      case MessageType.Stats:
        this.totalPlayers = jsonResponse.total
        break
      case MessageType.AskForFriend:
        this.askForInfo(MessageType.Friend)
        break
      case MessageType.Play:
        // TODO: Redirigir a la vista (por ruta se pasa el id de la batalla)
        alert("Partida encontrada :3")
        this.battleId = jsonResponse.battleId
        break
    }
    if (!this.askedForFriend) {
      this.askForInfo(MessageType.Friend)
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  processFriends() {
    this.acceptedFriends = []
    this.pendingFriends = []
    console.log(this.friendsRaw)
    for(const friend of this.friendsRaw)
    {
      if(friend.accepted)
      {
        this.acceptedFriends.push(friend)
      }
      if(friend.accepted === false)
      {

        if(this.user?.id == friend.receiverUserId)
        {

          this.pendingFriends.push(friend)
        }
      }
    }
    console.log("amigos: ", this.acceptedFriends)
    console.log("solicitudes: ", this.pendingFriends)
  }

  async removeFriend(friend: Friend) {
    // En el servidor se llamaría a un método para borrar la amistad, ( wesoque ->) el cual llamaría al socket del otro usuario para notificarle
    // Para recibir la notificación ya se encarga "processMesage", y de actualizar la lista
    await this.friendshipService.removeFriendById(friend.id)
  }

  async addFriend(user: User) {
    // Hago una petición para que cree el amigo, ( wesoque ->) y en back el servidor debería notificar a ambos usuarios enviando la lista de amigos
    const response = await this.friendshipService.addFriend(user)
    console.log("Respuesta de agregar al amigo: ", response)
  }

  async acceptFriendship(id: number) {
    const response = await this.friendshipService.acceptFriendship(id)
    console.log("Respuesta de aceptar al amigo: ", response)
  }

  async modifyBattle(battle: Battle) {
    // Aquí actualizaría el estado de la batalla con una petición, ( wesoque ->) que notificaría a todos los usuarios y los llevaría a ambos a la vista de batalla si se acepta
    // Si se rechaza, se borra de la BBDD
    const response = await this.battleService.modifyBattle(battle)
    console.log("Respuesta de aceptar la batalla: ", response)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }

  ngOnDestroy(): void {
    this.messageReceived$?.unsubscribe();
  }

  async getSearchedUsers(queryuser: string): Promise<User[]> {
    const result = await this.userService.searchUser(queryuser);
    console.log(result)

    this.searchedUsers = result;

    return result;
  }

  searchFriend(queryfriend: string): void {

    const query = this.removeAccents(queryfriend)

    let encontrados: Friend[] = [];
    const misAmigos: User[] = []

    this.acceptedFriends.forEach(friendship => {

      // si el receiver es nulo, busco entre los sender
      if (friendship.senderUser) {

        encontrados = (this.acceptedFriends.filter(user => user.senderUser?.nickname.includes(query)))

        encontrados.forEach(amigo => {
          if (amigo.senderUser) {
            misAmigos.push(amigo.senderUser)
          }
        });

      } else {
        encontrados = this.acceptedFriends.filter(user => user.receiverUser?.nickname.includes(query))
        encontrados.forEach(amigo => {
          if (amigo.receiverUser) {
            misAmigos.push(amigo.receiverUser)
          }
        });
      }
    } 
  );

  console.log("que pasaaaaaaaa",this.acceptedFriends);

    this.searchedFriends = misAmigos;

  }

  // quita tildes y pone minuscula
  removeAccents(str: string): string {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
  }



  emparejar() {

    // se crea una partida

    // HAY QUE PASARLE EL ID DE LA PARTIDA CREADA A LA RUTA !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    this.router.navigateByUrl("matching");
  }

}
