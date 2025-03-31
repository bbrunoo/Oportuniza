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

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'cadastro', component: CadastroComponent },
  { path: 'login', component: LoginComponent },
  { path: 'termo', component: TermoComponent },
  { path: 'primeira-etapa', component: PrimeiraEtapaComponent },
  { path: 'segunda-etapa', component: SegundaEtapaComponent },
  { path: 'terceira-etapa', component: TerceiraEtapaComponent },
  { path: 'redefinir-senha', component: RedefinirSenhaComponent },
  { path: 'home', component: HomePageComponent },
];
