import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { User } from '../../models/User.model';
import { AuthService } from '../../services/auth.service';

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

  constructor(private authService: AuthService, private router: Router) {
    this.restoreTermsAcceptance();
    this.restoreFormData();
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
    if (!this.email || !this.validateEmail(this.email)) {
      alert('Por favor, insira um e-mail válido.');
      return;
    }

    if (this.password.length < 6) {
      alert('A senha deve ter pelo menos 6 caracteres.');
      return;
    }

    if (this.password !== this.confirmPassword) {
      alert('As senhas não coincidem.');
      return;
    }

    if (!this.acceptTerms) {
      alert('Você precisa aceitar os Termos de Uso e a Política de Privacidade.');
      return;
    }

    const newUser: User = {
      name: 'user',
      email: this.email,
      password: this.password,
      isACompany: false,
    };

    this.authService.register(newUser).subscribe(
      (response) => {
        console.log('success');
        localStorage.clear();
        this.router.navigate(['/login']);
      },
      (error) => {
        if (error.status === 400) {
          this.errorMessage = 'Usuário não existe.';
        } else if (error.status === 401) {
          this.errorMessage = 'Usuário ou senha inválido.';
        } else {
          this.errorMessage = 'Ocorreu um erro ao tentar realizar o login. Tente novamente mais tarde.';
        }
        console.log('Error', error);
      }
    );
  }
}
