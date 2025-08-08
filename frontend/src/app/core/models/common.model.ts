export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
  timestamp: Date;
}

export interface ApiError {
  message: string;
  details?: string;
  code?: string;
  statusCode: number;
}

export interface PaginationParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface FilterParams {
  searchTerm?: string;
  status?: string;
  tags?: string[];
  dateFrom?: Date;
  dateTo?: Date;
}

export enum UserRole {
  Admin = 'Admin',
  Manager = 'Manager',
  Approver = 'Approver',
  User = 'User',
  Viewer = 'Viewer'
}

export interface AppConfig {
  apiBaseUrl: string;
  auth: {
    issuer: string;
    audience: string;
    tokenKey: string;
    refreshTokenKey: string;
  };
  features: {
    enableNotifications: boolean;
    enableReports: boolean;
    enableAI: boolean;
    maxFileUploadSize: number;
  };
}

export interface LoadingState {
  loading: boolean;
  error?: string;
}

export interface BreadcrumbItem {
  label: string;
  url?: string;
  icon?: string;
}
