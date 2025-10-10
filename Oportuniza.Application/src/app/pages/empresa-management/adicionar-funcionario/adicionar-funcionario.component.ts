import { Component } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import Swal from 'sweetalert2';
import { CompanyService } from '../../../services/company.service';
import { MatDialog } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Observable } from 'rxjs';
import { CompanyEmployeeService } from '../../../services/company-employee.service';
import { CompanyDto } from '../../../models/company-get.model';
import { UserSearchResult } from '../../../models/user-serach.model';

@Component({
  selector: 'app-adicionar-funcionario',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatIconModule,
    RouterModule,
    FormsModule
  ],
  templateUrl: './adicionar-funcionario.component.html',
  styleUrl: './adicionar-funcionario.component.css'
})
export class AdicionarFuncionarioComponent {
  empresaId: string | null = null;
  isLoading = false;
  isSubmitting = false;

  employeeName = '';
  employeeEmail = '';

  addMode: 'create' | 'link' = 'create';
  linkEmail = '';
  isSearching = false;
  searchedUser: UserSearchResult | null = null;
  searchError: string | null = null;

  constructor(
    private companyService: CompanyService,
    private employeeService: CompanyEmployeeService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.empresaId = this.route.parent?.snapshot.paramMap.get('id') ?? null;
  }

  isValidEmail(email: string | null | undefined): boolean {
    if (!email) return false;
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  searchUserByEmail(): void {
    this.searchedUser = null;
    this.searchError = null;

    if (!this.isValidEmail(this.linkEmail)) {
      Swal.fire('Aviso', 'Informe um email válido.', 'warning');
      return;
    }
    this.isSearching = true;

    this.employeeService.searchUserByEmail(this.linkEmail).subscribe({
      next: (user) => {
        this.searchedUser = user;
        this.isSearching = false;
      },
      error: (err) => {
        this.isSearching = false;
        if (err.status === 404) {
          Swal.fire('Não encontrado', 'Usuário não encontrado.', 'info');
        } else {
          Swal.fire('Erro', 'Erro ao buscar usuário. Tente novamente.', 'error');
        }
      }
    });
  }

  confirmLinkUser(): void {
    if (!this.searchedUser || !this.empresaId) return;

    Swal.fire({
      title: 'Confirmar Vinculação',
      html: `Deseja vincular <strong>${this.searchedUser.name || this.searchedUser.email}</strong> à sua empresa?`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: 'Sim, Vincular!',
      cancelButtonText: 'Cancelar',
      showLoaderOnConfirm: true,
      preConfirm: () => {
        return this.employeeService.linkEmployee(this.searchedUser!.email, this.empresaId!).toPromise()
          .then(res => res)
          .catch(err => {
            let errorMessage = 'Falha ao vincular o funcionário.';
            if (err.status === 400) errorMessage = err.error || 'Este usuário já está vinculado à empresa.';
            else if (err.status === 403) errorMessage = 'Você não tem permissão.';
            Swal.showValidationMessage(`Erro: ${errorMessage}`);
            return false;
          });
      },
      allowOutsideClick: () => !Swal.isLoading()
    }).then((result) => {
      if (result.isConfirmed && result.value) {
        Swal.fire('Sucesso!', `Usuário ${this.searchedUser!.email} vinculado com sucesso.`, 'success');
        this.searchedUser = null;
        this.linkEmail = '';
        this.router.navigate(['../funcionarios'], { relativeTo: this.route });
      }
    });
  }


  onSave(): void {
    if (this.addMode !== 'create') return;
    if (this.isSubmitting || !this.empresaId) return;

    if (!this.employeeName || !this.employeeEmail) {
      Swal.fire('Atenção', 'Preencha Nome e Email.', 'warning');
      return;
    }
    if (!this.isValidEmail(this.employeeEmail)) {
      Swal.fire('Email Inválido', 'Formato de email inválido.', 'warning');
      return;
    }

    this.isSubmitting = true;
    const payload = {
      email: this.employeeEmail,
      companyId: this.empresaId,
      employeeName: this.employeeName
    };

    this.employeeService.linkEmployee(this.employeeEmail, this.empresaId!).subscribe({
      next: () => {
        Swal.fire('Sucesso!', 'Funcionário vinculado/registrado com sucesso.', 'success');
        this.isSubmitting = false;
        this.router.navigate(['../funcionarios'], { relativeTo: this.route });
      },
      error: (err) => {
        let msg = 'Falha ao vincular o funcionário. Tente novamente mais tarde.';
        if (err.status === 409) msg = 'Este email já está sendo utilizado por outra empresa.';
        else if (err.status === 403) msg = 'Você não tem permissão.';
        Swal.fire('Erro', msg, 'error');
        this.isSubmitting = false;
      }
    });
  }
}
