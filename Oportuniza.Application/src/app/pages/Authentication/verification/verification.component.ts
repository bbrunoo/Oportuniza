import { Component, ElementRef, OnInit, QueryList, ViewChildren } from '@angular/core';
import { VerificationService } from '../../../services/verification.service';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { firstValueFrom } from 'rxjs';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-verification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verification.component.html',
  styleUrls: ['./verification.component.css']
})
export class VerificationComponent implements OnInit {
  email = '';
  codeInputs: string[] = new Array(8).fill('');
  isLoading = false;
  message = '';
  errorMessage = '';
  cooldown = 0;
  timer: any;
  codeLength: number = 8;

  @ViewChildren('d1, d2, d3, d4, d5, d6, d7, d8') codeFields!: QueryList<ElementRef>;

  constructor(
    public router: Router,
    private route: ActivatedRoute,
    private verificationService: VerificationService,
    private keyAuth: KeycloakOperationService
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.email = params.get('email') || '';
      if (!this.email) {
        Swal.fire({ icon: 'error', title: 'Erro', text: 'Dados de registro não fornecidos.' });
        this.router.navigate(['/cadastro']);
      };
    });
  }

  sendVerificationCode() {
    this.isLoading = true;
    this.message = 'Enviando código de verificação...';
    this.errorMessage = '';

    this.verificationService.sendVerificationCode(this.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.message = 'Código enviado! Verifique seu e-mail.';
        this.startCooldown(60);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Erro ao enviar o código. Tente novamente.';
        console.error(err);
      }
    });
  }

  resendCode() {
    if (this.cooldown > 0) return;
    this.sendVerificationCode();
  }

  startCooldown(seconds: number) {
    this.cooldown = seconds;
    clearInterval(this.timer);
    this.timer = setInterval(() => {
      this.cooldown--;
      if (this.cooldown <= 0) clearInterval(this.timer);
    }, 1000);
  }

  onInput(event: any, index: number) {
    const value = event.target.value;
    if (value.length > 1) event.target.value = value.slice(0, 1);
    if (value && index < this.codeInputs.length - 1) {
      this.focusNext(index + 1);
    }
  }

  onKeydown(event: KeyboardEvent, index: number) {
    if (event.key === 'Backspace' && !this.codeInputs[index] && index > 0) {
      this.focusNext(index - 1);
    }
  }

  focusNext(index: number) {
    const fields = this.codeFields.toArray();
    if (fields[index]) fields[index].nativeElement.focus();
  }

  async validateCodeAndRegisterUser() {
    this.isLoading = true;
    this.message = 'Validando código...';
    this.errorMessage = '';

    const code = this.codeInputs.join('');

    try {
      await firstValueFrom(this.verificationService.validateVerificationCode(this.email, code));

      Swal.fire({
        icon: 'success',
        title: 'Verificação concluída!',
        text: 'Seu e-mail foi confirmado com sucesso.',
        timer: 2000,
        showConfirmButton: false,
      }).then(() => this.router.navigate(['/login']));
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Código inválido ou expirado.';
      Swal.fire({ icon: 'error', title: 'Erro', text: this.errorMessage });
    } finally {
      this.isLoading = false;
    }
  }
}
