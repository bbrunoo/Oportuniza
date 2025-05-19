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
  targetUserId!: string;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
  ) { }

  ngOnInit() {
    this.targetUserId = this.route.snapshot.paramMap.get('id') ?? '';
  }

  startChat() {
    if (!this.targetUserId) {
      console.error('targetUserId não definido');
      return;
    }
    localStorage.setItem('chatTargetUserId', this.targetUserId);

    this.router.navigate(['/chat']);
  }
}
