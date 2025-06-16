import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { ConfigsComponent } from '../../../extras/configs/configs.component';

@Component({
  selector: 'app-initial-layout',
  imports: [RouterOutlet, CommonModule, RouterLink],
  templateUrl: './initial-layout.component.html',
  styleUrl: './initial-layout.component.css'
})
export class InitialLayoutComponent implements OnInit {
  userProfile!: UserProfile;

  constructor(private userService: UserService, private dialog: MatDialog) { }

  ngOnInit() {
    this.getLoggedUserProfile()
  }

  getLoggedUserProfile() {
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

  openDialog() {
    const dialogRef = this.dialog.open(ConfigsComponent, {
      minWidth: '230px',
      minHeight: '130px',
      position: {
        bottom: '80px',
        left: '130px'
      },
      panelClass: 'custom-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('fechou');
      }
    });
  }
}
