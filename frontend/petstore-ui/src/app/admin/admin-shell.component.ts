import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AdminApiService } from './admin-api.service';
import { ORDER_STATUSES } from './admin.models';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="admin-shell">
      <nav class="admin-nav">
        <a routerLink="/admin/pending" routerLinkActive="admin-nav__link--active" class="admin-nav__link">Pending queue</a>
        <a routerLink="/admin/orders" routerLinkActive="admin-nav__link--active" class="admin-nav__link">All orders</a>
      </nav>

      @if (counts(); as c) {
        <div class="admin-counts">
          @for (status of statuses; track status) {
            <span class="admin-counts__item">
              <span class="order-status" [class]="'order-status order-status--' + status.toLowerCase().replace('_', '-')">{{ status }}</span>
              <strong>{{ c[status] ?? 0 }}</strong>
            </span>
          }
        </div>
      }

      <router-outlet />
    </div>
  `
})
export class AdminShellComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly statuses = ORDER_STATUSES;
  readonly counts = signal<Record<string, number> | null>(null);

  ngOnInit(): void {
    this.refreshCounts();
  }

  refreshCounts(): void {
    // Legacy statistics reduced to per-status counts (plan DD-002).
    this.api.getOrders().subscribe({
      next: (orders) => {
        const counts: Record<string, number> = {};
        for (const order of orders) {
          counts[order.status] = (counts[order.status] ?? 0) + 1;
        }
        this.counts.set(counts);
      },
      error: () => this.counts.set(null)
    });
  }
}
