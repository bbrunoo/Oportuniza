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

  constructor(
    private keycloakService: KeycloakOperationService,
    private router: Router
  ) { }

  togglePassword() {
    this.passwordVisible = !this.passwordVisible;
  }

  sanitizeEmail() {
    this.email = this.email.replace(/[^a-zA-Z0-9@._%+-]/g, '');
  }

  sanitizePassword() {
    this.password = this.password.replace(/[^a-zA-Z0-9!@#$%^&*()_+=\-{}\[\]:;"'<>,.?/|\\~`]/g, '');
  }

  public login(): void {
    this.isLoading = true;
    sessionStorage.setItem('loginWithKeycloak', 'true');

    this.keycloakService.loginWithCredentials(this.email, this.password).subscribe(
      async (res: any) => {
        console.log('Login realizado com sucesso:', res);
        this.isLoading = false;

        this.keycloakService.setNewAccessToken(res.access_token || res.token);

        this.router.navigate(["/troca"]);
      },
      (err) => {
        console.error('Erro no login:', err);
        Swal.fire('Erro!', 'Falha no login. Verifique suas credenciais.', 'error');
        sessionStorage.removeItem('loginWithKeycloak');
        this.isLoading = false;
      }
    );
  }
}
