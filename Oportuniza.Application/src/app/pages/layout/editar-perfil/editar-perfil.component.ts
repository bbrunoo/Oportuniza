import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2';
import { UserService } from '../../../services/user.service';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-editar-perfil',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatDialogModule],
  templateUrl: './editar-perfil.component.html',
  styleUrl: './editar-perfil.component.css'
})
export class EditarPerfilComponent {
  userId: string | null = null;
  userProfile: any = {
    name: '',
    email: '',
    location: '',
    imageUrl: ''
  };

  selectedImage?: File;
  previewUrl: string | ArrayBuffer | null = null;

  isImageUploaded = false;
  isUploading = false;
  uploadError = false;

  editableProfile: any = {};
  isLoading = true;
  isSaving = false;
  saveSuccess = false;
  saveError: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (!this.userId) {
      this.router.navigate(['/inicio/perfil']);
      return;
    }
    this.loadProfile(this.userId);
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
        // garante que o result é um File válido
        const croppedFile = result instanceof File ? result : new File([result], 'cropped.jpg', { type: 'image/jpeg' });

        this.selectedImage = croppedFile;
        this.previewUrl = URL.createObjectURL(croppedFile);
        this.editableProfile.imageUrl = this.previewUrl;

        // upload imediato
        this.isUploading = true;
        this.userService.uploadProfilePicture(croppedFile).subscribe({
          next: res => {
            localStorage.setItem('profileImageUrl', res.imageUrl);
            this.editableProfile.imageUrl = res.imageUrl;
            Swal.fire('Sucesso!', 'Imagem de perfil atualizada.', 'success');
            this.isUploading = false;
            this.isImageUploaded = true;
          },
          error: err => {
            console.error('Erro ao enviar imagem de perfil:', err);
            Swal.fire('Erro', 'Não foi possível atualizar a imagem. Tente novamente.', 'error');
            this.isUploading = false;
            this.uploadError = true;
          }
        });
      }
    });
  }

  loadProfile(id: string): void {
    this.isLoading = true;
    this.userService.getUserById(id).subscribe({
      next: (profile) => {
        this.userProfile = {
          name: profile.name || '',
          email: profile.email || '',
          location: profile.local || '',
          imageUrl: profile.imageUrl || ''
        };
        this.editableProfile = { ...this.userProfile };
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Erro ao carregar perfil.';
        this.isLoading = false;
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (file) {
      this.handleFile(file);
    }
    event.target.value = '';
  }

  saveProfile(): void {
    if (!this.userId) return;
    this.isSaving = true;
    this.saveError = null;

    this.userService.editProfile(
      this.editableProfile.name,
      this.editableProfile.location,
      this.selectedImage ?? undefined
    ).subscribe({
      next: () => {
        this.isSaving = false;
        this.saveSuccess = true;

        Swal.fire({
          icon: 'success',
          title: 'Perfil atualizado!',
          text: 'Suas alterações foram salvas com sucesso.',
          confirmButtonColor: '#3D50CA'
        }).then(() => {
          this.loadProfile(this.userId!);
          window.location.reload();
        });
      },
      error: (err) => {
        console.error(err);
        this.isSaving = false;
        this.saveError = 'Erro ao salvar alterações.';
        Swal.fire({
          icon: 'error',
          title: 'Erro!',
          text: 'Não foi possível atualizar o perfil.',
          confirmButtonColor: '#d33'
        });
      }
    });
  }
}
