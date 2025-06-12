import { Component, OnInit } from '@angular/core';
import { Publication } from '../../../models/Publications.model';
import { PublicationService } from '../../../services/publication.service';
import { CommonModule } from '@angular/common';
import { UserService } from '../../../services/user.service';

@Component({
  selector: 'app-feed',
  imports: [CommonModule],
  templateUrl: './feed.component.html',
  styleUrl: './feed.component.css'
})
export class FeedComponent implements OnInit {
  publications: Publication[] = [];
  currentIndex: number = 0;

  constructor(private publicationService: PublicationService, private userService: UserService) { }

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
        console.log("res", publication);
      },
      error: (error: any) => {
        console.log(error);
      }
    })
  }

  handleImgError(event: Event) {
    const target = event.target as HTMLImageElement;
    target.src = '../../../../assets/logo.png';
  }
}
