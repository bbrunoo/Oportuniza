import { Component, Input, OnInit } from '@angular/core';
import { CandidateService } from '../../../services/candidate.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { CandidateDTO, PublicationWithCandidates, UserApplication } from '../../../models/candidate.model';
import { Publication } from '../../../models/Publications.model';

@Component({
  selector: 'app-interessados',
  imports: [FormsModule, CommonModule],
  templateUrl: './interessados.component.html',
  styleUrls: ['./interessados.component.css']
})
export class InteressadosComponent implements OnInit {
  @Input() publicationId?: string;

  activeTab: 'candidates' | 'inscription' = 'candidates';
  publicationsWithCandidates: PublicationWithCandidates[] = [];
  inscriptions: UserApplication[] = [];
  appliedStatus: { [publicationId: string]: boolean } = {};

  loading = false;
  hasData = true;

  constructor(private candidateService: CandidateService) { }

  ngOnInit() {
    this.loadData('candidates');
  }

  loadData(tab: 'candidates' | 'inscription') {
    this.loading = true;
    this.activeTab = tab;

    if (tab === 'candidates') {
      this.candidateService.getMyPublicationsWithCandidates().subscribe({
        next: (res) => {
          this.publicationsWithCandidates = res.filter(pub => pub.candidates && pub.candidates.length > 0);
          this.hasData = this.publicationsWithCandidates.length > 0;
          this.loading = false;
          console.log("log", res);
        },
        error: (err) => {
          console.error(err);
          Swal.fire('Erro', 'Não foi possível carregar os candidatos. Tente novamente.', 'error');
          this.hasData = false;
          this.loading = false;
        }
      });
    } else if (tab === 'inscription') {
      this.candidateService.getMyApplications().subscribe({
        next: (res: UserApplication[]) => {
          this.inscriptions = res;
          this.hasData = this.inscriptions.length > 0;
          this.loading = false;
          console.log("log", res);
        },
        error: (err) => {
          console.error(err);
          Swal.fire('Erro', 'Não foi possível carregar suas candidaturas. Tente novamente.', 'error');
          this.hasData = false;
          this.loading = false;
        }
      });
    }
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
}
