import { createContext } from 'react';
import type { NotificationContextType } from './useNotifications';

export const NotificationContext = createContext<NotificationContextType | undefined>(undefined);