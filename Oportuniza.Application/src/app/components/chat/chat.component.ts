import { AuthService } from './../../services/auth.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

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
export class ChatComponent implements OnInit, OnDestroy {
  // message: string = '';
  // connectedUsers$: any;
  // chatQueue$: any;
  // chatStarted$: any;
  // chatEnded!: ConnectedUser | null;
  // currentChatUser: any = null;
  // statusMessage: string = 'Nenhuma conversa selecionada';
  // messages: { sender: string; content: string }[] = [];

  // constructor(
  //   private authService: AuthService,
  //   private chatService: ChatService,
  //   private toastrService: ToastrService) { }

  // ngOnInit(): void {
  //   this.connectedUsers$ = this.chatService.connectedUsers$;
  //   this.chatQueue$ = this.chatService.chatQueue$;
  //   this.chatStarted$ = this.chatService.chatStarted$;
  //   this.chatService.chatEnded$.subscribe((s) => {
  //     this.chatEnded = s;
  //     if (this.messages.length > 0) {
  //       this.toastrService.info(
  //         'Your chat has been ended. You can check the transcript from the website.',
  //         'Chat Ended'
  //       );
  //     }
  //     this.messages = [];
  //   });
  //   this.chatService.startConnection();
  // }

  // connectWithSecondPerson(connectionIdOfUser: string) {
  //   this.chatService.connectWithUser(connectionIdOfUser)
  //   this.currentChatUser = connectionIdOfUser;
  // }

  // sendMessage(message: string) {
  //   if (message) {
  //     console.log('message to:', message);
  //     this.chatService.sendMessage(this.message);
  //     this.message = '';
  //   }
  // }

  // joinChatQueue() {
  //   this.chatService.joinChatQueue();
  // }

  // endChat() {
  //   this.chatService.endChat();
  // }

  // ngOnDestroy(): void {
  //   this.chatService.stopConnection();
  // }

  targetUserId!: string;
  message = '';
  messages: { from: string, message: string }[] = [];

  constructor(
    private chatService: ChatService,
    private route: ActivatedRoute,
    private authService:AuthService
  ) {}

  ngOnDestroy(): void {
    throw new Error('Method not implemented.');
  }


  ngOnInit() {
    const chatId = this.route.snapshot.paramMap.get('chatId');

    if (!chatId) {
      console.error('ChatId não encontrado na rota.');
      return;
    }

    const [userA, userB] = chatId.split('-');
    const userData = this.authService.getUserData();
    const loggedUserId = userData.id;
    this.targetUserId = loggedUserId === userA ? userB : userA;

    // ✅ Aguarda a conexão antes de iniciar o chat
    this.chatService.connectionEstablished$.subscribe(connected => {
      if (connected) {
        this.chatService.startPrivateChat(this.targetUserId);
      }
    });
  }



  sendMessage() {
    this.chatService.sendPrivateMessage(this.targetUserId, this.message);
    this.messages.push({ from: 'Você', message: this.message });
    this.message = '';
  }
}
