import { Component } from '@angular/core';
import { ProfileService } from '../../../services/profile.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-meuperfil',
  imports: [FormsModule,CommonModule],
  templateUrl: './meuperfil.component.html',
  styleUrl: './meuperfil.component.css'
})
export class MeuperfilComponent {
  userProfile: UserProfile | null = null;
  isLoading: boolean = true;
  errorMessage: string | null = null;

  constructor(private profileService: ProfileService) { }

  ngOnInit(): void {
    this.fetchUserProfile();
  }

  fetchUserProfile(): void {
    this.profileService.getMyProfile().subscribe({
      next: (profile: UserProfile) => {
        this.userProfile = profile;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Não foi possível carregar o perfil. Por favor, tente novamente.';
        this.isLoading = false;
        console.error('Erro ao buscar perfil:', error);
      }
    });
  }
}
