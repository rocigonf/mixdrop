import { Component, OnInit } from '@angular/core';
import { WebsocketService } from '../../services/websocket.service';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})

export class MenuComponent implements OnInit {

  constructor (private webSocketService : WebsocketService, 
    private router: Router,
    private userService: UserService
  ){}

  users: User[] = [];
  
  async ngOnInit(): Promise<void> {
  
    this.users = await this.getSearchedUsers("a");
    console.log(this.users)
  }

  async getSearchedUsers(query: string) : Promise<User[]> {
    const result = await this.userService.searchUser(query);
    return result;
  }
}
