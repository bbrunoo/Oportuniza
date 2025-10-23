import { Component, Input, OnInit, HostListener, ElementRef } from '@angular/core';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../services/user.service';
import { CandidateService } from '../../../services/candidate.service';
import { take } from 'rxjs';
import Swal from 'sweetalert2';
import { SearchbarComponent } from '../../../component/searchbar/searchbar.component';
import { PublicationFilterDto } from '../../../models/filter.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, SearchbarComponent],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css',
})
export class FeedComponent implements OnInit {
  publications: Publication[] = [];
  currentIndex: number = 0;
  @Input() publicationId!: string;
  userId: string | undefined;
  appliedStatus: { [publicationId: string]: boolean } = {};
  applicationIds: { [publicationId: string]: string } = {};
  hasApplied = false;
  isCompany: boolean = false;
  isMobile = false;

  constructor(
    private publicationService: PublicationService,
    private candidateService: CandidateService,
    private userService: UserService,
    private router: Router,
    private elRef: ElementRef
  ) {}

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

  isImageModalOpen = false;
  modalImageUrl: string | null = null;

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
    if (this.currentIndex > 0) {
      this.currentIndex--;
    }
  }

  goForward() {
    if (this.currentIndex < this.publications.length - 1) {
      this.currentIndex++;
    }
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
        if (this.userId) {
          this.getPublications();
        } else {
          console.error('ID do usuário não encontrado. O usuário pode não estar logado.');
        }
      },
      error: (err) => {
        console.error('Erro ao obter o ID do usuário:', err);
      },
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
      error: (error: any) => {
        console.log('Erro ao carregar publicações:', error);
      },
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
      error: (err) => {
        console.error('Erro ao carregar candidaturas do usuário:', err);
      },
    });
  }

  apply(publicationId: string) {
    Swal.fire({
      title: 'Confirmar candidatura?',
      text: 'Você deseja realmente se candidatar a esta vaga?',
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sim, candidatar!',
      cancelButtonText: 'Não, cancelar',
      reverseButtons: true,
    }).then((result) => {
      if (result.isConfirmed) {
        this.candidateService.applyToJob(publicationId).subscribe({
          next: (response) => {
            this.appliedStatus[publicationId] = true;
            this.applicationIds[publicationId] = response.id;
            Swal.fire({
              icon: 'success',
              title: 'Candidatura enviada!',
              text: 'Você se candidatou a esta vaga com sucesso.',
              confirmButtonText: 'Ok',
            });
          },
          error: (err) => {
            console.error(err);
            Swal.fire({
              icon: 'error',
              title: 'Oops...',
              text: 'Erro ao se candidatar. Tente novamente mais tarde.',
              confirmButtonText: 'Fechar',
            });
          },
        });
      }
    });
  }

  public cancelApplication(publicationId: string) {
    Swal.fire({
      title: 'Tem certeza?',
      text: 'Você não poderá reverter isso!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sim, cancelar candidatura!',
      cancelButtonText: 'Não, manter',
      reverseButtons: true,
    }).then((result) => {
      if (result.isConfirmed) {
        this._executeCancel(publicationId);
      }
    });
  }

  private _executeCancel(publicationId: string) {
    const applicationId = this.applicationIds[publicationId];
    if (!applicationId) {
      this.candidateService.getMyApplications().pipe(take(1)).subscribe({
        next: (applications) => {
          const app = applications.find((a) => a.publication.id === publicationId);
          if (app) {
            this.applicationIds[publicationId] = app.id;
            this._performApiCancel(app.id, publicationId);
          } else {
            Swal.fire({
              icon: 'error',
              title: 'Erro!',
              text: 'Não foi possível encontrar o ID da candidatura para cancelar.',
              confirmButtonText: 'Fechar',
            });
          }
        },
      });
    } else {
      this._performApiCancel(applicationId, publicationId);
    }
  }

  private _performApiCancel(applicationId: string, publicationId: string) {
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
          title: 'Oops...',
          text: 'Erro ao cancelar a candidatura. Por favor, tente novamente.',
          confirmButtonText: 'Fechar',
        });
      },
    });
  }
}
