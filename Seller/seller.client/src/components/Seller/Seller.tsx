import { useState, useEffect, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import './Seller.css';
import ProductsSection from './ProductsSection';
import OrdersSection from './OrdersSection';
import AnalyticsSection from './AnalyticsSection';
import CompanySection from './CompanySection';
import AnalyticsPeriodSelector from './AnalyticsPeriodSelector';
import { useLanguage } from '../../contexts/LanguageContext';
import type { PeriodType } from './AnalyticsPeriodSelector';

type SellerSection = 'analytics' | 'products' | 'orders' | 'company';

interface SellerProps {
    companies: Company[];
    onLogout: () => void;
}

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

function Seller({ companies, onLogout }: SellerProps) {
    const location = useLocation();
    const [activeSection, setActiveSection] = useState<SellerSection>('analytics');
    const [analyticsPeriod, setAnalyticsPeriod] = useState<PeriodType>('7d');
    const { language, setLanguage, t } = useLanguage();
    const navigate = useNavigate();
    const stateProcessedRef = useRef(false);

    // Check for navigation state to set initial section
    // Run only once on mount to avoid unnecessary re-runs
    useEffect(() => {
        const state = location.state as { section?: SellerSection } | null;
        // Only process state once and only if it contains a section
        if (state?.section && !stateProcessedRef.current) {
            setActiveSection(state.section);
            stateProcessedRef.current = true;
            // Clear the section from state to prevent it from persisting
            // Preserve any other state properties that might exist
            const { section, ...remainingState } = state;
            navigate(location.pathname, { replace: true, state: remainingState });
        }
    }, []); // Empty dependency array: run only once on mount

    const renderContent = () => {
        switch (activeSection) {
            case 'analytics':
                return <AnalyticsSection companies={companies} />;
            case 'products':
                return <ProductsSection 
                    companies={companies} 
                    viewMode="list"
                    onViewModeChange={() => {}}
                />;
            case 'orders':
                return <OrdersSection companies={companies} />;
            case 'company':
                return <CompanySection companies={companies} />;
            default:
                return <AnalyticsSection companies={companies} />;
        }
    };

    const renderActions = () => {
        switch (activeSection) {
            case 'analytics':
                return (
                    <AnalyticsPeriodSelector 
                        selectedPeriod={analyticsPeriod}
                        onPeriodChange={setAnalyticsPeriod}
                    />
                );
            case 'products':
                return (
                    <div className="action-buttons">
                        <button 
                            className="action-button"
                            onClick={() => navigate('/add-product')}
                        >
                            {t('products.addProduct')}
                        </button>
                    </div>
                );
            default:
                return null;
        }
    };

    const actions = renderActions();

    return (
        <div className="seller-container">
            <nav className="seller-nav">
                <div className="nav-logo">
                    <h1 className="nav-brand">{t('nav.brand')}</h1>
                </div>
                <div className="nav-tabs">
                    <button
                        className={`nav-tab ${activeSection === 'analytics' ? 'active' : ''}`}
                        onClick={() => setActiveSection('analytics')}
                    >
                        {t('nav.dashboard')}
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'products' ? 'active' : ''}`}
                        onClick={() => setActiveSection('products')}
                    >
                        {t('nav.products')}
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'orders' ? 'active' : ''}`}
                        onClick={() => setActiveSection('orders')}
                    >
                        {t('nav.orders')}
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'company' ? 'active' : ''}`}
                        onClick={() => setActiveSection('company')}
                    >
                        {t('nav.company')}
                    </button>
                </div>
                <div className="nav-buttons">
                    <select 
                        className="drop-down-language"
                        value={language}
                        onChange={(e) => setLanguage(e.target.value as 'en' | 'fr')}
                        title={t('nav.language')}
                    >
                        <option value="en">English</option>
                        <option value="fr">Français</option>
                    </select>
                    <button onClick={onLogout}>{t('nav.logout')}</button>
                </div>
            </nav>

            {actions && (
                <div className="seller-content-actions">
                    {actions}
                </div>
            )}

            <main className="seller-content">
                {renderContent()}
            </main>

            <footer className="seller-footer">
                <p>{t('footer.copyright')}</p>
            </footer>
        </div>
    );
}

export default Seller;