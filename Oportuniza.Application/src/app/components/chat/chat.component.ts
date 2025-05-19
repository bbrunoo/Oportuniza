import { AuthService } from './../../services/auth.service';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import {
  ChatService,
  GetMessage,
} from '../../services/chat.service';
import { Subscription } from 'rxjs';
import { ProfileService } from '../../services/profile.service';
import { ChatSummary } from '../../models/ChatSummary.model';

interface ConnectedUser {
  connectionId: string;
  userId: string;
  displayName: string;
}

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule, RouterLink, RouterModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css',
})
export class ChatComponent implements OnInit, OnDestroy {
  private messagesSub!: Subscription;

  chatService = inject(ChatService);
  authService = inject(AuthService);
  profileService = inject(ProfileService);
  route = inject(ActivatedRoute);

  getMessages: GetMessage[] = [];
  newMessage = '';
  currentUserName = this.authService.getUserData().name;
  targetUserId!: string;

  chatSummaries: ChatSummary[] = [];
  selectedChatId: string | null = null;
  nomeTarget: string = '';

  currentUserId = this.authService.getUserData().id;

  userInfo = this.profileService.getUserProfile(this.targetUserId);

  ngOnInit(): void {
    this.loadUserChats();

    const storedTargetId = localStorage.getItem('chatTargetUserId');
    if (storedTargetId) {
      this.messagesSub?.unsubscribe();
      this.initiateChatWithUser(storedTargetId);
      localStorage.removeItem('chatTargetUserId');
    }
  }

  initiateChatWithUser(userId: string) {
    this.targetUserId = userId;

    this.profileService.getUserProfile(userId).subscribe({
      next: (user) => {
        this.nomeTarget = user.name;
      },
      error: (err) => {
        console.error('Erro ao obter nome do usuário alvo', err);
        this.nomeTarget = 'usuário';
      },
    });

    this.chatService.getPrivateChatId(userId).subscribe({
      next: (response) => {
        this.selectedChatId = response.chatId;

        this.chatService.getChatMessages(response.chatId).subscribe({
          next: (oldMessages) => {
            this.getMessages = [...oldMessages];
          },
          error: (err) => {
            console.error('Erro ao carregar o historico do chat', err);
          },
        });

        this.chatService.stopConnection();
        this.chatService.startConnection(userId);

        this.messagesSub?.unsubscribe();
        this.messagesSub = this.chatService.messages$.subscribe((msgs) => {
          const existingIds = new Set(this.getMessages.map((m) => m.id));
          const newMessages = msgs.filter((m) => !existingIds.has(m.id));
          this.getMessages = [...this.getMessages, ...newMessages];
        });
      },
      error: (err) => {
        console.error('Erro ao obter o id do chat', err);
      },
    });
  }

  sendMessage() {
    if (!this.newMessage.trim()) return;
    this.chatService.sendMessage(this.newMessage);
    this.newMessage = '';
  }

  loadUserChats() {
    this.chatService.getUserChats().subscribe({
      next: (summaries) => (this.chatSummaries = summaries),
      error: (err) => console.error('Erro ao carregar conversas', err),
    });
  }

  openChat(summary: ChatSummary) {
    this.getMessages = [];
    this.initiateChatWithUser(summary.targetUserId);
  }

  ngOnDestroy() {
    this.messagesSub?.unsubscribe();
    this.chatService.stopConnection();
  }

  trackById(index: number, item: GetMessage) {
    return item.id;
  }

  logout() {
    this.authService.logout();
  }
}
