import { UserService } from '../../../services/user.service';
import { Component, NgModule } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { UserProfile } from '../../../models/UserProfile.model';

@Component({
  selector: 'app-primeira-etapa',
  templateUrl: './primeira-etapa.component.html',
  imports: [FormsModule, CommonModule, RouterModule],
  styleUrls: ['./primeira-etapa.component.css']
})
export class PrimeiraEtapaComponent {
  userId: string = '';
  nomeValue: string = '';
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;
  selectedImage?: File;

  isImageUploaded: boolean = false;

  isUploading: boolean = false;
  uploadError: boolean = false;

  constructor(private router: Router, private userService: UserService, private dialog: MatDialog,
  ) { }

  ngOnInit(): void {
    this.userService.getOwnProfile().subscribe({
      next: (profile: UserProfile) => {
        this.userId = profile.id;
        this.nomeValue = profile.name || '';
        localStorage.setItem("userId", this.userId);

        if (profile.imageUrl) {
          this.previewUrl = profile.imageUrl;
          this.isImageUploaded = true;
          localStorage.setItem("profileImageUrl", profile.imageUrl);
        }
      },
      error: (err) => {
        console.error('Erro ao buscar perfil:', err);
      }
    });
  }

  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
    event.target.value = '';
  }

  private handleFile(file: File): void {
    const validTypes = ['image/png', 'image/jpg', 'image/jpeg'];
    if (!validTypes.includes(file.type)) {
      Swal.fire('Tipo inválido', 'Apenas imagens PNG, JPG ou JPEG são permitidas.', 'warning');
      return;
    }

    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        this.openCropperDialog(reader.result);
      }
    };
    reader.onerror = (error) => {
      console.error('Erro ao ler o arquivo:', error);
      Swal.fire('Erro', 'Não foi possível ler o arquivo de imagem.', 'error');
    };
  }

  private openCropperDialog(imageBase64: string): void {
    const dialogData: CropperDialogData = { imageBase64 };

    const dialogRef = this.dialog.open(CropperDialogComponent, {
      minWidth: '1000px',
      minHeight: '600px',
      data: dialogData,
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.selectedImage = result;
        this.previewUrl = URL.createObjectURL(result);

        this.isUploading = true;
        this.isImageUploaded = false;
        this.uploadError = false;

        this.userService.uploadProfilePicture(result).subscribe({
          next: res => {
            localStorage.setItem('profileImageUrl', res.imageUrl);
            Swal.fire('Sucesso!', 'Imagem de perfil atualizada.', 'success');

            this.isImageUploaded = true;
            this.isUploading = false;
          },
          error: err => {
            console.error('Erro ao enviar imagem de perfil:', err);
            let errorMessage = 'Não foi possível atualizar a imagem. Tente novamente.';
            if (err.error && err.error.message) {
              errorMessage = err.error.message;
            }
            Swal.fire('Erro', errorMessage, 'error');

            this.isUploading = false;
            this.uploadError = true;
          }
        });
      }
    });
  }


  proximo() {
    this.router.navigate(['/segunda-etapa']);
  }
}
