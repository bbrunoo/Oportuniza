import { CadastroComponent } from './pages/Authentication/cadastro/cadastro.component';
import { Routes } from '@angular/router';
import { LandingPageComponent } from './pages/landing-page/landing-page.component';
import { TermoComponent } from './pages/termo/termo.component';
import { PrimeiraEtapaComponent } from './pages/etapas-perfil/primeira-etapa/primeira-etapa.component';
import { SegundaEtapaComponent } from './pages/etapas-perfil/segunda-etapa/segunda-etapa.component';
import { InitialLayoutComponent } from './pages/layout/initial-layout/initial-layout.component';
import { FeedComponent } from './pages/layout/feed/feed.component';
import { InteressadosComponent } from './pages/layout/interessados/interessados.component';
import { MeuperfilComponent } from './pages/layout/meuperfil/meuperfil.component';
import { PublicationComponent } from './pages/layout/publication/publication.component';
import { MeuspostsComponent } from './pages/layout/meusposts/meusposts.component';
import { CriarEmpresaComponent } from './pages/layout/criar-empresa/criar-empresa.component';
import { AuthTypeGuard } from './guards/auth-type.guard';
import { LoginComponent } from './pages/Authentication/login/login.component';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'termo', component: TermoComponent },
  { path: 'primeira-etapa', component: PrimeiraEtapaComponent },
  { path: 'segunda-etapa', component: SegundaEtapaComponent },
  { path: 'cadastro', component: CadastroComponent },
  { path: 'login', component: LoginComponent },
  {
    path: "inicio",
    component: InitialLayoutComponent,
    canActivate: [AuthTypeGuard],
    children: [
      { path: "", redirectTo: "feed", pathMatch: "full" },
      { path: 'criar-empresa', component: CriarEmpresaComponent },
      { path: "feed", component: FeedComponent },
      { path: "interessados", component: InteressadosComponent },
      { path: "perfil", component: MeuperfilComponent },
      { path: "post", component: PublicationComponent },
      { path: "meus-posts", component: MeuspostsComponent }
    ]
  }
];
