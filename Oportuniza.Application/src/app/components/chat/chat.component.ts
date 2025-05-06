import { AuthService } from './../../services/auth.service';
import { Component, OnInit } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
})
export class ChatComponent implements OnInit {
  chatId: string = '';
  currentUserId: string = '';
  message: string = '';
  messages: any[] = [];

  constructor(private route: ActivatedRoute, private chatService: ChatService, private authService: AuthService) { }

  ngOnInit(): void {
    const userData = this.authService.getUserData();
    if (!userData) return;

    this.currentUserId = userData.userId;

    const otherUserId = this.route.snapshot.paramMap.get('userId'); // ex: /chat/USERID
    if (!otherUserId) return;

    this.chatService.getOrCreateChat(this.currentUserId, otherUserId).subscribe(chat => {
      this.chatId = chat.id;

      // conecta no SignalR e escuta as mensagens
      this.chatService.connectToChat(this.chatId);
      this.chatService.onReceiveMessage((msg) => {
        this.messages.push(msg);
      });

      // busca mensagens antigas
      this.chatService.getMessages(this.chatId).subscribe(msgs => {
        this.messages = msgs;
      });
    });
  }

  sendMessage() {
    if (!this.message.trim()) return;
    this.chatService.sendMessage(this.chatId, this.message);
    this.message = '';
  }
}
