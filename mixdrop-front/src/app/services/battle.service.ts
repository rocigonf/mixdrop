import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class BattleService {

  constructor(private api: ApiService) { }

  async modifyBattle(id : number)
  {
    const result = await this.api.put(`Battle/${id}`)
    return result
  }

  async randomBattle()
  {
    const result = await this.api.post("Battle/Matchmaking")
    return result
  }

  async deleteBotBattle()
  {
    return await this.api.delete("Battle/bot")
  }
  

  async createBattle(user: number | null, isRandom : boolean ) : Promise<any> {
    const body = {
      "User2Id" : user,
      "IsRandom" : isRandom
    }
    const result = await this.api.post("Battle", body)
    return result
  }

  async deleteFromQueue()
  {
    await this.api.delete("Battle/Matchmaking/delete")
  }

  async acceptBattleById(id: number): Promise<any> {
    const result = await this.api.put(`Battle/${id}`)
    return result
  }

  async removeBattleById(id: number): Promise<any> {
    await this.api.delete(`Battle/${id}`)
  }

  async startBattle(id : number): Promise<any>
  {
    const result = await this.api.put(`Battle/start/${id}`)
    return result
  }
}
