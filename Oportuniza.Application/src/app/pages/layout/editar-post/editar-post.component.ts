import { Component, ViewChild } from '@angular/core';
import { PublicationCreate } from '../../../models/publication-create.model';
import Swal from 'sweetalert2';
import { CropperDialogComponent, CropperDialogData } from '../../../extras/cropper-dialog/cropper-dialog.component';
import { debounceTime, switchMap } from 'rxjs';
import { PublicationService } from '../../../services/publication.service';
import { UserService } from '../../../services/user.service';
import { MatDialog } from '@angular/material/dialog';
import { CompanyService } from '../../../services/company.service';
import { CityService } from '../../../services/city.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
import { UserProfile } from '../../../models/UserProfile.model';
import { CompanyListDto } from '../../../models/company-list-dto-model';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { Publication } from '../../../models/Publications.model';
import { PublicationUpdateDto } from '../../../models/PublicationUpdate.model';

@Component({
  selector: 'app-editar-post',
  imports: [
    CommonModule,
    FormsModule,
    MatToolbarModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    ReactiveFormsModule,
  ],
  templateUrl: './editar-post.component.html',
  styleUrl: './editar-post.component.css'
})
export class EditarPostComponent {
  @ViewChild('publicationForm') publicationForm!: NgForm;

  publicationId: string | null = null;

  publication: Publication = {
    hasApplied: false,
    id: '',
    title: '',
    description: '',
    creationDate: '',
    imageUrl: '',
    resumee: '',
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
    cityId: '',
    isActive: 0,
  };

  selectedImage?: File;
  previewUrl: any;
  isSubmitting = false;
  isDragging = false;
  userProfile?: UserProfile;
  userCompanies: CompanyListDto[] = [];
  selectedAuthorId: string | null = null;
  today = '';

  cityControl = new FormControl('');
  filteredCities: any[] = [];
  cityModalOpen = false;

  constructor(
    private publicationService: PublicationService,
    private userService: UserService,
    private dialog: MatDialog,
    private companyService: CompanyService,
    private cityService: CityService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    const date = new Date();
    this.today = date.toISOString().split('T')[0];
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      this.publicationId = params.get('id');
      if (this.publicationId) {
        this.loadPostData();
      } else {
        this.router.navigate(['/publicar']);
        Swal.fire('Erro', 'Nenhum ID de post fornecido para edição.', 'error');
      }
    });


    this.cityControl.valueChanges
      .pipe(
        debounceTime(300),
        switchMap((value) =>
          value && value.length > 0 ? this.cityService.searchCities(value, 1, 20) : this.cityService.getCities(1, 20)
        )
      )
      .subscribe({
        next: (cities) => (this.filteredCities = cities),
        error: (err) => console.error('Erro ao carregar cidades:', err),
      });
  }

  loadPostData(): void {
    if (!this.publicationId) return;

    this.publicationService.getPublicationById(this.publicationId).subscribe({
      next: (data) => {
        const formattedExpirationDate = data.expirationDate.split('T')[0];

        this.publication = { ...data, expirationDate: formattedExpirationDate };

        this.previewUrl = data.imageUrl;
        this.selectedAuthorId = data.authorId;
        this.cityControl.setValue(data.local, { emitEvent: false });

        this.userService.getOwnProfile().subscribe(profile => (this.userProfile = profile));
        this.companyService
        .getUserCompanies()
        .subscribe(companies => (this.userCompanies = companies));

        console.log('Dados da postagem carregados:', this.publication);
        console.log('ID do autor atribuído:', this.selectedAuthorId);
      },
      error: (err) => {
        Swal.fire('Erro', 'Não foi possível carregar os dados da publicação.', 'error');
        this.router.navigate(['/']);
      },
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
    const dialogData: CropperDialogData = { imageBase64: imageBase64 };
    const dialogRef = this.dialog.open(CropperDialogComponent, {
      minWidth: '1000px',
      minHeight: '600px',
      data: dialogData,
      disableClose: true,
    });
    dialogRef.afterClosed().subscribe((result) => {
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
  openCityModal() {
    this.cityModalOpen = true;
  }
  closeCityModal() {
    this.cityModalOpen = false;
  }
  selectCityFromModal(city: any) {
    this.publication.cityId = city.id;
    this.publication.local = city.name;
    this.cityControl.setValue(city.name, { emitEvent: false });
    this.closeCityModal();
  }

  editPost(): void {
    if (this.publicationForm.invalid) {
      Swal.fire('Atenção', 'Por favor, preencha todos os campos obrigatórios.', 'warning');
      return;
    }

    if (!this.publicationId) {
      Swal.fire('Erro', 'ID da publicação não encontrado.', 'error');
      return;
    }

    if (!this.selectedAuthorId) {
      Swal.fire('Erro', 'ID do autor não pode ser nulo.', 'error');
      this.isSubmitting = false;
      return;
    }

    this.isSubmitting = true;
    const isCompanyPost = this.userCompanies.some((company) => company.id === this.selectedAuthorId);

    const dto: PublicationUpdateDto = {
      title: this.publication.title,
      description: this.publication.description,
      salary: this.publication.salary,
      shift: this.publication.shift,
      contract: this.publication.contract,
      local: this.publication.local,
      expirationDate: this.publication.expirationDate,
      authorUserId: this.selectedAuthorId!,
    };

    this.publicationService.updatePublication(this.publicationId, dto, this.selectedImage).subscribe({
      next: () => {
        this.isSubmitting = false;
        Swal.fire('Sucesso!', 'Publicação atualizada com sucesso!', 'success');
        this.router.navigate(['/inicio']);
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error('Erro ao atualizar a publicação:', err);
        Swal.fire(
          'Erro!',
          `Não foi possível atualizar a publicação: ${err.error?.message || 'Erro desconhecido'}`,
          'error'
        );
      },
    });
  }
}
