import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';

interface Publication {
  id: string;
  title: string;
  local: string;
  salary: string;
  imageUrl: string;
}

@Component({
  selector: 'app-vagas',
  imports: [RouterModule, CommonModule],
  templateUrl: './vagas.component.html',
  styleUrl: './vagas.component.css'
})

export class VagasComponent implements OnInit {
  i: number = 0;
  vagas: Publication[] = [];
  isLoading = true;
  errorMessage: string | null = null;

  private apiUrl = 'http://localhost:5000/api/Publication/random-samples';

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.getRandomVagas();
  }

  getRandomVagas(): void {
    this.isLoading = true;
    this.http.get<Publication[]>(this.apiUrl).subscribe({
      next: (res) => {
        this.vagas = res;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erro ao buscar vagas aleatórias:', err);
        this.errorMessage = 'Não foi possível carregar as vagas.';
        this.isLoading = false;
      }
    });
  }

  setSlide(index: number) {
    this.i = index;
  }
}
