import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { 
  Document, 
  DocumentVersion, 
  UploadDocumentRequest, 
  UpdateDocumentRequest,
  DocumentUrlDto,
  OCRResult,
  PagedResult 
} from '../models/document.model';
import { ApiResponse, PaginationParams, FilterParams } from '../models/common.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/documents`;

  constructor(private http: HttpClient) {}

  uploadDocument(file: File, request: UploadDocumentRequest): Observable<Document> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('name', request.name);
    if (request.description) {
      formData.append('description', request.description);
    }
    if (request.tags) {
      request.tags.forEach(tag => formData.append('tags', tag));
    }

    return this.http.post<ApiResponse<Document>>(`${this.apiUrl}/upload`, formData)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getDocuments(
    pagination: PaginationParams,
    filters?: FilterParams
  ): Observable<PagedResult<Document>> {
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
      if (filters.tags && filters.tags.length > 0) {
        filters.tags.forEach(tag => params = params.append('tags', tag));
      }
      if (filters.dateFrom) {
        params = params.set('dateFrom', filters.dateFrom.toISOString());
      }
      if (filters.dateTo) {
        params = params.set('dateTo', filters.dateTo.toISOString());
      }
    }

    return this.http.get<ApiResponse<PagedResult<Document>>>(this.apiUrl, { params })
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getDocument(id: string): Observable<Document> {
    return this.http.get<ApiResponse<Document>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  updateDocument(id: string, request: UpdateDocumentRequest): Observable<Document> {
    return this.http.put<ApiResponse<Document>>(`${this.apiUrl}/${id}`, request)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  deleteDocument(id: string): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  addDocumentVersion(documentId: string, file: File, comment?: string): Observable<DocumentVersion> {
    const formData = new FormData();
    formData.append('file', file);
    if (comment) {
      formData.append('comment', comment);
    }

    return this.http.post<ApiResponse<DocumentVersion>>(`${this.apiUrl}/${documentId}/versions`, formData)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getDocumentVersions(documentId: string): Observable<DocumentVersion[]> {
    return this.http.get<ApiResponse<DocumentVersion[]>>(`${this.apiUrl}/${documentId}/versions`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  processOCR(documentId: string, language: string = 'en'): Observable<OCRResult> {
    return this.http.post<ApiResponse<OCRResult>>(`${this.apiUrl}/${documentId}/ocr`, { language })
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  getDocumentDownloadUrl(documentId: string): Observable<DocumentUrlDto> {
    return this.http.get<ApiResponse<DocumentUrlDto>>(`${this.apiUrl}/${documentId}/download`)
      .pipe(
        map(response => this.handleResponse(response)),
        catchError(error => this.handleError(error))
      );
  }

  downloadDocument(documentId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${documentId}/download`, { 
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

    console.error('Document Service Error:', error);
    throw new Error(errorMessage);
  }
}
