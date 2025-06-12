import { Component, OnInit, ViewChild } from '@angular/core';
import { PublicationService } from '../../../services/publication.service';
import { PublicationCreate } from '../../../models/PublicationCreate.model';
import Swal from 'sweetalert2';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';

@Component({
  selector: 'app-publication',
  imports: [CommonModule, FormsModule],
  templateUrl: './publication.component.html',
  styleUrl: './publication.component.css'
})
export class PublicationComponent implements OnInit {
  constructor(
    private publicationService: PublicationService,
    private userService: UserService
  ) { }

  selectedImage?: File;
  previewUrl: any;
  userProfile!: UserProfile;
  publication!: PublicationCreate;

  @ViewChild('publicationForm') publicationForm!: NgForm;

  ngOnInit() {
    this.getLoggedUserProfile();
  }

  getLoggedUserProfile() {
    this.userService.getOwnProfile().subscribe({
      next: (profile: UserProfile) => {
        this.userProfile = profile;
        console.log("dados do usuario logado", profile);

        // Cria a publicação após carregar o usuário logado
        this.publication = {
          title: '',
          content: '',
          authorId: this.userProfile.id
        };
      },
      error: (error: any) => {
        console.error(error);
        Swal.fire('Erro', 'Não foi possível carregar o perfil do usuário.', 'error');
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const validTypes = ['image/png', 'image/jpg', 'image/jpeg'];
      if (!validTypes.includes(file.type)) {
        Swal.fire('Tipo inválido', 'Apenas imagens PNG, JPG ou JPEG são permitidas.', 'warning');
        return;
      }

      this.selectedImage = file;

      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result;
      };
      reader.readAsDataURL(file);
    }
  }

  post() {
    if (!this.publication) {
      Swal.fire('Erro', 'Perfil do usuário ainda não carregado. Tente novamente.', 'error');
      return;
    }

    this.publicationService.createPublication(this.publication, this.selectedImage!).subscribe({
      next: res => {
        Swal.fire('Sucesso!', 'Publicação criada com sucesso!', 'success');

        // Limpa os campos do objeto publication
        this.publication = { title: '', content: '', authorId: this.userProfile.id };
        this.selectedImage = undefined;
        this.previewUrl = null;

        // Limpa o formulário visualmente
        this.publicationForm.resetForm();
      },
      error: err => {
        Swal.fire('Erro!', `Não foi possível criar a publicação: ${err.error}`, 'error');
      }
    });
  }
}
