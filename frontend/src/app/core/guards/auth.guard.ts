import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated) {
    return true;
  } else {
    router.navigate(['/auth/login']);
    return false;
  }
};

export const guestGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated) {
    return true;
  } else {
    router.navigate(['/dashboard']);
    return false;
  }
};

export const roleGuard = (allowedRoles: string[]) => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated) {
      router.navigate(['/auth/login']);
      return false;
    }

    if (authService.hasAnyRole(allowedRoles)) {
      return true;
    } else {
      router.navigate(['/unauthorized']);
      return false;
    }
  };
};
