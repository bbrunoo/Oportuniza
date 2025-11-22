import { Component, NgZone } from '@angular/core';
import { CommonModule, NgIf } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-comofunciona',
  imports: [CommonModule, RouterModule],
  standalone: true,
  templateUrl: './comofunciona.component.html',
  styleUrl: './comofunciona.component.css'
})

export class ComofuncionaComponent {
  i = 0;
  private intervalId?: any;

  constructor(private zone: NgZone) { }

  setSlide(index: number) {
    this.i = index;
  }

  ngOnInit(): void {
    this.zone.runOutsideAngular(() => {
      this.intervalId = setInterval(() => {
        this.zone.run(() => {
          this.i = (this.i + 1) % 2;
        });
      }, 10000);
    });
  }

  ngOnDestroy(): void {
    if (this.intervalId) clearInterval(this.intervalId);
  }
}
