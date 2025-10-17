import { CompanyCreatePayload, CompanyService } from './../../../services/company.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule, FormControl } from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime, switchMap } from 'rxjs';
import Swal from 'sweetalert2';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { NgxMaskDirective } from "ngx-mask";
import { CityService } from '../../../services/city.service';

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
    FormsModule,
    NgxMaskDirective
  ],
  templateUrl: './criar-empresa.component.html',
  styleUrls: ['./criar-empresa.component.css']
})

export class CriarEmpresaComponent implements OnInit {
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

  cityControl = new FormControl('');
  filteredCities: any[] = [];
  cityModalOpen = false;
  selectedCity: { id: string, name: string } | null = null;

  constructor(
    private companyService: CompanyService,
    private router: Router,
    private dialog: MatDialog,
    private cityService: CityService
  ) { }

  ngOnInit(): void {
    this.cityControl.valueChanges.pipe(
      debounceTime(300),
      switchMap(value =>
        value && value.length > 0
          ? this.cityService.searchCities(value, 1, 20)
          : this.cityService.getCities(1, 20)
      )
    ).subscribe({
      next: (cities) => this.filteredCities = cities,
      error: (err) => console.error('Erro ao carregar cidades:', err)
    });
  }

  openCityModal() { this.cityModalOpen = true; }

  closeCityModal() { this.cityModalOpen = false; }

  selectCity(city: any) {
    this.selectedCity = city;
    this.cityControl.setValue(city.name, { emitEvent: false });
    this.cityState = city.name;
    this.closeCityModal();
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
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.selectedImage = result;
        this.previewUrl = URL.createObjectURL(result);
      }
    });
  }

  isValidCnpj(cnpj: string): boolean {
    if (!cnpj) return false;

    cnpj = cnpj.replace(/[^\d]+/g, '');

    if (cnpj.length !== 14) return false;
    if (/^(\d)\1+$/.test(cnpj)) return false;

    const t = cnpj.length - 2;
    const d = cnpj.substring(t);
    const d1 = parseInt(d.charAt(0), 10);
    const d2 = parseInt(d.charAt(1), 10);
    const calc = (x: number) => {
      let n = 0;
      let y = x - 7;
      for (let i = x; i >= 1; i--) {
        n += parseInt(cnpj.charAt(x - i), 10) * y++;
        if (y > 9) y = 2;
      }
      const r = 11 - (n % 11);
      return r > 9 ? 0 : r;
    };

    return calc(12) === d1 && calc(13) === d2;
  }

  onSubmit(): void {
    if (!this.name || !this.selectedCity || !this.phone || !this.email || !this.cnpj || !this.selectedImage) {
      Swal.fire({
        icon: 'warning',
        title: 'Atenção',
        text: 'Por favor, preencha todos os campos obrigatórios e selecione uma imagem.'
      });
      return;
    }

    if (!this.isValidCnpj(this.cnpj)) {
      Swal.fire({
        icon: 'error',
        title: 'CNPJ inválido',
        text: 'Por favor, insira um CNPJ válido.'
      });
      return;
    }

    this.isLoading = true;

    this.companyService.consultarCnpj(this.cnpj).pipe(
      switchMap((dados) => {
        if (!dados.ativo) {
          Swal.fire({
            icon: 'error',
            title: 'CNPJ inativo ou não encontrado',
            text: 'O CNPJ informado não está ativo na Receita Federal.'
          });
          this.isLoading = false;
          throw new Error('CNPJ inativo');
        }

        // Se ativo → faz upload da imagem
        return this.companyService.uploadCompanyImage(this.selectedImage!);
      }),
      switchMap(uploadResponse => {
        // Monta o payload e envia para o backend
        const companyData: CompanyCreatePayload = {
          name: this.name,
          cityState: this.cityState,
          phone: this.phone,
          email: this.email,
          cnpj: this.cnpj,
          description: this.description,
          imageUrl: uploadResponse.imageUrl
        };

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
        this.isSubmitting = false;
        console.error('Erro no envio:', err);

        const backendMessage =
          err.error?.error ||
          err.error?.message ||
          err.error ||
          'Erro desconhecido.';

        Swal.fire({
          icon: 'error',
          title: 'Falha ao criar publicação',
          text: backendMessage.includes('imprópria')
            ? 'A imagem foi detectada como imprópria e não pode ser publicada. Escolha outra imagem.'
            : backendMessage,
          confirmButtonText: 'Entendi'
        });
      }
    });
  }

}

