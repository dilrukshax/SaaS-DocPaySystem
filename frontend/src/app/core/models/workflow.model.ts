export interface WorkflowDefinition {
  id: string;
  name: string;
  description?: string;
  tenantId: string;
  steps: WorkflowStep[];
  isActive: boolean;
  createdBy: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface WorkflowStep {
  id: string;
  name: string;
  description?: string;
  stepType: WorkflowStepType;
  order: number;
  requiredRole?: string;
  autoComplete: boolean;
  timeoutHours?: number;
  conditions?: { [key: string]: any };
}

export interface WorkflowInstance {
  id: string;
  workflowDefinitionId: string;
  workflowName: string;
  tenantId: string;
  entityId: string;
  entityType: string;
  status: WorkflowStatus;
  currentStepId?: string;
  currentStepName?: string;
  startedBy: string;
  startedAt: Date;
  completedAt?: Date;
  data?: { [key: string]: any };
  tasks: WorkflowTask[];
}

export interface WorkflowTask {
  id: string;
  workflowInstanceId: string;
  title: string;
  description?: string;
  assignedTo: string;
  assignedToName?: string;
  taskType: TaskType;
  priority: TaskPriority;
  status: TaskStatus;
  dueDate?: Date;
  completedAt?: Date;
  completedBy?: string;
  notes?: string;
  data?: { [key: string]: any };
  createdAt: Date;
}

export interface CreateWorkflowRequest {
  name: string;
  description?: string;
  steps: CreateWorkflowStepRequest[];
}

export interface CreateWorkflowStepRequest {
  name: string;
  description?: string;
  stepType: WorkflowStepType;
  order: number;
  requiredRole?: string;
  autoComplete: boolean;
  timeoutHours?: number;
}

export interface StartWorkflowRequest {
  workflowDefinitionId: string;
  entityId: string;
  entityType: string;
  data?: { [key: string]: any };
}

export interface CompleteTaskRequest {
  taskId: string;
  notes?: string;
  data?: { [key: string]: any };
}

export enum WorkflowStepType {
  Manual = 'Manual',
  Approval = 'Approval',
  Notification = 'Notification',
  Integration = 'Integration'
}

export enum WorkflowStatus {
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Failed = 'Failed'
}

export enum TaskType {
  Approval = 'Approval',
  Review = 'Review',
  Action = 'Action',
  Notification = 'Notification'
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical'
}

export enum TaskStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Overdue = 'Overdue'
}
