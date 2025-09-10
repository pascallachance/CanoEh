import { useState, useMemo } from 'react';

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
    const [selectedPeriod, setSelectedPeriod] = useState<'7d' | '30d' | '90d' | '1y'>('7d');
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
            <h2 className="section-title">Analytics & Reporting</h2>
            <p className="section-description">
                Track your sales performance, analyze customer behavior, and gain insights into your business growth.
            </p>

            <div style={{ marginBottom: '2rem', display: 'flex', gap: '1rem', alignItems: 'center' }}>
                <label style={{ fontWeight: '600' }}>Time Period:</label>
                <select 
                    value={selectedPeriod}
                    onChange={(e) => setSelectedPeriod(e.target.value as typeof selectedPeriod)}
                    style={{
                        padding: '0.5rem',
                        border: '1px solid #ced4da',
                        borderRadius: '4px',
                        fontSize: '1rem'
                    }}
                >
                    <option value="7d">Last 7 Days</option>
                    <option value="30d">Last 30 Days</option>
                    <option value="90d">Last 90 Days</option>
                    <option value="1y">Last Year</option>
                </select>
            </div>

            {/* Key Metrics */}
            <div style={{ 
                display: 'grid', 
                gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', 
                gap: '1.5rem',
                marginBottom: '3rem'
            }}>
                <div style={{
                    padding: '1.5rem',
                    background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                    color: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
                }}>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Total Revenue</div>
                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
                        {formatCurrency(analytics.totalRevenue)}
                    </div>
                    <div style={{ 
                        fontSize: '0.9rem', 
                        color: analytics.revenueGrowth >= 0 ? '#d4edda' : '#f8d7da'
                    }}>
                        {formatPercentage(analytics.revenueGrowth)} from last period
                    </div>
                </div>

                <div style={{
                    padding: '1.5rem',
                    background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
                    color: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
                }}>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Total Orders</div>
                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
                        {analytics.totalOrders.toLocaleString()}
                    </div>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9 }}>
                        Across {analytics.totalCustomers} customers
                    </div>
                </div>

                <div style={{
                    padding: '1.5rem',
                    background: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
                    color: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
                }}>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Average Order Value</div>
                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
                        {formatCurrency(analytics.averageOrderValue)}
                    </div>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9 }}>
                        Per transaction
                    </div>
                </div>

                <div style={{
                    padding: '1.5rem',
                    background: 'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
                    color: 'white',
                    borderRadius: '12px',
                    boxShadow: '0 4px 6px rgba(0, 0, 0, 0.1)'
                }}>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9, marginBottom: '0.5rem' }}>Unique Customers</div>
                    <div style={{ fontSize: '2.5rem', fontWeight: 'bold', marginBottom: '0.5rem' }}>
                        {analytics.totalCustomers.toLocaleString()}
                    </div>
                    <div style={{ fontSize: '0.9rem', opacity: 0.9 }}>
                        Active buyers
                    </div>
                </div>
            </div>

            {/* Sales Chart */}
            <div style={{ marginBottom: '3rem' }}>
                <h3 style={{ marginBottom: '1.5rem' }}>Daily Sales Overview</h3>
                <div style={{
                    background: 'white',
                    border: '1px solid #e1e5e9',
                    borderRadius: '8px',
                    padding: '1.5rem'
                }}>
                    <div style={{ overflowX: 'auto' }}>
                        <div style={{ display: 'flex', alignItems: 'end', gap: '1rem', minWidth: '600px', height: '300px' }}>
                            {salesData.map((day) => {
                                const maxRevenue = Math.max(...salesData.map(d => d.revenue));
                                const height = (day.revenue / maxRevenue) * 250;
                                
                                return (
                                    <div key={day.date} style={{ 
                                        display: 'flex', 
                                        flexDirection: 'column', 
                                        alignItems: 'center',
                                        flex: 1
                                    }}>
                                        <div style={{
                                            background: 'linear-gradient(to top, #667eea, #764ba2)',
                                            width: '40px',
                                            height: `${height}px`,
                                            borderRadius: '4px 4px 0 0',
                                            marginBottom: '0.5rem',
                                            position: 'relative',
                                            cursor: 'pointer'
                                        }} title={`${formatCurrency(day.revenue)} - ${day.orders} orders`}>
                                        </div>
                                        <div style={{ 
                                            fontSize: '0.8rem', 
                                            color: '#6c757d',
                                            textAlign: 'center',
                                            transform: 'rotate(-45deg)',
                                            whiteSpace: 'nowrap'
                                        }}>
                                            {new Date(day.date).toLocaleDateString('en-US', { 
                                                month: 'short', 
                                                day: 'numeric' 
                                            })}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    </div>
                    <div style={{ 
                        marginTop: '1rem', 
                        padding: '1rem', 
                        background: '#f8f9fa', 
                        borderRadius: '4px',
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center'
                    }}>
                        <div style={{ fontSize: '0.9rem', color: '#6c757d' }}>
                            Hover over bars to see details
                        </div>
                        <div style={{ display: 'flex', gap: '2rem' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                                <div style={{ 
                                    width: '12px', 
                                    height: '12px', 
                                    background: 'linear-gradient(45deg, #667eea, #764ba2)',
                                    borderRadius: '2px'
                                }}></div>
                                <span style={{ fontSize: '0.9rem' }}>Daily Revenue</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Product Performance */}
            <div style={{ marginBottom: '3rem' }}>
                <h3 style={{ marginBottom: '1.5rem' }}>Product Performance</h3>
                <div style={{
                    background: 'white',
                    border: '1px solid #e1e5e9',
                    borderRadius: '8px',
                    overflow: 'hidden'
                }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                        <thead>
                            <tr style={{ background: '#f8f9fa' }}>
                                <th style={{ padding: '1rem', textAlign: 'left', borderBottom: '1px solid #dee2e6' }}>
                                    Product
                                </th>
                                <th style={{ padding: '1rem', textAlign: 'right', borderBottom: '1px solid #dee2e6' }}>
                                    Units Sold
                                </th>
                                <th style={{ padding: '1rem', textAlign: 'right', borderBottom: '1px solid #dee2e6' }}>
                                    Revenue
                                </th>
                                <th style={{ padding: '1rem', textAlign: 'right', borderBottom: '1px solid #dee2e6' }}>
                                    Orders
                                </th>
                                <th style={{ padding: '1rem', textAlign: 'right', borderBottom: '1px solid #dee2e6' }}>
                                    Avg. Order Value
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            {productPerformance.map((product, index) => (
                                <tr key={product.productName} style={{ 
                                    borderBottom: index < productPerformance.length - 1 ? '1px solid #dee2e6' : 'none'
                                }}>
                                    <td style={{ padding: '1rem' }}>
                                        <div style={{ fontWeight: '600' }}>{product.productName}</div>
                                    </td>
                                    <td style={{ padding: '1rem', textAlign: 'right' }}>
                                        {product.sales.toLocaleString()}
                                    </td>
                                    <td style={{ padding: '1rem', textAlign: 'right', fontWeight: '600' }}>
                                        {formatCurrency(product.revenue)}
                                    </td>
                                    <td style={{ padding: '1rem', textAlign: 'right' }}>
                                        {product.orders.toLocaleString()}
                                    </td>
                                    <td style={{ padding: '1rem', textAlign: 'right' }}>
                                        {formatCurrency(product.revenue / product.orders)}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Additional Insights */}
            <div>
                <h3 style={{ marginBottom: '1.5rem' }}>Business Insights</h3>
                <div style={{ 
                    display: 'grid', 
                    gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', 
                    gap: '1.5rem'
                }}>
                    <div style={{
                        padding: '1.5rem',
                        background: 'white',
                        border: '1px solid #e1e5e9',
                        borderRadius: '8px'
                    }}>
                        <h4 style={{ marginTop: 0, color: '#28a745' }}>📈 Top Performing Product</h4>
                        <div style={{ marginBottom: '1rem' }}>
                            <strong>{productPerformance[0]?.productName}</strong>
                        </div>
                        <div style={{ color: '#6c757d', fontSize: '0.9rem' }}>
                            Generated {formatCurrency(productPerformance[0]?.revenue || 0)} in revenue 
                            with {productPerformance[0]?.sales || 0} units sold.
                        </div>
                    </div>

                    <div style={{
                        padding: '1.5rem',
                        background: 'white',
                        border: '1px solid #e1e5e9',
                        borderRadius: '8px'
                    }}>
                        <h4 style={{ marginTop: 0, color: '#17a2b8' }}>🎯 Conversion Rate</h4>
                        <div style={{ marginBottom: '1rem' }}>
                            <strong>85.2%</strong>
                        </div>
                        <div style={{ color: '#6c757d', fontSize: '0.9rem' }}>
                            Percentage of visitors who made a purchase. Above industry average!
                        </div>
                    </div>

                    <div style={{
                        padding: '1.5rem',
                        background: 'white',
                        border: '1px solid #e1e5e9',
                        borderRadius: '8px'
                    }}>
                        <h4 style={{ marginTop: 0, color: '#ffc107' }}>⏱️ Avg. Processing Time</h4>
                        <div style={{ marginBottom: '1rem' }}>
                            <strong>2.3 days</strong>
                        </div>
                        <div style={{ color: '#6c757d', fontSize: '0.9rem' }}>
                            Average time from order placement to shipment. Consider optimizing further.
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default AnalyticsSection;