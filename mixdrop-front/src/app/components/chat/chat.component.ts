import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { UserBattleDto } from '../../models/user-battle-dto';
import { FormsModule } from '@angular/forms';
import { Battle } from '../../models/battle';
import { WebsocketService } from '../../services/websocket.service';
import { Subscription } from 'rxjs';
import { MessageType } from '../../models/message-type';
import { User } from '../../models/user';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent {

  @Input() currentBattle: Battle | null = null
  @Input() userBattle: UserBattleDto | null = null

  @ViewChild('scrollMe') private scrollContainer!: ElementRef

  messageReceived$: Subscription | null = null;
  serverResponse: string = '';

  constructor(private webSocketService: WebsocketService, private authService: AuthService) {
  }

  enemyNickname: string | undefined = ""
  user: User | null = null;

  mensaje: string = "";
  // lista 
  mensajes: string[][] = []

  async ngOnInit(): Promise<void> {
    console.log("currentBattle en padre:", this.currentBattle);
    console.log("userBattle en padre:", this.userBattle);

    this.user = this.authService.getUser();

    const enemyUser = this.currentBattle?.battleUsers.find(u => u.userId != this.user?.id)
    this.enemyNickname = enemyUser?.user.nickname;

    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
  }

  chatVisible: boolean = true;

  envia() {
    this.mensaje.trim();

    if (this.mensaje === '') return false;

    console.log(this.mensaje)
    this.mensajes.push([this.mensaje, "User"]);

    this.sendMessage(this.mensaje);

    this.mensaje = '';

    setTimeout(() => this.scrollToBottom(), 0);
    return false;
  }

  sendMessage(mensaje: string) {
    const data = {
      "messageType": MessageType.Chat,
      "messageChat": mensaje
    }
    console.log("data", data)
    const message = JSON.stringify(data)
    this.webSocketService.sendNative(message)
  }

  private scrollToBottom() {
    try {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    } catch (err) {
      console.error('Error al hacer scroll:', err);
    }
  }

  processMessage(message: any) {
    if(message instanceof Blob)
    {
      return
    }
    
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)

    switch (jsonResponse.messageType) {
      case MessageType.Chat:
        var newMessage = jsonResponse.messageChat;
        var who = jsonResponse.who

        if (who == this.enemyNickname) {
          this.mensajes.push([newMessage, "Enemy"]);
        }
        break;
    }
    console.log("Respuesta del socket en JSON: ", jsonResponse)
  }



}
