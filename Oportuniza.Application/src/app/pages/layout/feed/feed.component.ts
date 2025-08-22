import { Component, Input, OnInit } from '@angular/core';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../services/user.service';
import { CandidateService } from '../../../services/candidate.service';

@Component({
  selector: 'app-feed',
  imports: [CommonModule],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css',
})
export class FeedComponent implements OnInit {
  publications: Publication[] = [];
  currentIndex: number = 0;
  @Input() publicationId!: string;
  @Input() userId!: string;

  hasApplied = false;

  constructor(
    private publicationService: PublicationService,
    private userService: UserService,
    private candidateService: CandidateService
  ) {}

  ngOnInit() {
    this.getPublications();
  }

  goBack() {
    if (this.currentIndex > 0) {
      this.currentIndex--;
    }
  }

  goForward() {
    if (this.currentIndex < this.publications.length - 1) {
      this.currentIndex++;
    }
  }

  getPublications() {
    this.publicationService.getPublications().subscribe({
      next: (publication: Publication[]) => {
        this.publications = publication;
      },
      error: (error: any) => {
        console.log(error);
      },
    });
  }

  handleImgError(event: Event) {
    const target = event.target as HTMLImageElement;
    target.src = '../../../../assets/logo.png';
  }

  apply() {
    this.candidateService.applyToJob(this.publicationId).subscribe({
      next: () => {
        this.hasApplied = true;
        alert('Você se candidatou com sucesso!');
      },
      error: (err) => {
        console.error(err);
        alert('Erro ao se candidatar. Tente novamente.');
      },
    });
  }

  cancel(applicationId: string) {
    this.candidateService.cancelApplication(applicationId).subscribe({
      next: () => {
        this.hasApplied = false;
        alert('Candidatura cancelada.');
      },
      error: (err) => console.error(err),
    });
  }
}
