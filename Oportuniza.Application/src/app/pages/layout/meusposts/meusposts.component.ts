import { Component, OnInit } from '@angular/core';
import { CommonModule, formatDate, NgFor } from '@angular/common';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { PostActionsComponent } from '../../../extras/post-actions/post-actions.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-meusposts',
  imports: [CommonModule, NgFor],
  templateUrl: './meusposts.component.html',
  styleUrl: './meusposts.component.css',
})
export class MeuspostsComponent implements OnInit {
  publications: Publication[] = [];
  pageNumber = 1;
  pageSize = 8;
  totalPages = 0;
  isLoading = false;

  constructor(
    private publicationService: PublicationService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.getPublications();
  }

  getPublications() {
    this.isLoading = true;
    this.publicationService
      .getMyPublications(this.pageNumber, this.pageSize)
      .subscribe({
        next: (res: any) => {
          this.publications = res.items;
          this.totalPages = res.totalPages;
          this.isLoading = false;
        },
        error: (error: any) => {
          console.log('Erro ao carregar publicações:', error);
          this.publications = [];
          this.totalPages = 0;
          this.isLoading = false;
        },
      });
  }

  nextPage() {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.getPublications();
    }
  }

  prevPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.getPublications();
    }
  }

  openDialog(event: MouseEvent, post: Publication) {
    const rect = (event.target as HTMLElement).getBoundingClientRect();

    const dialogRef = this.dialog.open(PostActionsComponent, {
      minWidth: '230px',
      minHeight: '130px',
      position: {
        top: `${rect.top}px`,
        left: `${rect.left}px`,
      },
      data: { post: post },
      panelClass: 'custom-dialog',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getPublications();
      }
    });
  }
}
