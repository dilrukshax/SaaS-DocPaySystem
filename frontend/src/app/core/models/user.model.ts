export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  department?: string;
  jobTitle?: string;
  phoneNumber?: string;
  timeZone?: string;
  language?: string;
  roles: string[];
  preferences?: { [key: string]: any };
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface AuthenticationResult {
  user: User;
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
}

export interface LoginRequest {
  email: string;
  password: string;
  deviceInfo?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  tenantId: string;
  department?: string;
  jobTitle?: string;
  phoneNumber?: string;
  timeZone?: string;
  language?: string;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  department?: string;
  jobTitle?: string;
  phoneNumber?: string;
  timeZone?: string;
  language?: string;
  preferences?: { [key: string]: any };
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}
