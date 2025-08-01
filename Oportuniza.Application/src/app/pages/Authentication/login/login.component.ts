import { KeycloakService } from 'keycloak-angular';
import { User } from '../../../models/User.model';
import { Component, Inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { MSAL_GUARD_CONFIG, MsalGuardConfiguration, MsalService } from '@azure/msal-angular';
import { RedirectRequest } from '@azure/msal-browser';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})

export class LoginComponent {
  passwordVisible = false;
  email: string = '';
  password: string = '';
  isLoading = false;
  loginDisplay = false;

  constructor(private keycloakService: KeycloakOperationService, private router: Router, private msalService: MsalService, @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration) { }

  microsoftLogin() {
    sessionStorage.removeItem('loginWithKeycloak');
    sessionStorage.setItem('loginWithMicrosoft', 'true');
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

  public login(): void {
    this.isLoading = true;
    sessionStorage.removeItem('loginWithMicrosoft');
    sessionStorage.setItem('loginWithKeycloak', 'true');

    this.keycloakService.loginWithCredentials(this.email, this.password).subscribe(
      async (res: any) => {
        console.log('Login realizado com sucesso:', res);

        sessionStorage.setItem('access_token', res.access_token);
        sessionStorage.setItem('refresh_token', res.refresh_token);
        sessionStorage.setItem('id_token', res.id_token);

        let userEmailToSave: string = this.email;

        if (res.id_token) {
          try {
            const decodedIdToken = JSON.parse(atob(res.id_token.split('.')[1]));
            userEmailToSave = decodedIdToken.email || this.email;
            console.log('Email do ID Token:', userEmailToSave);
          } catch (e) {
            console.error('Erro ao decodificar ID Token para extrair email:', e);
          }
        }

        this.isLoading = false;
        this.router.navigate(["/inicio"]);
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
