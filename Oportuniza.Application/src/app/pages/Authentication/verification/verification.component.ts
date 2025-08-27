import { Component, ElementRef, OnInit, QueryList, ViewChildren } from '@angular/core';
import { VerificationService } from '../../../services/verification.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';
import { firstValueFrom } from 'rxjs';
import { KeycloakOperationService } from '../../../services/keycloak.service';

@Component({
  selector: 'app-verification',
  imports: [CommonModule, FormsModule],
  templateUrl: './verification.component.html',
  styleUrl: './verification.component.css'
})
export class VerificationComponent implements OnInit {
  email: string = '';
  password: string = '';
  code: string = '';
  codeLength: number = 8;
  codeInputs: string[] = new Array(this.codeLength).fill('');
  isLoading: boolean = false;
  message: string = '';

  @ViewChildren('codeField') codeFields!: QueryList<ElementRef>;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private verificationService: VerificationService,
    private keyAuth: KeycloakOperationService,
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.email = params.get('email') || '';
      this.password = params.get('password') || '';

      if (!this.email || !this.password) {
        this.router.navigate(['/cadastro']);
        Swal.fire({
          icon: 'error',
          title: 'Erro',
          text: 'Dados de registro não fornecidos.'
        });
      } else {
        this.sendVerificationCode();
      }
    });
  }

sendVerificationCode() {
    this.isLoading = true;
    this.message = 'Enviando código de verificação...';
    this.verificationService.sendVerificationCode(this.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.message = 'Código enviado com sucesso! Verifique sua caixa de entrada.';
      },
      error: (error) => {
        this.isLoading = false;
        this.message = 'Erro ao enviar o código. Tente novamente.';
        console.error('Erro ao enviar código:', error);
        Swal.fire({
          icon: 'error',
          title: 'Erro ao enviar e-mail',
          text: 'Não foi possível enviar o e-mail de verificação.'
        });
      }
    });
  }
   onInput(event: any, index: number) {
    // Apenas manipula se for um único caractere
    if (event.target.value.length > 1) {
      event.target.value = event.target.value.slice(0, 1);
    }

    // Move o cursor para o próximo campo
    if (event.target.value && index < this.codeLength - 1) {
      const nextInput = this.codeFields.toArray()[index + 1].nativeElement;
      nextInput.focus();
    }
  }

  onKeydown(event: KeyboardEvent, index: number) {
    if (event.key === 'Backspace' && !this.codeInputs[index] && index > 0) {
      const prevInput = this.codeFields.toArray()[index - 1].nativeElement;
      prevInput.focus();
    }
  }

  async validateCodeAndRegisterUser() {
    this.isLoading = true;
    this.message = 'Validando código...';

    // Concatena os valores dos inputs individuais
    const code = this.codeInputs.join('');

    try {
      // 1. Valida o código com a sua API de verificação
      await firstValueFrom(this.verificationService.validateVerificationCode(this.email, code));

      this.message = 'Código válido!';

      // 2. Cria o usuário no Keycloak (apenas se o código for válido)
      await firstValueFrom(this.keyAuth.registerUser({ email: this.email, password: this.password }));

      Swal.fire({
        icon: 'success',
        title: 'Cadastro e verificação concluídos!',
        text: 'Sua conta foi criada e verificada com sucesso. Você será redirecionado.',
        timer: 2000,
        showConfirmButton: false,
      }).then(() => {
        this.router.navigate(['/inicio']);
      });

    } catch (error: any) {
      console.error('Erro no processo de verificação ou registro:', error);
      let errorMessage = 'Ocorreu um erro. Verifique o código e tente novamente.';
      if (error.status === 400) {
        errorMessage = 'Código inválido ou expirado. Tente novamente.';
      } else if (error.error?.error_description) {
        errorMessage = `Erro de registro: ${error.error.error_description}`;
      }

      this.message = errorMessage;
      Swal.fire({
        icon: 'error',
        title: 'Erro!',
        text: errorMessage,
      });

    } finally {
      this.isLoading = false;
    }
  }
}
