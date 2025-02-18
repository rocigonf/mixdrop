import { Component, OnInit } from '@angular/core';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { User } from '../../models/user';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';


@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [NavbarComponent, FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {

  public readonly IMG_URL = environment.apiImg;
  
  users: User[] = [] // lista de usuarios

  constructor(
    private userService: UserService, 
    private authService: AuthService, 
    private router: Router
  ){}

  async ngOnInit() {
    if (!this.authService.isAuthenticated() || !this.authService.isAdmin()) {
      this.router.navigate(['/']);
    }

    await this.loadUsers();
  }

  async loadUsers(){
    this.users = await this.userService.allUser()
  }

  // Editar el rol de un usuario
  async modifyUserRole(userId: number, newRole: string) {
    try {
      this.userService.modifyRole(userId, newRole)
      this.loadUsers()
    } catch (error) {
      console.error("Error al modificar el rol", error)
    }
    this.loadUsers()
  }

  // Banear a un usuario
  async banUser(userId: number) {
    try {
      this.userService.banUserAsync(userId)
      this.loadUsers()
    } catch (error) {
      console.error("Error al banear este usuario", error)
    }
    this.loadUsers()
  }
}