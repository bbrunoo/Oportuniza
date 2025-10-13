import { Component, Input, OnInit } from '@angular/core';
import { CandidateService } from '../../../services/candidate.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { CandidateDTO, PublicationWithCandidates, UserApplication } from '../../../models/candidate.model';
import { CandidateDisplay } from '../../../models/CandidateDispaly.model';

@Component({
  selector: 'app-interessados',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './interessados.component.html',
  styleUrls: ['./interessados.component.css']
})
export class InteressadosComponent implements OnInit {
  @Input() publicationId?: string;

  activeTab: 'candidates' | 'inscription' = 'candidates';
  publicationsWithCandidates: CandidateDisplay[] = [];
  inscriptions: UserApplication[] = [];
  loading = false;
  hasData = true;
  isCompany = false;

  constructor(private candidateService: CandidateService) { }

  ngOnInit() {
    this.detectActiveContext();
    this.loadData('candidates');
  }

  private detectActiveContext() {
    const active = localStorage.getItem('active_token');
    this.isCompany = active === 'company';
  }

  // loadData(tab: 'candidates' | 'inscription') {
  //   this.loading = true;
  //   this.activeTab = tab;

  //   if (tab === 'candidates') {
  //     this.candidateService.getApplicationsByContext().subscribe({
  //       next: (res: any) => {
  //         if (this.isCompany) {
  //           // Empresa → lista de CandidateDTO (agrupamos por publicação)
  //           const data = res as CandidateDTO[];
  //           this.publicationsWithCandidates = this.mapToPublicationWithCandidates(data);
  //           this.hasData = this.publicationsWithCandidates.length > 0;
  //         } else {
  //           const data = res as PublicationWithCandidates[];
  //           this.publicationsWithCandidates = data;
  //           this.hasData = this.publicationsWithCandidates.length > 0;
  //         }
  //         this.loading = false;
  //       },
  //       error: () => {
  //         Swal.fire('Erro', 'Não foi possível carregar os dados.', 'error');
  //         this.hasData = false;
  //         this.loading = false;
  //       }
  //     });
  //   }
  //   else if (tab === 'inscription' && !this.isCompany) {
  //     this.candidateService.getMyApplications().subscribe({
  //       next: (res: UserApplication[]) => {
  //         this.inscriptions = res;
  //         this.hasData = this.inscriptions.length > 0;
  //         this.loading = false;
  //       },
  //       error: () => {
  //         Swal.fire('Erro', 'Não foi possível carregar suas candidaturas.', 'error');
  //         this.hasData = false;
  //         this.loading = false;
  //       }
  //     });
  //   }
  // }

  loadData(tab: 'candidates' | 'inscription') {
    this.loading = true;
    this.activeTab = tab;

    if (this.isCompany) {
      if (tab === 'candidates') {
        this.candidateService.getApplicationsByCompany().subscribe({
          next: (res: CandidateDTO[]) => {
            this.publicationsWithCandidates = this.mapCompanyDataToCandidateDisplay(res);
            this.hasData = this.publicationsWithCandidates.length > 0;
            this.loading = false;
          },
          error: () => {
            Swal.fire('Erro', 'Não foi possível carregar os candidatos.', 'error');
            this.hasData = false;
            this.loading = false;
          }
        });
      } else {
        this.inscriptions = [];
        this.hasData = false;
        this.loading = false;
      }
      return;
    }

    if (tab === 'candidates') {
      this.candidateService.getMyPublicationsWithCandidates().subscribe({
        next: (res: PublicationWithCandidates[]) => {
          // Mapear para CandidateDisplay
          this.publicationsWithCandidates = res.map(pub => ({
            publicationId: pub.publicationId,
            title: pub.title,
            resumee: pub.resumee,
            authorImage: pub.authorImageUrl || '',
            authorName: pub.name || '',
            candidates: pub.candidates.map(c => ({
              applicationId: c.id,
              userId: c.userId,
              userName: c.userName,
              userImage: c.userImage,
              status: c.status,
              createdAt: c.applicationDate
            }))
          }));
          this.hasData = this.publicationsWithCandidates.length > 0;
          this.loading = false;
        },
        error: () => {
          Swal.fire('Erro', 'Não foi possível carregar os candidatos.', 'error');
          this.hasData = false;
          this.loading = false;
        }
      });
    } else if (tab === 'inscription') {
      this.candidateService.getApplicationsByUser().subscribe({
        next: (res: UserApplication[]) => {
          this.inscriptions = res;
          this.hasData = this.inscriptions.length > 0;
          this.loading = false;
        },
        error: () => {
          Swal.fire('Erro', 'Não foi possível carregar suas candidaturas.', 'error');
          this.hasData = false;
          this.loading = false;
        }
      });
    }
  }

  private mapCompanyDataToCandidateDisplay(res: any[]): CandidateDisplay[] {
    const map = new Map<string, CandidateDisplay>();

    res.forEach(app => {
      if (!map.has(app.publicationId)) {
        map.set(app.publicationId, {
          publicationId: app.publicationId,
          title: app.title,
          resumee: app.resumee || app.description || '',
          authorImage: app.authorImage || '',
          authorName: app.authorName || '',
          candidates: []
        });
      }

      map.get(app.publicationId)!.candidates.push({
        applicationId: app.applicationId,
        userId: app.userId,
        userName: app.userName,
        userImage: app.profileImage,
        status: app.status,
        createdAt: app.creationDate || app.applicationDate
      });
    });

    return Array.from(map.values());
  }

  // /** Agrupa os candidatos por publicação */
  // private mapToPublicationWithCandidates(res: CandidateDTO[]): PublicationWithCandidates[] {
  //   const map = new Map<string, PublicationWithCandidates>();

  //   res.forEach(app => {
  //     if (!map.has(app.publicationId)) {
  //       map.set(app.publicationId, {
  //         publicationId: app.publicationId,
  //         title: app.publicationTitle,
  //         resumee: '',
  //         authorImageUrl: '',
  //         name: '',
  //         candidates: []
  //       });
  //     }

  //     map.get(app.publicationId)!.candidates.push({
  //       id: app.id,
  //       publicationId: app.publicationId,
  //       publicationTitle: app.publicationTitle,
  //       userId: app.userId,
  //       userName: app.userName,
  //       userIdKeycloak: app.userIdKeycloak,
  //       applicationDate: app.applicationDate,
  //       userImage: app.userImage,
  //       status: app.status
  //     });
  //   });

  //   return Array.from(map.values());
  // }

  /** Cancela uma candidatura */
  cancelApplication(applicationId: string) {
    Swal.fire({
      title: 'Tem certeza?',
      text: "Você não poderá reverter isso!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sim, cancelar!',
      cancelButtonText: 'Não, manter',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this._executeCancel(applicationId);
      }
    });
  }

  private _executeCancel(applicationId: string) {
    this.candidateService.cancelApplication(applicationId).subscribe({
      next: () => {
        this.inscriptions = this.inscriptions.filter(app => app.id !== applicationId);
        Swal.fire('Sucesso!', 'Sua candidatura foi cancelada.', 'success');
      },
      error: () => {
        Swal.fire('Erro', 'Não foi possível cancelar sua candidatura.', 'error');
      }
    });
  }
}
