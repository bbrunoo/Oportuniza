import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})

export class HeaderComponent {
  isMenuOpen = false;
  lastScrollTop = 0;
  isHeaderHidden = false;
  scrollThreshold = 100;

  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    const scrollTop = window.scrollY || document.documentElement.scrollTop;

    if (scrollTop > this.lastScrollTop && scrollTop > this.scrollThreshold) {
      this.isHeaderHidden = true;
    } else if (scrollTop < this.lastScrollTop) {
      this.isHeaderHidden = false;
    }

    this.lastScrollTop = scrollTop;
  }


  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
    console.log("oi");
  }

  closeMenu() {
    this.isMenuOpen = false;
  }
}
