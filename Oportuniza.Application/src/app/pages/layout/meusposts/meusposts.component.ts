import { Component } from '@angular/core';
import { CommonModule, formatDate, NgFor } from '@angular/common';

@Component({
  selector: 'app-meusposts',
  imports: [CommonModule, NgFor],
  templateUrl: './meusposts.component.html',
  styleUrl: './meusposts.component.css'
})
export class MeuspostsComponent {
  posts = [
    {
      imageUrl: 'https://oportuniza.blob.core.windows.net/publications/publication-1a8a841a-048d-455c-a8c4-30b886c4779b-89cb2750-8ee8-4fc0-9cb3-0434b959c63e',
      name: 'João da Silva',
      data: '30/07/2025',
      pendente: true
    },
    {
      imageUrl: 'https://oportuniza.blob.core.windows.net/publications/publication-1a8a841a-048d-455c-a8c4-30b886c4779b-89cb2750-8ee8-4fc0-9cb3-0434b959c63e',
      name: 'Maria',
      data: '05/08/2025',
      pendente: true
    },

    {
      imageUrl: 'https://oportuniza.blob.core.windows.net/profile-images/92cd10aa-ad9e-47c7-aac0-685c4ae34b91',
      name: 'Maria',
      data: '05/08/2025',
      pendente: true
    },

    {
      imageUrl: 'https://oportuniza.blob.core.windows.net/publications/publication-1a8a841a-048d-455c-a8c4-30b886c4779b-89cb2750-8ee8-4fc0-9cb3-0434b959c63e',
      name: 'Maria',
      data: '05/08/2025',
      pendente: false
    }
  ];
}
