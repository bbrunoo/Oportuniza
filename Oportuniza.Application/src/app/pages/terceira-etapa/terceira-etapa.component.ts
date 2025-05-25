import { Component, OnInit } from '@angular/core';
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
export class TerceiraEtapaComponent implements OnInit {
  areaDeInteresse: string = '';

  constructor(private router: Router, private userService: UserService) { }

  ngOnInit(): void {
    const savedInterest = localStorage.getItem('areaDeInteresse');
    if (savedInterest) {
      this.areaDeInteresse = savedInterest;
    }
  }

  onAreaDeInteresseChange(value: string): void {
    this.areaDeInteresse = value;
    localStorage.setItem('areaDeInteresse', value);
  }


  concluirCadastro(): void {
    const nome = localStorage.getItem('profileName');
    const isACompany = localStorage.getItem('isACompany') === 'true';
    const userId = localStorage.getItem('userId');
    const imageUrl = localStorage.getItem('profileImageUrl');

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
      interests: this.areaDeInteresse.trim(),
      imageUrl: imageUrl || '',
    };

    this.userService.updateProfile(dados, userId).subscribe({
      next: () => {
        localStorage.removeItem("profileName");
        localStorage.removeItem("isACompany");
        localStorage.removeItem("userId");
        localStorage.removeItem("profileImageUrl");
        localStorage.removeItem("areaDeInteresse");
        localStorage.removeItem("selectedButton");

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
