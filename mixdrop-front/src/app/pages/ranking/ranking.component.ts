import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';
import { NavbarComponent } from "../../components/navbar/navbar.component";
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TranslocoModule } from '@jsverse/transloco';

@Component({
  selector: 'app-ranking',
  standalone: true,
  imports: [NavbarComponent, CommonModule, TranslocoModule],
  templateUrl: './ranking.component.html',
  styleUrl: './ranking.component.css'
})
export class RankingComponent implements OnInit {

  public readonly IMG_URL = environment.apiImg;

  users: User[] = []

  user: User | null = null;


  constructor(private userService: UserService,
    private router: Router, public authService: AuthService) { }

  async ngOnInit() {
    this.user = this.authService.getUser();
    console.log(this.user)
    this.users = await this.userService.getRanking()
  }

  visitUserProfile(user: User | null) {
    if (user && this.user != null) {
      this.router.navigateByUrl("profile/" + user?.id);
    }
  }
}