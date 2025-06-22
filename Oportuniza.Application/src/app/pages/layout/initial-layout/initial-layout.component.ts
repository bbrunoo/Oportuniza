// import { authConfig, useAuth } from './../../../authConfig';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { ConfigsComponent } from '../../../extras/configs/configs.component';
import { AuthenticationResult, EventMessage, EventType, InteractionStatus } from '@azure/msal-browser';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { filter } from 'rxjs';

@Component({
  selector: 'app-initial-layout',
  imports: [RouterOutlet, CommonModule, RouterLink],
  templateUrl: './initial-layout.component.html',
  styleUrl: './initial-layout.component.css'
})
export class InitialLayoutComponent implements OnInit {
  userProfile!: UserProfile;
  userName: string | null = null;
  username = '';
  loginDisplay = false;
  showCompleteProfileicon = true;

  constructor(private userService: UserService, private dialog: MatDialog, private authService: MsalService, private msalBroadcastService: MsalBroadcastService) { }

  ngOnInit(): void {
    this.msalBroadcastService.msalSubject$
      .pipe(
        filter((msg: EventMessage) => msg.eventType === EventType.LOGIN_SUCCESS)
      )
      .subscribe((result: EventMessage) => {
        console.log(result);
        const payload = result.payload as AuthenticationResult;
        this.authService.instance.setActiveAccount(payload.account);
      });

    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status: InteractionStatus) => status === InteractionStatus.None)
      )
      .subscribe(() => {
        this.setLoginDisplay();
      });

    this.getLoggedUserProfile();
  }

  setLoginDisplay() {
    this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
  }

  getLoggedUserProfile() {
    this.userService.getOwnProfile().subscribe({
      next: (profile: UserProfile) => {
        this.userProfile = profile;
        this.showCompleteProfileicon = !profile.isProfileCompleted;
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
