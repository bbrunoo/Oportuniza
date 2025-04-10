import { Component } from '@angular/core';
import { Router } from '@angular/router';  
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-primeira-etapa',
  templateUrl: './primeira-etapa.component.html',
  styleUrls: ['./primeira-etapa.component.css']
})
export class PrimeiraEtapaComponent {

  constructor(private router: Router) {}

  verificarNome(nomeInput: HTMLInputElement) {
    if (nomeInput.value.trim() === '') {
      alert('Por favor, insira seu nome antes de continuar.');
    } else {
      this.router.navigate(['/segunda-etapa']);  
    }
  }
}
