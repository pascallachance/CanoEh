import { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CreateUser from './components/CreateUser';

interface AuthStatus {
    isAuthenticated: boolean;
    username?: string;
}

function App() {
    const [authStatus, setAuthStatus] = useState<AuthStatus>({ isAuthenticated: false });
    const [authLoading, setAuthLoading] = useState(true);

    useEffect(() => {
        checkAuthStatus();
    }, []);

    useEffect(() => {
        if (authStatus.isAuthenticated) {
            displaywelcome();
        }
    }, [authStatus.isAuthenticated]);

    const checkAuthStatus = async () => {
        try {
            const response = await fetch('/api/store/demologin/status', {
                credentials: 'include', // Include cookies
            });
            
            if (response.ok) {
                const data: AuthStatus = await response.json();
                setAuthStatus(data);
            } else {
                setAuthStatus({ isAuthenticated: false });
            }
        } catch (error) {
            console.error('Error checking auth status:', error);
            setAuthStatus({ isAuthenticated: false });
        } finally {
            setAuthLoading(false);
        }
    };

    const handleLoginSuccess = () => {
        checkAuthStatus(); // Refresh auth status after successful login
    };

    const handleLogout = async () => {
        try {
            const response = await fetch('/api/store/demologin/logout', {
                method: 'POST',
                credentials: 'include',
                headers: {
                    'Content-Type': 'application/json',
                }
            });
            
            if (response.ok) {
                setAuthStatus({ isAuthenticated: false });
            }
        } catch (error) {
            console.error('Logout error:', error);
        }
    };

    async function displaywelcome() {
        try {
        } catch (error) {
            console.error('An error occurred:', error);
            alert('An error occurred. Please check your network connection and try again.');
        }
    }

    // Show loading spinner while checking authentication
    if (authLoading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                <p>Loading...</p>
            </div>
        );
    }

    // Show login form if not authenticated
    if (!authStatus.isAuthenticated) {
        return (
            <Router>
                <Routes>
                    <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
                    <Route path="/CreateUser" element={<CreateUser onCreateSuccess={() => {/* Navigate to login after creation */}} />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />
                </Routes>
            </Router>
        );
    }

    return (
        <div>
            <header style={{ 
                display: 'flex', 
                justifyContent: 'space-between', 
                alignItems: 'center', 
                padding: '1rem',
                borderBottom: '1px solid #eee'
            }}>
                <h1 id="tableLabel">CanoEh!</h1>
                <div>
                    <span>Welcome, {authStatus.username || 'User'}!</span>
                    <button 
                        onClick={handleLogout}
                        style={{ 
                            marginLeft: '1rem',
                            padding: '0.5rem 1rem',
                            background: '#dc3545',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer'
                        }}
                    >
                        Logout
                    </button>
                </div>
            </header>
            <main style={{ padding: '1rem' }}>
                <p>You have sucessfully connected</p>
            </main>
        </div>
    );
}

export default App;