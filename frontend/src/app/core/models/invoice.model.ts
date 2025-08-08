export interface Invoice {
  id: string;
  invoiceNumber: string;
  tenantId: string;
  customerId: string;
  customerName: string;
  customerEmail?: string;
  issueDate: Date;
  dueDate: Date;
  subtotal: number;
  taxAmount: number;
  discountAmount: number;
  totalAmount: number;
  currency: string;
  status: InvoiceStatus;
  description?: string;
  notes?: string;
  lineItems: InvoiceLineItem[];
  payments: InvoicePayment[];
  approvalWorkflowId?: string;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface InvoiceLineItem {
  id: string;
  invoiceId: string;
  description: string;
  quantity: number;
  unitPrice: number;
  totalAmount: number;
  taxRate?: number;
  taxAmount?: number;
}

export interface InvoicePayment {
  id: string;
  invoiceId: string;
  paymentId: string;
  amount: number;
  paymentDate: Date;
  paymentMethod: string;
  status: PaymentStatus;
}

export interface CreateInvoiceRequest {
  customerId: string;
  customerName: string;
  customerEmail?: string;
  issueDate: Date;
  dueDate: Date;
  currency: string;
  description?: string;
  notes?: string;
  lineItems: CreateInvoiceLineItemRequest[];
}

export interface CreateInvoiceLineItemRequest {
  description: string;
  quantity: number;
  unitPrice: number;
  taxRate?: number;
}

export interface UpdateInvoiceRequest {
  customerName: string;
  customerEmail?: string;
  dueDate: Date;
  description?: string;
  notes?: string;
  lineItems: CreateInvoiceLineItemRequest[];
}

export enum InvoiceStatus {
  Draft = 'Draft',
  PendingApproval = 'PendingApproval',
  Approved = 'Approved',
  Sent = 'Sent',
  Paid = 'Paid',
  Overdue = 'Overdue',
  Cancelled = 'Cancelled'
}

export enum PaymentStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Completed = 'Completed',
  Failed = 'Failed',
  Refunded = 'Refunded'
}
