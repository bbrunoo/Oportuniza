import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { ChatSummary } from '../models/ChatSummary.model';

export interface ChatMessage {
  userName: string;
  message: string;
  sentDateTime: string;
}

export interface GetMessage {
  id: string;
  chatId: string;
  senderId: string;
  senderName: string;
  message: string;
  sentAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  authService = inject(AuthService);
  http = inject(HttpClient);

  private hubConnection!: signalR.HubConnection;
  private messagesSubject = new BehaviorSubject<GetMessage[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  private currentChatId!: string;

  startConnection(targetUserId: string): void {
    this.messagesSubject.next([]);

    this.getPrivateChatId(targetUserId).subscribe({
      next: response => {
        this.currentChatId = response.chatId;
        this.initSignalRConnection(response.chatId);
      },
      error: (err) => {
        console.error('Erro ao obter o chatId da API:', err);
      },
    });
  }

  getPrivateChatId(targetUserId: string): Observable<{ chatId: string }> {
    return this.http.get<{ chatId: string }>(
      `https://localhost:5000/api/chat/private/${targetUserId}`
    );
  }

  private initSignalRConnection(chatId: string): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected)
      return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:5000/chathub', {
        accessTokenFactory: () => this.authService.getToken() || '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.hubConnection.invoke('JoinChat', chatId).catch(console.error);
      })
      .catch((error) => console.error('Erro ao conectar ao hub:', error));

    this.hubConnection.on('ReceiveMessage', (msg: GetMessage) => {
      this.addMessage(msg);
    });
  }

  sendMessage(message: string): void {
    if (!this.currentChatId) return;
    this.hubConnection
      .invoke('SendMessageToChat', this.currentChatId, message)
      .catch(console.error);
  }

  private addMessage(msg: GetMessage) {
    const current = this.messagesSubject.value;
    this.messagesSubject.next([...current, msg]);
  }

  stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.off('ReceiveMessage');
      this.hubConnection.stop().catch(console.error);
    }
  }

  getChatMessages(chatId: string): Observable<GetMessage[]> {
    return this.http.get<GetMessage[]>(
      `https://localhost:5000/api/chat/history/${chatId}`
    );
  }

  getUserChats(): Observable<ChatSummary[]> {
    return this.http.get<ChatSummary[]>(
      'https://localhost:5000/api/chat/conversations'
    );
  }
}
