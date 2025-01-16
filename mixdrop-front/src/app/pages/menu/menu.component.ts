import { Component, OnInit } from '@angular/core';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { MessageType } from '../../models/message-type';
import { NavbarComponent } from "../../components/navbar/navbar.component";

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent implements OnInit {
  messageReceived$: Subscription | null = null;
  serverResponse: string = '';
  
  constructor (private webSocketService : WebsocketService, private router: Router){}

  // TODO: Redirigir al login si no ha iniciado sesión
  ngOnInit(): void 
  {
    // Guarda la respuesta
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.serverResponse = message);
  }

  askForInfo(messageType : MessageType)
  {
    console.log("Mensaje pedido: ", messageType)
    this.webSocketService.sendRxjs(messageType.toString())
  }
}
