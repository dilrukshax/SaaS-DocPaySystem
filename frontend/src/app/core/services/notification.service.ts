import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private snackBar = inject(MatSnackBar);
  private toastr = inject(ToastrService);

  showSuccess(message: string, title?: string): void {
    this.toastr.success(message, title, {
      timeOut: 3000,
      progressBar: true,
      closeButton: true
    });
  }

  showError(message: string, title?: string): void {
    this.toastr.error(message, title || 'Error', {
      timeOut: 5000,
      progressBar: true,
      closeButton: true
    });
  }

  showWarning(message: string, title?: string): void {
    this.toastr.warning(message, title || 'Warning', {
      timeOut: 4000,
      progressBar: true,
      closeButton: true
    });
  }

  showInfo(message: string, title?: string): void {
    this.toastr.info(message, title || 'Info', {
      timeOut: 3000,
      progressBar: true,
      closeButton: true
    });
  }

  showSnackBar(message: string, action: string = 'Close', duration: number = 3000): void {
    this.snackBar.open(message, action, {
      duration,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }

  showApiError(error: any): void {
    let message = 'An unexpected error occurred';
    
    if (error?.error?.message) {
      message = error.error.message;
    } else if (error?.message) {
      message = error.message;
    } else if (typeof error === 'string') {
      message = error;
    }

    this.showError(message);
  }

  clearAll(): void {
    this.toastr.clear();
  }
}
