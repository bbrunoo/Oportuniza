import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import Swal from 'sweetalert2';
import { UserService } from '../../../services/user.service';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CityService } from '../../../services/city.service';
import { debounceTime, switchMap } from 'rxjs';

@Component({
  selector: 'app-editar-perfil',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatDialogModule, ReactiveFormsModule],
  templateUrl: './editar-perfil.component.html',
  styleUrl: './editar-perfil.component.css'
})
export class EditarPerfilComponent implements OnInit {
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

  cityControl = new FormControl('');
  filteredCities: any[] = [];
  cityModalOpen = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private cityService: CityService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.userId = this.route.snapshot.paramMap.get('id');
    if (!this.userId) {
      this.router.navigate(['/inicio/perfil']);
      return;
    }
    this.loadProfile(this.userId);

    this.cityControl.valueChanges.pipe(
      debounceTime(300),
      switchMap(value =>
        value && value.length > 0
          ? this.cityService.searchCities(value, 1, 20)
          : this.cityService.getCities(1, 20)
      )
    ).subscribe({
      next: cities => (this.filteredCities = cities),
      error: err => console.error('Erro ao carregar cidades:', err)
    });
  }

  openCityModal() {
    this.cityModalOpen = true;
  }

  closeCityModal() {
    this.cityModalOpen = false;
  }

  selectCity(city: any) {
    this.editableProfile.location = city.name;
    this.cityControl.setValue(city.name, { emitEvent: false });
    this.closeCityModal();
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
        const croppedFile =
          result instanceof File ? result : new File([result], 'cropped.jpg', { type: 'image/jpeg' });

        this.selectedImage = croppedFile;
        this.previewUrl = URL.createObjectURL(croppedFile);
        this.editableProfile.imageUrl = this.previewUrl;
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
    if (file) this.handleFile(file);
    event.target.value = '';
  }

  async saveProfile(): Promise<void> {
    if (!this.userId) return;

    this.isSaving = true;
    this.saveError = null;

    if (this.selectedImage) {
      const formData = new FormData();
      formData.append('file', this.selectedImage);

      try {
        const result = await this.userService.validateImageSafety(formData).toPromise();
        if (!result?.isSafe) {
          Swal.fire(
            'Imagem imprópria',
            'A imagem enviada foi detectada como inadequada. Escolha outra imagem.',
            'warning'
          );
          this.isSaving = false;
          return;
        }
      } catch (error) {
        console.error('[Image Validation] Erro:', error);
        Swal.fire('Erro', 'Falha ao validar a imagem. Tente novamente.', 'error');
        this.isSaving = false;
        return;
      }
    }

    this.userService.editProfile(
      this.editableProfile.name,
      this.editableProfile.location,
      this.selectedImage ?? undefined
    ).subscribe({
      next: () => {
        this.isSaving = false;
        Swal.fire('Perfil atualizado!', 'Suas alterações foram salvas.', 'success')
          .then(() => window.location.reload());
      },
      error: (err) => {
        console.error(err);
        this.isSaving = false;
        Swal.fire('Erro!', 'Não foi possível atualizar o perfil.', 'error');
      }
    });
  }
}
