import { Component, OnInit, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-admin-layout',
  standalone: false,
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayoutComponent implements OnInit {
  private platformId = inject(PLATFORM_ID);

  isMenuOpen = true;
  userName = 'ADMIN';
  companyName = 'EMPRESA';

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.isMenuOpen = window.innerWidth > 991;
      this.loadIdentity();
    }
  }

  private loadIdentity() {
    try {
      const token = localStorage.getItem('token');
      const userData = JSON.parse(localStorage.getItem('user') || '{}');
      const storageUserName = localStorage.getItem('userName') || '';
      const storageCompanyName = localStorage.getItem('companyName') || '';

      let role = '';
      if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        role = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
      }

      if (role === 'Master') {
        this.userName = 'MASTER';
        this.companyName = 'OMEGA SOLUTIONS';
        return;
      }

      const apiUserName = userData?.fullName || userData?.FullName || userData?.name || userData?.Name || '';
      const apiCompanyName =
        userData?.companyName ||
        userData?.CompanyName ||
        userData?.company?.name ||
        userData?.user?.company?.name ||
        '';

      this.userName = apiUserName || storageUserName || 'ADMIN';
      this.companyName = apiCompanyName || storageCompanyName || 'EMPRESA';
    } catch {
      this.userName = localStorage.getItem('userName') || 'ADMIN';
      this.companyName = localStorage.getItem('companyName') || 'EMPRESA';
    }
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }
}
