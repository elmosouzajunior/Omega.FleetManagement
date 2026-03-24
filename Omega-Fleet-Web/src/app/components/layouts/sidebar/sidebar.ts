import { Component, OnInit, Output, EventEmitter, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss'
})
export class SidebarComponent implements OnInit {
  private router = inject(Router);
  private platformId = inject(PLATFORM_ID);

  @Output() close = new EventEmitter<void>();

  companyName = 'EMPRESA';
  userName = 'USUARIO';
  userRole = '';

  ngOnInit() {
    this.loadRole();
    this.loadSidebarIdentity();
  }

  private loadRole() {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token');
      if (!token) return;

      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.userRole = payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
      } catch {
        this.userRole = '';
      }
    }
  }

  private loadSidebarIdentity() {
    if (isPlatformBrowser(this.platformId)) {
      try {
        const userData = JSON.parse(localStorage.getItem('user') || '{}');
        const storageUserName = localStorage.getItem('userName') || '';
        const storageCompanyName = localStorage.getItem('companyName') || '';

        if (this.isMaster()) {
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
      } catch (error) {
        console.error('Erro ao recuperar dados do sidebar:', error);
      }
    }
  }

  isMaster(): boolean {
    return this.userRole === 'Master';
  }

  isAdmin(): boolean {
    return this.userRole === 'CompanyAdmin';
  }

  onClose() {
    this.close.emit();
  }

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.clear();
    }
    this.router.navigate(['/login']);
  }
}
