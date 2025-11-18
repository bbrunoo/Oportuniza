import { Component, ElementRef, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
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

  verificationModalOpen = false;
  verificationCode = '';
  codeSent = false;
  isVerifying = false;

  userPhone: string = '';

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
    postAuthorName: '',
    cityId: '',
    isActive: 0,
  };

  @ViewChildren('d1, d2, d3, d4, d5, d6') codeFields!: QueryList<ElementRef>;

  codeInputs: string[] = ['', '', '', '', '', ''];
  codeLength = 6;
  cooldown = 0;
  timer: any;

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

  onSelectCity(city: any) {
    this.cityControl.setValue(city.name, { emitEvent: false });
    this.publication.cityId = city.id;
    this.publication.local = city.name;
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
  }

  sendEmailCode() {
    const email = this.userProfile?.email;
    if (!email) {
      Swal.fire({
        icon: 'error',
        title: 'E-mail não encontrado',
        text: 'Não foi possível identificar o e-mail do usuário logado. Faça login novamente.',
      });
      return;
    }

    this.publicationService.sendPostVerificationCode(email).subscribe({
      next: () => {
        this.codeSent = true;
        this.startCooldown(60);
        Swal.fire({
          icon: 'info',
          title: 'Código enviado!',
          text: `Enviamos um código de verificação para ${email}. Verifique sua caixa de entrada e também o spam.`,
        });
      },
      error: (err) => {
        console.error('[Email Verification] Erro:', err);
        let msg = 'Não foi possível enviar o código. Verifique sua conexão e tente novamente.';

        if (err.status === 429)
          msg = 'Você solicitou códigos muitas vezes. Aguarde alguns minutos antes de tentar novamente.';

        if (err.status === 404)
          msg = 'O e-mail informado não foi encontrado. Faça login novamente e tente enviar o código.';

        Swal.fire('Erro no envio do código', msg, 'error');
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
    const MIN_SIZE_PX = 400;

    if (!validTypes.includes(file.type)) {
      Swal.fire('Tipo inválido', 'Apenas imagens PNG, JPG ou JPEG são permitidas.', 'warning');
      return;
    }

    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      if (typeof reader.result === 'string') {
        const img = new Image();
        img.onload = () => {
          if (img.width < MIN_SIZE_PX || img.height < MIN_SIZE_PX) {
            Swal.fire(
              'Imagem Pequena',
              `A imagem deve ter no mínimo ${MIN_SIZE_PX}px de largura e altura.
             Esta imagem tem ${img.width}px por ${img.height}px.`,
              'warning'
            );
            return;
          }

          this.openCropperDialog(reader.result as string, file);
        };
        img.onerror = () => {
          Swal.fire('Erro', 'Não foi possível carregar a imagem para verificar as dimensões.', 'error');
        };
        img.src = reader.result as string;
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

  confirmPublication(): void {
    this.verificationCode = this.codeInputs.join('');

    if (!this.verificationCode || this.verificationCode.length < 6) {
      Swal.fire({
        icon: 'warning',
        title: 'Código incompleto',
        text: 'Digite os 6 dígitos enviados ao seu e-mail para continuar.',
      });
      return;
    }

    this.isVerifying = true;

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

    this.publicationService.createPublicationWithCode(dto, this.selectedImage!, this.verificationCode).subscribe({
      next: () => {
        this.isVerifying = false;
        this.closeVerificationModal();
        Swal.fire({
          icon: 'success',
          title: 'Publicação criada!',
          text: 'Sua vaga foi publicada com sucesso e já está visível para outros usuários.',
        });
        this.publicationForm.resetForm();
        this.previewUrl = null;
        this.selectedImage = undefined;
        this.codeInputs = ['', '', '', '', '', ''];
      },
      error: (err) => {
        this.isVerifying = false;
        const status = err.status;
        const msg = this.resolveServerError(err, status);
        Swal.fire('Erro ao publicar', msg, 'error');
      }
    });
  }

  private resolveServerError(err: any, status: number): string {
    const serverMsg = err?.error?.error || err?.error?.message || '';

    if (status === 400 && serverMsg.includes('código de verificação inválido'))
      return 'O código informado está incorreto ou expirou. Solicite um novo código e tente novamente.';

    if (status === 400 && serverMsg.includes('Imagem imprópria'))
      return 'A imagem enviada foi detectada como inadequada. Escolha outra imagem.';

    if (status === 403)
      return 'Você não tem permissão para publicar em nome da empresa selecionada.';

    if (status === 404 && serverMsg.includes('Usuário'))
      return 'Não foi possível validar seu usuário. Faça login novamente.';

    if (status === 503 || serverMsg.includes('serviço indisponível'))
      return 'Serviço temporariamente indisponível. Tente novamente em alguns minutos.';

    return 'Ocorreu um erro inesperado ao criar sua publicação. Verifique os dados e tente novamente.';
  }

  async post(): Promise<void> {
    if (this.isSubmitting) return;

    const missingFields = [];
    if (!this.selectedImage) missingFields.push('Imagem');
    if (!this.publication.title) missingFields.push('Título');
    if (!this.publication.description) missingFields.push('Descrição');
    if (!this.publication.salary) missingFields.push('Faixa salarial');
    if (!this.publication.shift) missingFields.push('Turno');
    if (!this.publication.contract) missingFields.push('Tipo de contrato');
    if (!this.publication.local || !this.publication.cityId) missingFields.push('Localização');
    if (!this.publication.expirationDate) missingFields.push('Data de expiração');
    if (!this.selectedAuthorId) missingFields.push('Autor');

    if (missingFields.length > 0) {
      Swal.fire({
        icon: 'warning',
        title: 'Campos obrigatórios ausentes',
        html: `
        <p>Preencha os seguintes campos antes de continuar:</p>
        <ul style="text-align:left; margin-left:1rem;">
          ${missingFields.map(f => `<li>${f}</li>`).join('')}
        </ul>
      `
      });
      return;
    }

    if (this.selectedImage) {
      const formData = new FormData();
      formData.append('file', this.selectedImage);

      try {
        const result = await this.publicationService.validateImageSafety(formData).toPromise();
        if (!result?.isSafe) {
          Swal.fire({
            icon: 'warning',
            title: 'Imagem imprópria',
            text: 'A imagem enviada foi detectada como inadequada. Escolha outra imagem.',
          });
          return;
        }
      } catch (error) {
        console.error('[Image Validation] Erro:', error);
        Swal.fire({
          icon: 'error',
          title: 'Falha ao validar imagem',
          text: 'Não foi possível verificar se a imagem é segura. Tente novamente.',
        });
        return;
      }
    }

    this.openVerificationModal();
  }
}
