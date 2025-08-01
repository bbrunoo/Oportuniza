import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from "@angular/router";
import { MatDialogRef } from '@angular/material/dialog';
import { UsersAPIResponse } from 'stream-chat';
import { UserService } from '../../services/user.service';
import { CommonModule } from '@angular/common';
import { KeycloakOperationService } from '../../services/keycloak.service';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-configs',
  imports: [CommonModule],
  templateUrl: './configs.component.html',
  styleUrl: './configs.component.css'
})

export class ConfigsComponent implements OnInit {
  showCompleteProfileButton = true;

  constructor(private authService: AuthService, private router: Router, public dialogRef: MatDialogRef<ConfigsComponent>, private msalSerivce: MsalService, private userService: UserService, private keycloakService: KeycloakOperationService) { }

  containerHeight = '200px';

  ngOnInit(): void {
    this.userService.getOwnProfile().subscribe({
      next: (profile) => {
        this.showCompleteProfileButton = !profile.isProfileCompleted;

        this.containerHeight = this.showCompleteProfileButton ? '200px' : '130px';
      },
      error: (err) => {
        console.error("Erro ao buscar perfil", err);
        this.showCompleteProfileButton = false;
        this.containerHeight = '140px';
      }
    });
  }


  async logout() {
    localStorage.clear();
    sessionStorage.clear();
    this.router.navigate(['/']);
    this.dialogRef.close();
  }

  completePerfil() {
    this.router.navigate(['/primeira-etapa']);
    this.dialogRef.close();
  }

  changeAccount() {
    this.authService.logout()
    this.router.navigate(['']);
    this.dialogRef.close();
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
