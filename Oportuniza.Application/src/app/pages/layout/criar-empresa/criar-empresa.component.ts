import { CompanyCreatePayload, CompanyService } from './../../../services/company.service';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { switchMap } from 'rxjs';
import Swal from 'sweetalert2';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-criar-empresa',
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
  templateUrl: './criar-empresa.component.html',
  styleUrls: ['./criar-empresa.component.css']
})

export class CriarEmpresaComponent {
  name: string = '';
  cityState: string = '';
  phone: string = '';
  email: string = '';
  cnpj: string = '';
  description: string = '';

  isLoading = false;
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;
  selectedImage?: File;

  isSubmitting = false;
  isDragging = false;

  constructor(
    private companyService: CompanyService,
    private router: Router,
    private dialog: MatDialog,
  ) { }

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
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.selectedImage = result;
        this.previewUrl = URL.createObjectURL(result);
      }
    });
  }

  onSubmit(): void {
    if (!this.name || !this.cityState || !this.phone || !this.email || !this.cnpj || !this.selectedImage) {
      Swal.fire({
        icon: 'warning',
        title: 'Atenção',
        text: 'Por favor, preencha todos os campos obrigatórios e selecione uma imagem.'
      });
      return;
    }

    this.isLoading = true;

    this.companyService.uploadCompanyImage(this.selectedImage).pipe(
      switchMap(uploadResponse => {
        console.log('Upload response:', uploadResponse);
        const companyData: CompanyCreatePayload = {
          name: this.name,
          cityState: this.cityState,
          phone: this.phone,
          email: this.email,
          cnpj: this.cnpj,
          description: this.description,
          imageUrl: uploadResponse.imageUrl
        };
        console.log('Payload enviado:', companyData);
        return this.companyService.createCompany(companyData);
      })
    ).subscribe({
      next: () => {
        this.isLoading = false;
        Swal.fire({
          icon: 'success',
          title: 'Sucesso!',
          text: 'Empresa criada com sucesso!'
        }).then(() => {
          this.name = '';
          this.cityState = '';
          this.phone = '';
          this.email = '';
          this.cnpj = '';
          this.description = '';
          this.previewUrl = null;
          this.selectedImage = undefined;
          this.router.navigate(['/inicio/minhas-empresas']);
        });
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Erro ao criar empresa', err);
        Swal.fire({
          icon: 'error',
          title: 'Erro',
          text: `Erro ao criar empresa: ${err.message || 'Verifique sua conexão ou dados.'}`
        });
      }
    });
  }
}

