import { User } from './../../models/User.model';
import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  passwordVisible = false;
  User: User = { name:'user', email: '', password: '', isACompany: false };
  isLoading: boolean = false;
  errorMessage: string = '';

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  constructor(private authService:AuthService, private router: Router){}

  login() {

    this.authService.login(this.User).subscribe(
      (response) => {
        console.log('logged with success');
        this.router.navigate(["/primeira-etapa"])
        this.authService.setToken(response.token)
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
