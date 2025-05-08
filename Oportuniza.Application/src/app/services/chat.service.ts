import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

interface ConnectedUser {
  connectionId: string;
  userId: string;
  displayName: string;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  // private hubConnection!: HubConnection;
  // private connectedUsersSubject = new BehaviorSubject<ConnectedUser[]>([]);
  // public connectedUsers$ = this.connectedUsersSubject.asObservable();

  // private chatQueueSubject = new BehaviorSubject<ConnectedUser[]>([]);
  // public chatQueue$ = this.chatQueueSubject.asObservable();

  // private chatStartedSubject = new BehaviorSubject<ConnectedUser | null>(null);
  // public chatStarted$ = this.chatStartedSubject.asObservable();

  // private messagesSubject = new BehaviorSubject<{sender: string; content: string}[]>([]);
  // public messages$ = this.messagesSubject.asObservable();

  // private chatEndedSubject = new BehaviorSubject<ConnectedUser | null>(null);
  // public chatEnded$ = this.chatEndedSubject.asObservable();

  // private messageSubject = new BehaviorSubject<{ from: string, message: string } | null>(null);
  // public receivedMessage$ = this.messageSubject.asObservable();

  chatHubUrl = 'https://localhost:5000/chat';

  // constructor(private authService: AuthService) { }

  // public startConnection() {
  //   this.hubConnection = new HubConnectionBuilder()
  //     .withUrl(this.chatHubUrl, {
  //       accessTokenFactory: async () => {
  //         const tokenResponse = await this.authService.getToken()
  //         return tokenResponse || '';
  //       },
  //     })
  //     .configureLogging(LogLevel.Information)
  //     .build();

  //   this.hubConnection
  //     .start()
  //     .then(() => {
  //       console.log("Connection started");
  //       this.registerHandlers();
  //     })
  //     .catch((err) => console.error("Error while starting connection: " + err));
  // }

  // private registerHandlers() {
  //   this.hubConnection.on('UpdateConnectedUsers', (users:  ConnectedUser[]) => {
  //     this.connectedUsersSubject.next(users);
  //   })

  //   this.hubConnection.on('UpdateChatQueue', (queue:  ConnectedUser[]) => {
  //     this.chatQueueSubject.next(queue);
  //   })

  //   this.hubConnection.on('StartChat', (user:  ConnectedUser) => {
  //     console.log("Chat started with user:", user);
  //     this.chatStartedSubject.next(user);
  //   })

  //   this.hubConnection.on('ReceiveMessage', (sender: string, message: string) => {
  //     const currentMessages = this.messagesSubject.getValue();
  //     currentMessages.push({sender, content: message});
  //     this.messagesSubject.next(currentMessages);
  //   })

  //   this.hubConnection.on('EndChat', (user:  ConnectedUser) => {
  //     console.log("Chat ended with user:", user);
  //     this.chatEndedSubject.next(user);
  //   })
  // }

  // public stopConnection() {
  //   this.hubConnection
  //   .stop()
  //   .catch((err) => console.error("error stopping connection:" + err));
  // }

  // public joinChatQueue() {
  //   this.hubConnection
  //   .invoke('JoinChatQueue')
  //   .catch((err) => console.error("error joining chat queue:" + err));
  // }

  // public connectWithUser(connectionId: string) {
  //   this.hubConnection
  //   .invoke('ConnectWithSecondPerson', connectionId)
  //   .catch((err) => console.error("error connecting with the user:" + err));
  // }

  // public sendMessage(message: string) {
  //   this.hubConnection
  //   .invoke('SendMessage', message)
  //   .catch((err) => console.error("error sending message:" + err));
  // }

  // public endChat() {
  //   this.hubConnection
  //     .invoke('EndChat')
  //     .catch((err) => console.error('Error connecting with user', err));
  // }

  //  public setUserIdLogado(userId: string) {
  //   this.userIdLogado = userId;
  // }

  // public startPrivateChat(targetUserId: string) {
  //   this.hubConnection.invoke("StartPrivateChat", targetUserId)
  //     .catch(err => console.error("Erro ao iniciar chat privado:", err));
  // }

  // public sendPrivateMessage(toUserId: string, message: string) {
  //   this.hubConnection.invoke("SendPrivateMessage", toUserId, message)
  //     .catch(err => console.error("Erro ao enviar mensagem:", err));
  // }

  private hubConnection!: HubConnection;
  private messageSubject = new BehaviorSubject<{ from: string, message: string } | null>(null);
  public receivedMessage$ = this.messageSubject.asObservable();

  private connectionEstablished = new BehaviorSubject<boolean>(false);
public connectionEstablished$ = this.connectionEstablished.asObservable();

  private userIdLogado!: string;

  constructor(private authService: AuthService) {
    this.startConnection();
  }

  private startConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.chatHubUrl, {
        accessTokenFactory: async () => {
          const tokenResponse = await this.authService.getToken();
          return tokenResponse || '';
        },
      })
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Conectado ao SignalR');
        this.connectionEstablished.next(true); // ✅ Dispara quando conectado
      })
      .catch(err => console.error('Erro ao conectar ao SignalR:', err));

    this.hubConnection.on("ReceiveMessage", (from: string, message: string) => {
      this.messageSubject.next({ from, message });
    });

    this.hubConnection.on("StartChat", (user: any) => {
      console.log("Chat privado iniciado com:", user);
    });
  }

  public setUserIdLogado(userId: string) {
    this.userIdLogado = userId;
  }

  public startPrivateChat(targetUserId: string) {
    this.hubConnection.invoke("StartPrivateChat", targetUserId)
      .catch(err => console.error("Erro ao iniciar chat privado:", err));
  }

  public sendPrivateMessage(toUserId: string, message: string) {
    this.hubConnection.invoke("SendPrivateMessage", toUserId, message)
      .catch(err => console.error("Erro ao enviar mensagem:", err));
  }

}
