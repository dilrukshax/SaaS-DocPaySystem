import { Component, OnInit } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { filter } from 'rxjs/operators';
import { MainLayout } from './layout/main-layout/main-layout';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, MainLayout],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  title = 'SaaS DocPay System';
  showLayout = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Subscribe to authentication state changes
    this.authService.currentUser$.subscribe(user => {
      console.log('App: User state changed:', user);
      this.updateLayoutVisibility();
    });

    // Check if we should show the main layout based on route
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        console.log('App: Navigation ended:', event.url);
        this.updateLayoutVisibility();
      });

    // Initial check
    this.updateLayoutVisibility();
  }

  private updateLayoutVisibility(): void {
    const currentUrl = this.router.url;
    const isAuthRoute = currentUrl.startsWith('/auth');
    const isAuthenticated = this.authService.isAuthenticated;
    
    this.showLayout = !isAuthRoute && isAuthenticated;
    
    // Debug logging
    console.log('Layout visibility check:', {
      currentUrl,
      isAuthRoute,
      isAuthenticated,
      showLayout: this.showLayout
    });
  }
}
