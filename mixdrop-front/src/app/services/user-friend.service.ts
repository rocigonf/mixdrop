import { Injectable } from '@angular/core';
import { User } from '../models/user';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class UserFriendService {

  constructor(private api: ApiService) { }

  async addFriend(user : User): Promise<any> {
    const result = await this.api.post("UserFriend", user)
    return result
  }

  async removeFriendById(id : number): Promise<any> {
    await this.api.delete("UserFriend", id)
  }
}
