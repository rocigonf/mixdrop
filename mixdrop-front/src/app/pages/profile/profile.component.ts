import { Component, OnDestroy, OnInit } from '@angular/core';
import { User } from '../../models/user';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserService } from '../../services/user.service';
import { environment } from '../../../environments/environment';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { PasswordValidatorService } from '../../services/password-validator.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { WebsocketService } from '../../services/websocket.service';
import { MessageType } from '../../models/message-type';
import { Friend } from '../../models/friend';
import { FriendshipService } from '../../services/friendship.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [NavbarComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit, OnDestroy {

  id: number = 0;
  userForm: FormGroup;
  passwordForm: FormGroup;
  isNewPasswordHidden = true // Mostrar div de cambiar contraseña

  user: User | null = null
  myUser: User | null = null;
  isItself = false
  routeParamMap$: Subscription | null = null;
  public readonly IMG_URL = environment.apiImg;

  image: File | null = null
  imagePreview!: string;

  isEditing = false; //modo edición
  deleteAvatar = false;

  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  friendsRaw: Friend[] = []
  acceptedFriends: Friend[] = []
  pendingFriends: Friend[] = []

  battlesPerPage = 3;
  currentPage = 1;
  totalBattles = 0;
  battlesPaginated: any[] = [];

  constructor(
    private activatedRoute: ActivatedRoute,
    private userService: UserService,
    private passwordValidator: PasswordValidatorService,
    private formBuild: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private webSocketService: WebsocketService,
    private friendshipService: FriendshipService
  ) {
    this.userForm = this.formBuild.group({
      nickname: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });

    this.passwordForm = this.formBuild.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    },
      { validators: this.passwordValidator.passwordMatchValidator });
  }

  async ngOnInit(): Promise<void> {

    if (!this.authService.isAuthenticated()) {
      this.router.navigateByUrl("login");
    } else {

      this.routeParamMap$ = this.activatedRoute.paramMap.subscribe(async paramMap => {
        this.id = paramMap.get('id') as unknown as number;
        await this.getUser()
        this.userForm.reset(this.user)
      })

      this.myUser = this.authService.getUser();
      this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
      this.askForInfo(MessageType.Friend)
    }
  }

  ngOnDestroy(): void {
      this.messageReceived$?.unsubscribe()
  }

  async getUser() {
    const result = await this.userService.getUserById(this.id)
    console.log("Resultado de pedir el perfil ", this.id, ": ", result)

    if (result != null) {
      this.user = result
      this.totalBattles = this.user!!.battles.length;
      this.paginateBattles();
      //console.error(this.user)

      // Pillo el id del JWT como en el ECommerce y si coincide con el usuario que he pedido, intenta acceder a sí mismo
      const jwt = this.userService.api.jwt
      if (jwt) {
        const id = JSON.parse(window.atob(jwt.split('.')[1])).nameid;
        if (id == this.user?.id) {
          this.isItself = true
        }
        //console.log(id)
      }
    }
  }

  onFileSelected(event: any) {
    const image = event.target.files[0] as File;
    this.image = image

    if (event.target.files.length > 0) {
      const reader = new FileReader();
      reader.onload = (event: any) => {
        this.imagePreview = event.target.result;
      }
      reader.readAsDataURL(event.target.files[0])
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
        this.askForInfo(MessageType.Friend)
        break
      case MessageType.AskForFriend:
        this.askForInfo(MessageType.Friend)
        break
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }

  askForInfo(messageType: MessageType) {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendNative(messageType.toString())
  }


  processFriends() {
    this.acceptedFriends = []
    console.log(this.friendsRaw)

    for (const friend of this.friendsRaw) {
      if (friend.accepted) {
        this.acceptedFriends.push(friend)
      }
      if (friend.accepted === false) {
        if (this.user?.id == friend.receiverUserId) {
          this.pendingFriends.push(friend)
        }
      }
    }
    console.log("amigos: ", this.acceptedFriends)
    console.log("solicitudes: ", this.pendingFriends)

  }

  async removeFriend(friend: Friend | undefined) {
    // En el servidor se llamaría a un método para borrar la amistad, ( wesoque ->) el cual llamaría al socket del otro usuario para notificarle
    // Para recibir la notificación ya se encarga "processMesage", y de actualizar la lista

    if (friend != undefined) {
      const nickname = friend.receiverUser?.nickname || friend.senderUser?.nickname;

      const confirmed = window.confirm(`¿Seguro que quieres dejar de ser amigo de ${nickname}?`);

      if (confirmed) {
        await this.friendshipService.removeFriendById(friend.id)
        alert(`Has dejado de ser amigo de ${nickname}.`);
      }
    }

  }

  async addFriend(user: User) {
    // Hago una petición para que cree el amigo, ( wesoque ->) y en back el servidor debería notificar a ambos usuarios enviando la lista de amigos
    const response = await this.friendshipService.addFriend(user)
    console.log("Respuesta de agregar al amigo: ", response)
  }

  // comprueba si se le ha enviado una solicitud de amistad y esta en espera
  waitingFriendship(user: User): boolean {
    const amistad = this.hasFriendship(user)
    
    if (amistad) {
      return !amistad.accepted
    } else return false
  }


  // TODO: Agregar verificación
  async updateUser(): Promise<void> {
    const role = this.user?.role.toString();
    const formData = new FormData();
    formData.append("Nickname", this.userForm.value.nickname)

    if (this.image) {
      formData.append("Image", this.image, this.image.name)
      formData.append("ChangeImage", "true")
    }
    else if (this.deleteAvatar) {
      formData.append("ChangeImage", "true")
    }

    formData.append("Email", this.userForm.value.email)
    formData.append("Password", this.passwordForm.get('newPassword')?.value)

    if (role) formData.append("Role", role)

    const result = await this.userService.updateUser(formData, this.id)
    console.error(result)
    this.authService.saveUser(result.data)
    window.location.reload()
  }

  edit() {
    this.isEditing = !this.isEditing;
    if (!this.isEditing) { // restaura los datos
      this.userForm.reset(this.user);
    }
  }

  editPassword() {
    const newPassword = this.passwordForm.get('newPassword')?.value;

    if (!newPassword) {
      console.error("Error: El campo de la contraseña está vacío.");
      return;
    }

    this.showEditPassword()
    /*if (this.passwordForm.valid) {
      
    } else{
      console.error("Error: El formulario de contraseña no es válido.")
    }*/
  }

  showEditPassword() {
    let element = document.getElementById("newPassword");
    let hidden = element?.getAttribute("hidden");

    if (hidden) {
      element?.removeAttribute("hidden");
    } else {
      element?.setAttribute("hidden", "hidden");
    }
  }

  // envia cambios para mofidicar el usuario
  onSubmit(): void {
    if (this.userForm.valid) {
      this.isEditing = false
      // ACTUALIZAR

    } else {
      alert("Los datos introducidos no son válidos")
    }
  }


  // comprueba q el usuatio ya tiene amistad (aceptada o no) con otro usuario
  hasFriendship(user: User): Friend | undefined {
    return this.friendsRaw.find(friend =>
      (friend.senderUserId === user.id && friend.receiverUserId === this.myUser?.id) ||
      (friend.receiverUserId === user.id && friend.senderUserId === this.myUser?.id)
    );
  }

  getDiffTime(begin: any, end: any): string {
    const startDate = new Date(begin);
    const endDate = new Date(end);
    const time = endDate.getTime() - startDate.getTime();
  
    const hours = Math.floor(time / (1000 * 3600));
    const minutes = Math.floor((time % (1000 * 3600)) / (1000 * 60));
  
    return `${hours} horas ${minutes} minutos`;
  }

  paginateBattles() {
    const startIndex = (this.currentPage - 1) * this.battlesPerPage;
    const endIndex = startIndex + this.battlesPerPage;
    this.battlesPaginated = this.user?.battles.slice(startIndex, endIndex) || [];
  }

  nextPage() {
    if (this.currentPage * this.battlesPerPage < this.totalBattles) {
      this.currentPage++;
      this.paginateBattles();
    }
  }
  
  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.paginateBattles();
    }
  }

}
