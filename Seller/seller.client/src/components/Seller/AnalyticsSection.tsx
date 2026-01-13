import { useState, useMemo } from 'react';
import './AnalyticsSection.css';
import { formatShortDate } from '../../utils/dateUtils';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

interface AnalyticsSectionProps {
    companies: Company[];
}

interface SalesData {
    date: string;
    revenue: number;
    orders: number;
    customers: number;
}

interface ProductPerformance {
    productName: string;
    sales: number;
    revenue: number;
    orders: number;
}

// Mock analytics data
const mockSalesData: SalesData[] = [
    { date: '2024-01-01', revenue: 1250.00, orders: 15, customers: 12 },
    { date: '2024-01-02', revenue: 980.50, orders: 8, customers: 7 },
    { date: '2024-01-03', revenue: 1450.75, orders: 18, customers: 15 },
    { date: '2024-01-04', revenue: 2100.00, orders: 25, customers: 20 },
    { date: '2024-01-05', revenue: 1800.25, orders: 22, customers: 18 },
    { date: '2024-01-06', revenue: 950.00, orders: 12, customers: 10 },
    { date: '2024-01-07', revenue: 1350.50, orders: 16, customers: 14 },
];

const mockProductPerformance: ProductPerformance[] = [
    { productName: 'T-Shirt', sales: 45, revenue: 1169.55, orders: 23 },
    { productName: 'Sneakers', sales: 28, revenue: 2519.72, orders: 18 },
    { productName: 'Jacket', sales: 15, revenue: 1949.85, orders: 12 },
    { productName: 'Jeans', sales: 32, revenue: 1919.68, orders: 20 },
    { productName: 'Hat', sales: 18, revenue: 359.82, orders: 15 },
];

