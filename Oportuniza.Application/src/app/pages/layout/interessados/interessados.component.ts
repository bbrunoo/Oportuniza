import { Component, Input, OnInit } from '@angular/core';
import { CandidateService } from '../../../services/candidate.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { CandidateDTO, PublicationWithCandidates, UserApplication } from '../../../models/candidate.model';
import { CandidateDisplay } from '../../../models/CandidateDispaly.model';
import { Router } from '@angular/router';

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

  constructor(private candidateService: CandidateService, private router: Router) { }

  ngOnInit() {
    this.detectActiveContext();
    this.loadData('candidates');
  }

  private detectActiveContext() {
    const active = localStorage.getItem('active_token');
    this.isCompany = active === 'company';
  }

  goToPublication(publicationId: string): void {
    this.router.navigate(['/inicio/post', publicationId]);
  }

  downloadResume(url: string): void {
    window.open(url, '_blank');
  }

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
          this.publicationsWithCandidates = res.map(pub => ({
            publicationId: pub.publicationId,
            title: pub.title,
            resumee: pub.resumee,
            authorImage: pub.authorImageUrl || '',
            authorName: pub.name || '',
            imageUrl: (pub as any).imageUrl || '',
            candidates: pub.candidates.map(c => ({
              applicationId: c.id,
              userId: c.userId,
              userName: c.userName,
              userImage: c.userImage,
              status: c.status,
              createdAt: c.applicationDate,
              resumeUrl: (c as any).resumeUrl || '',
              observation: (c as any).observation || ''
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
    }

    else if (tab === 'inscription') {
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
          imageUrl: app.imageUrl || '',
          candidates: []
        });
      }

      map.get(app.publicationId)!.candidates.push({
        applicationId: app.applicationId,
        userId: app.userId,
        userName: app.userName,
        userImage: app.profileImage,
        status: app.status,
        createdAt: app.creationDate || app.applicationDate,

        resumeUrl: app.resumeUrl || '',
        observation: app.observation || ''
      });
    });

    return Array.from(map.values());
  }

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
