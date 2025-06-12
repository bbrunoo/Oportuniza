import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { User } from '../../models/User.model';
import { AuthService } from '../../services/auth.service';
import Swal from "sweetalert2";
import { MatDialog } from '@angular/material/dialog';
import { ModalComponent } from 'stream-chat-angular';
import { TermosModalComponent } from '../../auxiliar/termos-modal/termos-modal.component';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cadastro.component.html',
  styleUrl: './cadastro.component.css',
})
export class CadastroComponent {
  passwordVisible: boolean = false;
  confirmPasswordVisible: boolean = false;
  isLoading: boolean = false;
  errorMessage: string = '';
  acceptTerms: boolean = false;
  termsAcceptedInternally: boolean = false;

  email: string = '';
  password: string = '';
  confirmPassword: string = '';

  passwordsMatch: boolean = false;
  passwordsMatchStatus: boolean = false;
  passwordMatchTimeout: any = null;

  passwordCriteria = {
    hasLowercase: false,
    hasUppercase: false,
    hasNumber: false,
    hasSymbol: false,
    hasMinLength: false
  };

  passwordStatus = {
    hasLowercase: false,
    hasUppercase: false,
    hasNumber: false,
    hasSymbol: false,
    hasMinLength: false
  };

  timeoutMap: any = {};

  criteriaList: { key: keyof typeof CadastroComponent.prototype['passwordCriteria']; message: string }[] = [];

  constructor(private authService: AuthService, private router: Router, private dialog: MatDialog) {

    this.criteriaList = [
      { key: 'hasLowercase', message: 'A senha deve conter letras minúsculas' },
      { key: 'hasUppercase', message: 'A senha deve conter letras maiúsculas' },
      { key: 'hasNumber', message: 'A senha deve conter números' },
      { key: 'hasSymbol', message: 'A senha deve conter símbolos (ex: @, #, $)' },
      { key: 'hasMinLength', message: 'A senha deve ter no mínimo 8 caracteres' }
    ];
  }

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  toggleConfirmPassword() {
    this.confirmPasswordVisible = !this.confirmPasswordVisible;
  }

  validateEmail(email: string): boolean {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailPattern.test(email);
  }

  validatePassword(password: string): boolean {
    const passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$/;
    return passwordPattern.test(password);
  }

  register() {
    this.errorMessage = '';

    if (!this.email || !this.validateEmail(this.email)) {
      Swal.fire({
        icon: 'warning',
        title: 'Email inválido',
        text: 'Por favor, insira um e-mail válido.'
      });
      return;
    }

    if (!this.validatePassword(this.password)) {
      Swal.fire({
        icon: 'warning',
        title: 'Senha inválida',
        text: 'A senha deve conter no mínimo 8 caracteres, com letras maiúsculas, minúsculas, números e símbolos.'
      });
      return;
    }

    if (this.password !== this.confirmPassword) {
      Swal.fire({
        icon: 'warning',
        title: 'Senhas diferentes',
        text: 'As senhas não coincidem.'
      });
      return;
    }

    if (!this.acceptTerms) {
      Swal.fire({
        icon: 'info',
        title: 'Termos não aceitos',
        text: 'Você precisa aceitar os Termos de Uso e a Política de Privacidade.'
      });
      return;
    }

    const newUser: User = {
      name: this.email.split('@')[0],
      email: this.email,
      password: this.password,
    };

    this.isLoading = true;

    this.authService.register(newUser).subscribe({
      next: (response: any) => {
        localStorage.clear();
        localStorage.setItem("userId", response.id);

        this.authService.login({ email: newUser.email, password: newUser.password }).subscribe({
          next: (loginResponse: any) => {
            sessionStorage.setItem('access_token', loginResponse.token);
            Swal.fire({
              icon: 'success',
              title: 'Cadastro realizado!',
              text: 'Sua conta foi criada com sucesso.',
              timer: 2000,
              showConfirmButton: false
            }).then(() => {
              this.router.navigate(['/primeira-etapa']);
            });
          },
          error: (error) => {
            this.isLoading = false;
            let message = 'Erro inesperado. Tente novamente mais tarde.';

            if (error.status === 400) {
              message = 'Dados inválidos. Verifique e tente novamente.';
            } else if (error.status === 409) {
              message = 'Este email já está cadastrado.';
            }

            Swal.fire({
              icon: 'error',
              title: 'Erro ao fazer login',
              text: message
            });
          }
        });
      },
      error: (error) => {
        this.isLoading = false;
        let message = 'Erro inesperado. Tente novamente mais tarde.';

        if (error.status === 400) {
          message = 'Dados inválidos. Verifique e tente novamente.';
        } else if (error.status === 409) {
          message = 'Este email já está cadastrado.';
        }

        Swal.fire({
          icon: 'error',
          title: 'Erro ao cadastrar',
          text: message
        });
      }
    });

  }

  onPasswordInput() {
    const pwd = this.password;

    const checks = {
      hasLowercase: /[a-z]/.test(pwd),
      hasUppercase: /[A-Z]/.test(pwd),
      hasNumber: /\d/.test(pwd),
      hasSymbol: /[^\w\s]/.test(pwd),
      hasMinLength: pwd.length >= 8
    };

    Object.entries(checks).forEach(([key, value]) => {
      const typedKey = key as keyof typeof this.passwordCriteria;

      if (value && !this.passwordCriteria[typedKey]) {
        this.passwordStatus[typedKey] = true;
        if (this.timeoutMap[typedKey]) clearTimeout(this.timeoutMap[typedKey]);
        this.timeoutMap[typedKey] = setTimeout(() => {
          this.passwordStatus[typedKey] = false;
        }, 500);
      }

      this.passwordCriteria[typedKey] = value;
    });

    this.checkPasswordsMatch();
  }

  onConfirmPasswordInput() {
    this.checkPasswordsMatch();
  }

  checkPasswordsMatch() {
    const match = this.password === this.confirmPassword;
    if (match && !this.passwordsMatch) {
      this.passwordsMatchStatus = true;
      if (this.passwordMatchTimeout) clearTimeout(this.passwordMatchTimeout);
      this.passwordMatchTimeout = setTimeout(() => {
        this.passwordsMatchStatus = false;
      }, 500);
    }
    this.passwordsMatch = match;
  }

  isPasswordValid(): boolean {
    return Object.values(this.passwordCriteria).every(Boolean);
  }

  canSubmit(): boolean {
    return (
      this.validateEmail(this.email) &&
      this.isPasswordValid() &&
      this.passwordsMatch &&
      this.acceptTerms
    );
  }

  getPasswordStatus(key: keyof typeof this.passwordStatus): boolean {
    return this.passwordStatus[key];
  }

  getPasswordCriteria(key: keyof typeof this.passwordCriteria): boolean {
    return this.passwordCriteria[key];
  }

  openModal() {
    const dialogRef = this.dialog.open(TermosModalComponent, {
      width: '35em',
      height: '32em',
      maxWidth: 'none',
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'aceito') {
        this.acceptTerms = true;
        this.termsAcceptedInternally = true;
      }
    });
  }
}
