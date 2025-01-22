import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import {LoginComponent } from './pages/login/login.component'
import { MenuComponent } from './pages/menu/menu.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { GameComponent } from './pages/game/game.component';

export const routes: Routes = [
    {path: "", component: HomeComponent},
    { path: 'login', component: LoginComponent },
    { path: 'menu', component: MenuComponent },
    { path: 'profile/:id', component: ProfileComponent},
    { path: 'game/:id', component: GameComponent}
];
