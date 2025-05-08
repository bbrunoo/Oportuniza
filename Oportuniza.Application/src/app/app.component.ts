import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ChannelService, ChatClientService, StreamChatModule, StreamI18nService } from 'stream-chat-angular';
import { AuthService } from './services/auth.service';
import { UserProfile } from './models/UserProfile.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, StreamChatModule],
  template: `<router-outlet></router-outlet>`,
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
  title = 'Oportuniza.Application';

  constructor(
    private authService: AuthService,
    private chatService: ChatClientService,
    private channelService: ChannelService,
    private streamI18nService: StreamI18nService,) {
      const userData = this.authService.getUserData();
      const apiKey = 'gzsm6tr3hy3r';
      const userId = userData.id;
      const userToken = this.authService.getToken();
      const userName = userData.name;

      const user: UserProfile = {
        id: userId,
        name: userName,
      };

      this.chatService.init(apiKey, userId, userToken);
      this.streamI18nService.setTranslation();
    }

  async ngOnInit() {
    const channel = this.chatService.chatClient.channel('messaging', 'chat')
    await channel.create()
    this.channelService.init({
      type: 'messaging',
      id: {$eq:'chat'}
    })
  }
}
