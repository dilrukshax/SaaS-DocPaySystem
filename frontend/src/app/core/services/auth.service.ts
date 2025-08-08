import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { JwtHelperService } from '@auth0/angular-jwt';
import { 
  User, 
  AuthenticationResult, 
  LoginRequest, 
  RegisterRequest, 
  UpdateProfileRequest, 
  ChangePasswordRequest 
} from '../models/user.model';
import { ApiResponse } from '../models/common.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/users/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private jwtHelper = new JwtHelperService();
  private isBrowser: boolean;

  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
    this.checkStoredAuth();
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;
    
    // Handle mock tokens for development
    if (token.startsWith('mock-access-token')) {
      console.log('Using mock token, checking expiry from localStorage');
      const expiresAt = localStorage.getItem('expires_at');
      if (!expiresAt) return false;
      const isValid = new Date(expiresAt) > new Date();
      console.log('Mock token authentication check:', { isValid, expiresAt });
      return isValid;
    }
    
    // Handle real JWT tokens
    try {
      const isValid = !this.jwtHelper.isTokenExpired(token);
      console.log('JWT token authentication check:', { isValid });
      return isValid;
    } catch (error) {
      console.warn('JWT token validation failed:', error);
      return false;
    }
  }

  get userRoles(): string[] {
    return this.currentUser?.roles || [];
  }

  hasRole(role: string): boolean {
    return this.userRoles.includes(role);
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some(role => this.hasRole(role));
  }

  login(request: LoginRequest): Observable<AuthenticationResult> {
    return this.http.post<ApiResponse<AuthenticationResult>>(`${this.apiUrl}/login`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Login failed');
          }
          return response.data;
        }),
        tap(result => this.setSession(result)),
        catchError(error => this.handleError(error))
      );
  }

  register(request: RegisterRequest): Observable<AuthenticationResult> {
    return this.http.post<ApiResponse<AuthenticationResult>>(`${this.apiUrl}/register`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Registration failed');
          }
          return response.data;
        }),
        tap(result => this.setSession(result)),
        catchError(error => this.handleError(error))
      );
  }

  refreshToken(): Observable<AuthenticationResult> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<ApiResponse<AuthenticationResult>>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Token refresh failed');
          }
          return response.data;
        }),
        tap(result => this.setSession(result)),
        catchError(error => {
          this.logout();
          return this.handleError(error);
        })
      );
  }

  logout(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    
    // Clear local session first
    this.clearSession();
    
    if (refreshToken) {
      return this.http.post(`${this.apiUrl}/logout`, { refreshToken })
        .pipe(
          catchError(error => {
            // Even if logout API fails, we've already cleared local session
            console.warn('Logout API failed:', error);
            return throwError(() => error);
          })
        );
    }

    return new Observable(observer => {
      observer.next(undefined);
      observer.complete();
    });
  }

  getProfile(): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/profile`)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to get profile');
          }
          return response.data;
        }),
        tap(user => this.currentUserSubject.next(user)),
        catchError(error => this.handleError(error))
      );
  }

  updateProfile(request: UpdateProfileRequest): Observable<User> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/profile`, request)
      .pipe(
        map(response => {
          if (!response.success || !response.data) {
            throw new Error(response.message || 'Failed to update profile');
          }
          return response.data;
        }),
        tap(user => this.currentUserSubject.next(user)),
        catchError(error => this.handleError(error))
      );
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/change-password`, request)
      .pipe(
        map(response => {
          if (!response.success) {
            throw new Error(response.message || 'Failed to change password');
          }
        }),
        catchError(error => this.handleError(error))
      );
  }

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('access_token');
  }

  getRefreshToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem('refresh_token');
  }

  private setSession(authResult: AuthenticationResult): void {
    if (!this.isBrowser) return;
    
    console.log('Setting session with auth result:', authResult);
    
    localStorage.setItem('access_token', authResult.accessToken);
    localStorage.setItem('refresh_token', authResult.refreshToken);
    localStorage.setItem('expires_at', authResult.expiresAt.toString());
    this.currentUserSubject.next(authResult.user);
    
    console.log('Session set, current user:', authResult.user);
    console.log('Is authenticated:', this.isAuthenticated);
  }

  private clearSession(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('expires_at');
    this.currentUserSubject.next(null);
  }

  private checkStoredAuth(): void {
    if (!this.isBrowser) return;
    
    const token = this.getAccessToken();
    if (token && this.isAuthenticated) {
      // For mock tokens, we need to set user data from a different source
      // or call getProfile to populate user data
      if (token.startsWith('mock-access-token')) {
        console.log('Found valid mock token, calling getProfile to set user data');
        this.getProfile().subscribe({
          next: (user) => {
            console.log('Profile loaded for mock token:', user);
          },
          error: (error) => {
            console.warn('Failed to load profile for mock token:', error);
            // For mock tokens, we might not have a profile endpoint that works
            // So we'll create a mock user
            const mockUser = {
              id: 'mock-user-id',
              email: 'demo@example.com',
              firstName: 'Demo',
              lastName: 'User',
              tenantId: 'demo-tenant',
              roles: ['User'],
              isActive: true,
              createdAt: new Date(),
              updatedAt: new Date()
            } as User;
            this.currentUserSubject.next(mockUser);
          }
        });
      } else {
        // Real JWT token - get current user profile
        this.getProfile().subscribe({
          error: () => this.clearSession()
        });
      }
    } else {
      this.clearSession();
    }
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'An error occurred';
    
    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.message) {
      errorMessage = error.message;
    } else if (typeof error === 'string') {
      errorMessage = error;
    }

    console.error('Auth Service Error:', error);
    return throwError(() => new Error(errorMessage));
  }
}
