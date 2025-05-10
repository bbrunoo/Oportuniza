import { AuthService } from './../../services/auth.service';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ChatService, ChatMessage } from '../../services/chat.service';
import { Subscription } from 'rxjs';
import { ProfileService } from '../../services/profile.service';

interface ConnectedUser {
  connectionId: string;
  userId: string;
  displayName: string;
}

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit, OnDestroy  {
  private messagesSub!: Subscription;

  chatService = inject(ChatService);
  authService = inject(AuthService);
  profileService = inject(ProfileService);
  route = inject(ActivatedRoute);

  messages: ChatMessage[] = [];
  newMessage = '';
  currentUserName = this.authService.getUserData().name;
  targetUserId!: string;

  nomeTarget: string = '';


  userInfo = this.profileService.getUserProfile(this.targetUserId)

  ngOnInit(): void {
    this.targetUserId = this.route.snapshot.paramMap.get('targetUserId')!;

    this.profileService.getUserProfile(this.targetUserId).subscribe({
      next: (user) => {
        this.nomeTarget = user.name;
      },
      error: (err) => {
        console.error('Erro ao obter nome do usuário alvo', err);
        this.nomeTarget = 'usuário';
      }
    });

    this.chatService.startConnection(this.targetUserId);

    this.messagesSub = this.chatService.messages$.subscribe(msgs => {
      this.messages = msgs;
    });
  }

  sendMessage() {
    if (!this.newMessage.trim()) return;
    this.chatService.sendMessage(this.newMessage);
    this.newMessage = '';
  }

  ngOnDestroy() {
    this.messagesSub?.unsubscribe();
    this.chatService.stopConnection();
  }
}
