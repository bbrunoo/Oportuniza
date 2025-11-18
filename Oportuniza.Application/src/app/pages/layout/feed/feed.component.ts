import { Component, Input, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { take } from 'rxjs';
import Swal from 'sweetalert2';

import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { UserService } from '../../../services/user.service';
import { CandidateService } from '../../../services/candidate.service';
import { SearchbarComponent } from '../../../component/searchbar/searchbar.component';
import { PublicationFilterDto } from '../../../models/filter.model';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, FormsModule, SearchbarComponent],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css',
})
export class FeedComponent implements OnInit {
  publications: Publication[] = [];
  currentIndex = 0;
  @Input() publicationId!: string;
  userId?: string;
  appliedStatus: { [publicationId: string]: boolean } = {};
  applicationIds: { [publicationId: string]: string } = {};
  isCompany = false;
  isMobile = false;

  isApplyModalOpen = false;
  selectedPublicationId: string | null = null;
  observationText = '';
  selectedResumeFile?: File;

  isImageModalOpen = false;
  modalImageUrl: string | null = null;

  constructor(
    private publicationService: PublicationService,
    private candidateService: CandidateService,
    private userService: UserService,
    private router: Router,
    private elRef: ElementRef
  ) { }

  ngOnInit() {
    this.checkIfMobile();

    this.userService.getOwnProfile().pipe(take(1)).subscribe({
      next: (profile: any) => {
        this.isCompany = profile.isCompany;
        this.getUserIdAndPublications();
      },
      error: (err) => {
        console.error('Erro ao obter perfil do usuário:', err);
        this.getUserIdAndPublications();
      },
    });
  }

  openApplyModal(publicationId: string) {
    this.selectedPublicationId = publicationId;
    this.isApplyModalOpen = true;
    document.body.style.overflow = 'hidden';
  }

  closeApplyModal() {
    this.isApplyModalOpen = false;
    this.selectedPublicationId = null;
    this.observationText = '';
    this.selectedResumeFile = undefined;
    document.body.style.overflow = 'auto';
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) this.selectedResumeFile = file;
  }

  confirmApplication() {
    if (!this.selectedPublicationId) return;

    if (!this.selectedResumeFile) {
      Swal.fire({
        icon: 'warning',
        title: 'Currículo obrigatório',
        text: 'Você deve enviar um currículo para concluir a candidatura.',
        confirmButtonText: 'Ok'
      });
      return;
    }

    this.candidateService.applyToJob(this.selectedPublicationId).subscribe({
      next: (response: any) => {
        const appId = response.id;
        this.appliedStatus[this.selectedPublicationId!] = true;
        this.applicationIds[this.selectedPublicationId!] = appId;

        this.candidateService.addCandidateExtra(appId, this.observationText, this.selectedResumeFile)
          .subscribe({
            next: () => {
              this.closeApplyModal();
              Swal.fire({
                icon: 'success',
                title: 'Candidatura enviada!',
                text: 'Sua candidatura foi enviada com sucesso.',
                confirmButtonText: 'Ok'
              });
            },
            error: (err) => {
              const msg = err?.error?.error || 'Erro ao enviar candidatura.';

              Swal.fire({
                icon: 'error',
                title: 'Atenção',
                text: msg,
                confirmButtonText: 'Ok',
              });

              this.closeApplyModal();
            }
          });
      },

      error: (err) => {
        let msg = 'Erro ao enviar candidatura.';

        if (typeof err?.error === 'string') {
          try {
            const parsed = JSON.parse(err.error);
            msg = parsed.error || parsed.message || msg;
          } catch {
            msg = err.error;
          }
        }
        else if (err?.error?.error) {
          msg = err.error.error;
        }
        else if (err?.error?.message) {
          msg = err.error.message;
        }

        Swal.fire({
          icon: 'error',
          title: 'Atenção',
          text: msg,
          confirmButtonText: 'Ok',
        });
      }
    });
  }

  openImageModal(imageUrl: string) {
    this.modalImageUrl = imageUrl;
    this.isImageModalOpen = true;
    document.body.style.overflow = 'hidden';
  }

  closeImageModal() {
    this.isImageModalOpen = false;
    this.modalImageUrl = null;
    document.body.style.overflow = 'auto';
  }

  @HostListener('window:resize')
  onResize() {
    this.checkIfMobile();
  }

  private checkIfMobile() {
    this.isMobile = window.innerWidth <= 1024;
  }

  goBack() {
    if (this.currentIndex > 0) this.currentIndex--;
  }

  goForward() {
    if (this.currentIndex < this.publications.length - 1) this.currentIndex++;
  }

  onSearchTriggered(filters: PublicationFilterDto) {
    const queryParams = {
      ...filters,
      contracts: filters.contracts?.join(','),
      shifts: filters.shifts?.join(','),
    };
    this.router.navigate(['/inicio/search-result'], { queryParams });
  }

  getUserIdAndPublications() {
    this.userService.getUserId().pipe(take(1)).subscribe({
      next: (userId) => {
        this.userId = userId;
        if (this.userId) this.getPublications();
        else console.error('ID do usuário não encontrado.');
      },
      error: (err) => console.error('Erro ao obter ID do usuário:', err),
    });
  }

  getPublications() {
    this.publicationService.getPublications().subscribe({
      next: (publications: Publication[]) => {
        this.publications = publications;
        if (this.publications.length > 0 && this.userId) {
          this.getApplicationsForUser();
        }
      },
      error: (error: any) => console.error('Erro ao carregar publicações:', error),
    });
  }

  getApplicationsForUser() {
    this.candidateService.getMyApplications().subscribe({
      next: (applications: any[]) => {
        applications.forEach((app) => {
          if (app.publication?.id) {
            this.appliedStatus[app.publication.id] = true;
            this.applicationIds[app.publication.id] = app.id;
          }
        });
      },
      error: (err) => console.error('Erro ao carregar candidaturas:', err),
    });
  }

  cancelApplication(publicationId: string) {
    const applicationId = this.applicationIds[publicationId];
    if (!applicationId) return;

    Swal.fire({
      title: 'Cancelar candidatura?',
      text: 'Você realmente deseja cancelar sua candidatura?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sim, cancelar',
      cancelButtonText: 'Não, manter',
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
    }).then((result) => {
      if (result.isConfirmed) {
        this.candidateService.cancelApplication(applicationId).subscribe({
          next: () => {
            this.appliedStatus[publicationId] = false;
            delete this.applicationIds[publicationId];
            Swal.fire({
              icon: 'success',
              title: 'Candidatura cancelada!',
              text: 'Sua candidatura foi cancelada com sucesso.',
              confirmButtonText: 'Ok',
            });
          },
          error: (err) => {
            console.error('Erro ao cancelar candidatura:', err);
            Swal.fire({
              icon: 'error',
              title: 'Erro!',
              text: 'Erro ao cancelar a candidatura. Por favor, tente novamente.',
              confirmButtonText: 'Fechar',
            });
          },
        });
      }
    });
  }
}
