import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from './admin-api.service';
import { AdminCategorySalesMetric, AdminSalesAnalytics } from './admin.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

interface ChartMetric extends AdminCategorySalesMetric {
  label: string;
  color: string;
  dashOffset: number;
}

const CHART_COLORS = ['#178d92', '#d9a441', '#7c6dd8', '#d96f5d', '#4f8f55', '#c15d90', '#66737a'];
const DONUT_RADIUS = 64;
const DONUT_CIRCUMFERENCE = 2 * Math.PI * DONUT_RADIUS;

@Component({
  selector: 'app-sales-dashboard',
  standalone: true,
  imports: [FormsModule, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <section class="sales-dashboard">
      <header class="sales-dashboard__header">
        <div>
          <p class="sales-dashboard__eyebrow">Legacy admin analytics</p>
          <h1 class="cart-title">Sales dashboard</h1>
        </div>

        <form class="sales-dashboard__filters" (ngSubmit)="load()">
          <label>
            From
            <input type="date" name="startDate" [(ngModel)]="startDate" />
          </label>
          <label>
            To
            <input type="date" name="endDate" [(ngModel)]="endDate" />
          </label>
          <button type="submit" [disabled]="state() === 'loading'">Refresh</button>
        </form>
      </header>

      @if (validationError()) {
        <p class="cart-error">{{ validationError() }}</p>
      }

      @if (state() === 'loading') {
        <app-loading-state />
      } @else if (state() === 'unavailable') {
        <app-unavailable-state message="Sales analytics are currently unavailable. Please try again later." />
      } @else if (isEmpty()) {
        <div class="state-empty sales-dashboard__empty">
          <p>No sales found for this date range.</p>
        </div>
      } @else if (analytics(); as data) {
        <div class="sales-summary" aria-label="Sales totals">
          <div>
            <span class="sales-summary__label">Total revenue</span>
            <strong>{{ money(data.totalRevenue) }}</strong>
          </div>
          <div>
            <span class="sales-summary__label">Sold items</span>
            <strong>{{ integer(data.totalSalesCount) }}</strong>
          </div>
          <div>
            <span class="sales-summary__label">Range</span>
            <strong>{{ data.startDate }} to {{ data.endDate }}</strong>
          </div>
        </div>

        <div class="sales-charts">
          <section class="sales-panel">
            <div class="sales-panel__head">
              <h2>Revenue share</h2>
              <p>Percent of sales value by category</p>
            </div>

            <div class="sales-donut-wrap">
              <svg class="sales-donut" viewBox="0 0 180 180" role="img" aria-labelledby="revenue-chart-title">
                <title id="revenue-chart-title">Revenue share by category</title>
                <circle class="sales-donut__track" cx="90" cy="90" [attr.r]="donutRadius" />
                @for (category of chartMetrics(); track category.categoryId) {
                  <circle
                    class="sales-donut__segment"
                    cx="90"
                    cy="90"
                    [attr.r]="donutRadius"
                    [attr.stroke]="category.color"
                    [attr.stroke-dasharray]="segmentDash(category)"
                    [attr.stroke-dashoffset]="category.dashOffset"
                  />
                }
                <text x="90" y="86" text-anchor="middle" class="sales-donut__value">{{ money(data.totalRevenue) }}</text>
                <text x="90" y="106" text-anchor="middle" class="sales-donut__label">revenue</text>
              </svg>

              <div class="sales-legend">
                @for (category of chartMetrics(); track category.categoryId) {
                  <div class="sales-legend__row">
                    <span class="sales-swatch" [style.background]="category.color"></span>
                    <span class="sales-legend__label">{{ category.label }}</span>
                    <strong>{{ money(category.revenue) }}</strong>
                    <span>{{ decimal(category.revenuePercent) }}%</span>
                  </div>
                }
              </div>
            </div>
          </section>

          <section class="sales-panel">
            <div class="sales-panel__head">
              <h2>Sales count</h2>
              <p>Total sold quantity by category</p>
            </div>

            <div class="sales-bars" role="img" aria-label="Sales count by category">
              @for (category of chartMetrics(); track category.categoryId) {
                <div class="sales-bar-row">
                  <span class="sales-bar-row__label">{{ category.label }}</span>
                  <div class="sales-bar-row__track">
                    <span
                      class="sales-bar-row__bar"
                      [style.width.%]="barWidth(category.salesCount)"
                      [style.background]="category.color"
                    ></span>
                  </div>
                  <strong>{{ integer(category.salesCount) }}</strong>
                </div>
              }
            </div>
          </section>
        </div>
      }
    </section>
  `
})
export class SalesDashboardComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly donutRadius = DONUT_RADIUS;
  readonly state = signal<'idle' | 'loading' | 'ready' | 'unavailable'>('idle');
  readonly analytics = signal<AdminSalesAnalytics | null>(null);
  readonly validationError = signal<string | null>(null);
  readonly chartMetrics = computed(() => this.decorate(this.analytics()?.categories ?? []));
  readonly isEmpty = computed(() => this.state() === 'ready' && this.chartMetrics().length === 0);
  readonly maxSalesCount = computed(() => Math.max(1, ...this.chartMetrics().map((category) => category.salesCount)));

  startDate = '2001-01-01';
  endDate = today();

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    if (!this.startDate || !this.endDate) {
      this.validationError.set('Both start and end dates are required.');
      return;
    }

    if (this.startDate > this.endDate) {
      this.validationError.set('Start date must be before or equal to end date.');
      return;
    }

    this.validationError.set(null);
    this.state.set('loading');
    this.api.getSalesAnalytics(this.startDate, this.endDate).subscribe({
      next: (analytics) => {
        this.analytics.set(analytics);
        this.state.set('ready');
      },
      error: () => {
        this.analytics.set(null);
        this.state.set('unavailable');
      }
    });
  }

  segmentDash(category: ChartMetric): string {
    const length = (Math.max(0, category.revenuePercent) / 100) * DONUT_CIRCUMFERENCE;
    return `${length} ${DONUT_CIRCUMFERENCE - length}`;
  }

  barWidth(value: number): number {
    return Math.max(3, (value / this.maxSalesCount()) * 100);
  }

  money(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  decimal(value: number): string {
    return new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value);
  }

  integer(value: number): string {
    return new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(value);
  }

  private decorate(categories: AdminCategorySalesMetric[]): ChartMetric[] {
    let offset = 0;
    return categories.map((category, index) => {
      const metric = {
        ...category,
        label: category.categoryName || category.categoryId,
        color: CHART_COLORS[index % CHART_COLORS.length],
        dashOffset: -offset
      };
      offset += (Math.max(0, category.revenuePercent) / 100) * DONUT_CIRCUMFERENCE;
      return metric;
    });
  }
}

function today(): string {
  const now = new Date();
  const month = `${now.getMonth() + 1}`.padStart(2, '0');
  const day = `${now.getDate()}`.padStart(2, '0');
  return `${now.getFullYear()}-${month}-${day}`;
}
