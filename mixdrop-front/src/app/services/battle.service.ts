import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Battle } from '../models/battle';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class BattleService {

  constructor(private api: ApiService) { }

  async modifyBattle(battle : Battle)
  {
    const result = await this.api.put("Battle", battle)
    return result
  }

  
  async createBattle(user: User): Promise<any> {
    const result = await this.api.post("Battle", user)
    return result
  }

  async acceptBattleById(id: number): Promise<any> {
    const result = await this.api.put(`Battle/${id}`)
    return result
  }

  async removebattleById(id: number): Promise<any> {
    await this.api.delete(`Battle/${id}`)
  }


    
}
