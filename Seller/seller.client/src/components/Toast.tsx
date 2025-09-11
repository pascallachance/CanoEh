import { useEffect } from 'react';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

interface ToastProps {
    id: string;
    type: ToastType;
    message: string;
    duration?: number;
    onClose: (id: string) => void;
}

const Toast = ({ id, type, message, duration = 4000, onClose }: ToastProps) => {
    useEffect(() => {
        const timer = setTimeout(() => {
            onClose(id);
        }, duration);

        return () => clearTimeout(timer);
    }, [id, duration, onClose]);

    const getToastStyles = () => {
        const baseStyles = {
            padding: '1rem 1.5rem',
            marginBottom: '0.5rem',
            borderRadius: '6px',
            boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            fontSize: '0.9rem',
            fontWeight: '500',
            minWidth: '300px',
            maxWidth: '500px',
            cursor: 'pointer'
        };

        const typeStyles = {
            success: {
                backgroundColor: '#d4edda',
                color: '#155724',
                border: '1px solid #c3e6cb'
            },
            error: {
                backgroundColor: '#f8d7da',
                color: '#721c24',
                border: '1px solid #f5c6cb'
            },
            warning: {
                backgroundColor: '#fff3cd',
                color: '#856404',
                border: '1px solid #ffeaa7'
            },
            info: {
                backgroundColor: '#d1ecf1',
                color: '#0c5460',
                border: '1px solid #bee5eb'
            }
        };

        return { ...baseStyles, ...typeStyles[type] };
    };

    return (
        <div
            className="toast-slide-in"
            style={getToastStyles()}
            onClick={() => onClose(id)}
            role="alert"
            aria-live="polite"
        >
            <span>{message}</span>
            <button
                onClick={(e) => {
                    e.stopPropagation();
                    onClose(id);
                }}
                style={{
                    background: 'none',
                    border: 'none',
                    fontSize: '1.2rem',
                    cursor: 'pointer',
                    marginLeft: '1rem',
                    opacity: 0.7,
                    color: 'inherit'
                }}
                aria-label="Close notification"
            >
                ×
            </button>
        </div>
    );
};

export default Toast;