import { Injectable } from '@angular/core';
import { ApiService } from './api.service';


@Injectable({
  providedIn: 'root'
})

export class UserService {

  constructor(public api: ApiService) {}

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

  async updateUser(formData: FormData, id: number): Promise<any> {
    return this.api.putWithImage<any>(`User/${id}`, formData);
  }
}
