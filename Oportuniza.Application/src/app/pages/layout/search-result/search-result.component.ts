import { Component, OnInit } from '@angular/core';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { MatDialog } from '@angular/material/dialog';
import { FilterComponent } from '../../../extras/filter/filter.component';
import { PublicationFilterDto } from '../../../models/filter.model';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search-result',
  imports: [FormsModule, CommonModule],
  templateUrl: './search-result.component.html',
  styleUrl: './search-result.component.css'
})
export class SearchResultComponent implements OnInit {
  publications: Publication[] = [];
  currentFilters: PublicationFilterDto = {};
  hasResults = true;

  constructor(
    private route: ActivatedRoute,
    private publicationService: PublicationService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.currentFilters = {
        searchTerm: params['searchTerm'] || '',
        local: params['local'] || '',
        contracts: params['contracts'] ? (params['contracts'] as string).split(',') : [],
        shifts: params['shifts'] ? (params['shifts'] as string).split(',') : [],
        salaryRange: params['salaryRange'] || ''
      };

      this.filterPublications();
    });
  }

  filterPublications(): void {
    this.publicationService.filterPublications(this.currentFilters).subscribe({
      next: (res: Publication[]) => {
        this.publications = res;
        this.hasResults = this.publications.length > 0;
      },
      error: (err) => {
        console.error('Erro ao buscar resultados:', err);
        this.publications = [];
        this.hasResults = false;
      }
    });
  }

    onPostClick(publication: Publication): void {
    console.log('ID da publicação clicada:', publication.id);
  }
}
