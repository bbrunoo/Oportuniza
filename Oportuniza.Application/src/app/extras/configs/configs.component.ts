import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from "@angular/router";
import { MatDialogRef } from '@angular/material/dialog';
import { UserService } from '../../services/user.service';
import { CommonModule } from '@angular/common';
import { KeycloakOperationService } from '../../services/keycloak.service';

@Component({
  selector: 'app-configs',
  standalone: true,
  imports: [CommonModule],
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

  async logout(): Promise<void> {
    this.dialogRef.close();
    await this.keycloakService.logout();
  }

  completePerfil(): void {
    this.router.navigate(['/primeira-etapa']);
    this.dialogRef.close();
  }

  changeAccount(): void {
    this.router.navigate(['']);
    this.dialogRef.close();
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
