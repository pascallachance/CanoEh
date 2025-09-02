import './CompanyCreatedSuccess.css';

interface Company {
    id: string;
    name: string;
    description?: string;
}

interface CompanyCreatedSuccessProps {
    company: Company;
    onContinueToItems: () => void;
}

function CompanyCreatedSuccess({ company, onContinueToItems }: CompanyCreatedSuccessProps) {
    return (
        <div className="company-success-container">
            <div className="company-success-content">
                <div className="success-icon">
                    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <circle cx="12" cy="12" r="10" fill="#27ae60"/>
                        <path d="m9 12 2 2 4-4" stroke="white" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                </div>
                
                <h1>Company Created Successfully!</h1>
                <p className="success-message">
                    Congratulations! Your company <strong>"{company.name}"</strong> has been successfully created.
                </p>
                
                {company.description && (
                    <div className="company-info">
                        <h3>Company Description:</h3>
                        <p>{company.description}</p>
                    </div>
                )}
                
                <div className="next-steps">
                    <h3>What's Next?</h3>
                    <ul>
                        <li>Add your first products to start selling</li>
                        <li>Set up product categories and pricing</li>
                        <li>Configure your inventory management</li>
                        <li>Review and update your company profile</li>
                    </ul>
                </div>
                
                <div className="success-actions">
                    <button 
                        className="continue-btn"
                        onClick={onContinueToItems}
                    >
                        Continue to Add Items
                    </button>
                </div>
                
                <div className="company-id">
                    <small>Company ID: {company.id}</small>
                </div>
            </div>
        </div>
    );
}

export default CompanyCreatedSuccess;