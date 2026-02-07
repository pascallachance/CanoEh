import { useState, useEffect, useRef, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import './Seller.css';
import ProductsSection from './ProductsSection';
import type { ProductsSectionRef } from './ProductsSection';
import OrdersSection from './OrdersSection';
import AnalyticsSection from './AnalyticsSection';
import CompanySection from './CompanySection';
import AnalyticsPeriodSelector from './AnalyticsPeriodSelector';
import { useLanguage } from '../../contexts/LanguageContext';
import type { PeriodType } from './AnalyticsPeriodSelector';

type SellerSection = 'analytics' | 'products' | 'orders' | 'company';

// Valid section values for type checking
const VALID_SECTIONS: readonly SellerSection[] = ['analytics', 'products', 'orders', 'company'] as const;

// Storage key for persisting active section
const SECTION_STORAGE_KEY = 'seller_active_section';

/**
 * Get the initial active section from navigation state, sessionStorage, or default
 * Priority: navigation state > sessionStorage > default 'analytics'
 */
function getInitialSection(location: ReturnType<typeof useLocation>): SellerSection {
    // Check navigation state first (validate to ensure it's a valid section)
    const stateSection = (location.state as NavigationState | null)?.section;
    if (stateSection && VALID_SECTIONS.includes(stateSection)) {
        return stateSection;
    }
    
    // Try to restore from sessionStorage
    const storedSection = sessionStorage.getItem(SECTION_STORAGE_KEY);
    if (storedSection && VALID_SECTIONS.includes(storedSection as SellerSection)) {
        return storedSection as SellerSection;
    }
    
    return 'analytics';
}

interface NavigationState {
    section?: SellerSection;
}

interface SellerProps {
    companies: Company[];
    onLogout: () => void;
    onCompanyUpdate?: (updatedCompany: Company) => void;
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

function Seller({ companies, onLogout, onCompanyUpdate }: SellerProps) {
    const location = useLocation();
    
    const [activeSection, setActiveSection] = useState<SellerSection>(() => getInitialSection(location));
    const [analyticsPeriod, setAnalyticsPeriod] = useState<PeriodType>('7d');
    const [isManageOffersDisabled, setIsManageOffersDisabled] = useState(true);
    const { language, setLanguage, t } = useLanguage();
    const productsSectionRef = useRef<ProductsSectionRef>(null);
    // Track the last navigation key we processed to avoid reprocessing
    // Empty string ensures first real navigation will always be different
    const lastProcessedKeyRef = useRef<string>('');
    
    // Persist the active section to sessionStorage whenever it changes
    // This ensures the section persists even if the component remounts
    useEffect(() => {
        sessionStorage.setItem(SECTION_STORAGE_KEY, activeSection);

        // Cleanup: clear stored section when this component unmounts
        // This prevents cross-user or cross-session leakage of the last active section
        return () => {
            sessionStorage.removeItem(SECTION_STORAGE_KEY);
        };
    }, [activeSection]);

    // Process navigation state to update active section when specified
    // Use location.key to detect and handle unique navigations
    useEffect(() => {
        const state = location.state as NavigationState | null;
        const currentKey = location.key;
        
        // Only process state if:
        // 1. State contains a section
        // 2. We haven't processed this specific navigation (tracked by location.key)
        if (state?.section && currentKey !== lastProcessedKeyRef.current) {
            setActiveSection(state.section);
            lastProcessedKeyRef.current = currentKey;
        }
    }, [location.key, location.state]);

    const handleManageOffersStateChange = useCallback((isLoading: boolean, hasItems: boolean) => {
        setIsManageOffersDisabled(isLoading || !hasItems);
    }, []);

    const renderContent = () => {
        switch (activeSection) {
            case 'analytics':
                return <AnalyticsSection companies={companies} />;
            case 'products':
                return <ProductsSection 
                    ref={productsSectionRef}
                    companies={companies} 
                    viewMode="list"
                    onViewModeChange={() => {}}
                    onManageOffersStateChange={handleManageOffersStateChange}
                />;
            case 'orders':
                return <OrdersSection companies={companies} />;
            case 'company':
                return <CompanySection companies={companies} onCompanyUpdate={onCompanyUpdate} />;
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
                            onClick={() => productsSectionRef.current?.openManageOffers()}
                            disabled={isManageOffersDisabled}
                        >
                            {t('products.manageOffers')}
                        </button>
                        <button 
                            className="action-button"
                            onClick={() => productsSectionRef.current?.openAddProduct()}
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