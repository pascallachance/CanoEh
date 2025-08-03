import { useState } from 'react';
import { Link } from 'react-router-dom';
import './Login.css';

interface LoginRequest {
    email: string;
    password: string;
}

interface LoginResponse {
    message: string;
    sessionId: string;
    csrfToken: string;
}

interface LoginProps {
    onLoginSuccess?: () => void;
}

function Login({ onLoginSuccess }: LoginProps) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    const getCsrfToken = (): string => {
        // Get CSRF token from cookie for API calls
        const cookies = document.cookie.split(';');
        for (const cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'X-CSRF-Token') {
                return value;
            }
        }
        return '';
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        setSuccess(false);

        try {
            const loginRequest: LoginRequest = {
                email,
                password
            };
            const response = await fetch('https://localhost:7182/api/Login/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Token': getCsrfToken(), // Include CSRF token if available
                },
                credentials: 'include', // Important: Include cookies in request
                body: JSON.stringify(loginRequest),
            });

            if (response.ok) {
                const result: LoginResponse = await response.json();
                setSuccess(true);
                console.log('Login successful:', result);

                // Store CSRF token for future requests (stored in cookie by server)
                console.log('CSRF Token received:', result.csrfToken);

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
                    <p>Your session is now secured with HTTP-only cookies.</p>
                    <p>Redirecting...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="login-container">
            <div style={{ width: "100%" }}>
                <h1 className="login-title">CanoEh!</h1>
                <form className="login-form" onSubmit={handleSubmit}>
                    <h2>Sign in or create account</h2>
                    <div className="form-group">
                        <label htmlFor="email">Email:</label>
                        <input
                            type="email"
                            id="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            placeholder="Enter your email"
                            autoComplete="email"
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
                            autoComplete="current-password"
                        />
                    </div>

                    {error && <div className="error-message">{error}</div>}

                    <button
                        type="submit"
                        className="connect-button"
                        disabled={loading}
                    >
                        {loading ? 'Connecting...' : 'Connect'}
                    </button>
                    <div className="separator"></div>
                    <div className="other-options">
                        <a href="/CreateUser" className="other-options">
                            Create account?
                        </a>
                        <a href="/RestorePassword">Forgot Password?</a>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default Login;