import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import Swal from 'sweetalert2';
import { CompanyService } from '../../../services/company.service';
import { MatDialog } from '@angular/material/dialog';
import { CompanyDto } from '../../../models/company-get.model';
import { CompanyUpdatePayload } from '../../../models/company-update.model';
import { Observable, Subject } from 'rxjs';

@Component({
  selector: 'app-editar-empresa',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatIconModule,
    RouterModule,
    FormsModule
  ],
  templateUrl: './editar-empresa.component.html',
  styleUrl: './editar-empresa.component.css'
})
export class EditarEmpresaComponent {
  empresaId: string | null = null;
  isLoading = false;
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;
  selectedImage?: File;

  isSubmitting = false;
  isDragging = false;

  companyName: string = '';
  companyDescription: string = '';

  currentCompanyData!: CompanyDto;

  maxDescriptionLength = 300;

  ngOnInit(): void {
    const parentId = this.route.parent?.snapshot.paramMap.get('id');

    if (parentId) {
      this.empresaId = parentId;
      console.log('ID da Empresa para edit company:', this.empresaId);
      this.loadCompanyData(this.empresaId);
    } else {
      console.error('ID da Empresa não encontrado na URL.');
      this.isLoading = false;
    }
  }

  constructor(
    private companyService: CompanyService,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog,
  ) { }

  loadCompanyData(id: string): void {
    this.isLoading = true;
    this.companyService.getCompanyById(id).subscribe({
      next: (data) => {
        this.currentCompanyData = data;
        this.isLoading = false;

        this.companyName = data.name;
        this.companyDescription = data.description || '';
        this.previewUrl = data.imageUrl || 'assets/placeholder.png';
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('Erro', 'Não foi possível carregar os dados da empresa.', 'error');
        this.router.navigate(['../informacoes'], { relativeTo: this.route });
      }
    });
  }

  onSave(): void {
    if (this.isSubmitting || !this.empresaId) {
      return;
    }

    if (!this.companyName || this.companyName.length > 100) {
      Swal.fire('Erro de Validação', 'O nome da empresa é obrigatório e não pode ter mais de 100 caracteres.', 'warning');
      return;
    }
    if (this.companyDescription.length > this.maxDescriptionLength) {
      Swal.fire('Erro de Validação', 'A descrição excede o limite de caracteres.', 'warning');
      return;
    }

    this.isSubmitting = true;
    this.isLoading = true;

    const updateObservable = this.selectedImage
      ? this.companyService.uploadCompanyImage(this.selectedImage)
      : new Observable<{ imageUrl: string }>(observer => {
        observer.next({ imageUrl: this.currentCompanyData.imageUrl });
        observer.complete();
      });

    updateObservable.subscribe({
      next: (uploadResult) => {
        const payload: CompanyUpdatePayload = {
          name: this.companyName,
          description: this.companyDescription,
          imageUrl: uploadResult.imageUrl,
        };

        this.companyService.updateCompany(this.empresaId!, payload).subscribe({
          next: () => {
            Swal.fire('Sucesso!', 'Empresa atualizada com sucesso.', 'success');
            this.loadCompanyData(this.empresaId!);
            this.isSubmitting = false;
            this.isLoading = false;
            this.router.navigate(['../informacoes'], { relativeTo: this.route });
          },
          error: (err) => {
            Swal.fire('Erro', 'Falha ao salvar as alterações.', 'error');
            this.isSubmitting = false;
            this.isLoading = false;
          }
        });
      },
      error: () => {
        Swal.fire('Erro', 'Falha ao fazer upload da imagem.', 'error');
        this.isSubmitting = false;
        this.isLoading = false;
      }
    });
  }

  get descriptionCharCount(): number {
    return this.companyDescription.length;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFile(files[0]);
    }
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
        this.openCropperDialog(reader.result, file);
      }
    };
    reader.onerror = (error) => {
      console.error('Erro ao ler o arquivo:', error);
      Swal.fire('Erro', 'Não foi possível ler o arquivo de imagem.', 'error');
    };
  }

  private openCropperDialog(imageBase64: string, originalFile: File): void {
    const dialogData: CropperDialogData = { imageBase64 };

    const dialogRef = this.dialog.open(CropperDialogComponent, {
      minWidth: '1000px',
      minHeight: '600px',
      data: dialogData,
      disableClose: true,
      panelClass: 'custom-transparent-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.selectedImage = result;
        this.previewUrl = URL.createObjectURL(result);
      }
    });
  }
}
