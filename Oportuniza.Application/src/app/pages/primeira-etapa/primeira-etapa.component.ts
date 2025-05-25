import { UserService } from './../../services/user.service';
import { Component, NgModule } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-primeira-etapa',
  templateUrl: './primeira-etapa.component.html',
  imports: [FormsModule, CommonModule],
  styleUrls: ['./primeira-etapa.component.css']
})
export class PrimeiraEtapaComponent {
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;
  nomeValue = localStorage.getItem("profileName");
  
  constructor(private router: Router, private userService: UserService) { }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const validTypes = ['image/png', 'image/jpg', 'image/jpeg'];
      if (!validTypes.includes(file.type)) {
        return;
      }

      this.selectedFile = file;

      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result;
      };
      reader.readAsDataURL(file);

      this.userService.uploadProfilePicture(file).subscribe({
        next: res => {
          localStorage.setItem('profileImageUrl', res.imageUrl);
        },
        error: err => {
          console.error('Erro ao enviar imagem de perfil:', err);
        }
      });
    }
  }

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
