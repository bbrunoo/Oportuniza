import { Component, Input, OnInit } from '@angular/core';
import { take } from 'rxjs';
import Swal from 'sweetalert2';
import { Publication } from '../../../models/Publications.model';
import { PublicationFilterDto } from '../../../models/filter.model';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { CandidateService } from '../../../services/candidate.service';
import { PublicationService } from '../../../services/publication.service';
import { CommonModule } from '@angular/common';
import { SearchbarComponent } from '../../../component/searchbar/searchbar.component';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-specific-publication',
  imports: [CommonModule, SearchbarComponent],
  templateUrl: './specific-publication.component.html',
  styleUrl: './specific-publication.component.css'
})
export class SpecificPublicationComponent implements OnInit {
  publication!: Publication;
  userId: string | undefined;
  appliedStatus: { [publicationId: string]: boolean } = {};
  applicationIds: { [publicationId: string]: string } = {};

  constructor(
    private publicationService: PublicationService,
    private candidateService: CandidateService,
    private userService: UserService,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    const publicationId = this.route.snapshot.paramMap.get('id');

    if (publicationId) {
      this.getUserId(publicationId);
    } else {
      console.error('ID da publicação não encontrado na URL.');
      this.router.navigate(['/']);
    }
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
        this.getPublication(publicationId);
      },
      error: (err) => {
        console.error('Erro ao obter o ID do usuário:', err);
        this.getPublication(publicationId);
      },
    });
  }

  getPublication(publicationId: string) {
    this.publicationService.getPublicationsById(publicationId).subscribe({
      next: (publication: Publication) => {
        this.publication = publication;
        this.checkAppliedStatus();
      },
      error: (error: any) => {
        console.log('Erro ao carregar publicação:', error);
      },
    });
  }

  checkAppliedStatus() {
    if (this.userId && this.publication?.id) {
      this.candidateService.getMyApplications().pipe(take(1)).subscribe({
        next: (applications: any[]) => {
          const applied = applications.find(app => app.publication?.id === this.publication.id);

          if (applied) {
            this.appliedStatus[this.publication.id] = true;
            this.applicationIds[this.publication.id] = applied.id;
          } else {
            this.appliedStatus[this.publication.id] = false;
          }
        },
        error: (err) => {
          console.error('Erro ao carregar candidaturas do usuário:', err);
        }
      });
    }
  }

  apply(publicationId: string) {
    Swal.fire({
      title: 'Confirmar candidatura?',
      text: "Você deseja realmente se candidatar a esta vaga?",
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sim, candidatar!',
      cancelButtonText: 'Não, cancelar',
      reverseButtons: true
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
              confirmButtonText: 'Ok'
            });
          },
          error: (err: HttpErrorResponse) => {
            console.error('Erro ao se candidatar:', err);

            if (err.status === 400) {
              const errorMessage = err.error || 'Você já se candidatou a esta vaga.';
              Swal.fire({
                icon: 'info',
                title: 'Ops!',
                text: errorMessage,
                confirmButtonText: 'Ok'
              });
            } else {
              Swal.fire({
                icon: 'error',
                title: 'Oops...',
                text: 'Erro ao se candidatar. Tente novamente mais tarde.',
                confirmButtonText: 'Fechar'
              });
            }
          },
        });
      }
    });
  }

  public cancelApplication(publicationId: string) {
    Swal.fire({
      title: 'Tem certeza?',
      text: "Você não poderá reverter isso!",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: 'Sim, cancelar candidatura!',
      cancelButtonText: 'Não, manter',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this._executeCancel(publicationId);
      }
    });
  }

  private _executeCancel(publicationId: string) {
    const applicationId = this.applicationIds[publicationId];
    if (!applicationId) {
      console.error('ID da candidatura não encontrado localmente. Tentando obter da API...');

      this.candidateService.getMyApplications().pipe(take(1)).subscribe({
        next: (applications) => {
          const app = applications.find(a => a.publication?.id === publicationId);
          if (app) {
            this.applicationIds[publicationId] = app.id;
            this._performApiCancel(app.id, publicationId);
          } else {
            console.error('ID da candidatura não encontrado após nova busca.');
            Swal.fire({
              icon: 'error',
              title: 'Erro!',
              text: 'Não foi possível encontrar o ID da candidatura para cancelar.',
              confirmButtonText: 'Fechar'
            });
          }
        },
        error: (err) => {
          console.error('Erro ao buscar candidaturas do usuário:', err);
          Swal.fire({
            icon: 'error',
            title: 'Erro!',
            text: 'Falha na comunicação com o servidor. Tente novamente.',
            confirmButtonText: 'Fechar'
          });
        }
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
          confirmButtonText: 'Ok'
        });
      },
      error: (err) => {
        console.error('Erro ao cancelar candidatura:', err);

        Swal.fire({
          icon: 'error',
          title: 'Oops...',
          text: 'Erro ao cancelar a candidatura. Por favor, tente novamente.',
          confirmButtonText: 'Fechar'
        });
      }
    });
  }

  handleImgError(event: Event) {
    const target = event.target as HTMLImageElement;
    target.src = '../../../../assets/logo.png';
  }
}
