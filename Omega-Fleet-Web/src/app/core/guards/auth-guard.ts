import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router, ActivatedRouteSnapshot } from '@angular/router';

export const authGuard = (route: ActivatedRouteSnapshot) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID);

  if (isPlatformBrowser(platformId)) {
    const token = localStorage.getItem('token');

    if (token) {
      // 1. Decodifica o payload do token (segunda parte do JWT)
      const payload = JSON.parse(atob(token.split('.')[1]));
      
      // 2. Pega a role (ajuste o nome da chave conforme seu backend envia: 'role', 'roles' ou a URL do schema)
      const userRole = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      // 3. Verifica se a rota exige uma role específica (definida no routing)
      const expectedRole = route.data['role'];

      if (expectedRole && userRole !== expectedRole) {
        // Se o cara é Admin e tenta entrar no Master, manda pro dashboard dele
        router.navigate([userRole === 'Master' ? '/Master/companies' : '/Admin/dashboard']);
        return false;
      }

      return true;
    }
  }

  router.navigate(['/login']);
  return false;
};
