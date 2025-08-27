import { Component } from '@angular/core';
import { CandidateService } from '../../../services/candidate.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

export interface CandidatoDto {
  id: string;
  publicationId: string;
  publicationTitle: string;
  userId: string;
  userName: string;
  userIdKeycloak: string;
  applicationDate: Date;
  status: number;

  showModal: boolean;

  userPhotoUrl?: string; // foto do candidato
  userObs?: string; // obs do candidato

  jobPhotoUrl?: string; // foto da vaga
}

@Component({
  selector: 'app-interessados',
  imports: [FormsModule, CommonModule],
  templateUrl: './interessados.component.html',
  styleUrl: './interessados.component.css'
})
export class InteressadosComponent {
  // isCandidatos: boolean = true; // true = Modo Candidatos, false = Modo Minhas Candidaturas
  // candidatos: Candidato[] = [];
  // minhasCandidaturas: MinhaCandidatura[] = [];
  // isLoading: boolean = false;

  // constructor(private candidateService: CandidateService) { }

  // ngOnInit(): void {
  //   // Carrega os dados iniciais com base no modo
  //   this.fetchData();
  // }

  // setMode(mode: 'candidatos' | 'candidaturas'): void {
  //   this.isCandidatos = (mode === 'candidatos');
  //   this.fetchData(); // Recarrega os dados ao mudar de modo
  // }

  // fetchData(): void {
  //   this.isLoading = true;
  //   if (this.isCandidatos) {
  //     // Para o modo 'Candidatos', você precisa de um publicationId
  //     // A lógica para obter esse ID depende de como sua aplicação funciona
  //     // Exemplo: this.route.paramMap.get('publicationId')
  //     const publicationId = 'id_da_publicacao_atual';
  //     this.candidateService.getApplicantsByJob(publicationId).subscribe({
  //       next: data => {
  //         this.candidatos = data;
  //         this.isLoading = false;
  //       },
  //       error: error => {
  //         console.error('Erro ao buscar candidatos:', error);
  //         this.isLoading = false;
  //         // Lógica para exibir mensagem de erro na tela
  //       }
  //     });
  //   } else {
  //     this.candidateService.getMyApplications().subscribe({
  //       next: data => {
  //         this.minhasCandidaturas = data.map(c => ({
  //           applicationId: c.applicationId,
  //           publicationId: c.publicationId,
  //           companyLogoUrl: c.companyLogoUrl, // Ajuste para o nome correto
  //           companyName: c.companyName, // Ajuste para o nome correto
  //           jobTitle: c.jobTitle, // Ajuste para o nome correto
  //           showModal: false
  //         }));
  //         this.isLoading = false;
  //       },
  //       error: error => {
  //         console.error('Erro ao buscar candidaturas:', error);
  //         this.isLoading = false;
  //         // Lógica para exibir mensagem de erro na tela
  //       }
  //     });
  //   }
  // }

  // toggleModal(candidatura: MinhaCandidatura): void {
  //   candidatura.showModal = !candidatura.showModal;
  // }

  // removerCandidatura(applicationId: string): void {
  //   this.candidateService.cancelApplication(applicationId).subscribe({
  //     next: () => {
  //       // Remove a candidatura da lista no front-end após o sucesso da API
  //       this.minhasCandidaturas = this.minhasCandidaturas.filter(c => c.applicationId !== applicationId);
  //     },
  //     error: error => {
  //       console.error('Erro ao remover candidatura:', error);
  //       // Lógica para exibir mensagem de erro na tela
  //     }
  //   });
  // }

  // verPerfil(userId: string): void {
  //   // Lógica de navegação para a tela de perfil do candidato
  //   // Ex: this.router.navigate(['/perfil', userId]);
  // }
}
