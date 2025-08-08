import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { 
  Invoice, 
  CreateInvoiceRequest, 
  UpdateInvoiceRequest
} from '../models/invoice.model';
import { ApiResponse, PaginationParams, FilterParams } from '../models/common.model';
import { PagedResult } from '../models/document.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/invoices`;

  constructor(private http: HttpClient) {}

  createInvoice(request: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<ApiResponse<Invoice>>(this.apiUrl, request)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getInvoices(
    pagination: PaginationParams,
    filters?: FilterParams
  ): Observable<PagedResult<Invoice>> {
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

    return this.http.get<ApiResponse<PagedResult<Invoice>>>(this.apiUrl, { params })
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getInvoice(id: string): Observable<Invoice> {
    return this.http.get<ApiResponse<Invoice>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  updateInvoice(id: string, request: UpdateInvoiceRequest): Observable<Invoice> {
    return this.http.put<ApiResponse<Invoice>>(`${this.apiUrl}/${id}`, request)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  deleteInvoice(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  sendInvoice(id: string): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/send`, {})
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  approveInvoice(id: string, notes?: string): Observable<Invoice> {
    return this.http.post<ApiResponse<Invoice>>(`${this.apiUrl}/${id}/approve`, { notes })
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  rejectInvoice(id: string, reason: string): Observable<Invoice> {
    return this.http.post<ApiResponse<Invoice>>(`${this.apiUrl}/${id}/reject`, { reason })
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getInvoicePdf(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/pdf`, { 
      responseType: 'blob' 
    });
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

    console.error('Invoice Service Error:', error);
    throw new Error(errorMessage);
  }
}
