import './NoCompanyPage.css';

interface NoCompanyPageProps {
    onCreateCompany: () => void;
    onBackToLogin: () => void;
}

function NoCompanyPage({ onCreateCompany, onBackToLogin }: NoCompanyPageProps) {
    return (
        <div className="no-company-container">
            <div className="no-company-content">
                <h1>Welcome to CanoEh! Seller</h1>
                <div className="no-company-message">
                    <h2>No Company Found</h2>
                    <p>You don't have a company created yet. To start selling on CanoEh!, you need to create a company profile.</p>
                    <p>This will allow you to:</p>
                    <ul>
                        <li>List and manage your products</li>
                        <li>Process customer orders</li>
                        <li>Track your sales and analytics</li>
                        <li>Manage your business information</li>
                    </ul>
                </div>
                
                <div className="no-company-actions">
                    <button 
                        className="create-company-btn"
                        onClick={onCreateCompany}
                    >
                        Create My Company
                    </button>
                    <button 
                        className="back-to-login-btn"
                        onClick={onBackToLogin}
                    >
                        Back to Login
                    </button>
                </div>
            </div>
        </div>
    );
}

export default NoCompanyPage;