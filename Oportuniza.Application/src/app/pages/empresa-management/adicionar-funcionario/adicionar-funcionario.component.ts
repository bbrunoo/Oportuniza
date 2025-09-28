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
  isLoading = true;
  isSubmitting = false;

  companyImageUrl: string | null = null;

  employeeName: string = '';
  employeeEmail: string = '';
  employeePassword: string = '';

  hidePassword = true;
  hideConfirmPassword = true;

  private readonly PASSWORD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$/;
  private readonly MIN_PASSWORD_LENGTH = 8;

  constructor(
    private companyService: CompanyService,
    private employeeService: CompanyEmployeeService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }

  validatePasswordComplex(password: string): boolean {
    return this.PASSWORD_REGEX.test(password);
  }

  private isValidEmail(email: string): boolean {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailPattern.test(email);
  }

  ngOnInit(): void {
    const parentId = this.route.parent?.snapshot.paramMap.get('id');

    if (parentId) {
      this.empresaId = parentId;
      this.loadCompanyImage(this.empresaId);
    } else {
      this.isLoading = false;
      Swal.fire('Erro', 'ID da Empresa não encontrado na URL.', 'error');
      console.error('ID da Empresa não encontrado na URL.');
    }
  }

  togglePasswordVisibility(field: 'password' | 'confirm'): void {
    if (field === 'password') {
      this.hidePassword = !this.hidePassword;
    } else if (field === 'confirm') {
      this.hideConfirmPassword = !this.hideConfirmPassword;
    }
  }

  loadCompanyImage(id: string): void {
    this.companyService.getCompanyById(id).subscribe({
      next: (data: CompanyDto) => {
        this.companyImageUrl = data.imageUrl;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.companyImageUrl = 'assets/placeholder-company.png';
        Swal.fire('Aviso', 'Não foi possível carregar o logo da empresa. Usando imagem padrão.', 'warning');
      }
    });
  }

  isFormValid(): boolean {
    const isBaseValid = (
      !!this.employeeName &&
      !!this.employeeEmail &&
      this.employeePassword.length >= this.MIN_PASSWORD_LENGTH
    );

    const isPasswordComplex = this.validatePasswordComplex(this.employeePassword);

    return isBaseValid && isPasswordComplex;
  }

  onSave(): void {
    if (this.isSubmitting || !this.empresaId) {
      return;
    }

    if (!this.employeeName || !this.employeeEmail || !this.employeePassword) {
      Swal.fire('Atenção', 'Por favor, preencha todos os campos obrigatórios (Nome, Email, Senha).', 'warning');
      return;
    }

    if (!this.isValidEmail(this.employeeEmail)) {
      Swal.fire('Email Inválido', 'O formato do endereço de email fornecido não é válido.', 'warning');
      return;
    }

    if (!this.validatePasswordComplex(this.employeePassword)) {
      Swal.fire(
        'Senha Fraca',
        `A senha deve ter no mínimo ${this.MIN_PASSWORD_LENGTH} caracteres e incluir letras maiúsculas, minúsculas, números e símbolos.`,
        'warning'
      );
      return;
    }

    this.isSubmitting = true;
    this.isLoading = true;

    const payload = {
      email: this.employeeEmail,
      password: this.employeePassword,
      companyId: this.empresaId,
      employeeName: this.employeeName,
      imageUrl: this.companyImageUrl || '../../../../assets/etapas/perfil.png'
    };

    this.employeeService.registerEmployee(payload).subscribe({
      next: () => {
        Swal.fire('Sucesso!', 'Funcionário registrado e vinculado com sucesso.', 'success');
        this.isSubmitting = false;
        this.isLoading = false;
        this.router.navigate(['../funcionarios'], { relativeTo: this.route });
      },
      error: (err) => {
        const errorStatus = err.status;
        let errorMessage = 'Falha ao registrar o funcionário. Tente novamente mais tarde.';

        if (errorStatus === 409) {
          errorMessage = 'Este email já está sendo utilizado por outro usuário.';
        } else if (errorStatus === 403) {
          errorMessage = 'Você não tem permissão para realizar esta ação.';
        }

        Swal.fire('Erro', errorMessage, 'error');
        this.isSubmitting = false;
        this.isLoading = false;
      }
    });
  }
}
