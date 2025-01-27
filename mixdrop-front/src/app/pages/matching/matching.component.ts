import { Component } from '@angular/core';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { environment } from '../../../environments/environment';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { FriendshipService } from '../../services/friendship.service';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

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
    private userService: UserService, 
    public authService: AuthService,
  ){}

  ngOnInit(): void 
  {
    this.user = this.authService.getUser();
  }

  gameWithBot(){
    
  }

}
