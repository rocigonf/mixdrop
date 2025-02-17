import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { WebSocketSubject, webSocket } from 'rxjs/webSocket';
import { environment } from '../../environments/environment';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {

  constructor(private api : ApiService){}
  // Eventos
  connected = new Subject<void>();
  messageReceived = new Subject<any>();
  disconnected = new Subject<void>();

  private onConnected() {
    console.log('Socket connected');
    this.connected.next();
  }

  private onMessageReceived(message: any) {
    console.warn(message)
    this.messageReceived.next(message);
  }

  private onError(error: any) {
    console.error('Error:', error);
  }

  private onDisconnected() {
    console.log('WebSocket connection closed');
    this.disconnected.next();
  }

  // ========= Usando Websockets nativos =============

  private nativeSocket: WebSocket | null = null;

  isConnectedNative() {
    return this.nativeSocket
      && (this.nativeSocket.readyState == WebSocket.CONNECTING || this.nativeSocket.readyState == WebSocket.OPEN);
  }

  connectNative() {
    this.nativeSocket = new WebSocket(environment.socketUrl + `/${this.api.jwt}`);

    // Evento de apertura de conexión
    this.nativeSocket.onopen = _ => this.onConnected();

    // Evento de mensaje recibido
    this.nativeSocket.onmessage = (event) => this.onMessageReceived(event.data);

    // Evento de error generado
    this.nativeSocket.onerror = (event) => this.onError(event);

    // Evento de cierre de conexión
    this.nativeSocket.onclose = (event) => this.onDisconnected();
  }

  sendNative(message: string) {
    this.nativeSocket?.send(message);
  }

  disconnectNative() {
    this.nativeSocket?.close(1000, 'Closed by client');
    this.nativeSocket = null;
  }

}
