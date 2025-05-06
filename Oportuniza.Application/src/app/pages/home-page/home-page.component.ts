import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ProfileService } from '../../services/profile.service';
import { RouterLink, RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, RouterModule],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css'
})

export class HomePageComponent implements OnInit{
  userId: string | null = null;
  email : string | null = null;
  fullName : string | null = null;

  constructor(private authService: AuthService, private profileService: ProfileService) {}

  ngOnInit(): void {
    const userData = this.authService.getUserData();

    if (userData) {
      this.userId = userData.id;
      this.email = userData.email;
    }
    this.getUserProfile(userData.id);
  }

  getUserProfile(userId: string){
    this.profileService.getUserProfileData(userId).subscribe({
      next: (data) => {
        this.fullName = data.fullName;
      },
      error: (error: any) => {error.message}
    })
  }

  logout() {
    this.authService.logout();
  }
}
