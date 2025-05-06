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
export class AppComponent{
  title = 'Oportuniza.Application';
}
