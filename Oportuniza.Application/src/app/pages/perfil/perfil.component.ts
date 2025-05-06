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
  userId: string | null = null;
  email : string | null = null;
  name : string | null = null;

  targetUserId: string = '';
  targetUserName: string = '';
  currentUserId: string = '';

  constructor(private authService: AuthService, private route: ActivatedRoute, private router: Router, private profileService: ProfileService, private chatService: ChatService) {}

  ngOnInit(): void {
    const idFromUrl = this.route.snapshot.paramMap.get("id");
    const userData = this.authService.getUserData();

    if (!idFromUrl || !userData) return;

    this.currentUserId = userData.id;

    this.profileService.getUserProfile(idFromUrl).subscribe(profile => {
      this.targetUserId = profile.id;
      this.targetUserName = profile.name;
    })
  }

  startChat(){
    const chatId = this.generateChatId(this.currentUserId, this.targetUserId);
    this.chatService.connectToChat(chatId);
    this.router.navigate([`/chat/${chatId}`]);
  }

  private generateChatId(id1: string, id2: string): string {
    return [id1, id2].sort().join('-');
  }

  logout() {
    this.authService.logout();
  }
}
