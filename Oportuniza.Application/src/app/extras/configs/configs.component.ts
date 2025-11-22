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
      }
    });
  }

  private loadUserProfile(): void {
    this.userService.getOwnProfile().subscribe({
      next: (profile) => {
        this.containerHeight = '130px';
      },
      error: (err) => {
        console.error("Erro ao buscar perfil", err);

        this.containerHeight = '200px';
      }
    });
  }

  async logout(): Promise<void> {
    this.dialogRef.close();
    this.router.navigate(['']);
    await this.keycloakService.logout();
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
