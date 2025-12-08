import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './ForgotPassword.css';

interface ForgotPasswordRequest {
    email: string;
}

interface ForgotPasswordResponse {
    email: string;
    message: string;
}

export interface ForgotPasswordProps {
    /**
     * Title displayed at the top of the forgot password form (e.g., "CanoEh!" or "CanoEh! Seller")
     */
    title: string;
    /**
     * Base URL for the API endpoint (e.g., from import.meta.env.VITE_API_STORE_BASE_URL)
     */
    apiBaseUrl: string;
    /**
     * Callback function called when the form is successfully submitted
     */
    onSubmitSuccess?: () => void;
    /**
     * Whether to enable escape key handling for accessibility (default: true)
     */
    enableEscapeKeyHandling?: boolean;
}

export function ForgotPassword({ 
    title, 
    apiBaseUrl, 
    onSubmitSuccess, 
    enableEscapeKeyHandling = true 
}: ForgotPasswordProps) {
    const [email, setEmail] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    // Add escape key handling for better accessibility
    useEffect(() => {
        if (!enableEscapeKeyHandling) return;
        
        const handleKeyDown = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && !loading) {
                // Clear errors on escape
                setError('');
            }
        };

        document.addEventListener('keydown', handleKeyDown);
        return () => document.removeEventListener('keydown', handleKeyDown);
    }, [loading, enableEscapeKeyHandling]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError('');
        setSuccess(false);

        try {
            const forgotPasswordRequest: ForgotPasswordRequest = {
                email
            };
            const response = await fetch(`${apiBaseUrl}/api/PasswordReset/ForgotPassword`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                credentials: 'include', // Include cookies if needed
                body: JSON.stringify(forgotPasswordRequest),
            });

            if (response.ok) {
                const result: ForgotPasswordResponse = await response.json();
                setSuccess(true);
                console.log('Forgot password request successful:', result);

                // Notify parent component of successful submission
                if (onSubmitSuccess) {
                    setTimeout(() => onSubmitSuccess(), 3000); // Small delay to show success message
                }
            } else {
                const errorText = await response.text();
                setError(errorText || 'Failed to process request');
            }
        } catch (err) {
            setError('Network error occurred. Please try again.');
            console.error('Forgot password error:', err);
        } finally {
            setLoading(false);
        }
    };

    if (success) {
        return (
            <div className="centered-container">
                <div className="forgotpassword-container">
                    <div className="forgotpassword-form">
                        <h2>Request Submitted!</h2>
                        <p>If the email address exists in our system, you will receive a password reset link shortly.</p>
                        <p>Please check your email inbox (and spam folder).</p>
                        <div className="login-link">
                            <Link to="/login" className="back-to-login-button">
                                Back to Login
                            </Link>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="centered-container">
            <div className="forgotpassword-container">
                <div className="forgotpassword-wrapper">
                    <h1 className="forgotpassword-title">{title}</h1>
                    <form className="forgotpassword-form" onSubmit={handleSubmit}>
                        <h2>Reset Password</h2>
                        <p className="instruction-text">
                            Enter your email address and we'll send you a link to reset your password.
                        </p>
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

                        {error && <div className="error-message">{error}</div>}

                        <button
                            type="submit"
                            className="submit-button"
                            disabled={loading}
                        >
                            {loading ? 'Sending...' : 'Send Reset Link'}
                        </button>
                        <div className="login-link">
                            <Link to="/login">
                                Back to Login
                            </Link>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}

export default ForgotPassword;
