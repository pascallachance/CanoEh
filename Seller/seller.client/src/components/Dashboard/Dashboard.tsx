import { useState } from 'react';
import './Dashboard.css';
import ProductsSection from './ProductsSection';
import OrdersSection from './OrdersSection';
import AnalyticsSection from './AnalyticsSection';
import CompanySection from './CompanySection';

type DashboardSection = 'products' | 'orders' | 'analytics' | 'company';

interface DashboardProps {
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

function Dashboard({ companies, onLogout }: DashboardProps) {
    const [activeSection, setActiveSection] = useState<DashboardSection>('products');

    const renderContent = () => {
        switch (activeSection) {
            case 'products':
                return <ProductsSection companies={companies} />;
            case 'orders':
                return <OrdersSection companies={companies} />;
            case 'analytics':
                return <AnalyticsSection companies={companies} />;
            case 'company':
                return <CompanySection companies={companies} />;
            default:
                return <ProductsSection companies={companies} />;
        }
    };

    return (
        <div className="dashboard-container">
            <nav className="dashboard-nav">
                <div className="nav-logo">
                    <h1 className="nav-brand">CanoEh! Seller</h1>
                </div>
                <div className="nav-tabs">
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
                        className={`nav-tab ${activeSection === 'analytics' ? 'active' : ''}`}
                        onClick={() => setActiveSection('analytics')}
                    >
                        Analytics
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

            <main className="dashboard-content">
                {renderContent()}
            </main>

            <footer className="dashboard-footer">
                <p>&copy; 2024 CanoEh! Seller Platform. All rights reserved.</p>
            </footer>
        </div>
    );
}

export default Dashboard;