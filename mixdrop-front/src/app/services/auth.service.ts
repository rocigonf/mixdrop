import { Injectable, OnDestroy } from '@angular/core';
import { ApiService } from './api.service';
import { Result } from '../models/result';
import { LoginRequest } from '../models/loginRequest';
import { LoginResult } from '../models/loginResult';
import { User } from '../models/user';
import { WebsocketService } from './websocket.service';


@Injectable({
  providedIn: 'root'
})
export class AuthService implements OnDestroy {

  private readonly USER_KEY = 'user';
  private readonly TOKEN_KEY = 'jwtToken';
  rememberMe : boolean = false


  constructor(private api: ApiService, private webSocket: WebsocketService) {
    const token = localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
    if (token) {
      this.api.jwt = token;
    }
  }

  async login(authData: LoginRequest, rememberMe: boolean): Promise<Result<LoginResult>> { // Iniciar sesión
    const result = await this.api.post<LoginResult>('Auth/login', authData);
    this.rememberMe = rememberMe

    if (result.success && result.data) {
      const { accessToken, user } = result.data; // guardo info de la respuesta AuthResponse
      this.api.jwt = accessToken;

      if (rememberMe) { // Si se pulsó el botón recuérdame
        localStorage.setItem(this.TOKEN_KEY, accessToken);
        this.saveUser(user)
      } else {
        sessionStorage.setItem(this.TOKEN_KEY, accessToken);
        this.saveUser(user)
      }
    }

    return result;
  }

  saveUser(user: User)
  {
    if(this.rememberMe)
    {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
    else
    {
      sessionStorage.setItem(this.USER_KEY, JSON.stringify(user));
    }
  }

  // Comprobar si el usuario esta logeado
  isAuthenticated(): boolean {
    const token = localStorage.getItem(this.TOKEN_KEY) || sessionStorage.getItem(this.TOKEN_KEY);
    return !!token;
  }


  // Cerrar sesión
  async logout(): Promise<void> {
    sessionStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.TOKEN_KEY);
    sessionStorage.removeItem(this.USER_KEY);
    localStorage.removeItem(this.USER_KEY);

    await this.disconnectUser()
  }

  async disconnectUser()
  {
    //const headers = this.api.getHeader();
    this.webSocket.disconnectNative()
    //return this.api.put(`Auth/disconnect`, { headers, responseType: 'text' })
  }


  getUser(): User | null { // Obtener datos del usuario
    const user = localStorage.getItem(this.USER_KEY) || sessionStorage.getItem(this.USER_KEY);
    if (user) {
      if (!this.webSocket.isConnectedNative()) {
        console.log("CONECTANDO...")
        this.webSocket.connectNative()
      }
      return JSON.parse(user)
    }
    return null
  }

  // comprueba si es admin
  isAdmin(): boolean {
    const user = this.getUser();
    if (user?.role == "Admin") {
      return true
    } else {
      return false
    }
  }

  // Registro
  async register(formData: FormData): Promise<Result<any>> {
    return this.api.postWithImage<any>('Auth/register', formData);
  }

  async ngOnDestroy(): Promise<void> {
    localStorage.setItem("MONDONGO", "true")
    await this.disconnectUser()
  }

}
