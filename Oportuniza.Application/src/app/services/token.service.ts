import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { AuthenticationResult } from '@azure/msal-browser';

@Injectable({
  providedIn: 'root'
})

export class TokenService {
  constructor(private msalService: MsalService) { }

  async getToken(): Promise<string> {
    const account = this.msalService.instance.getActiveAccount()
      || this.msalService.instance.getAllAccounts()[0];

    if (!account) {
      await this.msalService.loginRedirect();
      throw new Error('Usuário não autenticado');
    }

    try {
      const response: AuthenticationResult = await this.msalService.instance.acquireTokenSilent({
        account,
        scopes: ["api://a863e08f-99f4-4e08-ae28-afbc4d562269/oportuniza.read"]
      });
      return response.accessToken;
    } catch (error) {
      console.error('Erro ao obter token silencioso:', error);
      this.msalService.loginRedirect();
      throw error;
    }
  }
}
