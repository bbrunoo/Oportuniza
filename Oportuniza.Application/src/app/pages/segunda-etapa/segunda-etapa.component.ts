import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-segunda-etapa',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './segunda-etapa.component.html',
  styleUrls: ['./segunda-etapa.component.css']
})
export class SegundaEtapaComponent implements OnInit{
  selectedButton: string | null = null;
  constructor(private router: Router) { }

  ngOnInit(): void {
    const savedSelection = localStorage.getItem('selectedButton');
       if (savedSelection) {
      this.selectedButton = savedSelection;
    }
  }

    selecionarBotao(botao: string) {
    if (this.selectedButton === botao) {
      this.selectedButton = null;
      localStorage.removeItem('selectedButton');
    } else {
      this.selectedButton = botao;
      localStorage.setItem('selectedButton', botao);
    }
  }

  getBackgroundColor(botao: string): string {
    return this.selectedButton === botao ? '#2E3FFF' : '#d1d1d1';
  }

  getFontColor(botao: string): string {
    return this.selectedButton === botao ? 'white' : 'black';
  }

  proximaEtapa() {
    if (!this.selectedButton) {
      Swal.fire({
        icon: 'warning',
        title: 'Seleção obrigatória',
        text: 'Por favor, selecione uma opção para continuar.'
      });
      return;
    }

    const isACompany = this.selectedButton === 'contratar';
    localStorage.setItem('isACompany', JSON.stringify(isACompany));

    this.router.navigate(['/terceira-etapa']);
  }

}
