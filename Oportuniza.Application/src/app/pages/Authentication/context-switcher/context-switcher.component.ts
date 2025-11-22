import { CompanyEmployeeService } from './../../../services/company-employee.service';
import { Component } from '@angular/core';
import { AccessService } from '../../../services/access.service';
import { Router } from '@angular/router';
import { NgFor, NgIf } from '@angular/common';
import { KeycloakOperationService } from '../../../services/keycloak.service';
import Swal from 'sweetalert2';
import { MatDialog } from '@angular/material/dialog';

export interface AccessContext {
  type: 'User' | 'Company';
  id: string;
  name: string;
  imageUrl: string;
  email?: string;
  role?: string;
  ownerId?: string;
}

@Component({
  selector: 'app-context-switcher',
  imports: [NgIf, NgFor],
  templateUrl: './context-switcher.component.html',
  styleUrl: './context-switcher.component.css'
})
export class ContextSwitcherComponent {
  contexts: AccessContext[] = [];
  loading = true;
  error: string | null = null;
  currentContextId: string = '';

  constructor(
    private accessService: AccessService,
    private companyEmService: CompanyEmployeeService,
    private router: Router,
    private keycloakService: KeycloakOperationService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.dialog.closeAll();
    Swal.close();

    this.loadContexts();
    this.getCurrentActiveContext();
  }

  currentUserId: string = '';

  getCurrentActiveContext(): void {
    const token = localStorage.getItem('context_access_token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.currentContextId = payload['company_id'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
        this.currentUserId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      } catch (e) {
        console.error("Não foi possível decodificar o token para obter o contexto ativo.");
      }
    }
  }

  loadContexts(): void {
    this.accessService.getContexts().subscribe({
      next: (data) => {
        this.contexts = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Falha ao carregar contextos. Tente novamente mais tarde.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  selectContext(context: AccessContext): void {
    if (context.type === 'User') {
      const userToken = localStorage.getItem('user_token');
      if (userToken) {
        localStorage.setItem('active_token', 'user');
        this.keycloakService.setNewAccessToken(userToken);
        this.router.navigate(['/inicio/feed']);
      } else {
        console.warn("Token de usuário não encontrado, redirecionando para login...");
        this.router.navigate(['/login']);
      }
      return;
    }

    this.loading = true;

    this.accessService.switchContext(context.id).subscribe({
      next: (res) => {
        localStorage.setItem('company_token', res.token);
        localStorage.setItem('active_token', 'company');
        this.keycloakService.setNewAccessToken(res.token);

        this.loading = false;
        this.router.navigate(['/inicio/feed']);
      },
      error: (err) => {
        this.loading = false;
        this.error = 'Falha ao trocar de contexto. Verifique suas permissões.';
        console.error(err);
      }
    });
  }

  unlinkContext(context: AccessContext): void {
    if (context.type === 'User') {
      Swal.fire({
        icon: 'info',
        text: 'Esta é a sua conta pessoal e não pode ser desvinculada.',
        confirmButtonText: 'Entendido',
        width: '350px',
        color: '#252525'
      });
      return;
    }

    Swal.fire({
      text: `Tem certeza que deseja se desvincular da empresa "${context.name}"?`,
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      width: '380px',
      color: '#252525',
      confirmButtonText: 'Sim, desvincular',
      cancelButtonText: 'Cancelar',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.loading = true;

        this.companyEmService.unlinkCompany(context.id).subscribe({
          next: (res) => {
            Swal.fire({
              icon: 'success',
              title: 'Desvinculado!',
              text: res.message || 'Você saiu da empresa com sucesso.',
              timer: 2500,
              showConfirmButton: false
            });
            this.loading = false;
            this.loadContexts();
          },
          error: (err) => {
            this.loading = false;
            console.error('Erro ao desvincular:', err);

            let message = 'Não foi possível se desvincular da empresa.';

            switch (err.status) {
              case 400:
                message = err.error?.message || 'Requisição inválida.';
                break;
              case 401:
                message = 'Sua sessão expirou. Faça login novamente.';
                break;
              case 403:
                message = 'Você não tem permissão para desvincular esta empresa.';
                break;
              case 404:
                message = err.error?.message || 'Empresa ou vínculo não encontrado.';
                break;
              case 500:
                message = 'Erro interno no servidor. Tente novamente mais tarde.';
                break;
              default:
                message = err.error?.message || message;
                break;
            }

            Swal.fire({
              icon: 'error',
              title: 'Erro!',
              text: message,
              confirmButtonText: 'Fechar'
            });
          }
        });
      }
    });
  }

}
