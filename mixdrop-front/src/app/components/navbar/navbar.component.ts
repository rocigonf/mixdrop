import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { User } from '../../models/user';
import { Router } from '@angular/router';
import { WebsocketService } from '../../services/websocket.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit{
  @Input() frameName = "/images/speaker/speaker1.png"
  @Output() musicEvent = new EventEmitter();

  constructor(public authService: AuthService, public router: Router, private webSocketService : WebsocketService){}

  user: User | null = null;

  public readonly IMG_URL = environment.apiImg;

  startMusic() {
    this.musicEvent.emit();
  }

  ngOnInit() {
    // usuario logueado
    this.user = this.authService.getUser();
  }

  authClick() {
    // Cerrar sesión
    if (this.authService.isAuthenticated()) {

      this.authService.logout()
      this.webSocketService.disconnectRxjs()
      this.navigateToUrl("/")
      alert("Sesion cerrada corectamente.")
      
      // Iniciar sesión
    } else {
      this.navigateToUrl("login");
    }
  }

  navigateToUrl(url: string)
  {
    this.router.navigateByUrl(url);
  }
}
