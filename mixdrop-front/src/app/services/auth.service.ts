import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { ApiService } from './api.service';
import { Result } from '../models/result';
import { LoginRequest } from '../models/loginRequest';
import { LoginResult } from '../models/loginResult';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly USER_KEY = 'user';
  private readonly TOKEN_KEY = 'jwtToken';

  constructor(private api: ApiService) {
    const token = localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
    if (token) {
      this.api.jwt = token;
    }
  }

  async signup(formData: any): Promise<Result<any>> { // Registro
    return this.api.post<any>('Auth/Signup', formData);
  }

  async login(authData: LoginRequest, rememberMe: boolean): Promise<Result<LoginResult>> { // Iniciar sesión
    const result = await this.api.post<LoginResult>('Auth/login', authData);
  
    if (result.success && result.data) {
      const { accessToken, user } = result.data; // guardo info de la respuesta AuthResponse
      this.api.jwt = accessToken;
  
      if (rememberMe) { // Si se pulsó el botón recuérdame
        localStorage.setItem(this.TOKEN_KEY, accessToken);
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
      } else {
        sessionStorage.setItem(this.TOKEN_KEY, accessToken);
        sessionStorage.setItem(this.USER_KEY, JSON.stringify(user));
      }
    }
  
    return result;
  }

  // Comprobar si el usuario esta logeado
  isAuthenticated(): boolean {
    const token = localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
    return !!token;
  }

  logout(): void { // Cerrar sesión
    sessionStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  getUser(): User { // Obtener datos del usuario
    const user = localStorage.getItem(this.USER_KEY) || sessionStorage.getItem(this.USER_KEY);
    return user ? JSON.parse(user) : null;
  }

  // comprueba si es admin
  isAdmin(): boolean {
    const user = this.getUser();
    if (user.role == "Admin") {
      return true
    } else {
      return false
    }
  }

  // actualiza los datos que el usuario ha modificado
  updateUserData(updatedUser: any) {
    const user = localStorage.getItem(this.USER_KEY) || sessionStorage.getItem(this.USER_KEY);
    
    const newUser = { ...JSON.parse(user ?? '{}'), ...updatedUser };

    // guarda el usuario actualizado en localStorage o sessionStorage
    if (localStorage.getItem(this.USER_KEY)) {
      localStorage.setItem(this.USER_KEY, JSON.stringify(newUser));
    } else {
      sessionStorage.setItem(this.USER_KEY, JSON.stringify(newUser));
    }

  }


}
