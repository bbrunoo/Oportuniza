import { CompanyService } from './../../services/company.service';
import { Component, Inject, OnDestroy } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import Swal from 'sweetalert2';
import { CompanyListDto } from '../../models/company-list-dto-model';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Router } from '@angular/router';

@Component({
  selector: 'app-company-actions',
  imports: [],
  templateUrl: './company-actions.component.html',
  styleUrl: './company-actions.component.css'
})
export class CompanyActionsComponent implements OnDestroy {
  private destroy$ = new Subject<void>();
  company: CompanyListDto;
  containerHeight = '100px';

  constructor(
    public dialogRef: MatDialogRef<CompanyActionsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { company: CompanyListDto },
    private companyService: CompanyService,
    private router: Router
  ) {
    this.company = data.company;
  }

  ngOnInit(): void {
    console.log('Empresa recebida:', this.company);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  desativarPostagem() {
    Swal.fire({
      text: 'Tem certeza que deseja excluir a empresa?',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      width: '350px',
      color:'#252525',
      confirmButtonText: 'Sim',
      cancelButtonText: 'Cancelar',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.companyService.disableCompany(this.company.id)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              Swal.fire(
                'Desativada!',
                'Sua empresa foi desativada com sucesso.',
                'success'
              );
              this.dialogRef.close(true);
            },
            error: (err) => {
              Swal.fire(
                'Erro!',
                'Houve um erro ao desativar a postagem.',
                'error'
              );
              console.error('Erro ao desativar a postagem:', err);
              this.dialogRef.close(false);
            }
          });
      }
    });
  }
}
