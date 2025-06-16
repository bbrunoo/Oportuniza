import { User } from './../../models/User.model';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  passwordVisible = false;
  User: User = { name: 'user', email: '', password: '' };
  isLoading = false;

  constructor(private authService: AuthService, private router: Router) { }

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  login() {
  if (
    !this.User.email?.trim() ||
    !this.User.password?.trim()
  ) {
    Swal.fire({
      icon: 'warning',
      title: 'Campos obrigatórios',
      text: 'Preencha todos os campos antes de continuar.'
    });
    return;
  }

  this.isLoading = true;
  this.authService.login(this.User).subscribe({
    next: (response) => {
      this.authService.setToken(response.token);
      Swal.fire({
        icon: 'success',
        title: 'Login realizado com sucesso!',
        timer: 1500,
        showConfirmButton: false
      }).then(() => {
        this.router.navigate(['/inicio']);
      });
    },
    error: (error) => {
      this.isLoading = false;
      let message = 'Ocorreu um erro ao tentar realizar o login. Tente novamente mais tarde.';

      if (error.status === 400) {
        message = 'Usuário não existe.';
      } else if (error.status === 401) {
        message = 'Usuário ou senha inválido.';
      } else if (error.status === 423) {
        message = 'Usuário bloqueado por muitas tentativas!';
      } else if (error.status === 422) {
        message = 'Preencha todos os campos!';
      }

      Swal.fire({
        icon: 'error',
        title: 'Erro',
        text: message
      });

      console.error('Erro ao logar:', error);
    }
  });
}
}
