import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgModel } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { User } from '../../models/User.model';
import { AuthService } from '../../services/auth.service';
import { response } from 'express';
import { error } from 'console';

@Component({
  selector: 'app-cadastro',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './cadastro.component.html',
  styleUrl: './cadastro.component.css',
})
export class CadastroComponent {
  setPassword: string = '';
  confirmPassword: string = '';
  acceptTerms: boolean = false;
  passwordVisible: boolean = false;
  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(private authService: AuthService, private router:Router) {}

  User: User = { name:'user', email: '', password: '', isACompany: false };

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  register() {
    if (this.setPassword !== this.confirmPassword) {
      alert('As senhas não coincidem.');
      return;
    }

    this.authService.register(this.User).subscribe(
      (response) => {
        console.log('success');
        console.log(this.User.email);
        console.log(this.User.password);
        this.router.navigate(["/login"])
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
