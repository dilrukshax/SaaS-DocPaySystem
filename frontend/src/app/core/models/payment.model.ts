export interface Payment {
  id: string;
  invoiceId?: string;
  tenantId: string;
  amount: number;
  currency: string;
  status: PaymentStatus;
  paymentMethod: PaymentMethod;
  stripePaymentIntentId?: string;
  description?: string;
  metadata?: { [key: string]: any };
  processedAt?: Date;
  failureReason?: string;
  refundedAmount?: number;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface PaymentMethod {
  id: string;
  type: PaymentMethodType;
  cardLast4?: string;
  cardBrand?: string;
  expiryMonth?: number;
  expiryYear?: number;
  isDefault: boolean;
}

export interface CreatePaymentRequest {
  invoiceId?: string;
  amount: number;
  currency: string;
  paymentMethodId: string;
  description?: string;
  metadata?: { [key: string]: any };
}

export interface ProcessPaymentRequest {
  paymentIntentId: string;
  paymentMethodId: string;
}

export interface RefundPaymentRequest {
  paymentId: string;
  amount?: number;
  reason?: string;
}

export interface PaymentWebhookEvent {
  id: string;
  type: string;
  paymentIntentId: string;
  status: string;
  amount: number;
  currency: string;
  processedAt: Date;
}

export enum PaymentStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  RequiresAction = 'RequiresAction',
  Succeeded = 'Succeeded',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded'
}

export enum PaymentMethodType {
  Card = 'Card',
  BankTransfer = 'BankTransfer',
  Wallet = 'Wallet'
}
