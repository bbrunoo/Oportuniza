import { Component, Input, OnInit } from '@angular/core';
import { CandidateService } from '../../../services/candidate.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Candidato, Candidatura, PublicationWithCandidates } from '../../../models/candidatos.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-interessados',
  imports: [FormsModule, CommonModule],
  templateUrl: './interessados.component.html',
  styleUrl: './interessados.component.css'
})
export class InteressadosComponent implements OnInit {
  @Input() publicationId?: string;

  isCandidatos = true;
  candidatos: Candidato[] = [];
  minhasCandidaturas: Candidatura[] = [];
  minhasPublicacoes: PublicationWithCandidates[] = [];
  isLoading = false;

  constructor(private candidateAppService: CandidateService) {
    console.log('[DEBUG] InteressadosComponent instanciado');
  }

  ngOnInit(): void {
    console.log('[DEBUG] ngOnInit chamado. publicationId:', this.publicationId);
    this.loadData();
  }

  loadData(): void {
    console.log('[DEBUG] loadData iniciado. isCandidatos:', this.isCandidatos, 'publicationId:', this.publicationId);

    if (!this.candidateAppService) {
      console.error('[ERRO] CandidateService não injetado. Verifique providers/imports.');
      return;
    }

    this.isLoading = true;

    if (this.isCandidatos) {
      if (this.publicationId) {
        console.log('[DEBUG] Buscando candidatos de uma vaga específica. publicationId:', this.publicationId);
        this.candidateAppService.getApplicantsByJob(this.publicationId).subscribe({
          next: (data) => {
            console.log('[DEBUG] Candidatos carregados com sucesso:', data);
            this.candidatos = data;
            this.isLoading = false;
          },
          error: (err) => {
            console.error('[ERRO] Falha ao carregar candidatos da vaga:', this.publicationId, err);
            this.candidatos = [];
            this.isLoading = false;
          }
        });
      } else {
        console.log('[DEBUG] Buscando candidatos de todas as publicações do usuário logado...');
        this.candidateAppService.getMyPublicationsWithCandidates().subscribe({
          next: (data) => {
            console.log('[DEBUG] Publicações com candidatos carregadas com sucesso:', data);
            this.minhasPublicacoes = data;
            this.isLoading = false;
          },
          error: (err) => {
            console.error('[ERRO] Falha ao carregar publicações com candidatos:', err);
            this.minhasPublicacoes = [];
            this.isLoading = false;
          },
          complete: () => {
            console.log('[DEBUG] Requisição getMyPublicationsWithCandidates finalizada');
          }
        });
      }
    } else {
      console.log('[DEBUG] Buscando minhas candidaturas...');
      this.candidateAppService.getMyApplications().subscribe({
        next: (data) => {
          console.log('[DEBUG] Minhas candidaturas carregadas com sucesso:', data);
          this.minhasCandidaturas = data.map(c => ({ ...c, showModal: false }));
          this.isLoading = false;
        },
        error: (err) => {
          console.error('[ERRO] Falha ao carregar minhas candidaturas:', err);
          this.minhasCandidaturas = [];
          this.isLoading = false;
        }
      });
    }
  }

  setMode(mode: 'candidatos' | 'candidaturas'): void {
    console.log('[DEBUG] Alterando modo para:', mode);
    this.isCandidatos = (mode === 'candidatos');
    this.loadData();
  }

  removerCandidatura(id: string): void {
    console.log('[DEBUG] Tentando remover candidatura. id:', id);
    this.candidateAppService.cancelApplication(id).subscribe({
      next: () => {
        console.log('[DEBUG] Candidatura removida com sucesso. id:', id);
        this.minhasCandidaturas = this.minhasCandidaturas.filter(c => c.id !== id);
      },
      error: (err) => {
        console.error('[ERRO] Falha ao remover candidatura. id:', id, err);
      }
    });
  }

  // Substitua este método pelo SweetAlert2
  // toggleModal(candidatura: Candidatura): void {
  //   candidatura.showModal = !candidatura.showModal;
  //   console.log('[DEBUG] Toggle modal. id:', candidatura.id, 'showModal:', candidatura.showModal);
  // }

  confirmarRemocao(candidatura: Candidatura): void {
    Swal.fire({
      title: 'Deseja remover a candidatura?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sim',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.removerCandidatura(candidatura.id);
        Swal.fire(
          'Removido!',
          'Sua candidatura foi removida com sucesso.',
          'success'
        );
      }
    });
  }

  verPerfil(candidatoId: string): void {
    console.log('[DEBUG] Ver perfil do candidato. candidatoId:', candidatoId);
  }
}
