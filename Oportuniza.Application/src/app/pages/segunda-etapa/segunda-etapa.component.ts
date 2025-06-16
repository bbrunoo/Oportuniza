import { NgxMaskConfig } from './../../../../node_modules/ngx-mask/lib/ngx-mask.config.d';
import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { NgxMaskDirective, NgxMaskPipe } from 'ngx-mask';

@Component({
  selector: 'app-segunda-etapa',
  standalone: true,
  imports: [RouterModule, CommonModule, NgxMaskDirective],
  templateUrl: './segunda-etapa.component.html',
  styleUrls: ['./segunda-etapa.component.css']
})
export class SegundaEtapaComponent implements OnInit {
  selectedButton: string | null = null;
  telValue = localStorage.getItem("profileTel");
  constructor(private router: Router) { }

  ngOnInit(): void {
      this.telValue = localStorage.getItem("profileTel");
  }

 verificarTel(telInput: HTMLInputElement) {
  const raw = telInput.value;
  const numeroSemMascara = raw.replace(/\D/g, '');

  if (numeroSemMascara.length !== 11) {
    Swal.fire({
      icon: 'warning',
      title: 'Número inválido!',
      text: 'Deve conter 11 dígitos.'
    });
    return;
  }

  if (raw === '') {
    Swal.fire({
      icon: 'warning',
      title: 'Campo obrigatório',
      text: 'Por favor, insira seu telefone antes de continuar.'
    });
    return;
  }

  localStorage.setItem('profileTel', numeroSemMascara);
  this.router.navigate(['/terceira-etapa']);
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
