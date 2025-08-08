import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterLink } from '@angular/router';
import { Observable, forkJoin } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { User } from '../../../core/models/user.model';

interface DashboardStats {
  totalDocuments: number;
  pendingApprovals: number;
  totalInvoices: number;
  recentPayments: number;
}

interface RecentActivity {
  id: string;
  type: 'document' | 'invoice' | 'payment' | 'workflow';
  title: string;
  description: string;
  timestamp: Date;
  status: string;
}

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatGridListModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  currentUser$: Observable<User | null>;
  dashboardStats: DashboardStats = {
    totalDocuments: 0,
    pendingApprovals: 0,
    totalInvoices: 0,
    recentPayments: 0
  };
  
  recentActivities: RecentActivity[] = [];
  loading = false;

  constructor(private authService: AuthService) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    
    // Mock data for now - replace with actual API calls
    setTimeout(() => {
      this.dashboardStats = {
        totalDocuments: 156,
        pendingApprovals: 12,
        totalInvoices: 89,
        recentPayments: 34
      };

      this.recentActivities = [
        {
          id: '1',
          type: 'document',
          title: 'New Document Uploaded',
          description: 'Contract-2024-001.pdf uploaded successfully',
          timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
          status: 'completed'
        },
        {
          id: '2',
          type: 'invoice',
          title: 'Invoice Pending Approval',
          description: 'Invoice #INV-2024-156 requires approval',
          timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
          status: 'pending'
        },
        {
          id: '3',
          type: 'payment',
          title: 'Payment Processed',
          description: 'Payment of $1,250.00 processed successfully',
          timestamp: new Date(Date.now() - 1000 * 60 * 60 * 4), // 4 hours ago
          status: 'completed'
        },
        {
          id: '4',
          type: 'workflow',
          title: 'Workflow Task Assigned',
          description: 'Document review task assigned to you',
          timestamp: new Date(Date.now() - 1000 * 60 * 60 * 6), // 6 hours ago
          status: 'pending'
        }
      ];

      this.loading = false;
    }, 1000);
  }

  getActivityIcon(type: string): string {
    switch (type) {
      case 'document': return 'description';
      case 'invoice': return 'receipt';
      case 'payment': return 'payment';
      case 'workflow': return 'timeline';
      default: return 'info';
    }
  }

  getActivityColor(status: string): string {
    switch (status) {
      case 'completed': return 'primary';
      case 'pending': return 'warn';
      case 'failed': return 'warn';
      default: return 'primary';
    }
  }

  refreshDashboard(): void {
    this.loadDashboardData();
  }
}
