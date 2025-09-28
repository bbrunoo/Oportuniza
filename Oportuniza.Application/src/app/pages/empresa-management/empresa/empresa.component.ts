import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink, RouterModule, RouterOutlet } from '@angular/router';
import { CompanyService } from '../../../services/company.service';
import { CompanyDto } from '../../../models/company-get.model';
import { NgIf } from '@angular/common';
import { filter, Observable, Subject, Subscription } from 'rxjs';
import { NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-empresa',
  imports: [RouterOutlet, RouterLink, NgIf, RouterModule],
  templateUrl: './empresa.component.html',
  styleUrl: './empresa.component.css'
})
export class EmpresaComponent implements OnInit {
  company!: CompanyDto;
  isLoading: boolean = true;
  companyId: string | null = null;
  errorMessage: string | null = null;
  private subscription!: Subscription;


  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.companyId = params.get('id');
      if (this.companyId) {
        this.loadCompanyInformation();
      } else {
        this.isLoading = false;
        this.errorMessage = 'ID da empresa não encontrado na URL.';
      }
    });

    this.subscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        if (this.companyId) {
          this.loadCompanyInformation();
        }
      });
  }

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private companyService: CompanyService,
  ) { }

  loadCompanyInformation(): void {
    if (!this.companyId) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;

    this.companyService.getCompanyById(this.companyId).subscribe({
      next: (data) => {
        this.company = data;
        this.isLoading = false;
        console.log('Dados da empresa carregados:', this.company);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Erro ao carregar os dados da empresa.';
        console.error('Erro ao buscar a empresa:', err);
      }
    });
  }
}
