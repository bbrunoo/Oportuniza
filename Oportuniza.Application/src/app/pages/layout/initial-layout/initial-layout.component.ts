import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { HomeExtrasComponent } from '../../../extras/home-extras/home-extras.component';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';

@Component({
  selector: 'app-initial-layout',
  imports: [RouterOutlet, CommonModule, RouterLink],
  templateUrl: './initial-layout.component.html',
  styleUrl: './initial-layout.component.css'
})
export class InitialLayoutComponent implements OnInit{
  userProfile!: UserProfile;

  constructor(private userService: UserService) { }

  ngOnInit(){
    this.getLoggedUserProfile()
  }

  getLoggedUserProfile(){
    this.userService.getOwnProfile().subscribe({
      next: (profile: UserProfile) => {
        this.userProfile = profile;
        console.log("dados do usuario logado", profile);
      },
      error: (error: any) => {
        console.error(error);
      }
    })
  }
}
