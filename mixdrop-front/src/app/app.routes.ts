import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import {LoginComponent } from './pages/login/login.component'
import { MenuComponent } from './pages/menu/menu.component';
import { ProfileComponent } from './pages/profile/profile.component';
import { GameComponent } from './pages/game/game.component';
import { MatchmakingComponent } from './pages/matchmaking/matchmaking.component';
import { AdminComponent } from './pages/admin/admin.component';
import { RankingComponent } from './pages/ranking/ranking.component';

export const routes: Routes = [
    {path: "", component: HomeComponent},
    { path: 'login', component: LoginComponent },
    { path: 'menu', component: MenuComponent },
    { path: 'profile/:id', component: ProfileComponent},
    { path: 'admin/:id', component: AdminComponent},
    { path: 'game', component: GameComponent},
    { path: 'matchmaking', component: MatchmakingComponent },
    { path: 'ranking', component: RankingComponent }
];
