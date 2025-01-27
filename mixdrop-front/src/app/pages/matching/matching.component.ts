import { Component } from '@angular/core';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user';
import { BattleService } from '../../services/battle.service';

@Component({
  selector: 'app-matching',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './matching.component.html',
  styleUrl: './matching.component.css'
})
export class MatchingComponent {

  user: User | null = null;
  public readonly IMG_URL = environment.apiImg;

  constructor (private webSocketService : WebsocketService, 
    private router: Router, 
    private battleService: BattleService, 
    public authService: AuthService,
  ){}

  ngOnInit(): void 
  {
    this.user = this.authService.getUser();
  }

  async createRandomBattle(){
    const response = await this.battleService.randomBattle();
    console.log("Respuesta de batalla aleatoria: ", response);
  }
}
