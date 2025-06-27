import { CadastroComponent } from './pages/cadastro/cadastro.component';
import { Routes } from '@angular/router';
import { LandingPageComponent } from './pages/landing-page/landing-page.component';
import { TermoComponent } from './pages/termo/termo.component';
import { PrimeiraEtapaComponent } from './pages/primeira-etapa/primeira-etapa.component';
import { SegundaEtapaComponent } from './pages/segunda-etapa/segunda-etapa.component';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { authRedirectGuard } from './guards/auth-redirect.guard';
import { InitialLayoutComponent } from './pages/layout/initial-layout/initial-layout.component';
import { FeedComponent } from './pages/layout/feed/feed.component';
import { InteressadosComponent } from './pages/layout/interessados/interessados.component';
import { MeuperfilComponent } from './pages/layout/meuperfil/meuperfil.component';
import { PublicationComponent } from './pages/layout/publication/publication.component';
import { MeuspostsComponent } from './pages/layout/meusposts/meusposts.component';
import { CriarEmpresaComponent } from './extras/criar-empresa/criar-empresa.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'termo', component: TermoComponent, canActivate: [authRedirectGuard] },
  { path: 'primeira-etapa', component: PrimeiraEtapaComponent, canActivate: [authGuard] },
  { path: 'segunda-etapa', component: SegundaEtapaComponent, canActivate: [authGuard] },
  { path: 'home', component: HomePageComponent},
  {
    path: "inicio", component: InitialLayoutComponent, children: [
      { path: 'criar-empresa', component: CriarEmpresaComponent },
      { path: "", redirectTo: "feed", pathMatch: "full" },
      { path: "feed", component: FeedComponent },
      { path: "interessados", component: InteressadosComponent },
      { path: "perfil", component: MeuperfilComponent },
      { path: "post", component: PublicationComponent },
      { path: "meus-posts", component: MeuspostsComponent }
    ]
  }
];
