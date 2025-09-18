import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import NoCompanyPage from './components/NoCompanyPage';
import CreateCompanyStep1 from './components/CreateCompanyStep1';
import CreateCompanyStep2 from './components/CreateCompanyStep2';
import CompanyCreatedSuccess from './components/CompanyCreatedSuccess';
import Seller from './components/Seller/Seller';
import { ApiClient } from './utils/apiClient';
import { NotificationProvider } from './contexts/NotificationContext';
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
                    // Navigate to seller if on login page and user has companies
                    if (location.pathname === '/login' || location.pathname === '/') {
                        navigate('/seller', { replace: true });
                    }
                } else {
                    // Navigate to no-company page if on login page and no companies
                    if (location.pathname === '/login' || location.pathname === '/') {
                        navigate('/seller', { replace: true }); // Will show no company UI
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
        navigate('/seller');
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
        // Update companies list and navigate to seller 
        checkExistingSession().then(() => {
            navigate('/seller');
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
                    <div className="spinner"></div>
                </div>
            );
        }

        if (!isAuthenticated) {
            return <Navigate to="/login" replace />;
        }

        return <>{children}</>;
    };

    // Seller route - shows company status
    const SellerRoute = () => (
        <ProtectedRoute>
            {companies.length > 0 ? (
                <Seller companies={companies} onLogout={handleBackToLogin} />
            ) : (
                <NoCompanyPage
                    onCreateCompany={handleCreateCompany}
                    onBackToLogin={handleBackToLogin}
                />
            )}
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
            <Route path="/seller" element={<SellerRoute />} />
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
                        <Navigate to="/seller" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
    );
}

function App() {
    return (
        <Router>
            <NotificationProvider>
                <AppContent />
            </NotificationProvider>
        </Router>
    );
}

export default App;