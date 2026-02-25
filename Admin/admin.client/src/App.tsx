import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import Admin from './components/Admin/Admin';
import { ApiClient } from './utils/apiClient';
import { NotificationProvider } from './contexts/NotificationContext';
import { LanguageProvider } from './contexts/LanguageContext';

function AppContent() {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isCheckingSession, setIsCheckingSession] = useState(true);

    const navigate = useNavigate();
    const location = useLocation();

    const checkExistingSession = async () => {
        try {
            setIsCheckingSession(true);
            const response = await ApiClient.post(`${import.meta.env.VITE_API_ADMIN_BASE_URL}/api/Login/refresh`);

            if (response.ok) {
                setIsAuthenticated(true);
                if (location.pathname === '/login' || location.pathname === '/') {
                    navigate('/admin', { replace: true });
                }
            } else {
                setIsAuthenticated(false);
                if (location.pathname !== '/login') {
                    navigate('/login', { replace: true });
                }
            }
        } catch {
            setIsAuthenticated(false);
            if (location.pathname !== '/login') {
                navigate('/login', { replace: true });
            }
        } finally {
            setIsCheckingSession(false);
        }
    };

    useEffect(() => {
        checkExistingSession();
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleLoginSuccess = async () => {
        setIsAuthenticated(true);
        navigate('/admin', { replace: true });
    };

    const handleLogout = () => {
        setIsAuthenticated(false);
        navigate('/login', { replace: true });
    };

    const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
        if (isCheckingSession) {
            return (
                <div style={{
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    height: '100vh',
                    flexDirection: 'column'
                }}>
                    <h2>CanoEh! Admin</h2>
                    <p>Checking your session...</p>
                    <div className="spinner"></div>
                </div>
            );
        }

        if (!isAuthenticated) {
            return <Navigate to="/login" replace />;
        }

        return <>{children}</>;
    };

    return (
        <Routes>
            <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
            <Route path="/admin" element={
                <ProtectedRoute>
                    <Admin onLogout={handleLogout} />
                </ProtectedRoute>
            } />
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
    );
}

function App() {
    return (
        <Router>
            <LanguageProvider>
                <NotificationProvider>
                    <AppContent />
                </NotificationProvider>
            </LanguageProvider>
        </Router>
    );
}

export default App;
