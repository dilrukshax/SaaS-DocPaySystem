import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { 
    path: '', 
    redirectTo: '/dashboard', 
    pathMatch: 'full' 
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadChildren: () => import('./features/auth/auth-module').then(m => m.AuthModule)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadChildren: () => import('./features/dashboard/dashboard-module').then(m => m.DashboardModule)
  },
  {
    path: 'documents',
    canActivate: [authGuard],
    loadChildren: () => import('./features/documents/documents-module').then(m => m.DocumentsModule)
  },
  {
    path: 'invoices',
    canActivate: [authGuard],
    loadChildren: () => import('./features/invoices/invoices-module').then(m => m.InvoicesModule)
  },
  {
    path: 'payments',
    canActivate: [authGuard],
    loadChildren: () => import('./features/payments/payments-module').then(m => m.PaymentsModule)
  },
  {
    path: 'workflows',
    canActivate: [authGuard],
    loadChildren: () => import('./features/workflows/workflows-module').then(m => m.WorkflowsModule)
  },
  {
    path: 'notifications',
    canActivate: [authGuard],
    loadChildren: () => import('./features/notifications/notifications-module').then(m => m.NotificationsModule)
  },
  {
    path: 'analytics',
    canActivate: [authGuard],
    loadChildren: () => import('./features/analytics/analytics-module').then(m => m.AnalyticsModule)
  },
  {
    path: 'ai',
    canActivate: [authGuard],
    loadChildren: () => import('./features/ai/ai-module').then(m => m.AiModule)
  },
  {
    path: 'admin',
    canActivate: [roleGuard(['Admin'])],
    loadChildren: () => import('./features/admin/admin-module').then(m => m.AdminModule)
  },
  {
    path: 'unauthorized',
    loadComponent: () => import('./shared/components/unauthorized/unauthorized').then(c => c.Unauthorized)
  },
  {
    path: '**',
    loadComponent: () => import('./shared/components/not-found/not-found').then(c => c.NotFound)
  }
];
