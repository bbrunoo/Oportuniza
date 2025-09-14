import { Component, Input, OnInit } from '@angular/core';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { MatDialog } from '@angular/material/dialog';
import { FilterComponent } from '../../../extras/filter/filter.component';
import { PublicationFilterDto } from '../../../models/filter.model';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SearchbarComponent } from '../../../component/searchbar/searchbar.component';

@Component({
  selector: 'app-search-result',
  imports: [FormsModule, CommonModule, SearchbarComponent],
  templateUrl: './search-result.component.html',
  styleUrl: './search-result.component.css',
})
export class SearchResultComponent implements OnInit {
  publications: Publication[] = [];
  currentFilters: PublicationFilterDto = {};
  hasResults = true;
  @Input() publicationId!: string;

  constructor(
    private route: ActivatedRoute,
    private publicationService: PublicationService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.currentFilters = {
        searchTerm: params['searchTerm'] || '',
        local: params['local'] || '',
        contracts: params['contracts']
          ? (params['contracts'] as string).split(',')
          : [],
        shifts: params['shifts'] ? (params['shifts'] as string).split(',') : [],
        salaryRange: params['salaryRange'] || '',
      };

      this.filterPublications();
    });
  }

  goToPublication(publicationId: string): void {
    this.router.navigate(['/inicio/post', publicationId]);
  }

  filterPublications(): void {
    this.publicationService.filterPublications(this.currentFilters).subscribe({
      next: (res: Publication[]) => {
        this.publications = res;
        console.log('search', this.publications);
        this.hasResults = this.publications.length > 0;
      },
      error: (err) => {
        console.error('Erro ao buscar resultados:', err);
        this.publications = [];
        this.hasResults = false;
      },
    });
  }

  onSearchTriggered(filters: PublicationFilterDto) {
    const normalizedFilters = {
      searchTerm: filters.searchTerm?.toLowerCase() || '',
      local: filters.local?.toLowerCase() || '',
      contracts: (filters.contracts ?? []).map((c) => c.toLowerCase()),
      shifts: (filters.shifts ?? []).map((s) => s.toLowerCase()),
      salaryRange: filters.salaryRange?.toLowerCase() || null,
    };

    const queryParams: any = {};
    if (normalizedFilters.searchTerm) {
      queryParams.searchTerm = normalizedFilters.searchTerm;
    }
    if (normalizedFilters.local) {
      queryParams.local = normalizedFilters.local;
    }

    if (normalizedFilters.contracts.length > 0) {
      queryParams.contracts = normalizedFilters.contracts.join(',');
    }
    if (normalizedFilters.shifts.length > 0) {
      queryParams.shifts = normalizedFilters.shifts.join(',');
    }
    if (normalizedFilters.salaryRange) {
      queryParams.salaryRange = normalizedFilters.salaryRange;
    }

    this.router.navigate(['/inicio/search-result'], { queryParams });
  }

  private areFiltersEqual(
    filters1: PublicationFilterDto,
    filters2: PublicationFilterDto
  ): boolean {
    const normalizedFilters1 = {
      ...filters1,
      contracts: filters1.contracts ? [...filters1.contracts].sort() : [],
      shifts: filters1.shifts ? [...filters1.shifts].sort() : [],
    };

    const normalizedFilters2 = {
      ...filters2,
      contracts: filters2.contracts ? [...filters2.contracts].sort() : [],
      shifts: filters2.shifts ? [...filters2.shifts].sort() : [],
    };

    return (
      JSON.stringify(normalizedFilters1) === JSON.stringify(normalizedFilters2)
    );
  }

  onPostClick(publication: Publication): void {
    console.log('ID da publicação clicada:', publication.id);
  }
}
