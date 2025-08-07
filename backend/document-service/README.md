# Document Service

This microservice handles document upload/download, versioning, metadata management, and OCR integration.

## Features

- Document upload and storage
- Version management
- Metadata extraction and management
- OCR processing integration
- Search and filtering capabilities

## API Endpoints

- `GET /api/documents` - List documents
- `POST /api/documents` - Upload document
- `GET /api/documents/{id}` - Get document details
- `PUT /api/documents/{id}` - Update document
- `DELETE /api/documents/{id}` - Delete document
- `GET /api/documents/{id}/versions` - Get document versions
- `POST /api/documents/{id}/ocr` - Process OCR

## Configuration

Update `appsettings.json` with your database connection string and storage settings.
