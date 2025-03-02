import { Component, OnInit } from '@angular/core';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';
import { NavbarComponent } from "../../components/navbar/navbar.component";

@Component({
  selector: 'app-ranking',
  standalone: true,
  imports: [NavbarComponent],
  templateUrl: './ranking.component.html',
  styleUrl: './ranking.component.css'
})
export class RankingComponent implements OnInit{
  users: User[] = []

  constructor(private userService: UserService)
  {}

  async ngOnInit() {
    this.users = await this.userService.getRanking()
  }
}
