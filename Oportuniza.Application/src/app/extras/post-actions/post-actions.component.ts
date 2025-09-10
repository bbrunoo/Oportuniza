import { CommonModule } from '@angular/common';
import { Component, Inject, OnDestroy } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Publication } from '../../models/Publications.model';
import { PublicationService } from '../../services/publication.service';
import Swal from 'sweetalert2';
import { Subject, takeUntil } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-post-actions',
  imports: [CommonModule],
  templateUrl: './post-actions.component.html',
  styleUrl: './post-actions.component.css'
})
export class PostActionsComponent implements OnDestroy {
  showCompleteProfileButton = true;
  containerHeight = '200px';

  post: Publication;

  constructor(
    public dialogRef: MatDialogRef<PostActionsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { post: Publication },
    private publicationService: PublicationService,
    private router: Router
  ) {
    this.post = data.post;
  }
  ngOnInit(): void {
    console.log('Postagem recebida:', this.post);
  }

  editarPostagem() {
    console.log('Editando postagem:', this.post.id);
    this.dialogRef.close();
    this.router.navigate(['/inicio/editar-post', this.post.id]);
  }

  private destroy$ = new Subject<void>();

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  desativarPostagem() {
    Swal.fire({
      title: 'Tem certeza?',
      text: 'Você não poderá reverter isso!',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',
      cancelButtonColor: '#3085d6',
      confirmButtonText: 'Sim, desativar!',
      cancelButtonText: 'Cancelar',
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.publicationService.disablePublication(this.post.id)
          .pipe(takeUntil(this.destroy$)) // Add this line
          .subscribe({
            next: () => {
              Swal.fire(
                'Desativada!',
                'Sua postagem foi desativada com sucesso.',
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
