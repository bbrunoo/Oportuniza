import { AuthService } from './../../services/auth.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-perfil',
  imports: [],
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.css'
})
export class PerfilComponent implements OnInit {
  userId: string | null = null;
  email : string | null = null;
  name : string | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    const userData = this.authService.getUserData();

    if (userData) {
      this.userId = userData.id;
      this.email = userData.email;
      this.name = userData.name;
    }
  }

  logout() {
    this.authService.logout();
  }
}
