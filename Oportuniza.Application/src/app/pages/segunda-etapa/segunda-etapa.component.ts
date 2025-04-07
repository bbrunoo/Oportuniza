import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-segunda-etapa',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './segunda-etapa.component.html',
  styleUrls: ['./segunda-etapa.component.css']
})
export class SegundaEtapaComponent {
  selectedButton: string | null = null;

  selecionarBotao(botao: string) {
    if (this.selectedButton === botao) {
      this.selectedButton = null; 
    } else {
      this.selectedButton = botao; 
    }
  }
  getBackgroundColor(botao: string): string {
    return this.selectedButton === botao ? '#2E3FFF' : '#d1d1d1';  
  }

  getFontColor(botao: string): string {
    return this.selectedButton === botao ? 'white' : 'black';  
  }
}
