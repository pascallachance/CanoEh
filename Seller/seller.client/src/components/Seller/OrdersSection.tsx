import { useState, useEffect, useRef } from 'react';
import './OrdersSection.css';

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
    
    // Refs for accessibility
    const modalRef = useRef<HTMLDivElement>(null);
    const refundButtonRef = useRef<HTMLButtonElement>(null);
    const previousActiveElement = useRef<HTMLElement | null>(null);

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

    // Accessibility: Focus management and keyboard handling
    useEffect(() => {
        if (showRefundModal) {
            // Store the currently focused element
            previousActiveElement.current = document.activeElement as HTMLElement;
            
            // Focus the modal content
            const timer = setTimeout(() => {
                modalRef.current?.focus();
            }, 100);

            // Prevent body scroll when modal is open
            document.body.style.overflow = 'hidden';

            return () => {
                clearTimeout(timer);
                document.body.style.overflow = '';
            };
        } else if (previousActiveElement.current) {
            // Return focus to the element that opened the modal
            previousActiveElement.current.focus();
            previousActiveElement.current = null;
        }
    }, [showRefundModal]);

    // Handle escape key
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && showRefundModal) {
                setShowRefundModal(false);
                setSelectedOrder(null);
            }
        };

        if (showRefundModal) {
            document.addEventListener('keydown', handleEscape);
            return () => document.removeEventListener('keydown', handleEscape);
        }
    }, [showRefundModal]);

    // Focus trapping within modal
    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (!showRefundModal || !modalRef.current) return;

        if (event.key === 'Tab') {
            const focusableElements = modalRef.current.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            );
            const firstElement = focusableElements[0] as HTMLElement;
            const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

            if (event.shiftKey) {
                // Shift + Tab
                if (document.activeElement === firstElement) {
                    event.preventDefault();
                    lastElement?.focus();
                }
            } else {
                // Tab
                if (document.activeElement === lastElement) {
                    event.preventDefault();
                    firstElement?.focus();
                }
            }
        }
    };

    const closeModal = () => {
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

            <div className="orders-filter-container">
                <label className="orders-filter-label">Filter by Status:</label>
                <select 
                    value={selectedStatus}
                    onChange={(e) => setSelectedStatus(e.target.value as OrderStatus | 'all')}
                    className="orders-filter-select"
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

            <div className="orders-section">
                <h3>Orders ({filteredOrders.length})</h3>
                {filteredOrders.length === 0 ? (
                    <p className="orders-empty">
                        No orders found for the selected status.
                    </p>
                ) : (
                    <div className="orders-table-container">
                        <table className="orders-table">
                            <thead>
                                <tr>
                                    <th>Order #</th>
                                    <th>Customer</th>
                                    <th>Items</th>
                                    <th>Total</th>
                                    <th>Status</th>
                                    <th>Date</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                {filteredOrders.map(order => (
                                    <tr key={order.id}>
                                        <td>
                                            <strong>{order.orderNumber}</strong>
                                        </td>
                                        <td>
                                            <div className="orders-customer-info">
                                                <div className="orders-customer-name">{order.customerName}</div>
                                                <div className="orders-customer-email">{order.customerEmail}</div>
                                            </div>
                                        </td>
                                        <td>
                                            {order.items.map(item => (
                                                <div key={item.id} className="orders-item">
                                                    <strong>{item.productName}</strong> ({item.variant}) × {item.quantity}
                                                </div>
                                            ))}
                                        </td>
                                        <td>
                                            <strong className="orders-total">${order.totalAmount.toFixed(2)}</strong>
                                        </td>
                                        <td>
                                            <span 
                                                className="orders-status-badge"
                                                style={{ background: getStatusColor(order.status) }}
                                            >
                                                {order.status}
                                            </span>
                                        </td>
                                        <td>
                                            {new Date(order.createdAt).toLocaleDateString()}
                                        </td>
                                        <td>
                                            <div className="orders-actions">
                                                <select
                                                    value={order.status}
                                                    onChange={(e) => updateOrderStatus(order.id, e.target.value as OrderStatus)}
                                                    className="orders-status-select"
                                                >
                                                    <option value="pending">Pending</option>
                                                    <option value="processing">Processing</option>
                                                    <option value="shipped">Shipped</option>
                                                    <option value="delivered">Delivered</option>
                                                    <option value="cancelled">Cancelled</option>
                                                </select>
                                                {order.status !== 'refunded' && order.status !== 'cancelled' && (
                                                    <button
                                                        ref={refundButtonRef}
                                                        onClick={() => {
                                                            setSelectedOrder(order);
                                                            setShowRefundModal(true);
                                                        }}
                                                        className="orders-refund-button"
                                                        aria-label={`Process refund for order ${order.orderNumber}`}
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
                <div 
                    className="orders-modal-overlay"
                    onClick={(e) => {
                        // Close modal when clicking on overlay, but not on modal content
                        if (e.target === e.currentTarget) {
                            closeModal();
                        }
                    }}
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="modal-title"
                    aria-describedby="modal-description"
                >
                    <div 
                        className="orders-modal-content"
                        ref={modalRef}
                        tabIndex={-1}
                        onKeyDown={handleKeyDown}
                    >
                        <h3 id="modal-title">Process Refund</h3>
                        <div className="orders-modal-info" id="modal-description">
                            <p><strong>Order:</strong> {selectedOrder.orderNumber}</p>
                            <p><strong>Customer:</strong> {selectedOrder.customerName}</p>
                            <p><strong>Total Amount:</strong> ${selectedOrder.totalAmount.toFixed(2)}</p>
                        </div>

                        <div className="orders-modal-items">
                            <h4>Items to Refund:</h4>
                            {selectedOrder.items.map(item => (
                                <div key={item.id} className="orders-modal-item">
                                    <div className="orders-modal-item-header">
                                        <span>{item.productName} ({item.variant})</span>
                                        <span>${item.totalPrice.toFixed(2)}</span>
                                    </div>
                                    <div className="orders-modal-item-details">
                                        Quantity: {item.quantity} × ${item.unitPrice.toFixed(2)}
                                    </div>
                                </div>
                            ))}
                        </div>

                        <div className="orders-modal-warning">
                            <p>
                                <strong>Warning:</strong> This action will process a full refund for this order and change its status to "Refunded". This action cannot be undone.
                            </p>
                        </div>

                        <div className="orders-modal-actions">
                            <button
                                onClick={closeModal}
                                className="orders-modal-button orders-modal-button--cancel"
                                aria-label="Cancel refund process"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={() => processRefund(selectedOrder.id)}
                                className="orders-modal-button orders-modal-button--refund"
                                aria-label={`Confirm refund for order ${selectedOrder.orderNumber}`}
                            >
                                Process Refund
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Order Summary Stats */}
            <div className="orders-stats-grid">
                {(['pending', 'processing', 'shipped', 'delivered', 'cancelled', 'refunded'] as OrderStatus[]).map(status => {
                    const count = orders.filter(order => order.status === status).length;
                    const total = orders
                        .filter(order => order.status === status)
                        .reduce((sum, order) => sum + order.totalAmount, 0);
                    
                    return (
                        <div key={status} className="orders-stat-card">
                            <div 
                                className="orders-stat-count"
                                style={{ color: getStatusColor(status) }}
                            >
                                {count}
                            </div>
                            <div className="orders-stat-label">
                                {status} Orders
                            </div>
                            <div className="orders-stat-total">
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