import { CompanyCreatePayload, CompanyService } from './../../../services/company.service';
import { Component, ElementRef, OnInit, QueryList, ViewChildren } from '@angular/core';
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
  name = '';
  cityState = '';
  phone = '';
  email = '';
  cnpj = '';
  description = '';

  verificationModalOpen = false;
  verificationCode = '';
  codeSent = false;
  isVerifying = false;
  codeLength = 6;

  @ViewChildren('d1, d2, d3, d4, d5, d6') codeFields!: QueryList<ElementRef>;

  codeInputs: string[] = ['', '', '', '', '', ''];
  cooldown = 0;
  timer: any;

  isLoading = false;
  selectedImage?: File;
  previewUrl: string | ArrayBuffer | null = null;

  isSubmitting = false;
  isDragging = false;

  cityControl = new FormControl('');
  filteredCities: any[] = [];
  cityModalOpen = false;
  selectedCity: { id: string; name: string } | null = null;

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

  openVerificationModal() {
    this.verificationModalOpen = true;
    this.codeSent = false;
    this.verificationCode = '';
  }

  closeVerificationModal() {
    this.verificationModalOpen = false;
    this.verificationCode = '';
    this.isVerifying = false;
    this.codeInputs = ['', '', '', '', '', ''];
  }

  sendEmailCode() {
    if (!this.email) {
      Swal.fire('Erro', 'Informe um e-mail válido.', 'error');
      return;
    }

    this.companyService.sendCompanyVerificationCode(this.email).subscribe({
      next: () => {
        this.codeSent = true;
        this.startCooldown(60);
        Swal.fire('Código enviado!', `Enviamos um código para ${this.email}.`, 'info');
      },
      error: (err) => {
        console.error('Erro ao enviar código:', err);
        Swal.fire('Erro', 'Falha ao enviar o código.', 'error');
      }
    });
  }

  resendCode() {
    if (this.cooldown > 0) return;
    this.sendEmailCode();
  }

  startCooldown(seconds: number) {
    this.cooldown = seconds;
    clearInterval(this.timer);
    this.timer = setInterval(() => {
      this.cooldown--;
      if (this.cooldown <= 0) clearInterval(this.timer);
    }, 1000);
  }

  onInput(event: any, index: number) {
    const value = event.target.value;
    if (value.length > 1) event.target.value = value.slice(0, 1);
    if (value && index < this.codeInputs.length - 1) this.focusNext(index + 1);
  }

  onKeydown(event: KeyboardEvent, index: number) {
    if (event.key === 'Backspace' && !this.codeInputs[index] && index > 0)
      this.focusNext(index - 1);
  }

  focusNext(index: number) {
    const fields = this.codeFields.toArray();
    if (fields[index]) fields[index].nativeElement.focus();
  }

  private resetForm(): void {
    this.name = '';
    this.selectedCity = null;
    this.phone = '';
    this.email = '';
    this.cnpj = '';
    this.description = '';
    this.previewUrl = null;
    this.selectedImage = undefined;
    this.verificationCode = '';
  }

  private async processImageValidation(): Promise<void> {
    if (!this.selectedImage) {
      Swal.fire({
        icon: 'warning',
        title: 'Atenção',
        text: 'Selecione uma imagem antes de enviar.'
      });
      return;
    }

    this.isSubmitting = true;
    this.isLoading = true;

    if (this.selectedImage) {
      const formData = new FormData();
      formData.append('File', this.selectedImage);

      try {
        const result: any = await this.companyService.validateImageSafety(formData).toPromise();

        if (!result?.isSafe) {
          Swal.fire({
            icon: 'warning',
            title: 'Imagem imprópria',
            text: 'A imagem enviada foi detectada como inadequada. Escolha outra imagem.'
          });
          this.isSubmitting = false;
          this.isLoading = false;
          return;
        }
      } catch (error) {
        console.error('[Image Validation] Erro:', error);
        Swal.fire({
          icon: 'error',
          title: 'Falha ao validar imagem',
          text: 'Não foi possível verificar se a imagem é segura. Tente novamente.'
        });
        this.isSubmitting = false;
        this.isLoading = false;
        return;
      }
    }

    const dto: CompanyCreatePayload = {
      name: this.name,
      cityState: this.selectedCity!.name,
      phone: this.phone,
      email: this.email,
      cnpj: this.cnpj,
      description: this.description || ''
    };

    this.companyService.createCompany(dto, this.selectedImage!, this.verificationCode).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.isLoading = false;
        Swal.fire('Sucesso!', 'Empresa criada com sucesso!', 'success').then(() => {
          this.resetForm();
          this.router.navigate(['/inicio/minhas-empresas']);
        });
      },
      error: (err) => {
        this.isSubmitting = false;
        this.isLoading = false;
        const backendMessage =
          err.error?.error ||
          err.error?.message ||
          err.error ||
          'Erro desconhecido.';
        Swal.fire('Falha ao criar empresa', backendMessage, 'error');
      }
    });
  }

  async onSubmit(): Promise<void> {
    if (this.isSubmitting) return;

    if (!this.name || !this.selectedCity || !this.phone || !this.email || !this.cnpj || !this.selectedImage) {
      Swal.fire('Atenção', 'Preencha todos os campos obrigatórios.', 'warning');
      return;
    }

    this.isSubmitting = true;
    this.isLoading = true;

    const formData = new FormData();
    formData.append('File', this.selectedImage!);

    try {
      const result: any = await this.companyService.validateImageSafety(formData).toPromise();

      if (!result?.isSafe) {
        Swal.fire({
          icon: 'warning',
          title: 'Imagem imprópria',
          text: 'A imagem enviada foi detectada como inadequada. Escolha outra imagem.'
        });
        this.isSubmitting = false;
        this.isLoading = false;
        return;
      }

      this.verificationModalOpen = true;
      this.isSubmitting = false;
      this.isLoading = false;

    } catch (error) {
      console.error('[Image Validation] Erro:', error);
      Swal.fire({
        icon: 'error',
        title: 'Falha ao validar imagem',
        text: 'Não foi possível verificar se a imagem é segura. Tente novamente.'
      });
      this.isSubmitting = false;
      this.isLoading = false;
    }
  }

  confirmCompany() {
    this.verificationCode = this.codeInputs.join('');

    if (!this.verificationCode || this.verificationCode.length < 6) {
      Swal.fire({
        icon: 'warning',
        title: 'Código inválido',
        text: 'Digite o código completo enviado ao seu e-mail para continuar.'
      });
      return;
    }

    this.verificationModalOpen = false;
    this.isVerifying = true;

    const dto: CompanyCreatePayload = {
      name: this.name,
      cityState: this.selectedCity!.name,
      phone: this.phone,
      email: this.email,
      cnpj: this.cnpj,
      description: this.description || ''
    };

    this.companyService.createCompany(dto, this.selectedImage!, this.verificationCode).subscribe({
      next: () => {
        this.isVerifying = false;
        Swal.fire('Sucesso!', 'Empresa criada com sucesso!', 'success').then(() => {
          this.resetForm();
          this.router.navigate(['/inicio/minhas-empresas']);
        });
      },
      error: (err) => {
        this.isVerifying = false;

        let backendMessage = 'Erro desconhecido.';
        if (typeof err.error === 'string') {
          backendMessage = err.error;
        } else if (err.error?.message) {
          backendMessage = err.error.message;
        } else if (err.message) {
          backendMessage = err.message;
        }

        Swal.fire({
          icon: 'error',
          title: 'Falha ao criar empresa',
          text: backendMessage,
          confirmButtonColor: '#d33'
        });
      }
    });
  }
}

