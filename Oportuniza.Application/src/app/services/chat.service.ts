import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection!: signalR.HubConnection;
  private apiUrl = 'https://localhost:5000/api/v1/chat';

  constructor(private authService: AuthService, private http: HttpClient) { }

  connectToChat(chatId: string) {
    const token = this.authService.getToken();
    if (!token) return;

    if (this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      console.warn('Já conectado ao SignalR.');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:5000/chat', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

      this.hubConnection
      .start()
      .then(() => {
        console.log("SignalR conectado com sucesso");
      })
      .catch(error => {
        console.error("Erro ao conectar ao SignalR:", error);
      });
  }

  sendMessage(chatId: string, message: string) {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SendMessage', chatId, message).catch(err => {
        const errorMsg = err instanceof Error ? err.message : JSON.stringify(err);
        console.error('Erro ao enviar mensagem:', errorMsg);
        alert('Erro ao enviar mensagem: ' + errorMsg);
      });
    } else {
      const msg = 'Conexão com SignalR não está ativa.';
      console.error(msg);
      alert(msg);
    }
  }


  onReceiveMessage(callback: (message: any) => void) {
    if (!this.hubConnection) return;
    this.hubConnection.on('ReceiveMessage', callback);
  }

  getOrCreateChat(user1Id: string, user2Id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/get-or-create/${user1Id}/${user2Id}`);
  }

  // Busca mensagens de um chat existente
  getMessages(chatId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/messages/${chatId}`);
  }
}
