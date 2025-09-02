import { useState } from 'react';
import './CreateCompanyStep1.css';

interface CreateCompanyStep1Data {
    countryOfCitizenship: string;
    fullBirthName: string;
    countryOfBirth: string;
    birthDate: string;
    identityDocumentType: string;
    identityDocument: string;
    bankDocument: string;
    facturationDocument: string;
}

interface CreateCompanyStep1Props {
    onNext: (data: CreateCompanyStep1Data) => void;
    onBack: () => void;
    initialData?: CreateCompanyStep1Data;
}

function CreateCompanyStep1({ onNext, onBack, initialData }: CreateCompanyStep1Props) {
    const [formData, setFormData] = useState<CreateCompanyStep1Data>(initialData || {
        countryOfCitizenship: '',
        fullBirthName: '',
        countryOfBirth: '',
        birthDate: '',
        identityDocumentType: '',
        identityDocument: '',
        bankDocument: '',
        facturationDocument: ''
    });

    const [errors, setErrors] = useState<Partial<CreateCompanyStep1Data>>({});

    const identityDocumentTypes = [
        'passport',
        'Driver Licence', 
        'government delivered document'
    ];

    const validateForm = (): boolean => {
        const newErrors: Partial<CreateCompanyStep1Data> = {};

        if (!formData.countryOfCitizenship.trim()) {
            newErrors.countryOfCitizenship = 'Country of citizenship is required';
        }

        if (!formData.fullBirthName.trim()) {
            newErrors.fullBirthName = 'Full birth name is required';
        }

        if (!formData.countryOfBirth.trim()) {
            newErrors.countryOfBirth = 'Country of birth is required';
        }

        if (!formData.birthDate) {
            newErrors.birthDate = 'Birth date is required';
        } else {
            const birthDate = new Date(formData.birthDate);
            const today = new Date();
            const age = today.getFullYear() - birthDate.getFullYear();
            if (age < 18) {
                newErrors.birthDate = 'You must be at least 18 years old';
            }
        }

        if (!formData.identityDocumentType) {
            newErrors.identityDocumentType = 'Identity document type is required';
        }

        if (!formData.identityDocument.trim()) {
            newErrors.identityDocument = 'Identity document is required';
        }

        if (!formData.bankDocument.trim()) {
            newErrors.bankDocument = 'Bank document is required';
        }

        if (!formData.facturationDocument.trim()) {
            newErrors.facturationDocument = 'Facturation document is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof CreateCompanyStep1Data, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        // Clear error when user starts typing
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            onNext(formData);
        }
    };

    return (
        <div className="create-company-step1-container">
            <div className="create-company-step1-content">
                <header className="step-header">
                    <h1>Create Your Company</h1>
                    <div className="step-indicator">
                        <span className="step active">1</span>
                        <span className="step-divider"></span>
                        <span className="step">2</span>
                    </div>
                    <h2>Step 1: Identity Validation</h2>
                    <p>Please provide your personal information to validate your identity.</p>
                </header>

                <form className="identity-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <div className="form-group">
                            <label htmlFor="countryOfCitizenship">Country of Citizenship *</label>
                            <input
                                type="text"
                                id="countryOfCitizenship"
                                value={formData.countryOfCitizenship}
                                onChange={(e) => handleInputChange('countryOfCitizenship', e.target.value)}
                                placeholder="Enter your country of citizenship"
                                className={errors.countryOfCitizenship ? 'error' : ''}
                            />
                            {errors.countryOfCitizenship && (
                                <span className="error-message">{errors.countryOfCitizenship}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="fullBirthName">Full Birth Name *</label>
                            <input
                                type="text"
                                id="fullBirthName"
                                value={formData.fullBirthName}
                                onChange={(e) => handleInputChange('fullBirthName', e.target.value)}
                                placeholder="Enter your full birth name"
                                className={errors.fullBirthName ? 'error' : ''}
                            />
                            {errors.fullBirthName && (
                                <span className="error-message">{errors.fullBirthName}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="countryOfBirth">Country of Birth *</label>
                            <input
                                type="text"
                                id="countryOfBirth"
                                value={formData.countryOfBirth}
                                onChange={(e) => handleInputChange('countryOfBirth', e.target.value)}
                                placeholder="Enter your country of birth"
                                className={errors.countryOfBirth ? 'error' : ''}
                            />
                            {errors.countryOfBirth && (
                                <span className="error-message">{errors.countryOfBirth}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="birthDate">Birth Date *</label>
                            <input
                                type="date"
                                id="birthDate"
                                value={formData.birthDate}
                                onChange={(e) => handleInputChange('birthDate', e.target.value)}
                                className={errors.birthDate ? 'error' : ''}
                            />
                            {errors.birthDate && (
                                <span className="error-message">{errors.birthDate}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="identityDocumentType">Identity Document Type *</label>
                            <select
                                id="identityDocumentType"
                                value={formData.identityDocumentType}
                                onChange={(e) => handleInputChange('identityDocumentType', e.target.value)}
                                className={errors.identityDocumentType ? 'error' : ''}
                            >
                                <option value="">Select document type</option>
                                {identityDocumentTypes.map(type => (
                                    <option key={type} value={type}>{type}</option>
                                ))}
                            </select>
                            {errors.identityDocumentType && (
                                <span className="error-message">{errors.identityDocumentType}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="identityDocument">Identity Document *</label>
                            <input
                                type="text"
                                id="identityDocument"
                                value={formData.identityDocument}
                                onChange={(e) => handleInputChange('identityDocument', e.target.value)}
                                placeholder="Enter document number or details"
                                className={errors.identityDocument ? 'error' : ''}
                            />
                            {errors.identityDocument && (
                                <span className="error-message">{errors.identityDocument}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="bankDocument">Bank Document *</label>
                            <input
                                type="text"
                                id="bankDocument"
                                value={formData.bankDocument}
                                onChange={(e) => handleInputChange('bankDocument', e.target.value)}
                                placeholder="Bank statement or credit card details"
                                className={errors.bankDocument ? 'error' : ''}
                            />
                            {errors.bankDocument && (
                                <span className="error-message">{errors.bankDocument}</span>
                            )}
                        </div>

                        <div className="form-group">
                            <label htmlFor="facturationDocument">Facturation Document *</label>
                            <input
                                type="text"
                                id="facturationDocument"
                                value={formData.facturationDocument}
                                onChange={(e) => handleInputChange('facturationDocument', e.target.value)}
                                placeholder="Facturable credit card or debit card"
                                className={errors.facturationDocument ? 'error' : ''}
                            />
                            {errors.facturationDocument && (
                                <span className="error-message">{errors.facturationDocument}</span>
                            )}
                        </div>
                    </div>

                    <div className="form-actions">
                        <button type="button" className="back-btn" onClick={onBack}>
                            Back
                        </button>
                        <button type="submit" className="next-btn">
                            Next Step
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default CreateCompanyStep1;
export type { CreateCompanyStep1Data };