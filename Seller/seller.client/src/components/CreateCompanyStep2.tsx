import { useState } from 'react';
import './CreateCompanyStep2.css';
import type { CreateCompanyStep1Data } from './CreateCompanyStep1';

interface CreateCompanyStep2Data {
    name: string;
    description: string;
    logo: string;
    companyPhone: string;
    companyType: string;
    address1: string;
    address2: string;
    address3: string;
    city: string;
    provinceState: string;
    country: string;
    postalCode: string;
}

interface CreateCompanyStep2Props {
    onSubmit: (step1Data: CreateCompanyStep1Data, step2Data: CreateCompanyStep2Data) => void;
    onBack: () => void;
    step1Data: CreateCompanyStep1Data;
    initialData?: CreateCompanyStep2Data;
}

function CreateCompanyStep2({ onSubmit, onBack, step1Data, initialData }: CreateCompanyStep2Props) {
    const [formData, setFormData] = useState<CreateCompanyStep2Data>(initialData || {
        name: '',
        description: '',
        logo: '',
        companyPhone: '',
        companyType: '',
        address1: '',
        address2: '',
        address3: '',
        city: '',
        provinceState: '',
        country: '',
        postalCode: ''
    });

    const [errors, setErrors] = useState<Partial<CreateCompanyStep2Data>>({});
    const [loading, setLoading] = useState(false);

    const companyTypes = [
        'public company',
        'listed company', 
        'private company',
        'charity organization',
        'particular'
    ];

    const validateForm = (): boolean => {
        const newErrors: Partial<CreateCompanyStep2Data> = {};

        if (!formData.name.trim()) {
            newErrors.name = 'Company name is required';
        } else if (formData.name.length > 255) {
            newErrors.name = 'Company name must be 255 characters or less';
        }

        if (!formData.companyType) {
            newErrors.companyType = 'Company type is required';
        }

        if (!formData.address1.trim()) {
            newErrors.address1 = 'Address is required';
        }

        if (!formData.city.trim()) {
            newErrors.city = 'City is required';
        }

        if (!formData.provinceState.trim()) {
            newErrors.provinceState = 'Province/State is required';
        }

        if (!formData.country.trim()) {
            newErrors.country = 'Country is required';
        }

        if (!formData.postalCode.trim()) {
            newErrors.postalCode = 'Postal code is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof CreateCompanyStep2Data, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        // Clear error when user starts typing
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            setLoading(true);
            try {
                await onSubmit(step1Data, formData);
            } catch (error) {
                console.error('Submission error:', error);
            } finally {
                setLoading(false);
            }
        }
    };

    return (
        <div className="create-company-step2-container">
            <div className="create-company-step2-content">
                <header className="step-header">
                    <h1>Create Your Company</h1>
                    <div className="step-indicator">
                        <span className="step completed">1</span>
                        <span className="step-divider"></span>
                        <span className="step active">2</span>
                    </div>
                    <h2>Step 2: Company Information</h2>
                    <p>Please provide your company details to complete the setup.</p>
                </header>

                <form className="company-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <div className="form-group full-width">
                            <label htmlFor="name">Company Name *</label>
                            <input
                                type="text"
                                id="name"
                                value={formData.name}
                                onChange={(e) => handleInputChange('name', e.target.value)}
                                placeholder="Enter your company name"
                                className={errors.name ? 'error' : ''}
                                maxLength={255}
                            />
                            {errors.name && (
                                <span className="error-message">{errors.name}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="description">Company Description</label>
                            <textarea
                                id="description"
                                value={formData.description}
                                onChange={(e) => handleInputChange('description', e.target.value)}
                                placeholder="Describe your company (optional)"
                                rows={3}
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="companyType">Company Type *</label>
                            <select
                                id="companyType"
                                value={formData.companyType}
                                onChange={(e) => handleInputChange('companyType', e.target.value)}
                                className={errors.companyType ? 'error' : ''}
                            >
                                <option value="">Select company type</option>
                                {companyTypes.map(type => (
                                    <option key={type} value={type}>{type}</option>
                                ))}
                            </select>
                            {errors.companyType && (
                                <span className="error-message">{errors.companyType}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="companyPhone">Company Phone</label>
                            <input
                                type="tel"
                                id="companyPhone"
                                value={formData.companyPhone}
                                onChange={(e) => handleInputChange('companyPhone', e.target.value)}
                                placeholder="Enter company phone number"
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="logo">Company Logo URL</label>
                            <input
                                type="url"
                                id="logo"
                                value={formData.logo}
                                onChange={(e) => handleInputChange('logo', e.target.value)}
                                placeholder="https://example.com/logo.png"
                            />
                        </div>

                        <div className="form-group full-width">
                            <h3>Company Address</h3>
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="address1">Address Line 1 *</label>
                            <input
                                type="text"
                                id="address1"
                                value={formData.address1}
                                onChange={(e) => handleInputChange('address1', e.target.value)}
                                placeholder="Street address, P.O. box, etc."
                                className={errors.address1 ? 'error' : ''}
                            />
                            {errors.address1 && (
                                <span className="error-message">{errors.address1}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="address2">Address Line 2</label>
                            <input
                                type="text"
                                id="address2"
                                value={formData.address2}
                                onChange={(e) => handleInputChange('address2', e.target.value)}
                                placeholder="Apartment, suite, unit, etc."
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="address3">Address Line 3</label>
                            <input
                                type="text"
                                id="address3"
                                value={formData.address3}
                                onChange={(e) => handleInputChange('address3', e.target.value)}
                                placeholder="Additional address information"
                            />
                        </div>

                        <div className="form-group">
                            <label htmlFor="city">City *</label>
                            <input
                                type="text"
                                id="city"
                                value={formData.city}
                                onChange={(e) => handleInputChange('city', e.target.value)}
                                placeholder="Enter city"
                                className={errors.city ? 'error' : ''}
                            />
                            {errors.city && (
                                <span className="error-message">{errors.city}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="provinceState">Province/State *</label>
                            <input
                                type="text"
                                id="provinceState"
                                value={formData.provinceState}
                                onChange={(e) => handleInputChange('provinceState', e.target.value)}
                                placeholder="Enter province or state"
                                className={errors.provinceState ? 'error' : ''}
                            />
                            {errors.provinceState && (
                                <span className="error-message">{errors.provinceState}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="country">Country *</label>
                            <input
                                type="text"
                                id="country"
                                value={formData.country}
                                onChange={(e) => handleInputChange('country', e.target.value)}
                                placeholder="Enter country"
                                className={errors.country ? 'error' : ''}
                            />
                            {errors.country && (
                                <span className="error-message">{errors.country}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="postalCode">Postal Code *</label>
                            <input
                                type="text"
                                id="postalCode"
                                value={formData.postalCode}
                                onChange={(e) => handleInputChange('postalCode', e.target.value)}
                                placeholder="Enter postal code"
                                className={errors.postalCode ? 'error' : ''}
                            />
                            {errors.postalCode && (
                                <span className="error-message">{errors.postalCode}</span>
                            )}
                        </div>
                    </div>

                    <div className="form-actions">
                        <button type="button" className="back-btn" onClick={onBack} disabled={loading}>
                            Back
                        </button>
                        <button type="submit" className="create-btn" disabled={loading}>
                            {loading ? 'Creating Company...' : 'Create Company'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default CreateCompanyStep2;
export type { CreateCompanyStep2Data };