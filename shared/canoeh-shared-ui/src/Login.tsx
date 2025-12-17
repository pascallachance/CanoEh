import { useState, useEffect } from 'react';
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

export interface LoginProps {
    /**
     * Title displayed at the top of the login form (e.g., "CanoEh!" or "CanoEh! Seller")
     */
    title: string;
    /**
     * Base URL for the API endpoint (e.g., from import.meta.env.VITE_API_STORE_BASE_URL)
     */
    apiBaseUrl: string;
    /**
     * Callback function called when login is successful
     */
    onLoginSuccess?: () => void;
    /**
     * Whether to enable escape key handling for accessibility (default: true)
     */
    enableEscapeKeyHandling?: boolean;
}

export function Login({ 
    title, 
    apiBaseUrl, 
    onLoginSuccess, 
    enableEscapeKeyHandling = true 
}: LoginProps) {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [showPassword, setShowPassword] = useState(false);

    // Add escape key handling for better accessibility
    useEffect(() => {
        if (!enableEscapeKeyHandling) return;
        
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && !loading) {
                // Could navigate away or clear form, for now just clear errors
                setError('');
            }
        };

        document.addEventListener('keydown', handleKeyDown);
        return () => document.removeEventListener('keydown', handleKeyDown);
    }, [loading, enableEscapeKeyHandling]);

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

        try {
            const loginRequest: LoginRequest = {
                email,
                password
            };
            const response = await fetch(`${apiBaseUrl}/api/Login/login`, {
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
                console.log('Login successful:', result);

                // Store CSRF token for future requests (stored in cookie by server)
                console.log('CSRF Token received:', result.csrfToken);

                // Notify parent component of successful login
                if (onLoginSuccess) {
                    onLoginSuccess();
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

    return (
        <div className="centered-container">
            <div className="login-container">
                <div className="login-wrapper">
                    <h1 className="login-title">{title}</h1>
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
                            <div className="password-input-wrapper">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    id="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    required
                                    minLength={8}
                                    placeholder="Enter your password (min 8 characters)"
                                    autoComplete="current-password"
                                />
                                <button
                                    type="button"
                                    className="password-toggle-btn"
                                    onClick={() => setShowPassword(!showPassword)}
                                    aria-label={showPassword ? "Hide password" : "Show password"}
                                >
                                    {showPassword ? (
                                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" width="20" height="20">
                                            <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                                            <line x1="1" y1="1" x2="23" y2="23"></line>
                                        </svg>
                                    ) : (
                                        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" width="20" height="20">
                                            <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                                            <circle cx="12" cy="12" r="3"></circle>
                                        </svg>
                                    )}
                                </button>
                            </div>
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
                            <Link to="/CreateUser" className="other-options">
                                Create account?
                            </Link>
                            <Link to="/RestorePassword">Forgot Password?</Link>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default Login;
