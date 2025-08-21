import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-empresa',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './empresa.component.html',
  styleUrl: './empresa.component.css'
})
export class EmpresaComponent {
  company = {
    name: "",
    imageUrl: "",
  };
}
