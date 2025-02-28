import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
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
import {MatTooltipModule} from '@angular/material/tooltip';
import Swal, { SweetAlertIcon } from 'sweetalert2';


@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [NavbarComponent, FormsModule, MatTooltipModule],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent implements OnInit, OnDestroy {
  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  user: User | null = null;

  totalUsers = 0;
  totalPlayers = 0;
  totalBattles = 0;

  friendsRaw: Friend[] = []
  acceptedFriends: Friend[] = []
  pendingFriends: Friend[] = []

  pendingBattles: Battle[] = []

  searchedUsers!: User[];
  searchedFriends: Friend[] = [];
  queryuser: string = '';
  queryfriend: string = '';

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

    if(!this.authService.isAuthenticated()){
      this.navigateToUrl("login");
    } else {
      // Procesa la respuesta
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))

    this.user = this.authService.getUser();

    this.askForInfo(MessageType.Stats)
    }
  }

  processMessage(message: any) {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    // En función del tipo de mensaje que he recibido, sé que me han enviado unos datos u otros

    // Es posible que haya que hacer JSON.parse() otra vez en alguno de los casos
    switch (jsonResponse.messageType) {
      case MessageType.Friend:
        // Es posible que haya que hacer JSON.parse() otra vez
        this.friendsRaw = jsonResponse.friends
        this.processFriends()
        break
      case MessageType.Stats:
        console.log("recibidas estadísticas")
        this.totalUsers = jsonResponse.total | 0
        this.totalPlayers = jsonResponse.totalPlayers | 0
        this.totalBattles = jsonResponse.totalBattles | 0

        // Después de recibir las estadísticas, pido todo lo demás
        this.askForInfo(MessageType.Friend)
        this.askForInfo(MessageType.PendingBattle)
    
        break
      case MessageType.AskForFriend:
        this.askForInfo(MessageType.Friend)
        break
      case MessageType.AskForBattle:
        this.askForInfo(MessageType.PendingBattle)
        break
      case MessageType.PendingBattle:
        this.pendingBattles = jsonResponse.battles
        break
      case MessageType.Play:
        this.showAlert("Partida encontrada", "Partida encontrada :3", 'info')
        this.router.navigateByUrl("matchmaking")
        if(jsonResponse.battle)
        {
          sessionStorage.setItem("battle", JSON.stringify(jsonResponse.battle))
        }
        else
        {
          sessionStorage.setItem("battle", "null")
        }
        break
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

    this.searchFriend("")
  }

  async removeFriend(friend: Friend, accepted: boolean) {
    // En el servidor se llamaría a un método para borrar la amistad, ( wesoque ->) el cual llamaría al socket del otro usuario para notificarle
    // Para recibir la notificación ya se encarga "processMesage", y de actualizar la lista

    const nickname = friend.receiverUser?.nickname || friend.senderUser?.nickname;

    let confirmed

    if(accepted)
    {
      confirmed = window.confirm(`¿Seguro que quieres dejar de ser amigo de ${nickname}?`);
    }
    else
    {
      confirmed = window.confirm(`¿Seguro que quieres rechazar la solicitud de ser amigo de ${nickname}?`);
    }
  
    if (confirmed) {
      await this.friendshipService.removeFriendById(friend.id)
      this.showAlert("Éxito", `Amistad con ${nickname} rechazada.`, 'info')
    } 
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

  async acceptBattle(battle: Battle) {
    sessionStorage.setItem("battleId", battle.id.toString())
    const response = await this.battleService.acceptBattleById(battle.id)
    console.log("Respuesta de aceptar la batalla: ", response)
  }

  async deleteBattle(battle : Battle)
  {
    const response = await this.battleService.removeBattleById(battle.id, false)
    console.log("Respuesta de borrar la batalla: ", response)
  }

  async createBattle(user : User | null)
  {
    if(user == null) { return }
    const response = await this.battleService.createBattle(user.id, false) // En esta vista siempre será no random
    console.log("Respuesta de borrar la batalla: ", response)
    this.askForInfo(MessageType.PendingBattle)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendNative(messageType.toString())
  }

  ngOnDestroy(): void {
    this.messageReceived$?.unsubscribe();
  }

  navigateToUrl(url: string)
  {
    this.router.navigateByUrl(url);
  }

  visitUserProfile(user: User | null)
  {
    if(user){
      this.router.navigateByUrl("profile/" +user?.id  );
    }
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
    // aqui tambien se pueden guardar los usuarios USER de los amigos 
    this.searchedFriends = encontrados;
  }

  // comprueba q el usuatio ya tiene amistad (aceptada o no) con otro usuario
  hasFriendship(user: User): boolean {
    return this.friendsRaw.some(friend =>
      (friend.senderUserId === user.id && friend.receiverUserId === this.user?.id) || 
      (friend.receiverUserId === user.id && friend.senderUserId === this.user?.id)
    );
  }

  // comprueba si se le ha enviado una solicitud de amistad y esta en espera
  waitingFriendship(user: User): boolean {
    const amistad : Friend | undefined= this.friendsRaw.find(friend =>
      (friend.senderUserId === user.id && friend.receiverUserId === this.user?.id) || 
      (friend.receiverUserId === user.id && friend.senderUserId === this.user?.id)
    )
    if(amistad) {
      return !amistad.accepted
    } else return false
  }


    // comprueba q el usuario ya tiene una solicitud de batalla pendiente con otro
    hasBattle(user: User | null): boolean {
      const has : boolean =  this.pendingBattles.some(battle =>
        (battle.user.id === user?.id) || (battle.user.id === this.user?.id)
        || (battle.battleUsers[0].id === this.user?.id) || (battle.battleUsers[1].id === this.user?.id)
      );
      return has;
    }

  // quita tildes y pone minuscula
  removeAccents(str: string): string {
    return str.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
  }

  emparejar() {
    // te lleva al emparejamiento
    this.router.navigateByUrl("matchmaking");
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
