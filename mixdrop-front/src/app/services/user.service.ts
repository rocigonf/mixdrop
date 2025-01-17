import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { User } from '../models/user';
import { Observable, catchError, forkJoin, lastValueFrom, map } from 'rxjs';


@Injectable({
  providedIn: 'root'
})

export class UserService {

  private readonly USER_KEY = 'user';
  private readonly TOKEN_KEY = 'jwtToken';


  constructor(private api: ApiService) {
  }


  // buscar usuario -- en proceso --
  async searchUser(search: string): Promise<any> {
    const result = await this.api.get(`User/search?query=${search}`);
    const users: any = result.data;

    return users.users;

  }

  async getUserById(id : number): Promise<any> {
    const result = await this.api.get(`User/${id}`);
    const user: any = result.data;
    return user
  }
}
