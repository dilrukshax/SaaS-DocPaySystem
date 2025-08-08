export interface Notification {
  id: string;
  tenantId: string;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  channel: NotificationChannel;
  priority: NotificationPriority;
  status: NotificationStatus;
  entityId?: string;
  entityType?: string;
  actionUrl?: string;
  metadata?: { [key: string]: any };
  isRead: boolean;
  readAt?: Date;
  sentAt?: Date;
  createdAt: Date;
}

export interface NotificationTemplate {
  id: string;
  name: string;
  description?: string;
  tenantId: string;
  subject: string;
  bodyTemplate: string;
  type: NotificationType;
  isActive: boolean;
  variables: string[];
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateNotificationRequest {
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  channel: NotificationChannel;
  priority: NotificationPriority;
  entityId?: string;
  entityType?: string;
  actionUrl?: string;
  metadata?: { [key: string]: any };
}

export interface SendNotificationRequest {
  templateId: string;
  userId: string;
  variables: { [key: string]: any };
  channel: NotificationChannel;
  priority: NotificationPriority;
  entityId?: string;
  entityType?: string;
  actionUrl?: string;
}

export interface MarkNotificationReadRequest {
  notificationIds: string[];
}

export enum NotificationType {
  Info = 'Info',
  Warning = 'Warning',
  Error = 'Error',
  Success = 'Success',
  Reminder = 'Reminder',
  Alert = 'Alert'
}

export enum NotificationChannel {
  InApp = 'InApp',
  Email = 'Email',
  SMS = 'SMS',
  Push = 'Push'
}

export enum NotificationPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum NotificationStatus {
  Pending = 'Pending',
  Sent = 'Sent',
  Delivered = 'Delivered',
  Failed = 'Failed',
  Cancelled = 'Cancelled'
}
