import { Component, Input, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { User } from '../../models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit{
  @Input() frameName = "/images/speaker/speaker1.png"

  constructor(public authService: AuthService, public router: Router){}

  user: User | null = null;

  public readonly IMG_URL = environment.apiImg;

  ngOnInit() {
    // usuario logueado
    this.user = this.authService.getUser();
  }

  authClick() {
    // Cerrar sesión
    if (this.authService.isAuthenticated()) {

      this.authService.logout()
      // Iniciar sesión
    } else {
      this.router.navigate(['/login']);
    }
  }
}
