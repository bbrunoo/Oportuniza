import { ComofuncionaComponent } from './pages/landing-page/comofunciona/comofunciona.component';
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
import { LoginComponent } from './pages/Authentication/login/login.component';
import { LoadingComponent } from './extras/loading/loading.component';
import { MinhasEmpresasComponent } from './pages/layout/minhas-empresas/minhas-empresas.component';
import { InformacoesComponent } from './pages/empresa-management/informacoes/informacoes.component';
import { EditarEmpresaComponent } from './pages/empresa-management/editar-empresa/editar-empresa.component';
import { FuncionariosComponent } from './pages/empresa-management/funcionarios/funcionarios.component';
import { AdicionarFuncionarioComponent } from './pages/empresa-management/adicionar-funcionario/adicionar-funcionario.component';
import { EmpresaComponent } from './pages/empresa-management/empresa/empresa.component';
import { AuthTypeGuard } from './guards/auth-type.guard';
import { VerificationComponent } from './pages/Authentication/verification/verification.component';
import { SearchResultComponent } from './pages/layout/search-result/search-result.component';
import { EditarPostComponent } from './pages/layout/editar-post/editar-post.component';
import { SpecificPublicationComponent } from './pages/layout/specific-publication/specific-publication.component';
import { ContextSwitcherComponent } from './pages/Authentication/context-switcher/context-switcher.component';

export const routes: Routes = [
  { path: '', component: LandingPageComponent },
  { path: 'termo', component: TermoComponent },
  { path: 'primeira-etapa', component: PrimeiraEtapaComponent },
  { path: 'segunda-etapa', component: SegundaEtapaComponent },
  { path: 'cadastro', component: CadastroComponent },
  { path: 'login', component: LoginComponent },
  { path: 'verify/:email', component: VerificationComponent },
  { path: 'loading', component: LoadingComponent },
  { path: 'troca', component: ContextSwitcherComponent },
  {
    path: "inicio",
    component: InitialLayoutComponent,
    canActivate: [AuthTypeGuard],
    children: [
      { path: "", redirectTo: "feed", pathMatch: "full" },
      { path: 'minhas-empresas', component: MinhasEmpresasComponent },
      { path: 'criar-empresa', component: CriarEmpresaComponent },
      { path: "feed", component: FeedComponent },
      { path: "interessados", component: InteressadosComponent },
      { path: "perfil", component: MeuperfilComponent },
      { path: "post", component: PublicationComponent },
      { path: "editar-post/:id", component: EditarPostComponent },
      { path: "search-result", component: SearchResultComponent },
      { path: "meus-posts", component: MeuspostsComponent },
      { path: "post/:id", component: SpecificPublicationComponent }
    ]
  },
  {
    path: "empresa/:id", component: EmpresaComponent, children: [
      { path: "informacoes", component: InformacoesComponent },
      { path: "editar-empresa", component: EditarEmpresaComponent },
      { path: "funcionarios", component: FuncionariosComponent },
      { path: "adicionar-funcionario", component: AdicionarFuncionarioComponent }
    ]
  }
];
