import { useState } from 'react';
import './Seller.css';
import ProductsSection from './ProductsSection';
import OrdersSection from './OrdersSection';
import AnalyticsSection from './AnalyticsSection';
import CompanySection from './CompanySection';

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

    const renderContent = () => {
        switch (activeSection) {
            case 'analytics':
                return <AnalyticsSection companies={companies} />;
            case 'products':
                return <ProductsSection companies={companies} />;
            case 'orders':
                return <OrdersSection companies={companies} />;
            case 'company':
                return <CompanySection companies={companies} />;
            default:
                return <AnalyticsSection companies={companies} />;
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
                        onClick={() => setActiveSection('analytics')}
                    >
                        Dashboard
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'products' ? 'active' : ''}`}
                        onClick={() => setActiveSection('products')}
                    >
                        Products
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'orders' ? 'active' : ''}`}
                        onClick={() => setActiveSection('orders')}
                    >
                        Orders
                    </button>
                    <button
                        className={`nav-tab ${activeSection === 'company' ? 'active' : ''}`}
                        onClick={() => setActiveSection('company')}
                    >
                        Company
                    </button>
                </div>
                <div className="nav-buttons">
                    <button onClick={onLogout}>Logout</button>
                </div>
            </nav>

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