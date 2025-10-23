import { Component, OnInit, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import Swal from 'sweetalert2';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { CandidateService } from '../../../services/candidate.service';
import { UserService } from '../../../services/user.service';
import { SearchbarComponent } from '../../../component/searchbar/searchbar.component';
import { CommonModule } from '@angular/common';
import { PublicationFilterDto } from '../../../models/filter.model';

@Component({
  selector: 'app-specific-publication',
  imports: [CommonModule, SearchbarComponent],
  templateUrl: './specific-publication.component.html',
  styleUrl: './specific-publication.component.css'
})
export class SpecificPublicationComponent implements OnInit {
  publication!: Publication;
  userId: string | undefined;
  isCompany = false;
  isMobile = false;
  appliedStatus: { [publicationId: string]: boolean } = {};
  applicationIds: { [publicationId: string]: string } = {};

  isImageModalOpen = false;
  modalImageUrl: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private publicationService: PublicationService,
    private candidateService: CandidateService,
    private userService: UserService
  ) { }

  ngOnInit() {
    this.checkIfMobile();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.userService.getOwnProfile().pipe(take(1)).subscribe({
        next: (profile: any) => {
          this.isCompany = profile.isCompany;
          this.getUserId(id);
        },
        error: () => this.getUserId(id)
      });
    }
  }

  @HostListener('window:resize')
  onResize() {
    this.checkIfMobile();
  }

  private checkIfMobile() {
    this.isMobile = window.innerWidth <= 1024;
  }

  onSearchTriggered(filters: PublicationFilterDto) {
    const queryParams = {
      ...filters,
      contracts: filters.contracts?.join(','),
      shifts: filters.shifts?.join(',')
    };
    this.router.navigate(['/inicio/search-result'], { queryParams });
  }

  getUserId(publicationId: string) {
    this.userService.getUserId().pipe(take(1)).subscribe({
      next: (userId) => {
        this.userId = userId;
        this.loadPublication(publicationId);
      },
      error: () => this.loadPublication(publicationId)
    });
  }

  loadPublication(publicationId: string) {
    this.publicationService.getPublicationsById(publicationId).subscribe({
      next: (data: Publication) => {
        this.publication = data;
        this.checkAppliedStatus();
      },
      error: () => Swal.fire('Erro', 'Falha ao carregar publicação', 'error')
    });
  }

  checkAppliedStatus() {
    if (!this.userId || !this.publication?.id) return;
    this.candidateService.getMyApplications().pipe(take(1)).subscribe({
      next: (applications) => {
        const app = applications.find((a) => a.publication?.id === this.publication.id);
        if (app) {
          this.appliedStatus[this.publication.id] = true;
          this.applicationIds[this.publication.id] = app.id;
        }
      }
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
      cancelButtonText: 'Não, cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.candidateService.applyToJob(publicationId).subscribe({
          next: (response) => {
            this.appliedStatus[publicationId] = true;
            this.applicationIds[publicationId] = response.id;
            Swal.fire('Sucesso!', 'Você se candidatou a esta vaga.', 'success');
          },
          error: () => Swal.fire('Erro', 'Não foi possível se candidatar.', 'error')
        });
      }
    });
  }

  cancelApplication(publicationId: string) {
    Swal.fire({
      title: 'Cancelar candidatura?',
      text: 'Você não poderá reverter isso!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sim, cancelar',
      cancelButtonText: 'Não, manter'
    }).then((result) => {
      if (result.isConfirmed) this._executeCancel(publicationId);
    });
  }

  private _executeCancel(publicationId: string) {
    const appId = this.applicationIds[publicationId];
    if (appId) {
      this._performApiCancel(appId, publicationId);
    } else {
      this.candidateService.getMyApplications().pipe(take(1)).subscribe({
        next: (apps) => {
          const found = apps.find((a) => a.publication?.id === publicationId);
          if (found) this._performApiCancel(found.id, publicationId);
        }
      });
    }
  }

  private _performApiCancel(appId: string, pubId: string) {
    this.candidateService.cancelApplication(appId).subscribe({
      next: () => {
        this.appliedStatus[pubId] = false;
        delete this.applicationIds[pubId];
        Swal.fire('Candidatura cancelada!', '', 'success');
      },
      error: () => Swal.fire('Erro', 'Falha ao cancelar candidatura', 'error')
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

  handleImgError(event: Event) {
    (event.target as HTMLImageElement).src = '../../../../assets/logo.png';
  }
}
