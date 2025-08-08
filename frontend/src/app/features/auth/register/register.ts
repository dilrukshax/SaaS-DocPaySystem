import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatInputModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  registerForm: FormGroup;
  isLoading = false;
  hidePassword = true;
  hideConfirmPassword = true;

  departments = [
    'Administration',
    'Finance',
    'Operations',
    'Legal',
    'IT',
    'Sales',
    'Marketing',
    'Human Resources',
    'Other'
  ];

  timeZones = [
    'UTC-12:00', 'UTC-11:00', 'UTC-10:00', 'UTC-09:00', 'UTC-08:00',
    'UTC-07:00', 'UTC-06:00', 'UTC-05:00', 'UTC-04:00', 'UTC-03:00',
    'UTC-02:00', 'UTC-01:00', 'UTC+00:00', 'UTC+01:00', 'UTC+02:00',
    'UTC+03:00', 'UTC+04:00', 'UTC+05:00', 'UTC+06:00', 'UTC+07:00',
    'UTC+08:00', 'UTC+09:00', 'UTC+10:00', 'UTC+11:00', 'UTC+12:00'
  ];

  languages = [
    { code: 'en', name: 'English' },
    { code: 'es', name: 'Spanish' },
    { code: 'fr', name: 'French' },
    { code: 'de', name: 'German' },
    { code: 'it', name: 'Italian' },
    { code: 'pt', name: 'Portuguese' },
    { code: 'zh', name: 'Chinese' },
    { code: 'ja', name: 'Japanese' }
  ];

  constructor() {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      tenantId: ['', [Validators.required]],
      department: [''],
      jobTitle: [''],
      phoneNumber: ['', [Validators.pattern(/^\+?[\d\s\-\(\)]+$/)]],
      timeZone: ['UTC+00:00'],
      language: ['en']
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    if (confirmPassword?.hasError('passwordMismatch')) {
      delete confirmPassword.errors!['passwordMismatch'];
      if (Object.keys(confirmPassword.errors!).length === 0) {
        confirmPassword.setErrors(null);
      }
    }
    
    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid && !this.isLoading) {
      this.isLoading = true;
      
      const formValue = this.registerForm.value;
      const registerRequest: RegisterRequest = {
        email: formValue.email,
        password: formValue.password,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        tenantId: formValue.tenantId,
        department: formValue.department || undefined,
        jobTitle: formValue.jobTitle || undefined,
        phoneNumber: formValue.phoneNumber || undefined,
        timeZone: formValue.timeZone,
        language: formValue.language
      };

      this.authService.register(registerRequest).subscribe({
        next: (result) => {
          this.isLoading = false;
          console.log('Registration successful:', result);
          
          this.snackBar.open(
            `Welcome ${result.user.firstName}! Registration successful.`,
            'Close',
            { duration: 3000, panelClass: ['success-snackbar'] }
          );
          
          // Give a small delay to ensure auth state is updated
          setTimeout(() => {
            console.log('Navigating to dashboard, auth state:', this.authService.isAuthenticated);
            console.log('Current user:', this.authService.currentUser);
            console.log('Attempting navigation to dashboard...');
            this.router.navigate(['/dashboard']).then(success => {
              console.log('Navigation result:', success);
              console.log('Current URL after navigation:', this.router.url);
            }).catch(err => {
              console.error('Navigation error:', err);
            });
          }, 500);
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Registration failed:', error);
          this.snackBar.open(
            error.message || 'Registration failed. Please try again.',
            'Close',
            { duration: 5000, panelClass: ['error-snackbar'] }
          );
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.registerForm.controls).forEach(key => {
      const control = this.registerForm.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.registerForm.get(fieldName);
    
    if (!control || !control.errors || !control.touched) {
      return '';
    }

    if (control.errors['required']) {
      return `${this.getFieldDisplayName(fieldName)} is required`;
    }
    
    if (control.errors['email']) {
      return 'Please enter a valid email address';
    }
    
    if (control.errors['minlength']) {
      const requiredLength = control.errors['minlength'].requiredLength;
      return `${this.getFieldDisplayName(fieldName)} must be at least ${requiredLength} characters`;
    }
    
    if (control.errors['pattern']) {
      return 'Please enter a valid phone number';
    }
    
    if (control.errors['passwordMismatch']) {
      return 'Passwords do not match';
    }

    return 'Invalid input';
  }

  private getFieldDisplayName(fieldName: string): string {
    const displayNames: { [key: string]: string } = {
      email: 'Email',
      password: 'Password',
      confirmPassword: 'Confirm Password',
      firstName: 'First Name',
      lastName: 'Last Name',
      tenantId: 'Organization ID',
      department: 'Department',
      jobTitle: 'Job Title',
      phoneNumber: 'Phone Number',
      timeZone: 'Time Zone',
      language: 'Language'
    };
    return displayNames[fieldName] || fieldName;
  }
}
