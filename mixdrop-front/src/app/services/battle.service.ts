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

  async randomBattle()
  {
    const result = await this.api.post("Battle/Matchmaking")
    return result
  }
}
