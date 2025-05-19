import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../services/user.service';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-terceira-etapa',
  imports: [RouterModule, FormsModule],
  templateUrl: './terceira-etapa.component.html',
  styleUrls: ['./terceira-etapa.component.css']
})
export class TerceiraEtapaComponent {
  areaDeInteresse: string = '';

  constructor(private router: Router, private userService: UserService) { }

  concluirCadastro(): void {
    const nome = localStorage.getItem('profileName');
    const isACompany = localStorage.getItem('isACompany') === 'true';
    const userId = localStorage.getItem('userId');

    if (!userId) {
      Swal.fire({
        icon: 'error',
        title: 'Erro',
        text: 'Usuário não identificado.'
      });
      return;
    }

    if (!this.areaDeInteresse.trim()) {
      Swal.fire({
        icon: 'warning',
        title: 'Campo obrigatório',
        text: 'Por favor, preencha sua área de interesse antes de continuar.'
      });
      return;
    }

    const dados = {
      fullName: nome || '',
      isACompany: isACompany,
      interests: this.areaDeInteresse.trim()
    };

    this.userService.updateProfile(dados, userId).subscribe({
      next: () => {
        localStorage.removeItem("profileName");
        localStorage.removeItem("isACompany");
        localStorage.removeItem("userId");
        Swal.fire({
          icon: 'success',
          title: 'Perfil atualizado!',
          timer: 2000,
          text: 'Seu perfil foi completado com sucesso.'
        }).then(() => {
          this.router.navigate(['/home']);
        });
      }
    });
  }
}
