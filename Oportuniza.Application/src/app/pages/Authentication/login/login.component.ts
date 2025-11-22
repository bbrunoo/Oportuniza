import { Component, Inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})

export class LoginComponent {
  passwordVisible = false;
  email: string = '';
  password: string = '';
  isLoading = false;

  passwordsMatch: boolean = false;
  passwordsMatchStatus: boolean = false;
  passwordMatchTimeout: any = null;

  passwordCriteria = {
    hasLowercase: false,
    hasUppercase: false,
    hasNumber: false,
    hasSymbol: false,
    hasMinLength: false,
    equalsPassword: false,
  };

  passwordStatus = {
    hasLowercase: false,
    hasUppercase: false,
    hasNumber: false,
    hasSymbol: false,
    hasMinLength: false,
    equalsPassword: false,
  };

  timeoutMap: any = {};

  criteriaList: {
    key: keyof (typeof LoginComponent.prototype)['passwordCriteria'];
    message: string;
  }[] = [];

  constructor(
    private keycloakService: KeycloakOperationService,
    private router: Router
  ) { }

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  sanitizeEmail() {
    this.email = this.email
      .replace(/\s/g, '')
      .replace(/[^a-zA-Z0-9@._%+-]/g, '')
      .trim();
  }

  sanitizePassword() {
    this.password = this.password
      .replace(/\s/g, '')
      .replace(/[^\x00-\x7F]/g, '')
      .replace(/[^A-Za-z0-9!@#$%^&*()_\-+=\[\]{};:'",.<>?/\\|`~]/g, '');
  }

  validateEmail(email: string): boolean {
    const emailPattern =
      /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailPattern.test(email);
  }

  onEmailInput(event: any) {
    const input = event.target as HTMLInputElement;

    input.value = input.value
      .replace(/\s/g, '')
      .replace(/[^a-zA-Z0-9@._-]/g, '');

    this.email = input.value;
  }

  onPasswordInput(event?: any) {
    if (event) {
      const input = event.target as HTMLInputElement;

      input.value = input.value.replace(/[^A-Za-z0-9!#@$%&.]/g, '');
      this.password = input.value;
    }

    const pwd = this.password;

    const checks = {
      hasLowercase: /[a-z]/.test(pwd),
      hasUppercase: /[A-Z]/.test(pwd),
      hasNumber: /\d/.test(pwd),
      hasSymbol: /[!#@$%&]/.test(pwd),
      hasMinLength: pwd.length >= 8,
    };

    Object.entries(checks).forEach(([key, value]) => {
      const typedKey = key as keyof typeof this.passwordCriteria;
      if (value && !this.passwordCriteria[typedKey]) {
        this.passwordStatus[typedKey] = true;
        if (this.timeoutMap[typedKey]) clearTimeout(this.timeoutMap[typedKey]);
        this.timeoutMap[typedKey] = setTimeout(() => {
          this.passwordStatus[typedKey] = false;
        }, 400);
      }
      this.passwordCriteria[typedKey] = value;
    });
  }

  validatePassword(password: string): boolean {
    const passwordPattern =
      /^(?!.*\s)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'",.<>?/\\|`~]).{8,64}$/;
    return passwordPattern.test(password);
  }

  isPasswordForbidden(password: string): boolean {
    const lower = password.toLowerCase();
    const forbidden = [
      '123456', 'password', 'senha', 'admin', 'qwerty',
      'abc123', 'letmein', 'welcome', '111111', '000000'
    ];

    if (forbidden.some(p => lower.includes(p))) return true;
    if (/(.)\1{3,}/.test(password)) return true;
    return false;
  }

  async login(): Promise<void> {
    this.isLoading = true;
    this.sanitizeEmail();
    this.sanitizePassword();

    if (!this.email || !this.validateEmail(this.email)) {
      Swal.fire('E-mail inválido', 'Insira um e-mail válido, sem espaços ou caracteres especiais.', 'warning');
      this.isLoading = false;
      return;
    }

    if (!this.password) {
      Swal.fire('Senha obrigatória', 'Por favor, insira sua senha.', 'warning');
      this.isLoading = false;
      return;
    }

    try {
      sessionStorage.setItem('loginWithKeycloak', 'true');

      const res = await this.keycloakService
        .loginWithCredentials(this.email, this.password)
        .toPromise();

      this.keycloakService.setNewAccessToken(res.access_token || res.token);
      this.router.navigate(['/troca']);
    } catch (err: any) {

      const msg = this.extractErrorMessage(err);

      Swal.fire({
        icon: 'error',
        title: 'Atenção',
        text: msg,
        confirmButtonText: 'Ok'
      });

      sessionStorage.removeItem('loginWithKeycloak');
    } finally {
      this.isLoading = false;
    }
  }

  private extractErrorMessage(err: any): string {

    if (err?.status === 429) {
      if (err?.error?.error) {
        return err.error.error;
      }
      return 'Muitas tentativas. Aguarde antes de tentar novamente.';
    }

    if (err?.error?.error) {
      return err.error.error;
    }

    if (typeof err?.error === 'string') {
      try {
        const parsed = JSON.parse(err.error);
        return parsed.error || 'Erro inesperado.';
      } catch {
        return err.error;
      }
    }

    if (err?.message) {
      return err.message;
    }

    return 'Erro inesperado.';
  }
}
