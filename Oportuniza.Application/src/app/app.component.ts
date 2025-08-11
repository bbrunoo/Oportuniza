import { isPlatformBrowser } from '@angular/common';
import { Component, Inject, OnDestroy, OnInit, Optional, PLATFORM_ID } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MSAL_GUARD_CONFIG, MsalBroadcastService, MsalGuardConfiguration, MsalService } from '@azure/msal-angular';
import { EventMessage, EventType, InteractionStatus } from '@azure/msal-browser';
import { filter, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`,
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit, OnDestroy{
  isIframe = false;
  loginDisplay = false;
  private readonly _destroying$ = new Subject<void>();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,

    @Optional() @Inject(MSAL_GUARD_CONFIG) private msalGuardConfig: MsalGuardConfiguration,
    @Optional() private authService: MsalService,
    @Optional() private msalBroadcastService: MsalBroadcastService,
    private router: Router,
  ) { }

  async ngOnInit(): Promise<void> {
    if (isPlatformBrowser(this.platformId)) {
      this.isIframe = window.self !== window.top;

      if (this.authService) {
        this.authService.handleRedirectObservable().subscribe({
          next: (result) => {
            if (result?.account) {
              this.router.navigateByUrl('/inicio');
            }
          }
        });
      }
    }
  }

  ngOnDestroy(): void {
    this._destroying$.next(undefined);
    this._destroying$.complete();
  }

  checkAndSetActiveAccount(): void {
    if (!this.authService) {
      return;
    }
    let activeAccount = this.authService.instance.getActiveAccount();

    if (
      !activeAccount &&
      this.authService.instance.getAllAccounts().length > 0
    ) {
      let accounts = this.authService.instance.getAllAccounts();
      this.authService.instance.setActiveAccount(accounts[0]);
    }
  }

  setLoginDisplay(): void {
    if (this.authService) {
      this.loginDisplay = this.authService.instance.getAllAccounts().length > 0;
    }
  }

  title = 'Oportuniza.Application';
}

