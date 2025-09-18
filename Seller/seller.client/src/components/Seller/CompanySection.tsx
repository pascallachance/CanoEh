import { useState } from 'react';
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
    description: string;
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
}

function CompanySection({ companies }: CompanySectionProps) {
    const { showSuccess } = useNotifications();
    const [selectedCompany, setSelectedCompany] = useState<Company | null>(
        companies.length > 0 ? companies[0] : null
    );
    const [isEditing, setIsEditing] = useState(false);
    const [formData, setFormData] = useState<CompanyFormData>({
        name: selectedCompany?.name || '',
        description: selectedCompany?.description || '',
        logo: selectedCompany?.logo || '',
        phone: '',
        email: '',
        website: '',
        address1: '',
        address2: '',
        city: '',
        provinceState: '',
        country: '',
        postalCode: ''
    });

    const handleCompanySelect = (company: Company) => {
        setSelectedCompany(company);
        setFormData({
            name: company.name,
            description: company.description || '',
            logo: company.logo || '',
            phone: '',
            email: '',
            website: '',
            address1: '',
            address2: '',
            city: '',
            provinceState: '',
            country: '',
            postalCode: ''
        });
        setIsEditing(false);
    };

    const handleInputChange = (field: keyof CompanyFormData, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
    };

    const handleSave = () => {
        // Here you would typically make an API call to update the company
        console.log('Saving company data:', formData);
        setIsEditing(false);
        // Show success message with toast notification
        showSuccess('Company information updated successfully!');
    };

    const handleCancel = () => {
        if (selectedCompany) {
            setFormData({
                name: selectedCompany.name,
                description: selectedCompany.description || '',
                logo: selectedCompany.logo || '',
                phone: '',
                email: '',
                website: '',
                address1: '',
                address2: '',
                city: '',
                provinceState: '',
                country: '',
                postalCode: ''
            });
        }
        setIsEditing(false);
    };

    if (!selectedCompany) {
        return (
            <div className="section-container">
                <p>No companies found. Please create a company first to access this section.</p>
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

            <div className="company-details-container">
                <div className="company-details-header">
                    <div className="company-details-info">
                        <h3>{selectedCompany.name}</h3>
                        <p className="company-details-created">
                            Created: {new Date(selectedCompany.createdAt).toLocaleDateString()}
                        </p>
                    </div>
                    <button
                        onClick={() => setIsEditing(!isEditing)}
                        className={`company-edit-button ${isEditing ? 'company-edit-button--cancel' : 'company-edit-button--edit'}`}
                    >
                        {isEditing ? 'Cancel' : 'Edit Details'}
                    </button>
                </div>

                <div className="company-details-content">
                    <div className="company-form-grid">
                        {/* Basic Information */}
                        <div className="company-form-section">
                            <h4>Basic Information</h4>
                            
                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Company Name *
                                </label>
                                {isEditing ? (
                                    <input
                                        type="text"
                                        value={formData.name}
                                        onChange={(e) => handleInputChange('name', e.target.value)}
                                        className="company-form-input"
                                    />
                                ) : (
                                    <div className="company-form-display">
                                        {formData.name}
                                    </div>
                                )}
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Description
                                </label>
                                {isEditing ? (
                                    <textarea
                                        value={formData.description}
                                        onChange={(e) => handleInputChange('description', e.target.value)}
                                        className="company-form-textarea"
                                        placeholder="Brief description of your company"
                                    />
                                ) : (
                                    <div className="company-form-display company-form-display--description">
                                        {formData.description || 'No description provided'}
                                    </div>
                                )}
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Logo URL
                                </label>
                                {isEditing ? (
                                    <input
                                        type="url"
                                        value={formData.logo}
                                        onChange={(e) => handleInputChange('logo', e.target.value)}
                                        className="company-form-input"
                                        placeholder="https://example.com/logo.png"
                                    />
                                ) : (
                                    <div className="company-form-display">
                                        {formData.logo || 'No logo URL provided'}
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Contact Information */}
                        <div className="company-form-section">
                            <h4>Contact Information</h4>
                            
                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Phone Number
                                </label>
                                {isEditing ? (
                                    <input
                                        type="tel"
                                        value={formData.phone}
                                        onChange={(e) => handleInputChange('phone', e.target.value)}
                                        className="company-form-input"
                                        placeholder="+1 (555) 123-4567"
                                    />
                                ) : (
                                    <div className="company-form-display">
                                        {formData.phone || 'No phone number provided'}
                                    </div>
                                )}
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Email Address
                                </label>
                                {isEditing ? (
                                    <input
                                        type="email"
                                        value={formData.email}
                                        onChange={(e) => handleInputChange('email', e.target.value)}
                                        className="company-form-input"
                                        placeholder="contact@company.com"
                                    />
                                ) : (
                                    <div className="company-form-display">
                                        {formData.email || 'No email address provided'}
                                    </div>
                                )}
                            </div>

                            <div className="company-form-group">
                                <label className="company-form-label">
                                    Website
                                </label>
                                {isEditing ? (
                                    <input
                                        type="url"
                                        value={formData.website}
                                        onChange={(e) => handleInputChange('website', e.target.value)}
                                        className="company-form-input"
                                        placeholder="https://www.company.com"
                                    />
                                ) : (
                                    <div className="company-form-display">
                                        {formData.website || 'No website provided'}
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Address Information */}
                        <div className="company-address-section">
                            <h4>Business Address</h4>
                            
                            <div className="company-address-grid">
                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Address Line 1
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.address1}
                                            onChange={(e) => handleInputChange('address1', e.target.value)}
                                            className="company-form-input"
                                            placeholder="123 Main Street"
                                        />
                                    ) : (
                                        <div className="company-form-display">
                                            {formData.address1 || 'No address provided'}
                                        </div>
                                    )}
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Address Line 2
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.address2}
                                            onChange={(e) => handleInputChange('address2', e.target.value)}
                                            className="company-form-input"
                                            placeholder="Suite 100 (optional)"
                                        />
                                    ) : (
                                        <div className="company-form-display">
                                            {formData.address2 || 'Not provided'}
                                        </div>
                                    )}
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        City
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.city}
                                            onChange={(e) => handleInputChange('city', e.target.value)}
                                            className="company-form-input"
                                            placeholder="Toronto"
                                        />
                                    ) : (
                                        <div className="company-form-display">
                                            {formData.city || 'No city provided'}
                                        </div>
                                    )}
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Province/State
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.provinceState}
                                            onChange={(e) => handleInputChange('provinceState', e.target.value)}
                                            className="company-form-input"
                                            placeholder="Ontario"
                                        />
                                    ) : (
                                        <div className="company-form-display">
                                            {formData.provinceState || 'No province/state provided'}
                                        </div>
                                    )}
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Country
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.country}
                                            onChange={(e) => handleInputChange('country', e.target.value)}
                                            className="company-form-input"
                                            placeholder="Canada"
                                        />
                                    ) : (
                                        <div className="company-form-display">
                                            {formData.country || 'No country provided'}
                                        </div>
                                    )}
                                </div>

                                <div className="company-form-group">
                                    <label className="company-form-label">
                                        Postal Code
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.postalCode}
                                            onChange={(e) => handleInputChange('postalCode', e.target.value)}
                                            className="company-form-input"
                                            placeholder="M5V 3A8"
                                        />
                                    ) : (
                                        <div className="company-form-display">
                                            {formData.postalCode || 'No postal code provided'}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>

                    {isEditing && (
                        <div className="company-form-actions">
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
                    )}
                </div>
            </div>
        </div>
    );
}

export default CompanySection;