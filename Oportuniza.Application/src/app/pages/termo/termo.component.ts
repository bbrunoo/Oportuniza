import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-termo',
  templateUrl: './termo.component.html',
  styleUrls: ['./termo.component.css']
})
export class TermoComponent {

  constructor(private router: Router) { }

  acceptTermsAndContinue() {
   
    localStorage.setItem('acceptTerms', 'true');
    this.router.navigate(['/cadastro']); 
  }
}
 