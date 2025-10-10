import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CompanyEmployeeDto } from '../../../models/company-get.model';
import { CompanyEmployeeService } from '../../../services/company-employee.service';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { UserService } from '../../../services/user.service';

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

  selectedEmployee: CompanyEmployeeDto | undefined;
  currentUserId: string | undefined = undefined;
  isCurrentUser: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private employeeService: CompanyEmployeeService,
    private userService: UserService
  ) { }

  ngOnInit(): void {
    this.fetchCurrentUserId().then(() => {
      this.companyId = this.route.parent?.snapshot.paramMap.get('id') ?? null;

      if (this.companyId) {
        this.loadEmployees(this.companyId);
      } else {
        this.isLoading = false;
        this.errorMessage = 'ID da empresa não encontrado na URL.';
      }
    });
  }

  async fetchCurrentUserId(): Promise<void> {
    this.currentUserId = await this.userService.getLoggedInUserId();
    if (!this.currentUserId) {
      console.warn('ID do usuário logado não foi encontrado. A funcionalidade de bloqueio de edição própria pode falhar.');
    }
  }

  openSettings(employeeId: string): void {
    const emp = this.employees.find(e => e.id === employeeId);
    if (!emp) return;

    this.selectedEmployeeId = employeeId;
    this.selectedEmployee = emp;

    this.selectedEmployeeRoles.isAdmin = emp.roles === 'Owner';
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

  savePermissions(): void {
    if (!this.selectedEmployeeId || this.isCurrentUser) return;
    if (!this.selectedEmployeeRoles.isAdmin && !this.selectedEmployeeRoles.canPost) {
      Swal.fire({
        title: 'Erro de Permissão!',
        html: 'O funcionário deve ser definido como <b>Administrador</b> OU <b>Funcionário Padrão</b>. Alterne um dos botões.',
        icon: 'error',
        confirmButtonColor: '#007bff',
        confirmButtonText: 'Entendi'
      });
      return;
    }


    const employeeName = this.selectedEmployee?.userName || this.selectedEmployee?.userEmail || 'este funcionário';

    let newRoleDescription = this.selectedEmployeeRoles.isAdmin
      ? 'Administrador'
      : 'Funcionário Padrão';

    Swal.fire({
      title: 'Confirmar Alteração de Permissão?',
      html: `Você está prestes a alterar o cargo de <b>${employeeName}</b> para <b>${newRoleDescription}</b>.<br><br>Tem certeza que deseja prosseguir com esta mudança?`,
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
    const updateData = {
      isAdmin: this.selectedEmployeeRoles.isAdmin,
      canPost: this.selectedEmployeeRoles.canPost
    };

    this.employeeService.updateEmployeeRoles(this.selectedEmployeeId!, updateData).subscribe({
      next: () => {
        Swal.fire(
          'Sucesso!',
          'As permissões do funcionário foram atualizadas.',
          'success'
        );
        this.closeModal();
        this.loadEmployees(this.companyId!);
      },
      error: (err) => {
        console.error('Erro ao atualizar permissões:', err);
        Swal.fire(
          'Erro!',
          'Houve uma falha ao tentar atualizar as permissões.',
          'error'
        );
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
        console.log('Lista de funcionários ordenada:', this.employees);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Falha ao carregar a lista de funcionários.';
        console.error('Erro ao buscar funcionários:', err);
      }
    });
  }
}
