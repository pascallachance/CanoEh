import { useState } from 'react';
import './Login.css';

interface LoginRequest {
    username: string;
    password: string;
}

interface LoginResponse {
    token: string;
    sessionId: string;
}

interface LoginProps {
    onLoginSuccess?: () => void;
}

function Login({ onLoginSuccess }: LoginProps) {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        setSuccess(false);

        try {
            const loginRequest: LoginRequest = {
                username,
                password
            };

            const response = await fetch('/api/login/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(loginRequest),
            });

            if (response.ok) {
                const result: LoginResponse = await response.json();
                setSuccess(true);
                console.log('Login successful:', result);
                // Store the token and session ID
                localStorage.setItem('authToken', result.token);
                localStorage.setItem('sessionId', result.sessionId);
                
                // Notify parent component of successful login
                if (onLoginSuccess) {
                    setTimeout(() => onLoginSuccess(), 1500); // Small delay to show success message
                }
            } else {
                const errorText = await response.text();
                setError(errorText || 'Login failed');
            }
        } catch (err) {
            setError('Network error occurred. Please try again.');
            console.error('Login error:', err);
        } finally {
            setLoading(false);
        }
    };

    if (success) {
        return (
            <div className="login-container">
                <div className="login-form">
                    <h2>Login Successful!</h2>
                    <p>You have been successfully authenticated.</p>
                    <p>Redirecting...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="login-container">
            <form className="login-form" onSubmit={handleSubmit}>
                <h2>Login</h2>
                
                {error && <div className="error-message">{error}</div>}
                
                <div className="form-group">
                    <label htmlFor="username">Username:</label>
                    <input
                        type="text"
                        id="username"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                        minLength={8}
                        placeholder="Enter your username (min 8 characters)"
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="password">Password:</label>
                    <input
                        type="password"
                        id="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        minLength={8}
                        placeholder="Enter your password (min 8 characters)"
                    />
                </div>

                <button
                    type="submit"
                    className="connect-button"
                    disabled={loading}
                >
                    {loading ? 'Connecting...' : 'Connect'}
                </button>
            </form>
        </div>
    );
}

export default Login;