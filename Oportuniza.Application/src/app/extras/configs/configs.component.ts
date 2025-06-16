import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from "@angular/router";
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-configs',
  imports: [],
  templateUrl: './configs.component.html',
  styleUrl: './configs.component.css'
})

export class ConfigsComponent {
  constructor(private authService: AuthService, private router: Router, public dialogRef: MatDialogRef<ConfigsComponent>) { }

  logout() {
    this.authService.logout()
    this.router.navigate(['/login']);
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
