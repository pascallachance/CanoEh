import { useState } from 'react';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

interface OrdersSectionProps {
    companies: Company[];
}

interface Order {
    id: string;
    orderNumber: string;
    customerName: string;
    customerEmail: string;
    items: OrderItem[];
    totalAmount: number;
    status: OrderStatus;
    createdAt: string;
    updatedAt?: string;
}

interface OrderItem {
    id: string;
    productName: string;
    variant: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
}

type OrderStatus = 'pending' | 'processing' | 'shipped' | 'delivered' | 'cancelled' | 'refunded';

const mockOrders: Order[] = [
    {
        id: '1',
        orderNumber: 'ORD-001',
        customerName: 'John Smith',
        customerEmail: 'john.smith@email.com',
        items: [
            {
                id: '1',
                productName: 'T-Shirt',
                variant: 'Red, Large',
                quantity: 2,
                unitPrice: 25.99,
                totalPrice: 51.98
            }
        ],
        totalAmount: 51.98,
        status: 'pending',
        createdAt: '2024-01-15T10:30:00Z'
    },
    {
        id: '2',
        orderNumber: 'ORD-002',
        customerName: 'Jane Doe',
        customerEmail: 'jane.doe@email.com',
        items: [
            {
                id: '2',
                productName: 'Sneakers',
                variant: 'Blue, Size 9',
                quantity: 1,
                unitPrice: 89.99,
                totalPrice: 89.99
            }
        ],
        totalAmount: 89.99,
        status: 'processing',
        createdAt: '2024-01-14T14:22:00Z'
    },
    {
        id: '3',
        orderNumber: 'ORD-003',
        customerName: 'Bob Johnson',
        customerEmail: 'bob.johnson@email.com',
        items: [
            {
                id: '3',
                productName: 'Jacket',
                variant: 'Black, Medium',
                quantity: 1,
                unitPrice: 129.99,
                totalPrice: 129.99
            }
        ],
        totalAmount: 129.99,
        status: 'shipped',
        createdAt: '2024-01-13T09:15:00Z'
    }
];

