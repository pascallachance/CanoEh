import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, useNavigate } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CreateUser from './components/CreateUser';
import ForgotPassword from './components/ForgotPassword';
import Home from './components/Home';
import Cart from './components/Cart';

function AppContent() {
    const navigate = useNavigate();
    const [isAuthenticated, setIsAuthenticated] = useState(false);

    // Check for authentication on mount
    useEffect(() => {
        const checkAuth = () => {
            // Check if AuthToken cookie exists and has a value
            const cookies = document.cookie.split(';');
            const hasAuthToken = cookies.some(cookie => {
                const trimmed = cookie.trim();
                if (trimmed.startsWith('AuthToken=')) {
                    const value = trimmed.substring('AuthToken='.length);
                    return value.length > 0;
                }
                return false;
            });
            setIsAuthenticated(hasAuthToken);
        };
        checkAuth();
    }, []);

    const handleLoginSuccess = () => {
        // Navigate to home page after successful login
        console.log('Login successful - user authenticated');
        setIsAuthenticated(true);
        navigate('/');
    };

    const handleLogout = async () => {
        try {
            // Call logout API
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            const response = await fetch(`${apiBaseUrl}/api/Login/logout`, {
                method: 'POST',
                credentials: 'include',
            });

            if (response.ok) {
                console.log('Logout successful');
            } else {
                console.error('Logout failed:', await response.text());
            }
        } catch (error) {
            console.error('Logout error:', error);
        } finally {
            // Clear authentication state regardless of API response
            setIsAuthenticated(false);
        }
    };

    const handleCreateUserSuccess = () => {
        // Navigate to login page after successful user creation
        console.log('User created successfully');
        navigate('/login');
    };

    // Show login/register forms
    return (
        <Routes>
            <Route path="/" element={<Home isAuthenticated={isAuthenticated} onLogout={handleLogout} />} />
            <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
            <Route path="/CreateUser" element={<CreateUser onCreateSuccess={handleCreateUserSuccess} />} />
            <Route path="/RestorePassword" element={<ForgotPassword />} />
            <Route path="/cart" element={<Cart />} />
            <Route path="*" element={<Home isAuthenticated={isAuthenticated} onLogout={handleLogout} />} />
        </Routes>
    );
}

function App() {
    return (
        <Router>
            <AppContent />
        </Router>
    );
}

export default App;