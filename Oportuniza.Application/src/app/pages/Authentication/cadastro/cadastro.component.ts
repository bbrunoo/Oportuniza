import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2';
import { MatDialog } from '@angular/material/dialog';
import { TermosModalComponent } from '../../../extras/termos-modal/termos-modal.component';
import { firstValueFrom } from 'rxjs';
import { KeycloakOperationService } from '../../../services/keycloak.service';
import { VerificationService } from '../../../services/verification.service';

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
  loginDisplay = false;

  email: string = '';
  name: string = '';
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
    key: keyof (typeof CadastroComponent.prototype)['passwordCriteria'];
    message: string;
  }[] = [];

  constructor(
    private router: Router,
    private dialog: MatDialog,
    private keyAuth: KeycloakOperationService,
    private verificationService: VerificationService
  ) {
    this.criteriaList = [
      { key: 'hasLowercase', message: 'A senha deve conter letras minúsculas' },
      { key: 'hasUppercase', message: 'A senha deve conter letras maiúsculas' },
      { key: 'hasNumber', message: 'A senha deve conter números' },
      {
        key: 'hasSymbol',
        message: 'A senha deve conter símbolos (ex: @, #, $)',
      },
      {
        key: 'hasMinLength',
        message: 'A senha deve ter no mínimo 8 caracteres',
      },
      { key: 'equalsPassword', message: 'As senhas devem ser iguais' },
    ];
  }

  sanitizeInput(value: string): string {
    return value.replace(/[<>{}()'"`;$]/g, '').trim();
  }

  onNameInput(event: any) {
    const input = event.target as HTMLInputElement;
    input.value = input.value.replace(/[^A-Za-zÀ-ÿ\s]/g, '');
    this.name = input.value;
  }

  onEmailInput(event: any) {
    const input = event.target as HTMLInputElement;

    input.value = input.value
      .replace(/\s/g, '')
      .replace(/[^a-zA-Z0-9@._-]/g, '');

    this.email = input.value;
  }

  validatePassword(password: string): boolean {
    const passwordPattern =
      /^(?!.*\s)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};:'",.<>?/\\|`~]).{8,64}$/;
    return passwordPattern.test(password);
  }

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  toggleConfirmPassword() {
    this.confirmPasswordVisible = !this.confirmPasswordVisible;
  }

  validateEmail(email: string): boolean {
    const emailPattern =
      /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailPattern.test(email);
  }

  async proceedRegistration() {
    this.isLoading = true;

    try {
      this.name = this.sanitizeInput(this.name);
      this.email = this.sanitizeInput(this.email);

      const response = await firstValueFrom(
        this.keyAuth.registerUser({
          name: this.name,
          email: this.email,
          password: this.password,
        })
      );

      await Swal.fire({
        icon: 'info',
        title: 'Verifique seu e-mail',
        text: 'Enviamos um código de verificação para o seu e-mail.',
        confirmButtonText: 'OK',
      });

      this.router.navigate(['/verify', this.email]);
    } catch (error) {
      console.error('Erro no processo de registro:', error);
      this.errorMessage =
        'Ocorreu um erro ao tentar realizar o cadastro. Verifique os dados ou tente novamente mais tarde.';

      Swal.fire({
        icon: 'error',
        title: 'Erro no Cadastro',
        text: this.errorMessage,
      });
    } finally {
      this.isLoading = false;
    }
  }

  async register() {
    this.errorMessage = '';

    this.name = this.sanitizeInput(this.name);
    this.email = this.sanitizeInput(this.email);

    if (!this.name || this.name.length < 2) {
      Swal.fire({
        icon: 'warning',
        title: 'Nome inválido',
        text: 'Por favor, insira um nome válido.',
      });
      return;
    }

    if (!this.email || !this.validateEmail(this.email)) {
      Swal.fire({
        icon: 'warning',
        title: 'Email inválido',
        text: 'Por favor, insira um e-mail válido.',
      });
      return;
    }

    if (!this.validatePassword(this.password)) {
      Swal.fire({
        icon: 'warning',
        title: 'Senha insegura',
        text: 'A senha não atende aos critérios de segurança.',
      });
      return;
    }

    if (this.password !== this.confirmPassword) {
      Swal.fire({
        icon: 'warning',
        title: 'Senhas diferentes',
        text: 'As senhas não coincidem.',
      });
      return;
    }

    if (!this.acceptTerms) {
      this.openModal();
      return;
    }

    this.proceedRegistration();
  }

  onPasswordInput(event?: any) {
    if (event) {
      const input = event.target as HTMLInputElement;
      input.value = input.value
        .replace(/\s/g, '')
        .replace(/[^\x00-\x7F]/g, '')
        .replace(/[^A-Za-z0-9!@#$%^&*()_\-+=\[\]{};:'",.<>?/\\|`~]/g, '');

      this.password = input.value;
    }

    const pwd = this.password;

    const checks = {
      hasLowercase: /[a-z]/.test(pwd),
      hasUppercase: /[A-Z]/.test(pwd),
      hasNumber: /\d/.test(pwd),
      hasSymbol: /[!@#$%^&*()_\-+=\[\]{};:'",.<>?/\\|`~]/.test(pwd),
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

    this.checkPasswordsMatch();
  }

  onConfirmPasswordInput() {
    this.checkPasswordsMatch();
  }

  checkPasswordsMatch() {
    const match = this.password === this.confirmPassword;
    this.passwordCriteria.equalsPassword = match;

    if (match) {
      this.passwordStatus.equalsPassword = true;
      clearTimeout(this.passwordMatchTimeout);
      this.passwordMatchTimeout = setTimeout(() => {
        this.passwordStatus.equalsPassword = false;
      }, 400);
    }
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
      height: '35em',
      maxWidth: 'none',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result === 'aceito') {
        this.acceptTerms = true;
        this.termsAcceptedInternally = true;
        this.proceedRegistration();
      }
    });
  }
}
