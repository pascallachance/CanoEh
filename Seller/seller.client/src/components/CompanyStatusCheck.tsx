import { useState, useEffect } from 'react';
import './CompanyStatusCheck.css';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

interface CompanyStatusCheckProps {
    onHasCompany: (companies: Company[]) => void;
    onNoCompany: () => void;
    onError: (error: string) => void;
}

function CompanyStatusCheck({ onHasCompany, onNoCompany, onError }: CompanyStatusCheckProps) {
    const [loading, setLoading] = useState(true);

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

    useEffect(() => {
        const checkCompanyStatus = async () => {
            try {
                const response = await fetch(`${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Company/GetMyCompanies`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-Token': getCsrfToken(),
                    },
                    credentials: 'include',
                });

                if (response.ok) {
                    const companies: Company[] = await response.json();
                    if (companies && companies.length > 0) {
                        onHasCompany(companies);
                    } else {
                        onNoCompany();
                    }
                } else if (response.status === 401) {
                    onError('Authentication required. Please log in again.');
                } else {
                    const errorText = await response.text();
                    onError(errorText || 'Failed to check company status.');
                }
            } catch (err) {
                console.error('Company status check error:', err);
                onError('Network error. Please try again.');
            } finally {
                setLoading(false);
            }
        };

        checkCompanyStatus();
    }, [onHasCompany, onNoCompany, onError]);

    if (loading) {
        return (
            <div className="company-status-loading">
                <h2>Checking your company status...</h2>
                <p>Please wait while we verify your company information.</p>
            </div>
        );
    }

    return null; // Component handles the flow through callbacks
}

export default CompanyStatusCheck;