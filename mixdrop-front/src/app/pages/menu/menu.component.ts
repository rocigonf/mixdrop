import { Component, OnDestroy, OnInit } from '@angular/core';
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
export class MenuComponent implements OnInit, OnDestroy {
  messageReceived$: Subscription | null = null;
  serverResponse: string = '';
  totalPlayers = 0;
  
  constructor (private webSocketService : WebsocketService, private router: Router){}

  // TODO: Redirigir al login si no ha iniciado sesión
  ngOnInit(): void 
  {
    // Procesa la respuesta
    this.messageReceived$ = this.webSocketService.messageReceived.subscribe(message => this.processMessage(message))
  }

  processMessage(message : any)
  {
    this.serverResponse = message
    const jsonResponse = JSON.parse(this.serverResponse)
    // En función del tipo de mensaje que he recibido, sé que me han enviado unos datos u otros
    switch(jsonResponse.messageType)
    {
      case MessageType.Stats:
        this.totalPlayers = jsonResponse.total
        break
    }
    console.log(jsonResponse)
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
}
