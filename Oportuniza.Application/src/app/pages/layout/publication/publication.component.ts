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
import { Publication } from '../../../models/Publications.model';
@Component({
  selector: 'app-publication',
  imports: [CommonModule, FormsModule, MatToolbarModule, MatIconModule],
  templateUrl: './publication.component.html',
  styleUrl: './publication.component.css'
})

export class PublicationComponent implements OnInit {
  @ViewChild('publicationForm') publicationForm!: NgForm;

  publication: Publication & { tags: string[] } = {
    hasApplied: false,
    id: '',
    title: '',
    description: '',
    creationDate: '',
    imageUrl: '',
    expired: false,
    authorId: '',
    authorType: 0,
    authorName: '',
    shift: '',
    local: '',
    expirationDate: '',
    contract: '',
    salary: '',
    authorImageUrl: '',
    tags: [] // Propriedade adicional apenas para o formulário de criação
  };

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

  selectAuthor(authorId: string): void {
    this.selectedAuthorId = authorId;
  }

  selectTitle(title: string): void {
    this.publication.title = title;
  }

  selectSalary(salaryRange: string): void {
    this.publication.salary = salaryRange;
  }

  selectShift(shift: string): void {
    this.publication.shift = shift;
  }

  selectContract(contract: string): void {
    this.publication.contract = contract;
  }

  post(): void {
    if (
      this.publicationForm.invalid ||
      !this.selectedImage ||
      !this.publication.title ||
      !this.publication.salary ||
      !this.publication.shift ||
      !this.publication.contract ||
      !this.publication.local ||
      !this.publication.expirationDate ||
      !this.selectedAuthorId
    ) {
      Swal.fire('Atenção', 'Por favor, preencha todos os campos e selecione um autor, uma imagem, um título, salário, turno, tipo de contrato e localização.', 'warning');
      return;
    }

    this.isSubmitting = true;

    const isCompanyPost = this.userCompanies.some(company => company.id === this.selectedAuthorId);

    const dto: PublicationCreate = {
      title: this.publication.title,
      content: this.publication.description,
      salary: this.publication.salary,
      shift: this.publication.shift,
      contract: this.publication.contract,
      local: this.publication.local,
      expirationDate: this.publication.expirationDate,
      tags: [],
      postAsCompanyId: isCompanyPost ? this.selectedAuthorId! : null!
    };

    this.publicationService.createPublication(dto, this.selectedImage).subscribe({
      next: () => {
        this.isSubmitting = false;
        Swal.fire('Sucesso!', 'Publicação criada com sucesso!', 'success');

        this.publicationForm.resetForm();
        this.previewUrl = null;
        this.selectedImage = undefined;
        this.publication.title = '';
        this.publication.salary = '';
        this.publication.shift = '';
        this.publication.contract = '';
        this.publication.local = '';
        this.publication.expirationDate = '';
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
