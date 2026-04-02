import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import './CreateUser.css';
import './fieldValidation.css';
import { validateEmailFormat, validatePasswordLength, validateName, validatePhone } from './validation';

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

type CreateUserFields = 'email' | 'firstname' | 'lastname' | 'phone' | 'password' | 'retypePassword';

export function CreateUser({ 
    title, 
    apiBaseUrl, 
    onCreateSuccess, 
    enableEscapeKeyHandling = true 
}: CreateUserProps) {
    const [email, setEmail] = useState('');
    const [firstname, setFirstname] = useState('');
    const [lastname, setLastname] = useState('');
    const [phone, setPhone] = useState('');
    const [password, setPassword] = useState('');
    const [retypePassword, setRetypePassword] = useState(''); 
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [showRetypePassword, setShowRetypePassword] = useState(false);
    const [fieldErrors, setFieldErrors] = useState<Partial<Record<CreateUserFields, string>>>({});
    const [touched, setTouched] = useState<Partial<Record<CreateUserFields, boolean>>>({});

    const validateField = (name: CreateUserFields, value: string, currentPassword?: string): string => {
        if (name === 'email') return validateEmailFormat(value);
        if (name === 'firstname') return validateName(value, 'First name');
        if (name === 'lastname') return validateName(value, 'Last name');
        if (name === 'phone') return validatePhone(value);
        if (name === 'password') return validatePasswordLength(value);
        if (name === 'retypePassword') {
            if (!value) return 'Please retype your password';
            if (value !== (currentPassword ?? password)) return 'Passwords do not match';
            return '';
        }
        return '';
    };

    const handleFieldChange = (name: CreateUserFields, value: string) => {
        setTouched(prev => ({ ...prev, [name]: true }));
        const errMsg = name === 'retypePassword'
            ? validateField(name, value, password)
            : validateField(name, value);
        setFieldErrors(prev => ({ ...prev, [name]: errMsg }));
        if (name === 'password' && touched.retypePassword) {
            const retypeErr = retypePassword !== value ? 'Passwords do not match' : '';
            setFieldErrors(prev => ({ ...prev, retypePassword: retypeErr }));
        }
    };

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
                    setTimeout(() => onCreateSuccess(), 1500);
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
            <div className="createuser-wrapper">
                <h1 className="create-user-title">{title}</h1>
                <form className="createuser-form" onSubmit={handleSubmit}>
                    <h2>Create Account</h2>
                
                    {error && <div className="error-message">{error}</div>}
                
                    <div className="form-group">
                        <label htmlFor="email">Email:</label>
                        <input
                            type="email"
                            id="email"
                            value={email}
                            onChange={(e) => { setEmail(e.target.value); handleFieldChange('email', e.target.value); }}
                            required
                            placeholder="Enter your email address"
                            autoComplete="email"
                            className={touched.email && fieldErrors.email ? 'input-invalid' : ''}
                        />
                        {touched.email && fieldErrors.email && (
                            <span className="field-error" title={fieldErrors.email}>{fieldErrors.email}</span>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="firstname">First Name:</label>
                        <input
                            type="text"
                            id="firstname"
                            value={firstname}
                            onChange={(e) => { setFirstname(e.target.value); handleFieldChange('firstname', e.target.value); }}
                            required
                            placeholder="Enter your first name"
                            autoComplete="given-name"
                            className={touched.firstname && fieldErrors.firstname ? 'input-invalid' : ''}
                        />
                        {touched.firstname && fieldErrors.firstname && (
                            <span className="field-error" title={fieldErrors.firstname}>{fieldErrors.firstname}</span>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="lastname">Last Name:</label>
                        <input
                            type="text"
                            id="lastname"
                            value={lastname}
                            onChange={(e) => { setLastname(e.target.value); handleFieldChange('lastname', e.target.value); }}
                            required
                            placeholder="Enter your last name"
                            autoComplete="family-name"
                            className={touched.lastname && fieldErrors.lastname ? 'input-invalid' : ''}
                        />
                        {touched.lastname && fieldErrors.lastname && (
                            <span className="field-error" title={fieldErrors.lastname}>{fieldErrors.lastname}</span>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="phone">Phone (optional):</label>
                        <input
                            type="tel"
                            id="phone"
                            value={phone}
                            onChange={(e) => { setPhone(e.target.value); handleFieldChange('phone', e.target.value); }}
                            placeholder="Enter your phone number"
                            autoComplete="tel"
                            className={touched.phone && fieldErrors.phone ? 'input-invalid' : ''}
                        />
                        {touched.phone && fieldErrors.phone && (
                            <span className="field-error" title={fieldErrors.phone}>{fieldErrors.phone}</span>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="password">Password:</label>
                        <div className="password-input-wrapper">
                            <input
                                type={showPassword ? "text" : "password"}
                                id="password"
                                value={password}
                                onChange={(e) => { setPassword(e.target.value); handleFieldChange('password', e.target.value); }}
                                required
                                minLength={8}
                                placeholder="Enter your password (min 8 characters)"
                                autoComplete="new-password"
                                className={touched.password && fieldErrors.password ? 'input-invalid' : ''}
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
                        {touched.password && fieldErrors.password && (
                            <span className="field-error" title={fieldErrors.password}>{fieldErrors.password}</span>
                        )}
                    </div>

                    <div className="form-group">
                        <label htmlFor="retypePassword">Retype Password:</label>
                        <div className="password-input-wrapper">
                            <input
                                type={showRetypePassword ? "text" : "password"}
                                id="retypePassword"
                                value={retypePassword}
                                onChange={(e) => { setRetypePassword(e.target.value); handleFieldChange('retypePassword', e.target.value); }}
                                required
                                minLength={8}
                                placeholder="Retype your password"
                                autoComplete="new-password"
                                className={touched.retypePassword && fieldErrors.retypePassword ? 'input-invalid' : ''}
                            />
                            <button
                                type="button"
                                className="password-toggle-btn"
                                onClick={() => setShowRetypePassword(!showRetypePassword)}
                                aria-label={showRetypePassword ? "Hide password" : "Show password"}
                            >
                                {showRetypePassword ? (
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
                        {touched.retypePassword && fieldErrors.retypePassword && (
                            <span className="field-error" title={fieldErrors.retypePassword}>{fieldErrors.retypePassword}</span>
                        )}
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
