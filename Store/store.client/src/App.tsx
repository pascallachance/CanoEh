import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CreateUser from './components/CreateUser';

function App() {
    const [showSuccessPage, setShowSuccessPage] = useState(false);

    const handleLoginSuccess = () => {
        setShowSuccessPage(true);
    };

    const handleBackToLogin = () => {
        setShowSuccessPage(false);
    };

    // Show success page after login
    if (showSuccessPage) {
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
                        <button 
                            onClick={handleBackToLogin}
                            style={{ 
                                padding: '0.5rem 1rem',
                                background: '#dc3545',
                                color: 'white',
                                border: 'none',
                                borderRadius: '4px',
                                cursor: 'pointer'
                            }}
                        >
                            Back to Login
                        </button>
                    </div>
                </header>
                <main style={{ padding: '1rem' }}>
                    <h2>Login Successful!</h2>
                    <p>You have successfully connected to CanoEh!</p>
                    <p>Your authentication has been processed successfully.</p>
                </main>
            </div>
        );
    }

    // Show login/register forms
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

export default App;