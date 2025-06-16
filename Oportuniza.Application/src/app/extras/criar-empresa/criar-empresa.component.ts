import { CompanyService } from './../../services/company.service';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { CompanyCreatePayload } from '../../services/company.service';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { switchMap } from 'rxjs';
import Swal from 'sweetalert2';

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
    RouterModule
  ],
  templateUrl: './criar-empresa.component.html',
  styleUrl: './criar-empresa.component.css'
})

export class CriarEmpresaComponent {
  companyForm: FormGroup;
  isLoading = false;
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;

  constructor(
    private fb: FormBuilder,
    private companyService: CompanyService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.companyForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.maxLength(500)]],
      image: [null, [Validators.required]]
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;
      this.companyForm.patchValue({ image: file });
      this.companyForm.get('image')?.updateValueAndValidity();

      const reader = new FileReader();
      reader.onload = () => {
        this.previewUrl = reader.result;
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    this.companyForm.get('image')?.markAsTouched();

    if (this.companyForm.invalid || !this.selectedFile) {
      Swal.fire({
        icon: 'warning',
        title: 'Atenção',
        text: 'Por favor, preencha todos os campos e selecione uma imagem.'
      });
      return;
    }

    this.isLoading = true;

    
    this.companyService.uploadCompanyImage(this.selectedFile).pipe(
      switchMap(uploadResponse => {
        const companyData: CompanyCreatePayload = {
          name: this.companyForm.value.name,
          description: this.companyForm.value.description,
          imageUrl: uploadResponse.imageUrl
        };
        return this.companyService.createCompany(companyData);
      })
    ).subscribe({
      next: (response) => {
        this.isLoading = false;
        Swal.fire({
          icon: 'success',
          title: 'Sucesso!',
          text: 'Empresa criada com sucesso!'
        }).then(() => {
          this.companyForm.reset();
          this.previewUrl = null;
          this.selectedFile = null;
        });
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire({
          icon: 'error',
          title: 'Erro',
          text: `Erro ao criar empresa: ${err.message}`
        });
      }
    });
  }
}
