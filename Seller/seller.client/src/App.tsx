import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import NoCompanyPage from './components/NoCompanyPage';
import CreateCompanyStep1 from './components/CreateCompanyStep1';
import CreateCompanyStep2 from './components/CreateCompanyStep2';
import CompanyCreatedSuccess from './components/CompanyCreatedSuccess';
import { ApiClient } from './utils/apiClient';
import type { CreateCompanyStep1Data } from './components/CreateCompanyStep1';
import type { CreateCompanyStep2Data } from './components/CreateCompanyStep2';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

// Route wrapper component to handle authentication and routing logic
function AppContent() {
    const [companies, setCompanies] = useState<Company[]>([]);
    const [step1Data, setStep1Data] = useState<CreateCompanyStep1Data | null>(null);
    const [createdCompany, setCreatedCompany] = useState<Company | null>(null);
    const [error, setError] = useState<string>('');
    const [isCheckingSession, setIsCheckingSession] = useState(true);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const navigate = useNavigate();
    const location = useLocation();

    // Check for existing session on app load
    useEffect(() => {
        checkExistingSession();
    }, []);

    const checkExistingSession = async () => {
        try {
            setIsCheckingSession(true);
            const response = await ApiClient.get(`${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Company/GetMyCompanies`);

            if (response.ok) {
                const companies = await response.json();
                setIsAuthenticated(true);
                if (Array.isArray(companies) && companies.length > 0) {
                    setCompanies(companies);
                    // Navigate to dashboard if on login page and user has companies
                    if (location.pathname === '/login' || location.pathname === '/') {
                        navigate('/dashboard', { replace: true });
                    }
                } else {
                    // Navigate to no-company page if on login page and no companies
                    if (location.pathname === '/login' || location.pathname === '/') {
                        navigate('/dashboard', { replace: true }); // Will show no company UI
                    }
                }
                setError('');
            } else if (response.status === 401) {
                // No valid session, redirect to login
                setIsAuthenticated(false);
                if (location.pathname !== '/login') {
                    navigate('/login', { replace: true });
                }
            } else {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to fetch companies');
            }
        } catch (err) {
            console.error('Session check error:', err);
            // If there's an error checking session, go to login
            setIsAuthenticated(false);
            if (location.pathname !== '/login') {
                navigate('/login', { replace: true });
            }
        } finally {
            setIsCheckingSession(false);
        }
    };

    const handleLoginSuccess = async () => {
        // After successful login, check companies and navigate appropriately
        setIsAuthenticated(true);
        await checkExistingSession();
    };

    const handleBackToLogin = () => {
        setIsAuthenticated(false);
        setCompanies([]);
        setStep1Data(null);
        setCreatedCompany(null);
        setError('');
        navigate('/login', { replace: true });
    };

    const handleCreateCompany = () => {
        navigate('/create-company');
    };

    const handleStep1Next = (data: CreateCompanyStep1Data) => {
        setStep1Data(data);
        navigate('/create-company/step2');
    };

    const handleStep1Back = () => {
        navigate('/dashboard');
    };

    const handleStep2Back = () => {
        navigate('/create-company');
    };

    const getCsrfToken = (): string => {
        const cookies = document.cookie.split(';');
        for (const cookie of cookies) {
            const [name, value] = cookie.trim().split('=');
            if (name === 'X-CSRF-Token') {
                return value;
            }
        }
        return '';
    };

    const handleStep2Submit = async (step1: CreateCompanyStep1Data, step2: CreateCompanyStep2Data) => {
        try {
            const companyData = {
                name: step2.name,
                description: step2.description || undefined,
                logo: step2.logo || undefined,
                countryOfCitizenship: step1.countryOfCitizenship,
                fullBirthName: step1.fullBirthName,
                countryOfBirth: step1.countryOfBirth,
                birthDate: step1.birthDate ? new Date(step1.birthDate).toISOString() : undefined,
                identityDocumentType: step1.identityDocumentType,
                identityDocument: step1.identityDocument,
                bankDocument: step1.bankDocument,
                facturationDocument: step1.facturationDocument,
                companyPhone: step2.companyPhone || undefined,
                companyType: step2.companyType,
                address1: step2.address1,
                address2: step2.address2 || undefined,
                address3: step2.address3 || undefined,
                city: step2.city,
                provinceState: step2.provinceState,
                country: step2.country,
                postalCode: step2.postalCode
            };

            const response = await ApiClient.post(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Company/CreateCompany`,
                companyData,
                {
                    headers: {
                        'X-CSRF-Token': getCsrfToken(),
                    }
                }
            );

            if (response.ok) {
                const result = await response.json();
                setCreatedCompany({
                    id: result.id,
                    ownerID: result.ownerID,
                    name: result.name,
                    description: result.description,
                    logo: result.logo,
                    createdAt: result.createdAt,
                    updatedAt: result.updatedAt
                });
                navigate('/company-created');
            } else {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to create company');
            }
        } catch (err) {
            console.error('Company creation error:', err);
            setError(err instanceof Error ? err.message : 'Failed to create company');
        }
    };

    const handleContinueToItems = () => {
        // Update companies list and navigate to dashboard with items view
        checkExistingSession().then(() => {
            navigate('/items');
        });
    };

    // Protected route component
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
                    <h2>CanoEh! Seller</h2>
                    <p>Checking your session...</p>
                    <div style={{ 
                        border: '3px solid #f3f3f3',
                        borderTop: '3px solid #007bff',
                        borderRadius: '50%',
                        width: '30px',
                        height: '30px',
                        animation: 'spin 1s linear infinite'
                    }}></div>
                </div>
            );
        }

        if (!isAuthenticated) {
            return <Navigate to="/login" replace />;
        }

        return <>{children}</>;
    };

    // Dashboard route - shows company status
    const DashboardRoute = () => (
        <ProtectedRoute>
            {companies.length > 0 ? (
                <div>
                    <header style={{ 
                        display: 'flex', 
                        justifyContent: 'space-between', 
                        alignItems: 'center', 
                        padding: '1rem',
                        borderBottom: '1px solid #eee'
                    }}>
                        <h1 id="tableLabel">CanoEh! Seller</h1>
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
                                Logout
                            </button>
                        </div>
                    </header>
                    <main style={{ padding: '1rem' }}>
                        <h2>Welcome back!</h2>
                        <p>You have {companies.length} company(ies) registered.</p>
                        {companies.map(company => (
                            <div key={company.id} style={{ 
                                border: '1px solid #ddd', 
                                padding: '1rem', 
                                marginBottom: '1rem',
                                borderRadius: '4px'
                            }}>
                                <h3>{company.name}</h3>
                                {company.description && <p>{company.description}</p>}
                                <small>Created: {new Date(company.createdAt).toLocaleDateString()}</small>
                            </div>
                        ))}
                        <button 
                            onClick={() => navigate('/items')}
                            style={{
                                padding: '0.75rem 1.5rem',
                                background: '#007bff',
                                color: 'white',
                                border: 'none',
                                borderRadius: '4px',
                                cursor: 'pointer',
                                fontSize: '1rem'
                            }}
                        >
                            Manage Items
                        </button>
                    </main>
                </div>
            ) : (
                <NoCompanyPage
                    onCreateCompany={handleCreateCompany}
                    onBackToLogin={handleBackToLogin}
                />
            )}
        </ProtectedRoute>
    );

    // Items management route
    const ItemsRoute = () => (
        <ProtectedRoute>
            <div>
                <header style={{ 
                    display: 'flex', 
                    justifyContent: 'space-between', 
                    alignItems: 'center', 
                    padding: '1rem',
                    borderBottom: '1px solid #eee'
                }}>
                    <h1 id="tableLabel">CanoEh! Seller</h1>
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
                            Logout
                        </button>
                    </div>
                </header>
                <main style={{ padding: '1rem' }}>
                    <h2>Items Management</h2>
                    <p>This is where you would manage your items. This feature will be implemented in a future update.</p>
                    <button 
                        onClick={() => navigate('/dashboard')}
                        style={{
                            padding: '0.75rem 1.5rem',
                            background: '#6c757d',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '1rem'
                        }}
                    >
                        Back to Companies
                    </button>
                </main>
            </div>
        </ProtectedRoute>
    );

    // Error state display
    if (error) {
        return (
            <div style={{ padding: '2rem', textAlign: 'center' }}>
                <h2>Error</h2>
                <p style={{ color: 'red' }}>{error}</p>
                <button onClick={handleBackToLogin} style={{
                    padding: '0.75rem 1.5rem',
                    background: '#007bff',
                    color: 'white',
                    border: 'none',
                    borderRadius: '4px',
                    cursor: 'pointer'
                }}>
                    Back to Login
                </button>
            </div>
        );
    }

    return (
        <Routes>
            <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
            <Route path="/dashboard" element={<DashboardRoute />} />
            <Route path="/create-company" element={
                <ProtectedRoute>
                    <CreateCompanyStep1
                        onNext={handleStep1Next}
                        onBack={handleStep1Back}
                        initialData={step1Data || undefined}
                    />
                </ProtectedRoute>
            } />
            <Route path="/create-company/step2" element={
                <ProtectedRoute>
                    {step1Data ? (
                        <CreateCompanyStep2
                            onSubmit={handleStep2Submit}
                            onBack={handleStep2Back}
                            step1Data={step1Data}
                        />
                    ) : (
                        <Navigate to="/create-company" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/company-created" element={
                <ProtectedRoute>
                    {createdCompany ? (
                        <CompanyCreatedSuccess
                            company={createdCompany}
                            onContinueToItems={handleContinueToItems}
                        />
                    ) : (
                        <Navigate to="/dashboard" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/items" element={<ItemsRoute />} />
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="*" element={<Navigate to="/login" replace />} />
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