import { Component, Input, OnInit } from '@angular/core';
import { CandidateService } from '../../../services/candidate.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { CandidateDTO, PublicationWithCandidates, UserApplication } from '../../../models/candidate.model';

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
  publicationsWithCandidates: PublicationWithCandidates[] = [];
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

  loadData(tab: 'candidates' | 'inscription') {
    this.loading = true;
    this.activeTab = tab;

    if (this.isCompany) {
      if (tab === 'candidates') {
        this.candidateService.getApplicationsByCompany().subscribe({
          next: (res: CandidateDTO[]) => {
            this.publicationsWithCandidates = this.mapToPublicationWithCandidates(res);
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
          this.publicationsWithCandidates = res;
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
      // Ver candidaturas que eu me inscrevi
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

  /** Agrupa os candidatos por publicação */
  private mapToPublicationWithCandidates(res: CandidateDTO[]): PublicationWithCandidates[] {
    const map = new Map<string, PublicationWithCandidates>();

    res.forEach(app => {
      if (!map.has(app.publicationId)) {
        map.set(app.publicationId, {
          publicationId: app.publicationId,
          title: app.publicationTitle,
          resumee: '',
          authorImageUrl: '',
          name: '',
          candidates: []
        });
      }

      map.get(app.publicationId)!.candidates.push({
        id: app.id,
        publicationId: app.publicationId,
        publicationTitle: app.publicationTitle,
        userId: app.userId,
        userName: app.userName,
        userIdKeycloak: app.userIdKeycloak,
        applicationDate: app.applicationDate,
        userImage: app.userImage,
        status: app.status
      });
    });

    return Array.from(map.values());
  }

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
