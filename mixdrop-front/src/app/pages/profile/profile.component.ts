import { Component, OnInit } from '@angular/core';
import { User } from '../../models/user';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  constructor(private activatedRoute: ActivatedRoute, private userService: UserService) { }

  user: User | null = null
  isItself = false
  routeParamMap$: Subscription | null = null;

  async ngOnInit(): Promise<void> {
    this.routeParamMap$ = this.activatedRoute.paramMap.subscribe(async paramMap => {
      const id = paramMap.get('id') as unknown as number;
      const result = await this.userService.getUserById(id)
      console.log("Resultado de pedir el perfil ", id, ": ", result)

      if (result != null) {
        this.user = result
        // TODO: Verificar si se trata del mismo usuario que ha iniciado sesión
      }
    });
  }
}
