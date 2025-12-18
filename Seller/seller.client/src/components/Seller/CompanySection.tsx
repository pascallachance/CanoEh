import { useState, useEffect } from 'react';
import { useNotifications } from '../../contexts/useNotifications';
import './CompanySection.css';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

interface CompanySectionProps {
    companies: Company[];
}

interface CompanyFormData {
    name: string;
    logo: string;
    phone: string;
    email: string;
    website: string;
    address1: string;
    address2: string;
    city: string;
    provinceState: string;
    country: string;
    postalCode: string;
    bankDocument: string;
    facturationDocument: string;
}

type CardSection = 'basic' | 'contact' | 'address' | 'owner' | null;

function CompanySection({ companies }: CompanySectionProps) {
    const { showSuccess, showError } = useNotifications();
    const [selectedCompany, setSelectedCompany] = useState<Company | null>(
        companies.length > 0 ? companies[0] : null
    );
    const [expandedCard, setExpandedCard] = useState<CardSection>(null);
    const [formData, setFormData] = useState<CompanyFormData>({
        name: selectedCompany?.name || '',
        logo: selectedCompany?.logo || '',
        phone: '',
        email: '',
        website: '',
        address1: '',
        address2: '',
        city: '',
        provinceState: '',
        country: '',
        postalCode: '',
        bankDocument: '',
        facturationDocument: ''
    });
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [previewUrl, setPreviewUrl] = useState<string>(selectedCompany?.logo || '');

    // Update preview URL when selected company changes
    useEffect(() => {
        setPreviewUrl(prev => {
            if (prev && prev.startsWith('blob:')) {
                URL.revokeObjectURL(prev);
            }
            return selectedCompany?.logo || '';
        });
    }, [selectedCompany]);

    const handleCompanySelect = (company: Company) => {
        setSelectedCompany(company);
        setFormData({
            name: company.name,
            logo: company.logo || '',
            phone: '',
            email: '',
            website: '',
            address1: '',
            address2: '',
            city: '',
            provinceState: '',
            country: '',
            postalCode: '',
            bankDocument: '',
            facturationDocument: ''
        });
        setSelectedFile(null);
        if (previewUrl && previewUrl.startsWith('blob:')) {
            URL.revokeObjectURL(previewUrl);
        }
        setPreviewUrl(company.logo || '');
        setExpandedCard(null);
    };

    const handleInputChange = (field: keyof CompanyFormData, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            // Validate file type
            if (!file.type.startsWith('image/')) {
                showError('Please select an image file');
                e.target.value = '';
                return;
            }
            
            // Validate file size (limit to 5MB)
            if (file.size > 5 * 1024 * 1024) {
                showError('File size must be less than 5MB');
                e.target.value = '';
                return;
            }

            // Revoke previous object URL before creating a new one
            if (previewUrl && previewUrl.startsWith('blob:')) {
                URL.revokeObjectURL(previewUrl);
            }

            setSelectedFile(file);
            
            // Create preview URL (do not store blob URL in form data)
            const objectUrl = URL.createObjectURL(file);
            setPreviewUrl(objectUrl);
        }
    };

    const handleSave = () => {
        // TODO: Implement actual file upload to server
        // Currently stores blob URL temporarily for preview purposes
        // In production, should upload selectedFile to server and update with permanent URL
        console.log('Saving company data:', formData);
        if (selectedFile) {
            console.log('File to upload:', selectedFile.name, selectedFile.type, selectedFile.size);
        }
        setExpandedCard(null);
        // Show success message with toast notification
        showSuccess('Company information updated successfully!');
    };

    const handleCancel = () => {
        if (selectedCompany) {
            setFormData({
                name: selectedCompany.name,
                logo: selectedCompany.logo || '',
                phone: '',
                email: '',
                website: '',
                address1: '',
                address2: '',
                city: '',
                provinceState: '',
                country: '',
                postalCode: '',
                bankDocument: '',
                facturationDocument: ''
            });
            // Revoke any existing blob URL used for preview to avoid memory leaks
            if (previewUrl && previewUrl.startsWith('blob:')) {
                URL.revokeObjectURL(previewUrl);
            }
            setSelectedFile(null);
            setPreviewUrl(selectedCompany.logo || '');
        }
        setExpandedCard(null);
    };

    const toggleCard = (card: CardSection) => {
        setExpandedCard(expandedCard === card ? null : card);
    };

    if (!selectedCompany) {
        return (
            <div className="section-container">
                <p className="no-data-message">No companies found. Please create a company first to access this section.</p>
            </div>
        );
    }

    return (
        <div className="section-container">
            {companies.length > 1 && (
                <div className="company-selector">
                    <label className="company-selector-label">
                        Select Company:
                    </label>
                    <select
                        value={selectedCompany.id}
                        onChange={(e) => {
                            const company = companies.find(c => c.id === e.target.value);
                            if (company) handleCompanySelect(company);
                        }}
                        className="company-selector-select"
                    >
                        {companies.map(company => (
                            <option key={company.id} value={company.id}>
                                {company.name}
                            </option>
                        ))}
                    </select>
                </div>
            )}

            <div className="company-header">
                <h2>{selectedCompany.name}</h2>
                <p className="company-created">
                    Created: {new Date(selectedCompany.createdAt).toLocaleDateString()}
                </p>
            </div>

            <div className="company-cards-container">
                {/* Basic Information Card */}
                <div className={`company-card ${expandedCard === 'basic' ? 'expanded' : ''}`}>
                    <div className="company-card-header" onClick={() => toggleCard('basic')}>
                        <div className="company-card-title">
                            <h3>Basic Information</h3>
                            <p className="company-card-description">
                                Company name and branding details
                            </p>
                        </div>
                        <span className="company-card-icon">
                            {expandedCard === 'basic' ? '−' : '+'}
                        </span>
                    </div>
                    {expandedCard === 'basic' && (
                        <div className="company-card-content">
                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Company Name *
                                </label>
                                <input
                                    type="text"
                                    value={formData.name}
                                    onChange={(e) => handleInputChange('name', e.target.value)}
                                    className="company-form-input"
                                />
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Company Logo
                                </label>
                                <div className="company-logo-upload-container">
                                    <input
                                        type="file"
                                        id="logo-file"
                                        accept="image/*"
                                        onChange={handleFileChange}
                                        className="company-file-input"
                                    />
                                    <label htmlFor="logo-file" className="company-file-input-label">
                                        {selectedFile ? selectedFile.name : 'Choose logo image'}
                                    </label>
                                    
                                    {previewUrl ? (
                                        <div className="company-logo-preview">
                                            <img src={previewUrl} alt="Company logo preview" className="company-preview-image" />
                                        </div>
                                    ) : (
                                        <div className="company-no-logo-message">
                                            No logo uploaded. Click "Choose logo image" to select an image from your device.
                                        </div>
                                    )}
                                </div>
                            </div>

                            <div className="company-card-actions">
                                <button
                                    onClick={handleCancel}
                                    className="company-action-button company-action-button--cancel"
                                >
                                    Cancel
                                </button>
                                <button
                                    onClick={handleSave}
                                    className="company-action-button company-action-button--save"
                                >
                                    Save Changes
                                </button>
                            </div>
                        </div>
                    )}
                </div>

                {/* Contact Information Card */}
                <div className={`company-card ${expandedCard === 'contact' ? 'expanded' : ''}`}>
                    <div className="company-card-header" onClick={() => toggleCard('contact')}>
                        <div className="company-card-title">
                            <h3>Contact Information</h3>
                            <p className="company-card-description">
                                Phone, email, and website details
                            </p>
                        </div>
                        <span className="company-card-icon">
                            {expandedCard === 'contact' ? '−' : '+'}
                        </span>
                    </div>
                    {expandedCard === 'contact' && (
                        <div className="company-card-content">
                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Phone Number
                                </label>
                                <input
                                    type="tel"
                                    value={formData.phone}
                                    onChange={(e) => handleInputChange('phone', e.target.value)}
                                    className="company-form-input"
                                    placeholder="+1 (555) 123-4567"
                                />
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Email Address
                                </label>
                                <input
                                    type="email"
                                    value={formData.email}
                                    onChange={(e) => handleInputChange('email', e.target.value)}
                                    className="company-form-input"
                                    placeholder="contact@company.com"
                                />
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Website
                                </label>
                                <input
                                    type="url"
                                    value={formData.website}
                                    onChange={(e) => handleInputChange('website', e.target.value)}
                                    className="company-form-input"
                                    placeholder="https://www.company.com"
                                />
                            </div>

                            <div className="company-card-actions">
                                <button
                                    onClick={handleCancel}
                                    className="company-action-button company-action-button--cancel"
                                >
                                    Cancel
                                </button>
                                <button
                                    onClick={handleSave}
                                    className="company-action-button company-action-button--save"
                                >
                                    Save Changes
                                </button>
                            </div>
                        </div>
                    )}
                </div>

                {/* Business Address Card */}
                <div className={`company-card ${expandedCard === 'address' ? 'expanded' : ''}`}>
                    <div className="company-card-header" onClick={() => toggleCard('address')}>
                        <div className="company-card-title">
                            <h3>Business Address</h3>
                            <p className="company-card-description">
                                Physical location and mailing address
                            </p>
                        </div>
                        <span className="company-card-icon">
                            {expandedCard === 'address' ? '−' : '+'}
                        </span>
                    </div>
                    {expandedCard === 'address' && (
                        <div className="company-card-content">
                            <div className="company-address-grid">
                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Address Line 1
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.address1}
                                        onChange={(e) => handleInputChange('address1', e.target.value)}
                                        className="company-form-input"
                                        placeholder="123 Main Street"
                                    />
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Address Line 2
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.address2}
                                        onChange={(e) => handleInputChange('address2', e.target.value)}
                                        className="company-form-input"
                                        placeholder="Suite 100 (optional)"
                                    />
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        City
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.city}
                                        onChange={(e) => handleInputChange('city', e.target.value)}
                                        className="company-form-input"
                                        placeholder="Toronto"
                                    />
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Province/State
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.provinceState}
                                        onChange={(e) => handleInputChange('provinceState', e.target.value)}
                                        className="company-form-input"
                                        placeholder="Ontario"
                                    />
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Country
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.country}
                                        onChange={(e) => handleInputChange('country', e.target.value)}
                                        className="company-form-input"
                                        placeholder="Canada"
                                    />
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Postal Code
                                    </label>
                                    <input
                                        type="text"
                                        value={formData.postalCode}
                                        onChange={(e) => handleInputChange('postalCode', e.target.value)}
                                        className="company-form-input"
                                        placeholder="M5V 3A8"
                                    />
                                </div>
                            </div>

                            <div className="company-card-actions">
                                <button
                                    onClick={handleCancel}
                                    className="company-action-button company-action-button--cancel"
                                >
                                    Cancel
                                </button>
                                <button
                                    onClick={handleSave}
                                    className="company-action-button company-action-button--save"
                                >
                                    Save Changes
                                </button>
                            </div>
                        </div>
                    )}
                </div>

                {/* Owner Information Card */}
                <div className={`company-card ${expandedCard === 'owner' ? 'expanded' : ''}`}>
                    <div className="company-card-header" onClick={() => toggleCard('owner')}>
                        <div className="company-card-title">
                            <h3>Owner Information</h3>
                            <p className="company-card-description">
                                Bank and facturation documents
                            </p>
                        </div>
                        <span className="company-card-icon">
                            {expandedCard === 'owner' ? '−' : '+'}
                        </span>
                    </div>
                    {expandedCard === 'owner' && (
                        <div className="company-card-content">
                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Bank Document
                                </label>
                                <input
                                    type="text"
                                    value={formData.bankDocument}
                                    onChange={(e) => handleInputChange('bankDocument', e.target.value)}
                                    className="company-form-input"
                                    placeholder="Bank document reference or upload"
                                />
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Facturation Document
                                </label>
                                <input
                                    type="text"
                                    value={formData.facturationDocument}
                                    onChange={(e) => handleInputChange('facturationDocument', e.target.value)}
                                    className="company-form-input"
                                    placeholder="Facturation document reference or upload"
                                />
                            </div>

                            <div className="company-card-actions">
                                <button
                                    onClick={handleCancel}
                                    className="company-action-button company-action-button--cancel"
                                >
                                    Cancel
                                </button>
                                <button
                                    onClick={handleSave}
                                    className="company-action-button company-action-button--save"
                                >
                                    Save Changes
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default CompanySection;