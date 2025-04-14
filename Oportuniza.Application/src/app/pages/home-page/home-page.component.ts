import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home-page',
  imports: [],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css'
})

export class HomePageComponent implements OnInit{
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