function AnalyticsSection(_props: AnalyticsSectionProps) {
    const [salesData] = useState<SalesData[]>(mockSalesData);
    const [productPerformance] = useState<ProductPerformance[]>(mockProductPerformance);

    const analytics = useMemo(() => {
        const totalRevenue = salesData.reduce((sum, day) => sum + day.revenue, 0);
        const totalOrders = salesData.reduce((sum, day) => sum + day.orders, 0);
        const totalCustomers = salesData.reduce((sum, day) => sum + day.customers, 0);
        const averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        
        // Calculate growth (comparing with previous period - simplified)
        const recentRevenue = salesData.slice(-3).reduce((sum, day) => sum + day.revenue, 0);
        const previousRevenue = salesData.slice(-6, -3).reduce((sum, day) => sum + day.revenue, 0);
        const revenueGrowth = previousRevenue > 0 ? ((recentRevenue - previousRevenue) / previousRevenue) * 100 : 0;

        return {
            totalRevenue,
            totalOrders,
            totalCustomers,
            averageOrderValue,
            revenueGrowth
        };
    }, [salesData]);

    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD'
        }).format(amount);
    };

    const formatPercentage = (value: number) => {
        const sign = value >= 0 ? '+' : '';
        return `${sign}${value.toFixed(1)}%`;
    };

    return (
        <div className="section-container">
            {/* Key Metrics */}
            <div className="analytics-metrics-grid">
                <div className="analytics-metric-card analytics-metric-card--revenue">
                    <div className="analytics-metric-label">Total Revenue</div>
                    <div className="analytics-metric-value">
                        {formatCurrency(analytics.totalRevenue)}
                    </div>
                    <div className={`analytics-metric-change ${analytics.revenueGrowth >= 0 ? 'analytics-metric-change--positive' : 'analytics-metric-change--negative'}`}>
                        {formatPercentage(analytics.revenueGrowth)} from last period
                    </div>
                </div>

                <div className="analytics-metric-card analytics-metric-card--orders">
                    <div className="analytics-metric-label">Total Orders</div>
                    <div className="analytics-metric-value">
                        {analytics.totalOrders.toLocaleString()}
                    </div>
                    <div className="analytics-metric-subtitle">
                        Across {analytics.totalCustomers} customers
                    </div>
                </div>

                <div className="analytics-metric-card analytics-metric-card--avg-order">
                    <div className="analytics-metric-label">Average Order Value</div>
                    <div className="analytics-metric-value">
                        {formatCurrency(analytics.averageOrderValue)}
                    </div>
                    <div className="analytics-metric-subtitle">
                        Per transaction
                    </div>
                </div>

                <div className="analytics-metric-card analytics-metric-card--customers">
                    <div className="analytics-metric-label">Unique Customers</div>
                    <div className="analytics-metric-value">
                        {analytics.totalCustomers.toLocaleString()}
                    </div>
                    <div className="analytics-metric-subtitle">
                        Active buyers
                    </div>
                </div>
            </div>

            {/* Sales Chart */}
            <div className="analytics-chart-section">
                <h3 className="analytics-chart-title">Daily Sales Overview</h3>
                <div className="analytics-chart-container">
                    <div className="analytics-chart-scroll">
                        <div className="analytics-chart-bars">
                            {salesData.map((day) => {
                                const maxRevenue = Math.max(...salesData.map(d => d.revenue));
                                const height = (day.revenue / maxRevenue) * 250;
                                
                                return (
                                    <div key={day.date} className="analytics-chart-bar-container">
                                        <div 
                                            className="analytics-chart-bar"
                                            style={{ height: `${height}px` }}
                                            title={`${formatCurrency(day.revenue)} - ${day.orders} orders`}
                                        >
                                        </div>
                                        <div className="analytics-chart-date">
                                            {formatShortDate(day.date)}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                    <div className="analytics-chart-legend">
                        <div className="analytics-chart-help">
                            Hover over bars to see details
                        </div>
                        <div className="analytics-chart-legend-items">
                            <div className="analytics-chart-legend-item">
                                <div className="analytics-chart-legend-color"></div>
                                <span className="analytics-chart-legend-text">Daily Revenue</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Product Performance */}
            <div className="analytics-products-section">
                <h3 className="analytics-products-title">Product Performance</h3>
                <div className="analytics-products-container">
                    <table className="analytics-products-table">
                        <thead>
                            <tr>
                                <th>Product</th>
                                <th>Units Sold</th>
                                <th>Revenue</th>
                                <th>Orders</th>
                                <th>Avg. Order Value</th>
                            </tr>
                        </thead>
                        <tbody>
                            {productPerformance.map((product) => (
                                <tr key={product.productName}>
                                    <td>
                                        <div className="analytics-product-name">{product.productName}</div>
                                    </td>
                                    <td>
                                        {product.sales.toLocaleString()}
                                    </td>
                                    <td className="analytics-product-revenue">
                                        {formatCurrency(product.revenue)}
                                    </td>
                                    <td>
                                        {product.orders.toLocaleString()}
                                    </td>
                                    <td>
                                        {formatCurrency(product.revenue / product.orders)}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Additional Insights */}
            <div className="analytics-insights-section">
                <h3 className="analytics-insights-title">Business Insights</h3>
                <div className="analytics-insights-grid">
                    <div className="analytics-insight-card">
                        <h4 className="analytics-insight-title analytics-insight-title--green">📈 Top Performing Product</h4>
                        <div className="analytics-insight-value">
                            <strong>{productPerformance[0]?.productName}</strong>
                        </div>
                        <div className="analytics-insight-description">
                            Generated {formatCurrency(productPerformance[0]?.revenue || 0)} in revenue 
                            with {productPerformance[0]?.sales || 0} units sold.
                        </div>
                    </div>

                    <div className="analytics-insight-card">
                        <h4 className="analytics-insight-title analytics-insight-title--blue">🎯 Conversion Rate</h4>
                        <div className="analytics-insight-value">
                            <strong>85.2%</strong>
                        </div>
                        <div className="analytics-insight-description">
                            Percentage of visitors who made a purchase. Above industry average!
                        </div>
                    </div>

                    <div className="analytics-insight-card">
                        <h4 className="analytics-insight-title analytics-insight-title--yellow">⏱️ Avg. Processing Time</h4>
                        <div className="analytics-insight-value">
                            <strong>2.3 days</strong>
                        </div>
                        <div className="analytics-insight-description">
                            Average time from order placement to shipment. Consider optimizing further.
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default AnalyticsSection;