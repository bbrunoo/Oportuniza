import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { User } from '../../models/User.model';
import { AuthService } from '../../services/auth.service';
import Swal from "sweetalert2";

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

  constructor(private authService: AuthService, private router: Router) {
    this.restoreTermsAcceptance();
    this.restoreFormData();

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

  saveTermsAcceptance() {
    localStorage.setItem('acceptTerms', JSON.stringify(this.acceptTerms));
  }

  restoreTermsAcceptance() {
    const storedAcceptance = localStorage.getItem('acceptTerms');
    if (storedAcceptance) {
      this.acceptTerms = JSON.parse(storedAcceptance);
    }
  }

  saveFormData() {
    localStorage.setItem('cadastroEmail', this.email);
    localStorage.setItem('cadastroPassword', this.password);
    localStorage.setItem('cadastroConfirmPassword', this.confirmPassword);
  }

  restoreFormData() {
    const savedEmail = localStorage.getItem('cadastroEmail');
    const savedPassword = localStorage.getItem('cadastroPassword');
    const savedConfirmPassword = localStorage.getItem('cadastroConfirmPassword');

    if (savedEmail) this.email = savedEmail;
    if (savedPassword) this.password = savedPassword;
    if (savedConfirmPassword) this.confirmPassword = savedConfirmPassword;
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
    isACompany: false,
  };

  this.isLoading = true;

  this.authService.register(newUser).subscribe(
    () => {
      localStorage.clear();
      Swal.fire({
        icon: 'success',
        title: 'Cadastro realizado!',
        text: 'Sua conta foi criada com sucesso.',
        timer: 2000,
        showConfirmButton: false
      }).then(() => {
        this.router.navigate(['/login']);
      });
    },
    (error) => {
      this.isLoading = false;
      if (error.status === 400) {
        this.errorMessage = error.error || 'Dados inválidos. Verifique e tente novamente.';
      } else if (error.status === 409) {
        this.errorMessage = 'Este email já está cadastrado.';
      } else {
        this.errorMessage = 'Erro inesperado. Tente novamente mais tarde.';
      }

      Swal.fire({
        icon: 'error',
        title: 'Erro ao cadastrar',
        text: this.errorMessage
      });
    }
  );
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

      // Se a condição passou e ainda não estava marcada
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
}
