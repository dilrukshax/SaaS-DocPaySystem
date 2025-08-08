import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { NgxSpinnerModule } from 'ngx-spinner';
import { filter } from 'rxjs/operators';

import { AuthService } from './core/services/auth.service';
import { LoadingService } from './core/services/loading.service';
import { NotificationService } from './core/services/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatMenuModule,
    NgxSpinnerModule
  ],
  template: `
    <div class="app-container">
      <mat-toolbar color="primary" class="app-toolbar">
        <button mat-icon-button (click)="toggleSidenav()" *ngIf="isAuthenticated">
          <mat-icon>menu</mat-icon>
        </button>
        <span class="app-title">{{ title }}</span>
        <span class="spacer"></span>
        
        <div *ngIf="isAuthenticated" class="user-menu">
          <button mat-button [matMenuTriggerFor]="userMenu">
            <mat-icon>account_circle</mat-icon>
            {{ currentUser?.firstName || 'User' }}
            <mat-icon>arrow_drop_down</mat-icon>
          </button>
          <mat-menu #userMenu="matMenu">
            <button mat-menu-item routerLink="/profile">
              <mat-icon>person</mat-icon>
              <span>Profile</span>
            </button>
            <button mat-menu-item routerLink="/settings">
              <mat-icon>settings</mat-icon>
              <span>Settings</span>
            </button>
            <mat-divider></mat-divider>
            <button mat-menu-item (click)="logout()">
              <mat-icon>logout</mat-icon>
              <span>Logout</span>
            </button>
          </mat-menu>
        </div>
        
        <div *ngIf="!isAuthenticated">
          <button mat-button routerLink="/auth/login">Login</button>
          <button mat-raised-button color="accent" routerLink="/auth/register">Sign Up</button>
        </div>
      </mat-toolbar>

      <mat-sidenav-container class="sidenav-container" *ngIf="isAuthenticated">
        <mat-sidenav #sidenav mode="side" opened class="app-sidenav">
          <mat-nav-list>
            <a mat-list-item routerLink="/dashboard" routerLinkActive="active">
              <mat-icon matListItemIcon>dashboard</mat-icon>
              <span matListItemTitle>Dashboard</span>
            </a>
            <a mat-list-item routerLink="/documents" routerLinkActive="active">
              <mat-icon matListItemIcon>description</mat-icon>
              <span matListItemTitle>Documents</span>
            </a>
            <a mat-list-item routerLink="/payments" routerLinkActive="active">
              <mat-icon matListItemIcon>payment</mat-icon>
              <span matListItemTitle>Payments</span>
            </a>
            <a mat-list-item routerLink="/workflows" routerLinkActive="active">
              <mat-icon matListItemIcon>timeline</mat-icon>
              <span matListItemTitle>Workflows</span>
            </a>
            <a mat-list-item routerLink="/notifications" routerLinkActive="active">
              <mat-icon matListItemIcon>notifications</mat-icon>
              <span matListItemTitle>Notifications</span>
            </a>
            <a mat-list-item routerLink="/reports" routerLinkActive="active">
              <mat-icon matListItemIcon>analytics</mat-icon>
              <span matListItemTitle>Reports</span>
            </a>
            
            <mat-divider *ngIf="isAdmin"></mat-divider>
            <div *ngIf="isAdmin">
              <h3 matSubheader>Administration</h3>
              <a mat-list-item routerLink="/admin/users" routerLinkActive="active">
                <mat-icon matListItemIcon>group</mat-icon>
                <span matListItemTitle>User Management</span>
              </a>
              <a mat-list-item routerLink="/admin/system" routerLinkActive="active">
                <mat-icon matListItemIcon>settings_applications</mat-icon>
                <span matListItemTitle>System Settings</span>
              </a>
            </div>
          </mat-nav-list>
        </mat-sidenav>

        <mat-sidenav-content class="main-content">
          <router-outlet></router-outlet>
        </mat-sidenav-content>
      </mat-sidenav-container>

      <div *ngIf="!isAuthenticated" class="auth-container">
        <router-outlet></router-outlet>
      </div>
    </div>

    <ngx-spinner></ngx-spinner>
  `,
  styles: [`
    .app-container {
      height: 100vh;
      display: flex;
      flex-direction: column;
    }

    .app-toolbar {
      z-index: 1000;
    }

    .app-title {
      font-weight: 600;
    }

    .spacer {
      flex: 1 1 auto;
    }

    .user-menu {
      display: flex;
      align-items: center;
    }

    .sidenav-container {
      flex: 1;
    }

    .app-sidenav {
      width: 250px;
      border-right: 1px solid #e0e0e0;
    }

    .main-content {
      padding: 20px;
      background-color: #f5f5f5;
      min-height: calc(100vh - 64px);
    }

    .auth-container {
      flex: 1;
      display: flex;
      justify-content: center;
      align-items: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .active {
      background-color: rgba(0, 0, 0, 0.1) !important;
    }

    @media (max-width: 768px) {
      .app-sidenav {
        width: 200px;
      }
      
      .main-content {
        padding: 10px;
      }
    }
  `]
})
export class AppComponent implements OnInit {
  title = 'SaaS DocPay System';
  
  private authService = inject(AuthService);
  private loadingService = inject(LoadingService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  get currentUser() {
    return this.authService.getCurrentUser();
  }

  get isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  ngOnInit() {
    // Subscribe to router events for page tracking
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      // Analytics tracking can be added here
      console.log('Navigation to:', event.url);
    });

    // Initialize authentication state
    this.authService.initializeAuth();
    
    // Setup global error handling
    this.setupGlobalErrorHandling();
  }

  toggleSidenav() {
    // Will be implemented with ViewChild for sidenav reference
  }

  logout() {
    this.authService.logout();
    this.notificationService.showSuccess('Logged out successfully');
    this.router.navigate(['/auth/login']);
  }

  private setupGlobalErrorHandling() {
    window.addEventListener('unhandledrejection', (event) => {
      console.error('Unhandled promise rejection:', event.reason);
      this.notificationService.showError('An unexpected error occurred');
    });
  }
}
