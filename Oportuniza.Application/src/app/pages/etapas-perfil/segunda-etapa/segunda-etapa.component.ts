import {
  Component, ElementRef, EventEmitter, HostListener, Input, OnInit, Output, ViewChild, inject
} from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../services/user.service';
import { AreaService } from '../../../services/area.service';
import { AreaInteresse } from '../../../models/AreaInteresse.model';
import Swal from 'sweetalert2';
import { MultiSelectModule } from 'primeng/multiselect';

interface SelectedItem {
  id: string,
  interestArea: string,
  selected?: boolean
}

@Component({
  selector: 'app-terceira-etapa',
  standalone: true,
  imports: [RouterModule, FormsModule, CommonModule, MultiSelectModule, RouterModule],
  templateUrl: './segunda-etapa.component.html',
  styleUrls: ['./segunda-etapa.component.css']
})
export class SegundaEtapaComponent implements OnInit {
  private elementRef = inject(ElementRef);
  @ViewChild('dropdownContainer') dropdownRef!: ElementRef;

  allAreas: AreaInteresse[] = [];
  filteredAreas: AreaInteresse[] = [];
  selectedAreaIds: string[] = [];
  isDropdownVisible = false;

  constructor(
    private areaService: AreaService,
    private userService: UserService,
    private router: Router,
  ) { }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.dropdownRef && !this.dropdownRef.nativeElement.contains(event.target)) {
      this.isDropdownVisible = false;
    }
  }

  ngOnInit() {
    this.areaService.getAllAreas().subscribe({
      next: (res) => {
        this.allAreas = res.map(a => ({ ...a, selected: false }));
        this.filteredAreas = [...this.allAreas];
      },
      error: (err) => {
        console.error('Erro ao carregar áreas:', err);
        Swal.fire('Erro', 'Não foi possível carregar áreas de interesse', 'error');
      }
    });
  }

  showDropdown() {
    this.isDropdownVisible = true;
  }

  filterItems(event: Event) {
    const termo = (event.target as HTMLInputElement).value.toLowerCase();
    this.filteredAreas = this.allAreas.filter(area =>
      area.interestArea.toLowerCase().includes(termo)
    );
  }

  toggleItem(area: AreaInteresse, event: MouseEvent) {
    event.stopPropagation();

    if (area.selected) {
      area.selected = false;
      this.selectedAreaIds = this.selectedAreaIds.filter(id => id !== area.id);
      return
    } else {
      if (this.selectedAreaIds.length >= 3) {
        Swal.fire('Limite atingido', 'Você só pode selecionar até 3 áreas de interesse.', 'warning');
        return;
      }
      area.selected = true;
      this.selectedAreaIds.push(area.id);
      return;
    }
  }

  removeItem(area: AreaInteresse, event: MouseEvent) {
    event.stopPropagation();
    area.selected = false;
    this.selectedAreaIds = this.selectedAreaIds.filter(id => id !== area.id);
  }

  concluirCadastro(): void {
    const nome = localStorage.getItem('profileName');
    const phone = localStorage.getItem('profileTel');
    const userId = localStorage.getItem('userId');
    const imageUrl = localStorage.getItem('profileImageUrl');

    if (!userId) {
      Swal.fire('Erro', 'Usuário não identificado.', 'error');
      this.router.navigateByUrl("/inicio");
      return;
    }

    if (this.selectedAreaIds.length === 0) {
      Swal.fire('Campo obrigatório', 'Selecione pelo menos uma área de interesse.', 'warning');
      return;
    }

    const dados = {
      fullName: nome || '',
      imageUrl: imageUrl || '',
      phone: phone || '',
      interests: this.selectedAreaIds.join(','),
      areaOfInterestIds: this.selectedAreaIds,
      isProfileCompleted: true
    };

    console.log("dados antes de enviar:", dados);

    this.userService.updateProfile(dados, userId).subscribe({
      next: () => {
        localStorage.removeItem('profileName');
        localStorage.removeItem('profileTel');
        localStorage.removeItem('userId');
        localStorage.removeItem('profileImageUrl');

        Swal.fire('Sucesso', 'Perfil atualizado com sucesso.', 'success').then(() => {
          this.router.navigate(['/inicio']);
        });
      }
    });
  }
}
