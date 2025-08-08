import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { 
  Payment, 
  CreatePaymentRequest, 
  ProcessPaymentRequest,
  RefundPaymentRequest
} from '../models/payment.model';
import { ApiResponse, PaginationParams, FilterParams } from '../models/common.model';
import { PagedResult } from '../models/document.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/payments`;

  constructor(private http: HttpClient) {}

  createPayment(request: CreatePaymentRequest): Observable<Payment> {
    return this.http.post<ApiResponse<Payment>>(this.apiUrl, request)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  processPayment(request: ProcessPaymentRequest): Observable<Payment> {
    return this.http.post<ApiResponse<Payment>>(`${this.apiUrl}/process`, request)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getPayments(
    pagination: PaginationParams,
    filters?: FilterParams
  ): Observable<PagedResult<Payment>> {
    let params = new HttpParams()
      .set('page', pagination.page.toString())
      .set('pageSize', pagination.pageSize.toString());

    if (pagination.sortBy) {
      params = params.set('sortBy', pagination.sortBy);
    }
    if (pagination.sortDirection) {
      params = params.set('sortDirection', pagination.sortDirection);
    }

    if (filters) {
      if (filters.searchTerm) {
        params = params.set('searchTerm', filters.searchTerm);
      }
      if (filters.status) {
        params = params.set('status', filters.status);
      }
      if (filters.dateFrom) {
        params = params.set('dateFrom', filters.dateFrom.toISOString());
      }
      if (filters.dateTo) {
        params = params.set('dateTo', filters.dateTo.toISOString());
      }
    }

    return this.http.get<ApiResponse<PagedResult<Payment>>>(this.apiUrl, { params })
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getPayment(id: string): Observable<Payment> {
    return this.http.get<ApiResponse<Payment>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  refundPayment(request: RefundPaymentRequest): Observable<Payment> {
    return this.http.post<ApiResponse<Payment>>(`${this.apiUrl}/${request.paymentId}/refund`, request)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  cancelPayment(id: string): Observable<Payment> {
    return this.http.post<ApiResponse<Payment>>(`${this.apiUrl}/${id}/cancel`, {})
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  private handleResponse<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data === undefined) {
      throw new Error(response.message || 'API request failed');
    }
    return response.data;
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

    console.error('Payment Service Error:', error);
    throw new Error(errorMessage);
  }
}
