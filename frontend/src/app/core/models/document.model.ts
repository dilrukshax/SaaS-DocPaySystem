export interface Document {
  id: string;
  name: string;
  description?: string;
  fileName: string;
  contentType: string;
  size: number;
  status: DocumentStatus;
  tenantId: string;
  uploadedBy: string;
  tags: string[];
  metadata?: { [key: string]: any };
  ocrStatus?: OCRStatus;
  ocrText?: string;
  isVersioned: boolean;
  currentVersion: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface DocumentVersion {
  id: string;
  documentId: string;
  version: number;
  fileName: string;
  contentType: string;
  size: number;
  comment?: string;
  createdBy: string;
  createdAt: Date;
}

export interface UploadDocumentRequest {
  name: string;
  description?: string;
  tags?: string[];
}

export interface UpdateDocumentRequest {
  name: string;
  description?: string;
  tags: string[];
  updatedBy: string;
}

export interface DocumentUrlDto {
  downloadUrl: string;
  expiresAt: Date;
}

export interface OCRResult {
  text: string;
  confidence: number;
  language: string;
  processedAt: Date;
}

export enum DocumentStatus {
  Uploading = 'Uploading',
  Processing = 'Processing',
  Ready = 'Ready',
  Failed = 'Failed',
  Archived = 'Archived'
}

export enum OCRStatus {
  NotProcessed = 'NotProcessed',
  Processing = 'Processing',
  Completed = 'Completed',
  Failed = 'Failed'
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
