import { Injectable } from '@angular/core';
import { User } from '../models/user';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class FriendshipService {

  constructor(private api: ApiService) { }

  async addFriend(user: User): Promise<any> {
    const result = await this.api.post("Friendship", user)
    return result
  }

  async acceptFriendship(id: number): Promise<any> {
    const result = await this.api.put(`Friendship/${id}`)
    return result
  }

  async removeFriendById(id: number): Promise<any> {
    await this.api.delete(`Friendship/${id}`)
  }
}
