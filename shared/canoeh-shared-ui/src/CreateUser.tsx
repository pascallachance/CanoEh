import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './CreateUser.css';

interface CreateUserRequest {
    email: string;
    firstname: string;
    lastname: string;
    phone?: string;
    password: string;
}

export interface CreateUserProps {
    /**
     * Title displayed at the top of the create user form (e.g., "CanoEh!" or "CanoEh! Seller")
     */
    title: string;
    /**
     * Base URL for the API endpoint (e.g., from import.meta.env.VITE_API_STORE_BASE_URL)
     */
    apiBaseUrl: string;
    /**
     * Callback function called when user creation is successful
     */
    onCreateSuccess?: () => void;
    /**
     * Whether to enable escape key handling for accessibility (default: true)
     */
    enableEscapeKeyHandling?: boolean;
}

export function CreateUser({ 
    title, 
    apiBaseUrl, 
    onCreateSuccess, 
    enableEscapeKeyHandling = true 
}: CreateUserProps) {
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [firstname, setFirstname] = useState('');
    const [lastname, setLastname] = useState('');
    const [phone, setPhone] = useState('');
    const [password, setPassword] = useState('');
    const [retypePassword, setRetypePassword] = useState(''); 
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

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
        setSuccess(false);

        // Password match validation
        if (password !== retypePassword) {
            setError('Passwords do not match.');
            setLoading(false);
            return;
        }

        try {
            const createUserRequest: CreateUserRequest = {
                email,
                firstname,
                lastname,
                phone: phone || undefined,
                password
            };

            const response = await fetch(`${apiBaseUrl}/api/User/CreateUser`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-Token': getCsrfToken(),
                },
                credentials: 'include',
                body: JSON.stringify(createUserRequest),
            });

            if (response.ok) {
                setSuccess(true);
                console.log('User created successfully');
                
                if (onCreateSuccess) {
                    setTimeout(() => onCreateSuccess(), 2000);
                } else {
                    // Navigate to login after 2 seconds
                    setTimeout(() => navigate('/login'), 2000);
                }
            } else {
                const errorText = await response.text();
                setError(errorText || 'Account creation failed');
            }
        } catch (err) {
            setError('Network error occurred. Please try again.');
            console.error('Create user error:', err);
        } finally {
            setLoading(false);
        }
    };

    if (success) {
        return (
            <div className="createuser-container">
                <div className="createuser-form">
                    <h2>Account Created Successfully!</h2>
                    <p>Your account has been created successfully.</p>
                    <p>You can now login with your credentials.</p>
                    <Link to="/login" className="back-to-login-button">
                        Go to Login
                    </Link>
                </div>
            </div>
        );
    }

    return (
        <div className="createuser-container">
            <div style={{ width: "100%" }}>
                <h1 className="create-user-title">{title}</h1>
                <form className="createuser-form" onSubmit={handleSubmit}>
                    <h2>Create Account</h2>
                
                    {error && <div className="error-message">{error}</div>}
                
                    <div className="form-group">
                        <label htmlFor="username">Email:</label>
                        <input
                            type="text"
                            id="email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            minLength={8}
                            placeholder="Enter your username (min 8 characters)"
                            autoComplete="username"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="firstname">First Name:</label>
                        <input
                            type="text"
                            id="firstname"
                            value={firstname}
                            onChange={(e) => setFirstname(e.target.value)}
                            required
                            placeholder="Enter your first name"
                            autoComplete="given-name"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="lastname">Last Name:</label>
                        <input
                            type="text"
                            id="lastname"
                            value={lastname}
                            onChange={(e) => setLastname(e.target.value)}
                            required
                            placeholder="Enter your last name"
                            autoComplete="family-name"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="phone">Phone (optional):</label>
                        <input
                            type="tel"
                            id="phone"
                            value={phone}
                            onChange={(e) => setPhone(e.target.value)}
                            placeholder="Enter your phone number"
                            autoComplete="tel"
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
                            autoComplete="new-password"
                        />
                    </div>

                    <div className="form-group">
                        <label htmlFor="retypePassword">Retype Password:</label>
                        <input
                            type="password"
                            id="retypePassword"
                            value={retypePassword}
                            onChange={(e) => setRetypePassword(e.target.value)}
                            required
                            minLength={8}
                            placeholder="Retype your password"
                            autoComplete="new-password"
                        />
                    </div>

                    <button
                        type="submit"
                        className="complete-button"
                        disabled={loading}
                    >
                        {loading ? 'Creating Account...' : 'Complete'}
                    </button>
                
                    <div className="login-link">
                        <Link to="/login">Already have an account? Login here</Link>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default CreateUser;
