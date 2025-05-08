import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from './../../services/auth.service';
import { Component, OnInit } from '@angular/core';
import { ProfileService } from '../../services/profile.service';
import { ChatService } from '../../services/chat.service';

@Component({
  selector: 'app-perfil',
  imports: [],
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.css'
})
export class PerfilComponent implements OnInit {
  targetUserId!: string;
  targetUserName!: string;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}
  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

  startChat() {
    const userData = this.authService.getUserData();
    const loggedUserId = userData.id; // precisa fornecer esse método
    const ids = [loggedUserId, this.targetUserId].sort(); // ordena os dois GUIDs
    const chatId = `${ids[0]}-${ids[1]}`;
    this.router.navigate(['/chat', chatId]); // URL será /chat/{userA-userB}, sempre igual para ambos
  }
}
