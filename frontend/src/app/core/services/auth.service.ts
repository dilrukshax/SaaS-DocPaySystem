import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

import { environment } from '@environments/environment';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyName?: string;
  isEmailConfirmed: boolean;
  subscriptionType: string;
  createdAt: Date;
}

export interface AuthRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  companyName: string;
  subscriptionType: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface JwtPayload {
  sub: string;
  email: string;
  role: string;
  exp: number;
  iat: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);

  public currentUser$ = this.currentUserSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  private apiUrl = `${environment.apiUrl}/auth`;

  constructor() {
    this.initializeAuth();
  }

  initializeAuth(): void {
    const token = this.getToken();
    if (token && !this.isTokenExpired(token)) {
      const user = this.getUserFromToken(token);
      if (user) {
        this.currentUserSubject.next(user);
        this.isAuthenticatedSubject.next(true);
      }
    } else {
      this.clearAuthData();
    }
  }

  login(credentials: AuthRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(error => {
          console.error('Login error:', error);
          throw error;
        })
      );
  }

  register(userData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, userData)
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(error => {
          console.error('Registration error:', error);
          throw error;
        })
      );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    
    // Call logout endpoint to invalidate tokens on server
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken })
        .subscribe({
          error: (error) => console.error('Logout error:', error)
        });
    }

    this.clearAuthData();
    this.router.navigate(['/auth/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      this.logout();
      return of();
    }

    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          this.setAuthData(response);
        }),
        catchError(error => {
          console.error('Token refresh error:', error);
          this.logout();
          throw error;
        })
      );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, { 
      token, 
      newPassword 
    });
  }

  confirmEmail(token: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/confirm-email`, { token });
  }

  resendConfirmationEmail(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/resend-confirmation`, { email });
  }

  changePassword(currentPassword: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, {
      currentPassword,
      newPassword
    });
  }

  updateProfile(userData: Partial<User>): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/profile`, userData)
      .pipe(
        tap(user => {
          this.currentUserSubject.next(user);
        })
      );
  }

  // Token management methods
  getToken(): string | null {
    return localStorage.getItem(environment.auth.tokenKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(environment.auth.refreshTokenKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return token ? !this.isTokenExpired(token) : false;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user ? user.role === role : false;
  }

  hasAnyRole(roles: string[]): boolean {
    const user = this.getCurrentUser();
    return user ? roles.includes(user.role) : false;
  }

  // Utility methods
  private setAuthData(response: AuthResponse): void {
    localStorage.setItem(environment.auth.tokenKey, response.token);
    localStorage.setItem(environment.auth.refreshTokenKey, response.refreshToken);
    
    this.currentUserSubject.next(response.user);
    this.isAuthenticatedSubject.next(true);
  }

  private clearAuthData(): void {
    localStorage.removeItem(environment.auth.tokenKey);
    localStorage.removeItem(environment.auth.refreshTokenKey);
    
    this.currentUserSubject.next(null);
    this.isAuthenticatedSubject.next(false);
  }

  private isTokenExpired(token: string): boolean {
    try {
      const decoded: JwtPayload = jwtDecode(token);
      const currentTime = Math.floor(Date.now() / 1000);
      const bufferTime = environment.auth.tokenExpirationBuffer / 1000;
      
      return decoded.exp < (currentTime + bufferTime);
    } catch (error) {
      console.error('Error decoding token:', error);
      return true;
    }
  }

  private getUserFromToken(token: string): User | null {
    try {
      const decoded: JwtPayload = jwtDecode(token);
      const currentUser = this.currentUserSubject.value;
      
      // If we have current user data, use it; otherwise create minimal user from token
      if (currentUser) {
        return currentUser;
      }
      
      return {
        id: decoded.sub,
        email: decoded.email,
        firstName: '',
        lastName: '',
        role: decoded.role,
        isEmailConfirmed: false,
        subscriptionType: 'Basic',
        createdAt: new Date()
      };
    } catch (error) {
      console.error('Error extracting user from token:', error);
      return null;
    }
  }

  // Token expiration check with auto-refresh
  checkTokenExpiration(): void {
    const token = this.getToken();
    
    if (token && this.isTokenExpired(token)) {
      this.refreshToken().subscribe({
        error: () => {
          // Refresh failed, redirect to login
          this.logout();
        }
      });
    }
  }
}
