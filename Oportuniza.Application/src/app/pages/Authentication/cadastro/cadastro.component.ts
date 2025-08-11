import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2';
import { MatDialog } from '@angular/material/dialog';
import { TermosModalComponent } from '../../../extras/termos-modal/termos-modal.component';
import { firstValueFrom } from 'rxjs';
import {
  MSAL_GUARD_CONFIG,
  MsalGuardConfiguration,
  MsalService,
} from '@azure/msal-angular';
import { RedirectRequest } from '@azure/msal-browser';
import { KeycloakOperationService } from '../../../services/keycloak.service';

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
    private msalService: MsalService,
    @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration
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

  microsoftLogin() {
    if (this.msalGuardConfig.authRequest) {
      this.msalService.loginRedirect({
        ...this.msalGuardConfig.authRequest,
      } as RedirectRequest);
    } else {
      this.msalService.loginRedirect();
    }
  }

  setLoginDisplay() {
    this.loginDisplay = this.msalService.instance.getAllAccounts().length > 0;
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
    const passwordPattern =
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$/;
    return passwordPattern.test(password);
  }

  async proceedRegistration() {
    this.isLoading = true;

    try {
      const response = await firstValueFrom(
        this.keyAuth.registerUser({
          email: this.email,
          password: this.password,
        })
      );
      console.log('Resposta do registro:', response);

      console.log('Usuário registrado com sucesso no Keycloak!', response);

      const tokens = await firstValueFrom(
        this.keyAuth.loginWithCredentials(this.email, this.password)
      );
      console.log('Login bem-sucedido, tokens recebidos:', tokens);

      this.keyAuth.saveTokens(tokens);

      await Swal.fire({
        icon: 'success',
        title: 'Cadastro realizado!',
        text: 'Seu cadastro foi efetuado com sucesso. Você será redirecionado.',
        timer: 2000,
        showConfirmButton: false,
      });

      this.router.navigate(['/inicio']);
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

    if (!this.email || !this.validateEmail(this.email)) {
      Swal.fire({
        icon: 'warning',
        title: 'Email inválido',
        text: 'Por favor, insira um e-mail válido.',
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
      this.openModal()
    } else {
      this.proceedRegistration();
    }
  }

  onPasswordInput() {
    const pwd = this.password;

    const checks = {
      hasLowercase: /[a-z]/.test(pwd),
      hasUppercase: /[A-Z]/.test(pwd),
      hasNumber: /\d/.test(pwd),
      hasSymbol: /[^\w\s]/.test(pwd),
      hasMinLength: pwd.length >= 8,
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

    this.passwordCriteria.equalsPassword = match;

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
