import { CadastroComponent } from './pages/cadastro/cadastro.component';
import { Routes } from '@angular/router';
import { LandingPageComponent } from './pages/landing-page/landing-page.component';
import { LoginComponent } from './pages/login/login.component';
import { TermoComponent } from './pages/termo/termo.component';
import { PrimeiraEtapaComponent } from './pages/primeira-etapa/primeira-etapa.component';
import { SegundaEtapaComponent } from './pages/segunda-etapa/segunda-etapa.component';
import { TerceiraEtapaComponent } from './pages/terceira-etapa/terceira-etapa.component';
import { RedefinirSenhaComponent } from './pages/redefinir-senha/redefinir-senha.component';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { authGuard } from './guards/auth.guard';
import { authRedirectGuard } from './guards/auth-redirect.guard';
import { PerfilComponent } from './pages/perfil/perfil.component';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'cadastro', component: CadastroComponent,canActivate: [authRedirectGuard] },
  { path: 'login', component: LoginComponent, canActivate: [authRedirectGuard] },
  { path: 'termo', component: TermoComponent, canActivate: [authRedirectGuard] },
  { path: 'primeira-etapa', component: PrimeiraEtapaComponent, canActivate: [authGuard] },
  { path: 'segunda-etapa', component: SegundaEtapaComponent, canActivate: [authGuard]  },
  { path: 'terceira-etapa', component: TerceiraEtapaComponent, canActivate: [authGuard]  },
  { path: 'redefinir-senha', component: RedefinirSenhaComponent },
  { path: 'home', component: HomePageComponent, canActivate: [authGuard] },
  { path: 'perfil', component: PerfilComponent, canActivate: [authGuard] },
];
