import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-primeira-etapa',
  templateUrl: './primeira-etapa.component.html',
  styleUrls: ['./primeira-etapa.component.css']
})
export class PrimeiraEtapaComponent {

  constructor(private router: Router) {}

   verificarNome(nomeInput: HTMLInputElement) {
    const nome = nomeInput.value.trim();

    if (nome === '') {
      Swal.fire({
        icon: 'warning',
        title: 'Campo obrigatório',
        text: 'Por favor, insira seu nome antes de continuar.'
      });
    } else {
      localStorage.setItem('profileName', nome); 
      this.router.navigate(['/segunda-etapa']);
    }
  }
}
