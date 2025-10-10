import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router, RouterLink } from "@angular/router";
import { MatDialogRef } from '@angular/material/dialog';
import { UserService } from '../../services/user.service';
import { CommonModule } from '@angular/common';
import { KeycloakOperationService } from '../../services/keycloak.service';

@Component({
  selector: 'app-configs',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './configs.component.html',
  styleUrl: './configs.component.css'
})

export class ConfigsComponent implements OnInit {
  showCompleteProfileButton = true;
  containerHeight = '200px';

  constructor(
    private authService: AuthService,
    private router: Router,
    public dialogRef: MatDialogRef<ConfigsComponent>,
    private userService: UserService,
    private keycloakService: KeycloakOperationService
  ) { }

  ngOnInit(): void {
    this.keycloakService.isLoggedIn().then(isLoggedIn => {
      if (isLoggedIn) {
        this.loadUserProfile();
      } else {
        this.showCompleteProfileButton = false;
      }
    });
  }

  private loadUserProfile(): void {
    this.userService.getOwnProfile().subscribe({
      next: (profile) => {
        this.showCompleteProfileButton = !profile.isProfileCompleted && !profile.isCompany;
        this.containerHeight = this.showCompleteProfileButton ? '200px' : '130px';
      },
      error: (err) => {
        console.error("Erro ao buscar perfil", err);

        this.showCompleteProfileButton = true;
        this.containerHeight = '200px';
      }
    });
  }

  async logout(): Promise<void> {
    this.dialogRef.close();
    this.router.navigate(['']);
    await this.keycloakService.logout();
  }

  completePerfil(): void {
    this.router.navigate(['/primeira-etapa']);
    this.dialogRef.close();
  }

  async changeAccount(): Promise<void> {
    this.dialogRef.close();
    this.router.navigate(['/login']);
    await this.keycloakService.logout();
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
