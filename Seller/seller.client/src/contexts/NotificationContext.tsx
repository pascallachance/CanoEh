import React, { createContext, useState } from 'react';
import type { ReactNode } from 'react';
import Toast from '../components/Toast';
import type { ToastType } from '../components/Toast';
import type { NotificationContextType } from './useNotifications';

interface NotificationItem {
    id: string;
    type: ToastType;
    message: string;
    duration?: number;
}

export const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

interface NotificationProviderProps {
    children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
    const [notifications, setNotifications] = useState<NotificationItem[]>([]);

    const generateId = () => `notification-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;

    const removeNotification = (id: string) => {
        setNotifications(prev => prev.filter(notification => notification.id !== id));
    };

    const showNotification = (type: ToastType, message: string, duration = 4000) => {
        const id = generateId();
        const notification: NotificationItem = {
            id,
            type,
            message,
            duration
        };

        setNotifications(prev => [...prev, notification]);
    };

    const showSuccess = (message: string, duration?: number) => {
        showNotification('success', message, duration);
    };

    const showError = (message: string, duration?: number) => {
        showNotification('error', message, duration);
    };

    const showWarning = (message: string, duration?: number) => {
        showNotification('warning', message, duration);
    };

    const showInfo = (message: string, duration?: number) => {
        showNotification('info', message, duration);
    };

    const contextValue: NotificationContextType = {
        showNotification,
        showSuccess,
        showError,
        showWarning,
        showInfo
    };

    return (
        <NotificationContext.Provider value={contextValue}>
            {children}
            <div
                style={{
                    position: 'fixed',
                    top: '20px',
                    right: '20px',
                    zIndex: 9999,
                    pointerEvents: 'none'
                }}
            >
                <style>
                    {`
                        @keyframes slideInRight {
                            from {
                                transform: translateX(100%);
                                opacity: 0;
                            }
                            to {
                                transform: translateX(0);
                                opacity: 1;
                            }
                        }
                    `}
                </style>
                {notifications.map(notification => (
                    <div key={notification.id} style={{ pointerEvents: 'auto' }}>
                        <Toast
                            id={notification.id}
                            type={notification.type}
                            message={notification.message}
                            duration={notification.duration}
                            onClose={removeNotification}
                        />
                    </div>
                ))}
            </div>
        </NotificationContext.Provider>
    );
};