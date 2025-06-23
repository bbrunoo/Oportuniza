import { Component, HostListener, Inject} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MSAL_GUARD_CONFIG, MsalGuardConfiguration, MsalService } from '@azure/msal-angular';
import { RedirectRequest } from '@azure/msal-browser';

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
  loginDisplay = false;

  constructor(private router: Router, private authService: MsalService, @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration) { }

  loginRedirect() {
    if (this.msalGuardConfig.authRequest) {
      this.authService.loginRedirect({
        ...this.msalGuardConfig.authRequest,
      } as RedirectRequest);
    } else {
      this.authService.loginRedirect();
    }
  }

  setLoginDisplay() {
    this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
  }

  @HostListener('window:scroll', [])
  onWindowScroll(): void {
    const scrollTop = window.scrollY || document.documentElement.scrollTop;

    if (scrollTop > this.lastScrollTop && scrollTop > this.scrollThreshold) {
      this.isHeaderHidden = true;
      this.isMenuOpen = false;
    } else if (scrollTop < this.lastScrollTop) {
      this.isHeaderHidden = false;
    }

    this.lastScrollTop = scrollTop;
  }

  @HostListener('window:mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    const triggerZoneHeight = 100;

    if (
      event.clientY <= triggerZoneHeight &&
      this.isHeaderHidden &&
      !this.isMenuOpen
    ) {
      this.isMenuOpen = true;
      this.isHeaderHidden = false;
    }
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu() {
    this.isMenuOpen = false;
  }
}
