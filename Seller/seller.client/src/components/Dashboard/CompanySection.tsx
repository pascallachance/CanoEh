import { useState } from 'react';
import { useNotifications } from '../../contexts/useNotifications';

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
                <h2 className="section-title">Company Management</h2>
                <p className="section-description">
                    No companies found. Please create a company first to access this section.
                </p>
            </div>
        );
    }

    return (
        <div className="section-container">
            <h2 className="section-title">Company Management</h2>
            <p className="section-description">
                Manage your company information, contact details, and business settings.
            </p>

            {companies.length > 1 && (
                <div style={{ marginBottom: '2rem' }}>
                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                        Select Company:
                    </label>
                    <select
                        value={selectedCompany.id}
                        onChange={(e) => {
                            const company = companies.find(c => c.id === e.target.value);
                            if (company) handleCompanySelect(company);
                        }}
                        style={{
                            padding: '0.75rem',
                            border: '1px solid #ced4da',
                            borderRadius: '4px',
                            fontSize: '1rem',
                            minWidth: '250px'
                        }}
                    >
                        {companies.map(company => (
                            <option key={company.id} value={company.id}>
                                {company.name}
                            </option>
                        ))}
                    </select>
                </div>
            )}

            <div style={{
                background: 'white',
                border: '1px solid #e1e5e9',
                borderRadius: '8px',
                overflow: 'hidden'
            }}>
                <div style={{
                    padding: '1.5rem',
                    background: '#f8f9fa',
                    borderBottom: '1px solid #e1e5e9',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center'
                }}>
                    <div>
                        <h3 style={{ margin: 0 }}>{selectedCompany.name}</h3>
                        <p style={{ margin: '0.25rem 0 0 0', color: '#6c757d', fontSize: '0.9rem' }}>
                            Created: {new Date(selectedCompany.createdAt).toLocaleDateString()}
                        </p>
                    </div>
                    <button
                        onClick={() => setIsEditing(!isEditing)}
                        style={{
                            padding: '0.75rem 1.5rem',
                            background: isEditing ? '#6c757d' : '#007bff',
                            color: 'white',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '1rem'
                        }}
                    >
                        {isEditing ? 'Cancel' : 'Edit Details'}
                    </button>
                </div>

                <div style={{ padding: '2rem' }}>
                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '2rem' }}>
                        {/* Basic Information */}
                        <div>
                            <h4 style={{ marginTop: 0, marginBottom: '1.5rem', color: '#333' }}>Basic Information</h4>
                            
                            <div style={{ marginBottom: '1rem' }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Company Name *
                                </label>
                                {isEditing ? (
                                    <input
                                        type="text"
                                        value={formData.name}
                                        onChange={(e) => handleInputChange('name', e.target.value)}
                                        style={{
                                            width: '100%',
                                            padding: '0.75rem',
                                            border: '1px solid #ced4da',
                                            borderRadius: '4px',
                                            fontSize: '1rem'
                                        }}
                                    />
                                ) : (
                                    <div style={{ 
                                        padding: '0.75rem',
                                        background: '#f8f9fa',
                                        border: '1px solid #e9ecef',
                                        borderRadius: '4px'
                                    }}>
                                        {formData.name}
                                    </div>
                                )}
                            </div>

                            <div style={{ marginBottom: '1rem' }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Description
                                </label>
                                {isEditing ? (
                                    <textarea
                                        value={formData.description}
                                        onChange={(e) => handleInputChange('description', e.target.value)}
                                        style={{
                                            width: '100%',
                                            padding: '0.75rem',
                                            border: '1px solid #ced4da',
                                            borderRadius: '4px',
                                            fontSize: '1rem',
                                            minHeight: '100px',
                                            resize: 'vertical'
                                        }}
                                        placeholder="Brief description of your company"
                                    />
                                ) : (
                                    <div style={{ 
                                        padding: '0.75rem',
                                        background: '#f8f9fa',
                                        border: '1px solid #e9ecef',
                                        borderRadius: '4px',
                                        minHeight: '60px'
                                    }}>
                                        {formData.description || 'No description provided'}
                                    </div>
                                )}
                            </div>

                            <div style={{ marginBottom: '1rem' }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Logo URL
                                </label>
                                {isEditing ? (
                                    <input
                                        type="url"
                                        value={formData.logo}
                                        onChange={(e) => handleInputChange('logo', e.target.value)}
                                        style={{
                                            width: '100%',
                                            padding: '0.75rem',
                                            border: '1px solid #ced4da',
                                            borderRadius: '4px',
                                            fontSize: '1rem'
                                        }}
                                        placeholder="https://example.com/logo.png"
                                    />
                                ) : (
                                    <div style={{ 
                                        padding: '0.75rem',
                                        background: '#f8f9fa',
                                        border: '1px solid #e9ecef',
                                        borderRadius: '4px'
                                    }}>
                                        {formData.logo || 'No logo URL provided'}
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Contact Information */}
                        <div>
                            <h4 style={{ marginTop: 0, marginBottom: '1.5rem', color: '#333' }}>Contact Information</h4>
                            
                            <div style={{ marginBottom: '1rem' }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Phone Number
                                </label>
                                {isEditing ? (
                                    <input
                                        type="tel"
                                        value={formData.phone}
                                        onChange={(e) => handleInputChange('phone', e.target.value)}
                                        style={{
                                            width: '100%',
                                            padding: '0.75rem',
                                            border: '1px solid #ced4da',
                                            borderRadius: '4px',
                                            fontSize: '1rem'
                                        }}
                                        placeholder="+1 (555) 123-4567"
                                    />
                                ) : (
                                    <div style={{ 
                                        padding: '0.75rem',
                                        background: '#f8f9fa',
                                        border: '1px solid #e9ecef',
                                        borderRadius: '4px'
                                    }}>
                                        {formData.phone || 'No phone number provided'}
                                    </div>
                                )}
                            </div>

                            <div style={{ marginBottom: '1rem' }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Email Address
                                </label>
                                {isEditing ? (
                                    <input
                                        type="email"
                                        value={formData.email}
                                        onChange={(e) => handleInputChange('email', e.target.value)}
                                        style={{
                                            width: '100%',
                                            padding: '0.75rem',
                                            border: '1px solid #ced4da',
                                            borderRadius: '4px',
                                            fontSize: '1rem'
                                        }}
                                        placeholder="contact@company.com"
                                    />
                                ) : (
                                    <div style={{ 
                                        padding: '0.75rem',
                                        background: '#f8f9fa',
                                        border: '1px solid #e9ecef',
                                        borderRadius: '4px'
                                    }}>
                                        {formData.email || 'No email address provided'}
                                    </div>
                                )}
                            </div>

                            <div style={{ marginBottom: '1rem' }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Website
                                </label>
                                {isEditing ? (
                                    <input
                                        type="url"
                                        value={formData.website}
                                        onChange={(e) => handleInputChange('website', e.target.value)}
                                        style={{
                                            width: '100%',
                                            padding: '0.75rem',
                                            border: '1px solid #ced4da',
                                            borderRadius: '4px',
                                            fontSize: '1rem'
                                        }}
                                        placeholder="https://www.company.com"
                                    />
                                ) : (
                                    <div style={{ 
                                        padding: '0.75rem',
                                        background: '#f8f9fa',
                                        border: '1px solid #e9ecef',
                                        borderRadius: '4px'
                                    }}>
                                        {formData.website || 'No website provided'}
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Address Information */}
                        <div style={{ gridColumn: 'span 2' }}>
                            <h4 style={{ marginTop: 0, marginBottom: '1.5rem', color: '#333' }}>Business Address</h4>
                            
                            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1rem' }}>
                                <div>
                                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                        Address Line 1
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.address1}
                                            onChange={(e) => handleInputChange('address1', e.target.value)}
                                            style={{
                                                width: '100%',
                                                padding: '0.75rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '1rem'
                                            }}
                                            placeholder="123 Main Street"
                                        />
                                    ) : (
                                        <div style={{ 
                                            padding: '0.75rem',
                                            background: '#f8f9fa',
                                            border: '1px solid #e9ecef',
                                            borderRadius: '4px'
                                        }}>
                                            {formData.address1 || 'No address provided'}
                                        </div>
                                    )}
                                </div>

                                <div>
                                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                        Address Line 2
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.address2}
                                            onChange={(e) => handleInputChange('address2', e.target.value)}
                                            style={{
                                                width: '100%',
                                                padding: '0.75rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '1rem'
                                            }}
                                            placeholder="Suite 100 (optional)"
                                        />
                                    ) : (
                                        <div style={{ 
                                            padding: '0.75rem',
                                            background: '#f8f9fa',
                                            border: '1px solid #e9ecef',
                                            borderRadius: '4px'
                                        }}>
                                            {formData.address2 || 'Not provided'}
                                        </div>
                                    )}
                                </div>

                                <div>
                                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                        City
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.city}
                                            onChange={(e) => handleInputChange('city', e.target.value)}
                                            style={{
                                                width: '100%',
                                                padding: '0.75rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '1rem'
                                            }}
                                            placeholder="Toronto"
                                        />
                                    ) : (
                                        <div style={{ 
                                            padding: '0.75rem',
                                            background: '#f8f9fa',
                                            border: '1px solid #e9ecef',
                                            borderRadius: '4px'
                                        }}>
                                            {formData.city || 'No city provided'}
                                        </div>
                                    )}
                                </div>

                                <div>
                                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                        Province/State
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.provinceState}
                                            onChange={(e) => handleInputChange('provinceState', e.target.value)}
                                            style={{
                                                width: '100%',
                                                padding: '0.75rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '1rem'
                                            }}
                                            placeholder="Ontario"
                                        />
                                    ) : (
                                        <div style={{ 
                                            padding: '0.75rem',
                                            background: '#f8f9fa',
                                            border: '1px solid #e9ecef',
                                            borderRadius: '4px'
                                        }}>
                                            {formData.provinceState || 'No province/state provided'}
                                        </div>
                                    )}
                                </div>

                                <div>
                                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                        Country
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.country}
                                            onChange={(e) => handleInputChange('country', e.target.value)}
                                            style={{
                                                width: '100%',
                                                padding: '0.75rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '1rem'
                                            }}
                                            placeholder="Canada"
                                        />
                                    ) : (
                                        <div style={{ 
                                            padding: '0.75rem',
                                            background: '#f8f9fa',
                                            border: '1px solid #e9ecef',
                                            borderRadius: '4px'
                                        }}>
                                            {formData.country || 'No country provided'}
                                        </div>
                                    )}
                                </div>

                                <div>
                                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                        Postal Code
                                    </label>
                                    {isEditing ? (
                                        <input
                                            type="text"
                                            value={formData.postalCode}
                                            onChange={(e) => handleInputChange('postalCode', e.target.value)}
                                            style={{
                                                width: '100%',
                                                padding: '0.75rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '1rem'
                                            }}
                                            placeholder="M5V 3A8"
                                        />
                                    ) : (
                                        <div style={{ 
                                            padding: '0.75rem',
                                            background: '#f8f9fa',
                                            border: '1px solid #e9ecef',
                                            borderRadius: '4px'
                                        }}>
                                            {formData.postalCode || 'No postal code provided'}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>

                    {isEditing && (
                        <div style={{ 
                            marginTop: '2rem', 
                            paddingTop: '2rem', 
                            borderTop: '1px solid #e1e5e9',
                            display: 'flex',
                            gap: '1rem',
                            justifyContent: 'flex-end'
                        }}>
                            <button
                                onClick={handleCancel}
                                style={{
                                    padding: '0.75rem 1.5rem',
                                    background: '#6c757d',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: 'pointer',
                                    fontSize: '1rem'
                                }}
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleSave}
                                style={{
                                    padding: '0.75rem 1.5rem',
                                    background: '#28a745',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: 'pointer',
                                    fontSize: '1rem'
                                }}
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