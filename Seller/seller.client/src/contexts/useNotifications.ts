import { useContext } from 'react';
import type { ToastType } from '../components/Toast';
import { NotificationContext } from './notificationContext';

export interface NotificationContextType {
    showNotification: (type: ToastType, message: string, duration?: number) => void;
    showSuccess: (message: string, duration?: number) => void;
    showError: (message: string, duration?: number) => void;
    showWarning: (message: string, duration?: number) => void;
    showInfo: (message: string, duration?: number) => void;
}

export const useNotifications = (): NotificationContextType => {
    const context = useContext(NotificationContext);
    if (!context) {
        throw new Error('useNotifications must be used within a NotificationProvider');
    }
    return context;
};