import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const notificationService = inject(NotificationService);
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError(error => {
      let errorMessage = 'An unexpected error occurred';

      if (error.status === 401) {
        // Unauthorized - redirect to login
        authService.logout();
        router.navigate(['/auth/login']);
        errorMessage = 'Your session has expired. Please login again.';
      } else if (error.status === 403) {
        // Forbidden
        errorMessage = 'You do not have permission to perform this action.';
      } else if (error.status === 404) {
        // Not found
        errorMessage = 'The requested resource was not found.';
      } else if (error.status === 400) {
        // Bad request
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.error?.errors) {
          // Validation errors
          const validationErrors = Object.values(error.error.errors).flat();
          errorMessage = validationErrors.join(', ');
        } else {
          errorMessage = 'Invalid request. Please check your input.';
        }
      } else if (error.status === 500) {
        // Server error
        errorMessage = 'Server error. Please try again later.';
      } else if (error.status === 0) {
        // Network error
        errorMessage = 'Network error. Please check your connection.';
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      }

      // Don't show notification for auth endpoints that are expected to fail
      const isAuthEndpoint = req.url.includes('/auth/');
      const isLoginFailure = isAuthEndpoint && error.status === 400;
      
      if (!isLoginFailure) {
        notificationService.showError(errorMessage);
      }

      return throwError(() => error);
    })
  );
};
