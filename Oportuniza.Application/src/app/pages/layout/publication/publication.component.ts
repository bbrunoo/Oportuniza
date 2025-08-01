import { Component, OnInit, ViewChild } from '@angular/core';
import { PublicationService } from '../../../services/publication.service';
import Swal from 'sweetalert2';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { UserProfile } from '../../../models/UserProfile.model';
import { MatToolbarModule } from "@angular/material/toolbar"
import { MatDialog } from '@angular/material/dialog';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { MatIconModule } from '@angular/material/icon';
import { CompanyListDto } from '../../../models/company-list-dto-model';
import { CompanyService } from '../../../services/company.service';
import { PublicationCreate } from '../../../models/publication-create.model';
@Component({
  selector: 'app-publication',
  imports: [CommonModule, FormsModule, MatToolbarModule, MatIconModule],
  templateUrl: './publication.component.html',
  styleUrl: './publication.component.css'
})

export class PublicationComponent implements OnInit {
  @ViewChild('publicationForm') publicationForm!: NgForm;

  publication: { title: string; content: string } = { title: '', content: '' };
  selectedSalary: string | null = null;
  selectedImage?: File;
  previewUrl: any;

  isSubmitting = false;
  isDragging = false;

  userProfile?: UserProfile;
  userCompanies: CompanyListDto[] = [];
  selectedAuthorId: string | null = null;

  constructor(
    private publicationService: PublicationService,
    private userService: UserService,
    private dialog: MatDialog,
    private companyService: CompanyService
  ) { }

  ngOnInit(): void {
    this.loadInitialData();
  }

  loadInitialData(): void {
    this.userService.getOwnProfile().subscribe(profile => {
      this.userProfile = profile;
      this.selectedAuthorId = profile.id;
    });

    this.companyService.getUserCompanies().subscribe(companies => {
      this.userCompanies = companies;
    });
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
    const dialogData: CropperDialogData = {
      imageBase64: imageBase64,

    };

    console.log(imageBase64);

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

  selectSalary(salaryRange: string): void {
    this.selectedSalary = salaryRange;
  }

   post(): void {
    if (this.publicationForm.invalid || !this.selectedImage || !this.selectedSalary || !this.selectedAuthorId) {
      Swal.fire('Atenção', 'Por favor, preencha todos os campos e selecione um autor, uma imagem e um salário.', 'warning');
      return;
    }

    this.isSubmitting = true;

    const isCompanyPost = this.selectedAuthorId !== this.userProfile?.id;

    const dto: PublicationCreate = {
      title: this.publication.title,
      content: this.publication.content,
      salary: this.selectedSalary!,
      postAsCompanyId: isCompanyPost ? this.selectedAuthorId : null
    };

      console.log('Enviando para o backend:', dto);

    this.publicationService.createPublication(dto, this.selectedImage).subscribe({
      next: () => {
        this.isSubmitting = false;
        Swal.fire('Sucesso!', 'Publicação criada com sucesso!', 'success');

        this.publicationForm.resetForm();
        this.previewUrl = null;
        this.selectedImage = undefined;
        this.selectedSalary = null;
        this.selectedAuthorId = this.userProfile?.id ?? null; 
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error(err);
        Swal.fire('Erro!', `Não foi possível criar a publicação: ${err.error?.message || 'Erro desconhecido'}`, 'error');
      }
    });
  }
}
