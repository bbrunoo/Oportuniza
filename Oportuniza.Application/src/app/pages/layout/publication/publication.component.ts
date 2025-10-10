import { Component, OnInit, ViewChild } from '@angular/core';
import { PublicationService } from '../../../services/publication.service';
import Swal from 'sweetalert2';
import { CommonModule } from '@angular/common';
import { FormControl, FormsModule, NgForm, ReactiveFormsModule } from '@angular/forms';
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
import { CityService } from '../../../services/city.service';
import { debounceTime, switchMap } from 'rxjs';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { GetProfiles } from '../../../models/new-models/Profiles.model';
@Component({
  selector: 'app-publication',
  imports: [CommonModule, FormsModule, MatToolbarModule, MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    ReactiveFormsModule
  ],
  templateUrl: './publication.component.html',
  styleUrl: './publication.component.css'
})

export class PublicationComponent implements OnInit {
  @ViewChild('publicationForm') publicationForm!: NgForm;

  publication: Publication = {
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
    resumee: '',
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

  showCities = false;

  hideCities() {
    setTimeout(() => this.showCities = false, 200);
  }

  cityControl = new FormControl('');
  filteredCities: any[] = [];

  cityModalOpen = false;

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

  constructor(
    private publicationService: PublicationService,
    private userService: UserService,
    private dialog: MatDialog,
    private companyService: CompanyService,
    private cityService: CityService
  ) {

    const date = new Date();
    this.today = date.toISOString().split('T')[0];
  }

  ngOnInit(): void {
    this.loadInitialData();

    this.cityControl.valueChanges.pipe(
      debounceTime(300),
      switchMap(value =>
        value && value.length > 0
          ? this.cityService.searchCities(value, 1, 20)
          : this.cityService.getCities(1, 20)
      )
    ).subscribe({
      next: (cities) => {
        this.filteredCities = cities;
      },
      error: (err) => console.error('Erro ao carregar cidades:', err)
    });
  }

  onSelectCity(city: any) {
    this.cityControl.setValue(city.name, { emitEvent: false });
    this.publication.cityId = city.id;
    this.publication.local = city.name;
  }

  loadInitialData(): void {
    this.userService.getOwnProfile().subscribe(profile => {
      this.userProfile = profile;
      this.selectedAuthorId = profile.id;

      if (profile.isCompany) {
        this.userCompanies = [
          {
            id: profile.id,
            name: profile.name,
            description: '',
            imageUrl: profile.imageUrl ?? '',
            cityState: '',
            phone: profile.phone ?? '',
            email: profile.email ?? '',
            cnpj: '',
            UserRole: profile.role ?? '',
            OwnerId: profile.id,
            isActive: profile.isActive,
          }
        ];
      }
      else {
        this.companyService.getUserCompanies().subscribe(companies => {
          this.userCompanies = companies;
        });
      }
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
      !this.publication.cityId ||
      !this.publication.expirationDate ||
      !this.selectedAuthorId
    ) {
      Swal.fire('Atenção', 'Por favor, selecione uma cidade válida.', 'warning');
      return;
    }

    this.isSubmitting = true;

    const selectedCompany = this.userCompanies.find(c => c.id === this.selectedAuthorId);

    const dto: PublicationCreate = {
      title: this.publication.title,
      description: this.publication.description,
      salary: this.publication.salary,
      shift: this.publication.shift,
      contract: this.publication.contract,
      local: this.publication.local,
      expirationDate: this.publication.expirationDate,
      cityId: this.publication.cityId!,
      postAsCompanyId: selectedCompany ? selectedCompany.id : null
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
