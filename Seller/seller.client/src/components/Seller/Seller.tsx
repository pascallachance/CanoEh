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
import type { AddProductStep1Data } from '../AddProductStep1';
import type { AddProductStep2Data } from '../AddProductStep2';
import type { AddProductStep3Data } from '../AddProductStep3';

type SellerSection = 'analytics' | 'products' | 'orders' | 'company';

interface NavigationState {
    section?: SellerSection;
}

interface SellerProps {
    companies: Company[];
    onLogout: () => void;
    onEditProduct: (itemId: string, step1Data: AddProductStep1Data, step2Data: AddProductStep2Data, step3Data: AddProductStep3Data, existingVariants: any[]) => void;
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

function Seller({ companies, onLogout, onEditProduct }: SellerProps) {
    const location = useLocation();
    const [activeSection, setActiveSection] = useState<SellerSection>('analytics');
    const [analyticsPeriod, setAnalyticsPeriod] = useState<PeriodType>('7d');
    const { language, setLanguage, t } = useLanguage();
    const navigate = useNavigate();
    // Track the last navigation key we processed to avoid reprocessing
    // Empty string ensures first real navigation will always be different
    const lastProcessedKeyRef = useRef<string>('');

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

    const renderContent = () => {
        switch (activeSection) {
            case 'analytics':
                return <AnalyticsSection companies={companies} />;
            case 'products':
                return <ProductsSection 
                    companies={companies} 
                    viewMode="list"
                    onViewModeChange={() => {}}
                    onEditProduct={onEditProduct}
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