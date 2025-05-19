import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ProfileService } from '../../services/profile.service';
import { AllUsersInfoModel } from '../../models/AllUsersInfo.model';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterModule } from '@angular/router';

@Component({
  selector: 'app-lista-contas',
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './lista-contas.component.html',
  styleUrl: './lista-contas.component.css'
})
export class ListaContasComponent implements OnInit {

  constructor(private authService: AuthService, private profileService: ProfileService, private router: Router) {}

  userProfiles: AllUsersInfoModel[] = [];

  ngOnInit(): void {
    this.profileService.getAllProfiles().subscribe({
      next: (data) => {
        this.userProfiles = data;
      },
      error: (err) => {
        console.error('Erro ao buscar perfis:', err);
      }
    });
  }

  logout() {
    this.authService.logout();
  }

  iniciarConversa(usuarioId: string) {
    localStorage.setItem('chatTargetUserId', usuarioId);

    this.router.navigate(['/chat']);
  }

  getFirstTwoNames(fullName: string): string {
    if (!fullName) return '';
    const names = fullName.trim().split(' ');
    return names.slice(0, 2).join(' ').toUpperCase();
  }
}
