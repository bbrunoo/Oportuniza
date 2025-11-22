import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CompanyEmployeeDto } from '../../../models/company-get.model';
import { CompanyEmployeeService } from '../../../services/company-employee.service';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { UserService } from '../../../services/user.service';
import { KeycloakOperationService } from '../../../services/keycloak.service';
import { takeUntil, Subject } from 'rxjs';

@Component({
  selector: 'app-funcionarios',
  standalone: true,
  imports: [CommonModule, NgFor, NgIf, FormsModule],
  templateUrl: './funcionarios.component.html',
  styleUrl: './funcionarios.component.css'
})
export class FuncionariosComponent {
  empresaId: string | null = null;
  companyId: string | null = null;
  employees: CompanyEmployeeDto[] = [];
  isLoading: boolean = true;
  errorMessage: string | null = null;

  showPermissionModal = false;
  selectedEmployeeId: string | null = null;
  selectedEmployeeRoles = {
    isAdmin: false,
    canPost: false
  };

  showStatusModal = false;
  selectedEmployee: CompanyEmployeeDto | null = null;
  currentUserId: string | undefined = undefined;
  isCurrentUser: boolean = false;

  userRoleInCompany: string | null = null;
  isUserAdmin: boolean = false;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private employeeService: CompanyEmployeeService,
    private userService: UserService,
    private keycloakService: KeycloakOperationService
  ) { }

  ngOnInit(): void {
    this.fetchCurrentUserId().then(() => {
      this.companyId = this.route.parent?.snapshot.paramMap.get('id') ?? null;

      if (this.companyId) {
        this.loadUserRoleAndEmployees(this.companyId);
      } else {
        this.isLoading = false;
        this.errorMessage = 'ID da empresa não encontrado na URL.';
      }
    });
  }

  private loadUserRoleAndEmployees(companyId: string): void {
    this.isLoading = true;
    this.keycloakService.verifyUserRole(companyId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (res.hasRole && res.role) {
            this.userRoleInCompany = res.role;
            this.isUserAdmin = res.role === 'Owner' || res.role === 'Administrator';
          } else {
            this.userRoleInCompany = null;
            this.isUserAdmin = false;
          }

          this.loadEmployees(companyId);
        },
        error: (err) => {
          console.error('[FuncionariosComponent] Erro ao verificar role:', err);
          this.errorMessage = 'Erro ao verificar o papel do usuário.';
          this.isLoading = false;
        }
      });
  }

  async fetchCurrentUserId(): Promise<void> {
    this.currentUserId = await this.userService.getLoggedInUserId();
  }

  openSettings(employeeId: string): void {
    if (!this.isUserAdmin) {
      Swal.fire('Acesso negado', 'Apenas administradores podem alterar permissões.', 'warning');
      return;
    }

    const emp = this.employees.find(e => e.id === employeeId);
    if (!emp) return;

    if (emp.roles === 'Owner') {
      Swal.fire('Ação não permitida', 'O dono da empresa não pode ter seu cargo alterado.', 'info');
      return;
    }

    this.selectedEmployeeId = employeeId;
    this.selectedEmployee = emp;

    this.selectedEmployeeRoles.isAdmin = emp.roles === 'Administrator';
    this.selectedEmployeeRoles.canPost = emp.roles === 'Worker';

    if (!this.selectedEmployeeRoles.isAdmin && !this.selectedEmployeeRoles.canPost) {
      this.selectedEmployeeRoles.canPost = true;
    }

    this.isCurrentUser = this.selectedEmployeeId === this.currentUserId;
    this.showPermissionModal = true;
  }

  onAdminChange(): void {
    if (this.selectedEmployeeRoles.isAdmin) {
      this.selectedEmployeeRoles.canPost = false;
    } else {
      this.selectedEmployeeRoles.canPost = true;
    }
  }

  onPostChange(): void {
    if (this.selectedEmployeeRoles.canPost) {
      this.selectedEmployeeRoles.isAdmin = false;
    } else {
      this.selectedEmployeeRoles.isAdmin = true;
    }
  }

  closeModal(): void {
    this.showPermissionModal = false;
  }

  openStatusModal(employee: CompanyEmployeeDto): void {
    if (employee.roles === 'Owner') {
      Swal.fire('Ação não permitida', 'O dono da empresa não pode ser desativado.', 'info');
      return;
    }

    const newStatus = employee.isActive === 0 ? 1 : 0;
    const actionText = newStatus === 1 ? 'desativar' : 'reativar';

    Swal.fire({
      title: `Confirmar ${actionText}?`,
      html: `Tem certeza que deseja ${actionText} o funcionário <b>${employee.userName || employee.userEmail}</b>?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: newStatus === 1 ? '#dc3545' : '#007bff',
      cancelButtonColor: '#6c757d',
      confirmButtonText: 'Sim, confirmar!',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.employeeService.toggleEmployeeStatus(employee.id, newStatus).subscribe({
          next: () => {
            employee.isActive = newStatus;

            Swal.fire(
              'Sucesso!',
              `Funcionário ${actionText === 'desativar' ? 'desativado' : 'reativado'} com sucesso!`,
              'success'
            );
          },
          error: (err) => {
            let msg = 'Erro inesperado.';

            if (err?.error?.error) {
              msg = err.error.error;
            }
            else if (typeof err?.error === 'string') {
              try {
                const parsed = JSON.parse(err.error);
                msg = parsed.error || msg;
              } catch {
                msg = err.error;
              }
            }

            Swal.fire({
              icon: 'error',
              title: 'Atenção',
              text: msg,
              confirmButtonText: 'Ok'
            });
          }
        });
      }
    });
  }

  savePermissions(): void {
    if (!this.selectedEmployeeId || this.isCurrentUser) return;

    if (!this.selectedEmployeeRoles.isAdmin && !this.selectedEmployeeRoles.canPost) {
      Swal.fire({
        title: 'Erro de Permissão!',
        html: 'O funcionário deve ser definido como <b>Administrador</b> OU <b>Funcionário Padrão</b>.',
        icon: 'error',
        confirmButtonColor: '#007bff'
      });
      return;
    }

    const employeeName = this.selectedEmployee?.userName || this.selectedEmployee?.userEmail || 'este funcionário';
    const newRoleDescription = this.selectedEmployeeRoles.isAdmin ? 'Administrador' : 'Funcionário Padrão';

    Swal.fire({
      title: 'Confirmar Alteração de Permissão?',
      html: `Você está prestes a alterar o cargo de <b>${employeeName}</b> para <b>${newRoleDescription}</b>.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#007bff',
      cancelButtonColor: '#dc3545',
      confirmButtonText: 'Sim, alterar cargo!',
      cancelButtonText: 'Cancelar'
    }).then((result) => {
      if (result.isConfirmed) {
        this.executeSavePermissions();
      }
    });
  }

  private executeSavePermissions(): void {
    if (!this.selectedEmployeeId) return;

    const updateData = {
      isAdmin: this.selectedEmployeeRoles.isAdmin,
      canPost: this.selectedEmployeeRoles.canPost
    };

    this.employeeService.updateEmployeeRoles(this.selectedEmployeeId, updateData)
      .subscribe({
        next: () => {
          Swal.fire('Sucesso!', 'As permissões do funcionário foram atualizadas.', 'success');
          this.closeModal();

          const employeeIndex = this.employees.findIndex(e => e.id === this.selectedEmployeeId);
          if (employeeIndex !== -1 && this.selectedEmployee) {
            this.employees[employeeIndex].roles = this.selectedEmployeeRoles.isAdmin
              ? 'Administrator'
              : 'Worker';

            (this.employees[employeeIndex] as any).canPostJobs = this.selectedEmployeeRoles.canPost;
          }

          if (this.selectedEmployeeId === this.currentUserId) {
            this.userRoleInCompany = this.selectedEmployeeRoles.isAdmin ? 'Administrator' : 'Worker';
            this.isUserAdmin = this.selectedEmployeeRoles.isAdmin;
          }
        },
        error: (err) => {
          let msg = 'Erro inesperado.';

          if (err?.error?.error) {
            msg = err.error.error;
          }
          else if (typeof err?.error === 'string') {
            try {
              const parsed = JSON.parse(err.error);
              msg = parsed.error || msg;
            } catch {
              msg = err.error;
            }
          }

          Swal.fire({
            icon: 'error',
            title: 'Atenção',
            text: msg,
            confirmButtonText: 'Ok'
          });

          this.closeModal();
        }
      });
  }

  loadEmployees(id: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.employeeService.getOrderedEmployees(id).subscribe({
      next: (data) => {
        this.employees = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Falha ao carregar a lista de funcionários.';
        console.error('Erro ao buscar funcionários:', err);
      }
    });
  }
}