function OrdersSection(_props: OrdersSectionProps) {
    const [orders, setOrders] = useState<Order[]>(mockOrders);
    const [selectedStatus, setSelectedStatus] = useState<OrderStatus | 'all'>('all');
    const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);
    const [showRefundModal, setShowRefundModal] = useState(false);

    const getStatusColor = (status: OrderStatus): string => {
        switch (status) {
            case 'pending': return '#ffc107';
            case 'processing': return '#17a2b8';
            case 'shipped': return '#007bff';
            case 'delivered': return '#28a745';
            case 'cancelled': return '#6c757d';
            case 'refunded': return '#dc3545';
            default: return '#6c757d';
        }
    };

    const updateOrderStatus = (orderId: string, newStatus: OrderStatus) => {
        setOrders(prev => prev.map(order => 
            order.id === orderId 
                ? { ...order, status: newStatus, updatedAt: new Date().toISOString() }
                : order
        ));
    };

    const processRefund = (orderId: string) => {
        updateOrderStatus(orderId, 'refunded');
        setShowRefundModal(false);
        setSelectedOrder(null);
    };

    const filteredOrders = selectedStatus === 'all' 
        ? orders 
        : orders.filter(order => order.status === selectedStatus);

    return (
        <div className="section-container">
            <h2 className="section-title">Orders Management</h2>
            <p className="section-description">
                View and manage all orders for your company. Update order statuses, track fulfillment, and process refunds.
            </p>

            <div style={{ marginBottom: '2rem', display: 'flex', gap: '1rem', alignItems: 'center' }}>
                <label style={{ fontWeight: '600' }}>Filter by Status:</label>
                <select 
                    value={selectedStatus}
                    onChange={(e) => setSelectedStatus(e.target.value as OrderStatus | 'all')}
                    style={{
                        padding: '0.5rem',
                        border: '1px solid #ced4da',
                        borderRadius: '4px',
                        fontSize: '1rem'
                    }}
                >
                    <option value="all">All Orders</option>
                    <option value="pending">Pending</option>
                    <option value="processing">Processing</option>
                    <option value="shipped">Shipped</option>
                    <option value="delivered">Delivered</option>
                    <option value="cancelled">Cancelled</option>
                    <option value="refunded">Refunded</option>
                </select>
            </div>

            <div style={{ marginBottom: '2rem' }}>
                <h3>Orders ({filteredOrders.length})</h3>
                {filteredOrders.length === 0 ? (
                    <p style={{ color: '#6c757d', fontStyle: 'italic' }}>
                        No orders found for the selected status.
                    </p>
                ) : (
                    <div style={{ overflowX: 'auto' }}>
                        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                            <thead>
                                <tr style={{ background: '#f8f9fa' }}>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Order #</th>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Customer</th>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Items</th>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Total</th>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Status</th>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Date</th>
                                    <th style={{ padding: '1rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredOrders.map(order => (
                                    <tr key={order.id} style={{ borderBottom: '1px solid #dee2e6' }}>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            <strong>{order.orderNumber}</strong>
                                        </td>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            <div>
                                                <div style={{ fontWeight: '600' }}>{order.customerName}</div>
                                                <div style={{ fontSize: '0.9rem', color: '#6c757d' }}>{order.customerEmail}</div>
                                            </div>
                                        </td>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            {order.items.map(item => (
                                                <div key={item.id} style={{ marginBottom: '0.25rem' }}>
                                                    <strong>{item.productName}</strong> ({item.variant}) × {item.quantity}
                                                </div>
                                            ))}
                                        </td>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            <strong>${order.totalAmount.toFixed(2)}</strong>
                                        </td>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            <span style={{
                                                padding: '0.25rem 0.75rem',
                                                borderRadius: '12px',
                                                fontSize: '0.85rem',
                                                fontWeight: '600',
                                                color: 'white',
                                                background: getStatusColor(order.status),
                                                textTransform: 'capitalize'
                                            }}>
                                                {order.status}
                                            </span>
                                        </td>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            {new Date(order.createdAt).toLocaleDateString()}
                                        </td>
                                        <td style={{ padding: '1rem', border: '1px solid #dee2e6' }}>
                                            <div style={{ display: 'flex', gap: '0.5rem', flexDirection: 'column' }}>
                                                <select
                                                    value={order.status}
                                                    onChange={(e) => updateOrderStatus(order.id, e.target.value as OrderStatus)}
                                                    style={{
                                                        padding: '0.25rem 0.5rem',
                                                        border: '1px solid #ced4da',
                                                        borderRadius: '4px',
                                                        fontSize: '0.85rem'
                                                    }}
                                                >
                                                    <option value="pending">Pending</option>
                                                    <option value="processing">Processing</option>
                                                    <option value="shipped">Shipped</option>
                                                    <option value="delivered">Delivered</option>
                                                    <option value="cancelled">Cancelled</option>
                                                </select>
                                                {order.status !== 'refunded' && order.status !== 'cancelled' && (
                                                    <button
                                                        onClick={() => {
                                                            setSelectedOrder(order);
                                                            setShowRefundModal(true);
                                                        }}
                                                        style={{
                                                            padding: '0.25rem 0.5rem',
                                                            background: '#dc3545',
                                                            color: 'white',
                                                            border: 'none',
                                                            borderRadius: '4px',
                                                            cursor: 'pointer',
                                                            fontSize: '0.8rem'
                                                        }}
                                                    >
                                                        Refund
                                                    </button>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>

            {/* Refund Modal */}
            {showRefundModal && selectedOrder && (
                <div style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
                    background: 'rgba(0, 0, 0, 0.5)',
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    zIndex: 1000
                }}>
                    <div style={{
                        background: 'white',
                        padding: '2rem',
                        borderRadius: '8px',
                        maxWidth: '500px',
                        width: '90%',
                        maxHeight: '80vh',
                        overflow: 'auto'
                    }}>
                        <h3>Process Refund</h3>
                        <div style={{ marginBottom: '1.5rem' }}>
                            <p><strong>Order:</strong> {selectedOrder.orderNumber}</p>
                            <p><strong>Customer:</strong> {selectedOrder.customerName}</p>
                            <p><strong>Total Amount:</strong> ${selectedOrder.totalAmount.toFixed(2)}</p>
                        </div>

                        <div style={{ marginBottom: '1.5rem' }}>
                            <h4>Items to Refund:</h4>
                            {selectedOrder.items.map(item => (
                                <div key={item.id} style={{
                                    padding: '0.75rem',
                                    border: '1px solid #e1e5e9',
                                    borderRadius: '4px',
                                    marginBottom: '0.5rem'
                                }}>
                                    <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                                        <span>{item.productName} ({item.variant})</span>
                                        <span>${item.totalPrice.toFixed(2)}</span>
                                    </div>
                                    <div style={{ fontSize: '0.9rem', color: '#6c757d' }}>
                                        Quantity: {item.quantity} × ${item.unitPrice.toFixed(2)}
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div style={{ 
                            padding: '1rem', 
                            background: '#f8f9fa', 
                            borderRadius: '4px', 
                            marginBottom: '1.5rem',
                            border: '1px solid #e1e5e9'
                        }}>
                            <p style={{ margin: 0, color: '#495057' }}>
                                <strong>Warning:</strong> This action will process a full refund for this order and change its status to "Refunded". This action cannot be undone.
                            </p>
                        </div>

                        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
                            <button
                                onClick={() => {
                                    setShowRefundModal(false);
                                    setSelectedOrder(null);
                                }}
                                style={{
                                    padding: '0.75rem 1.5rem',
                                    background: '#6c757d',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: 'pointer'
                                }}
                            >
                                Cancel
                            </button>
                            <button
                                onClick={() => processRefund(selectedOrder.id)}
                                style={{
                                    padding: '0.75rem 1.5rem',
                                    background: '#dc3545',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: 'pointer'
                                }}
                            >
                                Process Refund
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Order Summary Stats */}
            <div style={{ 
                display: 'grid', 
                gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', 
                gap: '1rem',
                marginTop: '2rem'
            }}>
                {(['pending', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded'] as OrderStatus[]).map(status => {
                    const count = orders.filter(order => order.status === status).length;
                    const total = orders
                        .filter(order => order.status === status)
                        .reduce((sum, order) => sum + order.totalAmount, 0);
                    
                    return (
                        <div key={status} style={{
                            padding: '1rem',
                            background: 'white',
                            border: '1px solid #e1e5e9',
                            borderRadius: '8px',
                            textAlign: 'center'
                        }}>
                            <div style={{ 
                                fontSize: '2rem', 
                                fontWeight: 'bold',
                                color: getStatusColor(status),
                                marginBottom: '0.5rem'
                            }}>
                                {count}
                            </div>
                            <div style={{ 
                                textTransform: 'capitalize', 
                                fontWeight: '600',
                                marginBottom: '0.25rem'
                            }}>
                                {status} Orders
                            </div>
                            <div style={{ fontSize: '0.9rem', color: '#6c757d' }}>
                                ${total.toFixed(2)} total
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

export default OrdersSection;