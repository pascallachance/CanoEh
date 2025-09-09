import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CompanyStatusCheck from './components/CompanyStatusCheck';
import NoCompanyPage from './components/NoCompanyPage';
import CreateCompanyStep1 from './components/CreateCompanyStep1';
import CreateCompanyStep2 from './components/CreateCompanyStep2';
import CompanyCreatedSuccess from './components/CompanyCreatedSuccess';
import { ApiClient } from './utils/apiClient';
import type { CreateCompanyStep1Data } from './components/CreateCompanyStep1';
import type { CreateCompanyStep2Data } from './components/CreateCompanyStep2';

type AppState = 
    | 'login'
    | 'checking-company'
    | 'no-company'
    | 'has-company'
    | 'create-step1'
    | 'create-step2'
    | 'company-created'
    | 'items-management';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

function App() {
    const [appState, setAppState] = useState<AppState>('login');
    const [companies, setCompanies] = useState<Company[]>([]);
    const [step1Data, setStep1Data] = useState<CreateCompanyStep1Data | null>(null);
    const [createdCompany, setCreatedCompany] = useState<Company | null>(null);
    const [error, setError] = useState<string>('');
    const [isCheckingSession, setIsCheckingSession] = useState(true);

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
                if (Array.isArray(companies) && companies.length > 0) {
                    setCompanies(companies);
                    setAppState('has-company');
                } else {
                    setAppState('no-company');
                }
                setError('');
            } else if (response.status === 401) {
                // No valid session, show login
                setAppState('login');
            } else {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to fetch companies');
            }
        } catch (err) {
            console.error('Session check error:', err);
            // If there's an error checking session, go to login
            setAppState('login');
        } finally {
            setIsCheckingSession(false);
        }
    };

    const handleLoginSuccess = async () => {
        // After successful login, check companies using the new API client
        await checkExistingSession();
    };

    const handleBackToLogin = () => {
        setAppState('login');
        setCompanies([]);
        setStep1Data(null);
        setCreatedCompany(null);
        setError('');
    };

    const handleHasCompany = (userCompanies: Company[]) => {
        setCompanies(userCompanies);
        setAppState('has-company');
    };

    const handleNoCompany = () => {
        setAppState('no-company');
    };

    const handleCompanyCheckError = (errorMessage: string) => {
        setError(errorMessage);
        // If authentication error, go back to login
        if (errorMessage.includes('Authentication') || errorMessage.includes('log in')) {
            handleBackToLogin();
        }
    };

    const handleCreateCompany = () => {
        setAppState('create-step1');
    };

    const handleStep1Next = (data: CreateCompanyStep1Data) => {
        setStep1Data(data);
        setAppState('create-step2');
    };

    const handleStep1Back = () => {
        setAppState('no-company');
    };

    const handleStep2Back = () => {
        setAppState('create-step1');
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
                setAppState('company-created');
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
        setAppState('items-management');
    };

    // Show loading screen while checking existing session
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

    // Render based on current state
    if (appState === 'login') {
        return (
            <Router>
                <Routes>
                    <Route path="/login" element={<Login onLoginSuccess={handleLoginSuccess} />} />
                    <Route path="*" element={<Navigate to="/login" replace />} />
                </Routes>
            </Router>
        );
    }

    if (appState === 'checking-company') {
        return (
            <CompanyStatusCheck
                onHasCompany={handleHasCompany}
                onNoCompany={handleNoCompany}
                onError={handleCompanyCheckError}
            />
        );
    }

    if (appState === 'no-company') {
        return (
            <NoCompanyPage
                onCreateCompany={handleCreateCompany}
                onBackToLogin={handleBackToLogin}
            />
        );
    }

    if (appState === 'create-step1') {
        return (
            <CreateCompanyStep1
                onNext={handleStep1Next}
                onBack={handleStep1Back}
                initialData={step1Data || undefined}
            />
        );
    }

    if (appState === 'create-step2' && step1Data) {
        return (
            <CreateCompanyStep2
                onSubmit={handleStep2Submit}
                onBack={handleStep2Back}
                step1Data={step1Data}
            />
        );
    }

    if (appState === 'company-created' && createdCompany) {
        return (
            <CompanyCreatedSuccess
                company={createdCompany}
                onContinueToItems={handleContinueToItems}
            />
        );
    }

    if (appState === 'has-company' || appState === 'items-management') {
        // Show existing company or items management interface
        return (
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
                    {appState === 'has-company' && (
                        <>
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
                                onClick={() => setAppState('items-management')}
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
                        </>
                    )}
                    {appState === 'items-management' && (
                        <>
                            <h2>Items Management</h2>
                            <p>This is where you would manage your items. This feature will be implemented in a future update.</p>
                            <button 
                                onClick={() => setAppState('has-company')}
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
                        </>
                    )}
                </main>
            </div>
        );
    }

    // Error state
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

    // Fallback
    return <Navigate to="/login" replace />;
}

export default App;