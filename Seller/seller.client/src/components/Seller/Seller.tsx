import { useState } from 'react';
import './Seller.css';
import ProductsSection from './ProductsSection';
import OrdersSection from './OrdersSection';
import AnalyticsSection from './AnalyticsSection';
import CompanySection from './CompanySection';
import AnalyticsPeriodSelector from './AnalyticsPeriodSelector';
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
    const [activeSection, setActiveSection] = useState<SellerSection>('analytics');
    const [analyticsPeriod, setAnalyticsPeriod] = useState<PeriodType>('7d');
    const [productsViewMode, setProductsViewMode] = useState<'list' | 'add'>('list');

    const renderContent = () => {
        switch (activeSection) {
            case 'analytics':
                return <AnalyticsSection companies={companies} />;
            case 'products':
                return <ProductsSection 
                    companies={companies} 
                    viewMode={productsViewMode}
                    onViewModeChange={setProductsViewMode}
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
                            className={`action-button ${productsViewMode === 'list' ? '' : 'secondary'}`}
                            onClick={() => setProductsViewMode('list')}
                        >
                            List Products
                        </button>
                        <button 
                            className={`action-button ${productsViewMode === 'add' ? '' : 'secondary'}`}
                            onClick={() => setProductsViewMode('add')}
                        >
                            Add Product
                        </button>
                    </div>
                );
            default:
                return null;
        }
    };

    return (
        <div className="seller-container">
            <nav className="seller-nav">
                <div className="nav-logo">
                    <h1 className="nav-brand">CanoEh! Seller</h1>
                </div>
                <div className="nav-tabs">
                    <button
                        className={`nav-tab ${activeSection === 'analytics' ? 'active' : ''}`}
                        onClick={() => {
                            setActiveSection('analytics');
                            setProductsViewMode('list');
                        }}
                    >
                        Dashboard
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'products' ? 'active' : ''}`}
                        onClick={() => {
                            setActiveSection('products');
                            setProductsViewMode('list');
                        }}
                    >
                        Products
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'orders' ? 'active' : ''}`}
                        onClick={() => {
                            setActiveSection('orders');
                            setProductsViewMode('list');
                        }}
                    >
                        Orders
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'company' ? 'active' : ''}`}
                        onClick={() => {
                            setActiveSection('company');
                            setProductsViewMode('list');
                        }}
                    >
                        Company
                    </button>
                </div>
                <div className="nav-buttons">
                    <button onClick={onLogout}>Logout</button>
                </div>
            </nav>

            {renderActions() && (
                <div className="seller-content-actions">
                    {renderActions()}
                </div>
            )}

            <main className="seller-content">
                {renderContent()}
            </main>

            <footer className="seller-footer">
                <p>&copy; 2024 CanoEh! Seller Platform. All rights reserved.</p>
            </footer>
        </div>
    );
}

export default Seller;