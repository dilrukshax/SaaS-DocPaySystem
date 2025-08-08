import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { AuthenticationResult, RegisterRequest, User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class MockAuthService {
  register(request: RegisterRequest): Observable<AuthenticationResult> {
    // Simulate API delay
    return of({
      user: {
        id: 'mock-user-id-' + Date.now(),
        email: request.email,
        firstName: request.firstName,
        lastName: request.lastName,
        tenantId: request.tenantId,
        department: request.department,
        jobTitle: request.jobTitle,
        phoneNumber: request.phoneNumber,
        timeZone: request.timeZone || 'UTC+00:00',
        language: request.language || 'en',
        roles: ['User'],
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      },
      accessToken: 'mock-access-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      expiresAt: new Date(Date.now() + 3600000) // 1 hour from now
    } as AuthenticationResult).pipe(delay(1500)); // Simulate network delay
  }
}
