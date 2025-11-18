import { CommonModule, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CompanyDto } from '../../../models/company-get.model';
import { CompanyService } from '../../../services/company.service';

@Component({
  selector: 'app-informacoes',
  imports: [NgIf, CommonModule],
  templateUrl: './informacoes.component.html',
  styleUrl: './informacoes.component.css'
})
export class InformacoesComponent {
  company!: CompanyDto;
  isLoading: boolean = true;
  empresaId: string | null = null;
  errorMessage: string | null = null;

  constructor(private route: ActivatedRoute, private companyService: CompanyService) { }

  ngOnInit(): void {
    const parentRoute = this.route.parent;

    if (parentRoute) {
      this.empresaId = parentRoute.snapshot.paramMap.get('id');
    }

    if (this.empresaId) {
      this.loadCompanyInformation(this.empresaId);

    } else {
      this.isLoading = false;
      this.errorMessage = 'ID da Empresa não encontrado na URL.';
      console.error(this.errorMessage);
    }
  }

  loadCompanyInformation(companyId: string): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.companyService.getCompanyById(companyId).subscribe({
      next: (data) => {
        this.company = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Erro ao carregar os dados da empresa.';
        console.error('Erro ao buscar a empresa:', err);
      }
    });
  }
}
