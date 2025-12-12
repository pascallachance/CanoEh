import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate, useLocation } from 'react-router-dom';
import './App.css';
import Login from './components/Login';
import CreateUser from './components/CreateUser';
import ForgotPassword from './components/ForgotPassword';
import NoCompanyPage from './components/NoCompanyPage';
import CreateCompanyStep1 from './components/CreateCompanyStep1';
import CreateCompanyStep2 from './components/CreateCompanyStep2';
import CompanyCreatedSuccess from './components/CompanyCreatedSuccess';
import AddProductStep1 from './components/AddProductStep1';
import AddProductStep2 from './components/AddProductStep2';
import AddProductStep3 from './components/AddProductStep3';
import AddProductStep4 from './components/AddProductStep4';
import Seller from './components/Seller/Seller';
import { ApiClient } from './utils/apiClient';
import { NotificationProvider } from './contexts/NotificationContext';
import { LanguageProvider } from './contexts/LanguageContext';
import type { CreateCompanyStep1Data } from './components/CreateCompanyStep1';
import type { CreateCompanyStep2Data } from './components/CreateCompanyStep2';
import type { AddProductStep1Data } from './components/AddProductStep1';
import type { AddProductStep2Data } from './components/AddProductStep2';
import type { AddProductStep3Data } from './components/AddProductStep3';

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
    
    // Product creation state
    const [productStep1Data, setProductStep1Data] = useState<AddProductStep1Data | null>(null);
    const [productStep2Data, setProductStep2Data] = useState<AddProductStep2Data | null>(null);
    const [productStep3Data, setProductStep3Data] = useState<AddProductStep3Data | null>(null);
    
    // Product editing state
    const [editingItemId, setEditingItemId] = useState<string | null>(null);
    const [editProductStep1Data, setEditProductStep1Data] = useState<AddProductStep1Data | null>(null);
    const [editProductStep2Data, setEditProductStep2Data] = useState<AddProductStep2Data | null>(null);
    const [editProductStep3Data, setEditProductStep3Data] = useState<AddProductStep3Data | null>(null);
    const [editProductExistingVariants, setEditProductExistingVariants] = useState<any[] | null>(null);
    
    const navigate = useNavigate();
    const location = useLocation();

    // Check for existing session on app load
    useEffect(() => {
        checkExistingSession();
    }, []);

    // Clear product creation state when navigating away from add-product routes
    useEffect(() => {
        if (!location.pathname.startsWith('/add-product')) {
            setProductStep1Data(null);
            setProductStep2Data(null);
            setProductStep3Data(null);
        }
    }, [location.pathname]);

    // Clear product editing state when navigating away from edit-product routes
    useEffect(() => {
        if (!location.pathname.startsWith('/edit-product')) {
            setEditingItemId(null);
            setEditProductStep1Data(null);
            setEditProductStep2Data(null);
            setEditProductStep3Data(null);
            setEditProductExistingVariants(null);
        }
    }, [location.pathname]);

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

    // Product creation handlers
    const handleProductStep1Next = (data: AddProductStep1Data) => {
        setProductStep1Data(data);
        navigate('/add-product/step2');
    };

    const handleProductStep1Cancel = () => {
        setProductStep1Data(null);
        navigate('/seller');
    };

    const handleProductStep2Next = (data: AddProductStep2Data) => {
        setProductStep2Data(data);
        navigate('/add-product/step3');
    };

    const handleProductStep2Back = () => {
        navigate('/add-product');
    };

    const handleProductStep3Next = (data: AddProductStep3Data) => {
        setProductStep3Data(data);
        navigate('/add-product/step4');
    };

    const handleProductStep3Back = () => {
        navigate('/add-product/step2');
    };

    const handleProductStep4Back = () => {
        navigate('/add-product/step3');
    };

    const handleProductSubmit = () => {
        // Navigate to seller with products section active
        // State will be cleared by the useEffect that watches location.pathname
        checkExistingSession().then(() => {
            navigate('/seller', { state: { section: 'products' }, replace: true });
        });
    };

    // Edit product handlers
    const handleEditProductStart = (itemId: string, step1Data: AddProductStep1Data, step2Data: AddProductStep2Data, step3Data: AddProductStep3Data, existingVariants: any[]) => {
        setEditingItemId(itemId);
        setEditProductStep1Data(step1Data);
        setEditProductStep2Data(step2Data);
        setEditProductStep3Data(step3Data);
        setEditProductExistingVariants(existingVariants);
        navigate('/edit-product');
    };

    const handleEditProductStep1Next = (data: AddProductStep1Data) => {
        setEditProductStep1Data(data);
        navigate('/edit-product/step2');
    };

    const handleEditProductStep1Cancel = () => {
        setEditingItemId(null);
        setEditProductStep1Data(null);
        setEditProductStep2Data(null);
        setEditProductStep3Data(null);
        setEditProductExistingVariants(null);
        navigate('/seller');
    };

    const handleEditProductStep2Next = (data: AddProductStep2Data) => {
        setEditProductStep2Data(data);
        navigate('/edit-product/step3');
    };

    const handleEditProductStep2Back = () => {
        navigate('/edit-product');
    };

    const handleEditProductStep3Next = (data: AddProductStep3Data) => {
        setEditProductStep3Data(data);
        navigate('/edit-product/step4');
    };

    const handleEditProductStep3Back = () => {
        navigate('/edit-product/step2');
    };

    const handleEditProductStep4Back = () => {
        navigate('/edit-product/step3');
    };

    const handleEditProductSubmit = () => {
        // Navigate to seller with products section active
        // State will be cleared by the useEffect that watches location.pathname
        checkExistingSession().then(() => {
            navigate('/seller', { state: { section: 'products' }, replace: true });
        });
    };

    // Direct step navigation for edit mode
    const handleEditProductStepNavigate = (step: number) => {
        // Navigate to the specified step if we have the required data
        switch (step) {
            case 1:
                navigate('/edit-product');
                break;
            case 2:
                if (editProductStep1Data) {
                    navigate('/edit-product/step2');
                } else {
                    console.warn('Cannot navigate to step 2: Step 1 data is missing');
                }
                break;
            case 3:
                if (editProductStep1Data && editProductStep2Data) {
                    navigate('/edit-product/step3');
                } else {
                    console.warn('Cannot navigate to step 3: Previous step data is missing');
                }
                break;
            case 4:
                if (editProductStep1Data && editProductStep2Data && editProductStep3Data) {
                    navigate('/edit-product/step4');
                } else {
                    console.warn('Cannot navigate to step 4: Previous step data is missing');
                }
                break;
        }
    };

    // Helper to compute completed steps based on existing data
    const getEditModeCompletedSteps = (): number[] => {
        const completed: number[] = [];
        if (editProductStep1Data) completed.push(1);
        if (editProductStep2Data) completed.push(2);
        if (editProductStep3Data) completed.push(3);
        // Step 4 doesn't have its own data, it's computed from previous steps
        return completed;
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
                <Seller companies={companies} onLogout={handleBackToLogin} onEditProduct={handleEditProductStart} />
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
            <Route path="/CreateUser" element={<CreateUser onCreateSuccess={() => navigate('/login')} />} />
            <Route path="/RestorePassword" element={<ForgotPassword />} />
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
            <Route path="/add-product" element={
                <ProtectedRoute>
                    <AddProductStep1
                        onNext={handleProductStep1Next}
                        onCancel={handleProductStep1Cancel}
                        initialData={productStep1Data || undefined}
                    />
                </ProtectedRoute>
            } />
            <Route path="/add-product/step2" element={
                <ProtectedRoute>
                    {productStep1Data ? (
                        <AddProductStep2
                            onNext={handleProductStep2Next}
                            onBack={handleProductStep2Back}
                            step1Data={productStep1Data}
                            initialData={productStep2Data || undefined}
                        />
                    ) : (
                        <Navigate to="/add-product" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/add-product/step3" element={
                <ProtectedRoute>
                    {productStep1Data && productStep2Data ? (
                        <AddProductStep3
                            onNext={handleProductStep3Next}
                            onBack={handleProductStep3Back}
                            step1Data={productStep1Data}
                            step2Data={productStep2Data}
                            initialData={productStep3Data || undefined}
                        />
                    ) : (
                        <Navigate to="/add-product" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/add-product/step4" element={
                <ProtectedRoute>
                    {productStep1Data && productStep2Data && productStep3Data ? (
                        <AddProductStep4
                            onSubmit={handleProductSubmit}
                            onBack={handleProductStep4Back}
                            step1Data={productStep1Data}
                            step2Data={productStep2Data}
                            step3Data={productStep3Data}
                            companies={companies}
                        />
                    ) : (
                        <Navigate to="/add-product" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/edit-product" element={
                <ProtectedRoute>
                    {editProductStep1Data ? (
                        <AddProductStep1
                            onNext={handleEditProductStep1Next}
                            onCancel={handleEditProductStep1Cancel}
                            initialData={editProductStep1Data}
                            editMode={true}
                            onStepNavigate={handleEditProductStepNavigate}
                            completedSteps={getEditModeCompletedSteps()}
                        />
                    ) : (
                        <Navigate to="/seller" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/edit-product/step2" element={
                <ProtectedRoute>
                    {editProductStep1Data && editProductStep2Data ? (
                        <AddProductStep2
                            onNext={handleEditProductStep2Next}
                            onBack={handleEditProductStep2Back}
                            step1Data={editProductStep1Data}
                            initialData={editProductStep2Data}
                            editMode={true}
                            onStepNavigate={handleEditProductStepNavigate}
                            completedSteps={getEditModeCompletedSteps()}
                        />
                    ) : (
                        <Navigate to="/edit-product" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/edit-product/step3" element={
                <ProtectedRoute>
                    {editProductStep1Data && editProductStep2Data && editProductStep3Data ? (
                        <AddProductStep3
                            onNext={handleEditProductStep3Next}
                            onBack={handleEditProductStep3Back}
                            step1Data={editProductStep1Data}
                            step2Data={editProductStep2Data}
                            initialData={editProductStep3Data}
                            editMode={true}
                            onStepNavigate={handleEditProductStepNavigate}
                            completedSteps={getEditModeCompletedSteps()}
                        />
                    ) : (
                        <Navigate to="/edit-product" replace />
                    )}
                </ProtectedRoute>
            } />
            <Route path="/edit-product/step4" element={
                <ProtectedRoute>
                    {editProductStep1Data && editProductStep2Data && editProductStep3Data && editingItemId ? (
                        <AddProductStep4
                            onSubmit={handleEditProductSubmit}
                            onBack={handleEditProductStep4Back}
                            step1Data={editProductStep1Data}
                            step2Data={editProductStep2Data}
                            step3Data={editProductStep3Data}
                            companies={companies}
                            editMode={true}
                            itemId={editingItemId}
                            existingVariants={editProductExistingVariants || undefined}
                            onStepNavigate={handleEditProductStepNavigate}
                            completedSteps={getEditModeCompletedSteps()}
                        />
                    ) : (
                        <Navigate to="/edit-product" replace />
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
            <LanguageProvider>
                <NotificationProvider>
                    <AppContent />
                </NotificationProvider>
            </LanguageProvider>
        </Router>
    );
}

export default App;