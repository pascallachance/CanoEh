import { useState, useEffect, useCallback, useRef } from 'react';
import { useNotifications } from '../../contexts/useNotifications';
import { toAbsoluteUrl } from '../../utils/urlUtils';
import { ApiClient } from '../../utils/apiClient';
import { formatDate } from '../../utils/dateUtils';
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

interface CompanyDetailsResponse {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
    companyPhone?: string;
    email: string;
    webSite?: string;
    address1?: string;
    address2?: string;
    address3?: string;
    city?: string;
    provinceState?: string;
    country?: string;
    postalCode?: string;
    bankDocument?: string;
    facturationDocument?: string;
    // Additional fields from the Company entity
    countryOfCitizenship?: string;
    fullBirthName?: string;
    countryOfBirth?: string;
    birthDate?: string;
    identityDocumentType?: string;
    identityDocument?: string;
    companyType?: string;
}

interface CompanySectionProps {
    companies: Company[];
    onCompanyUpdate?: (updatedCompany: Company) => void;
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

/**
 * Constructs the logo path for a company based on its ID.
 * According to IMAGE_STORAGE_STRUCTURE.md, company logos are stored at:
 * /uploads/{CompanyID}/{CompanyID}_logo.jpg
 * 
 * @param companyId - The company ID
 * @returns The relative path to the logo, or empty string if companyId is falsy
 */
function getCompanyLogoPath(companyId: string | undefined): string {
    if (!companyId) {
        return '';
    }
    return `/uploads/${companyId}/${companyId}_logo.jpg`;
}

function CompanySection({ companies, onCompanyUpdate }: CompanySectionProps) {
    const { showSuccess, showError } = useNotifications();
    const [selectedCompany, setSelectedCompany] = useState<Company | null>(
        companies.length > 0 ? companies[0] : null
    );
    const [expandedCard, setExpandedCard] = useState<CardSection>(null);
    const [companyDetails, setCompanyDetails] = useState<CompanyDetailsResponse | null>(null);
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
    const [previewUrl, setPreviewUrl] = useState<string>(
        toAbsoluteUrl(getCompanyLogoPath(selectedCompany?.id))
    );
    
    // Use ref to track preview URL for cleanup without causing handleCancel to recreate
    const previewUrlRef = useRef<string>(previewUrl);
    
    // Keep ref in sync with state
    useEffect(() => {
        previewUrlRef.current = previewUrl;
    }, [previewUrl]);

    // Fetch complete company data from API
    const fetchCompanyData = useCallback(async (companyId: string) => {
        try {
            const response = await ApiClient.get(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Company/GetMyCompany`
            );

            if (!response.ok) {
                console.error('Failed to fetch company data:', response.status, response.statusText);
                showError('Failed to load company data');
                return;
            }

            // GetMyCompany returns an array of companies owned by the user
            // According to the API, a user can have multiple companies
            const companies: CompanyDetailsResponse[] = await response.json();
            
            // Find the currently selected company in the response
            const currentCompanyData = companies.find(c => c.id === companyId);
            
            if (currentCompanyData) {
                setCompanyDetails(currentCompanyData);
                // Update form data with fetched values
                setFormData({
                    name: currentCompanyData.name || '',
                    logo: currentCompanyData.logo || '',
                    phone: currentCompanyData.companyPhone || '',
                    email: currentCompanyData.email || '',
                    website: currentCompanyData.webSite || '',
                    address1: currentCompanyData.address1 || '',
                    address2: currentCompanyData.address2 || '',
                    city: currentCompanyData.city || '',
                    provinceState: currentCompanyData.provinceState || '',
                    country: currentCompanyData.country || '',
                    postalCode: currentCompanyData.postalCode || '',
                    bankDocument: currentCompanyData.bankDocument || '',
                    facturationDocument: currentCompanyData.facturationDocument || ''
                });
            } else {
                console.warn('Selected company not found in API response');
            }
        } catch (error) {
            console.error('Error fetching company data:', error);
            showError('An error occurred while loading company data');
        }
    }, [showError]);

    // Fetch company data when component mounts or selected company ID changes
    useEffect(() => {
        if (selectedCompany?.id) {
            fetchCompanyData(selectedCompany.id);
        }
    }, [selectedCompany?.id]); // eslint-disable-line react-hooks/exhaustive-deps

    // Update preview URL when selected company changes
    // Always construct the logo path based on company ID to check for stored logo
    useEffect(() => {
        setPreviewUrl(prev => {
            if (prev && prev.startsWith('blob:')) {
                URL.revokeObjectURL(prev);
            }
            return toAbsoluteUrl(getCompanyLogoPath(selectedCompany?.id));
        });
    }, [selectedCompany]);

    // Cancel handler - defined before use in useEffect
    const handleCancel = useCallback(() => {
        if (selectedCompany) {
            if (companyDetails) {
                // Restore from fetched company details if available
                setFormData({
                    name: companyDetails.name || '',
                    logo: companyDetails.logo || '',
                    phone: companyDetails.companyPhone || '',
                    email: companyDetails.email || '',
                    website: companyDetails.webSite || '',
                    address1: companyDetails.address1 || '',
                    address2: companyDetails.address2 || '',
                    city: companyDetails.city || '',
                    provinceState: companyDetails.provinceState || '',
                    country: companyDetails.country || '',
                    postalCode: companyDetails.postalCode || '',
                    bankDocument: companyDetails.bankDocument || '',
                    facturationDocument: companyDetails.facturationDocument || ''
                });
            } else {
                // Fallback to basic company data if details haven't loaded
                setFormData({
                    name: selectedCompany.name || '',
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
            }
            // Revoke any existing blob URL used for preview to avoid memory leaks
            const currentPreviewUrl = previewUrlRef.current;
            if (currentPreviewUrl && currentPreviewUrl.startsWith('blob:')) {
                URL.revokeObjectURL(currentPreviewUrl);
            }
            setSelectedFile(null);
            // Construct logo path based on company ID
            setPreviewUrl(toAbsoluteUrl(getCompanyLogoPath(selectedCompany.id)));
        }
        setExpandedCard(null);
    }, [selectedCompany, companyDetails]);

    // Handle Escape key to close expanded card for keyboard accessibility
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && expandedCard) {
                handleCancel();
            }
        };

        if (expandedCard) {
            document.addEventListener('keydown', handleEscape);
            // Prevent body scroll when modal is open
            document.body.style.overflow = 'hidden';
        }

        return () => {
            document.removeEventListener('keydown', handleEscape);
            // Restore body scroll when modal closes
            document.body.style.overflow = '';
        };
    }, [expandedCard, handleCancel]);

    const handleCompanySelect = (company: Company) => {
        setSelectedCompany(company);
        // Form data will be populated by fetchCompanyData useEffect
        setSelectedFile(null);
        if (previewUrl && previewUrl.startsWith('blob:')) {
            URL.revokeObjectURL(previewUrl);
        }
        // Construct logo path based on company ID
        setPreviewUrl(toAbsoluteUrl(getCompanyLogoPath(company.id)));
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

    const handleSave = async () => {
        if (!selectedCompany) {
            showError('No company selected');
            return;
        }

        // Validate required fields before making API call
        if (!formData.name || formData.name.trim() === '') {
            showError('Company name is required');
            return;
        }

        if (formData.name.length > 255) {
            showError('Company name must be 255 characters or less');
            return;
        }

        if (!formData.email || formData.email.trim() === '') {
            showError('Email is required');
            return;
        }

        if (formData.email.length > 255) {
            showError('Email must be 255 characters or less');
            return;
        }

        // Basic email validation
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(formData.email)) {
            showError('Please enter a valid email address');
            return;
        }

        // Allow save even if companyDetails is not loaded (e.g., for logo-only updates)
        // For fields not in companyDetails, we'll use empty strings or preserve what's in the form
        
        try {
            // Upload logo file if selected
            if (selectedFile) {
                const logoUploadFormData = new FormData();
                logoUploadFormData.append('file', selectedFile);

                const uploadResponse = await fetch(
                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Company/UploadLogo?companyId=${selectedCompany.id}`,
                    {
                        method: 'POST',
                        credentials: 'include',
                        body: logoUploadFormData,
                    }
                );

                if (!uploadResponse.ok) {
                    const errorText = await uploadResponse.text();
                    console.error(`Failed to upload logo for company ${selectedCompany.id}: ${uploadResponse.status} ${uploadResponse.statusText}`, errorText);
                    showError(`Failed to upload logo: ${errorText || uploadResponse.statusText}`);
                    return;
                }

                const uploadResult = await uploadResponse.json();
                const logoUrl = uploadResult.logoUrl;
                console.log('Logo uploaded successfully:', logoUrl);

                // Update form data with the permanent URL
                setFormData(prev => ({ ...prev, logo: logoUrl }));

                // Clear the selected file and blob URL
                setSelectedFile(null);
                if (previewUrl && previewUrl.startsWith('blob:')) {
                    URL.revokeObjectURL(previewUrl);
                }
                setPreviewUrl(logoUrl);
            }

            // Prepare update request with all form data
            // Use companyDetails if available, otherwise use minimal data
            const updateRequest = {
                id: selectedCompany.id,
                name: formData.name,
                description: companyDetails?.description,
                logo: formData.logo,
                email: formData.email,
                CompanyPhone: formData.phone,
                WebSite: formData.website,
                Address1: formData.address1,
                Address2: formData.address2,
                Address3: companyDetails?.address3,
                city: formData.city,
                ProvinceState: formData.provinceState,
                country: formData.country,
                PostalCode: formData.postalCode,
                BankDocument: formData.bankDocument,
                FacturationDocument: formData.facturationDocument,
                // Preserve existing fields that are not in the form (only if companyDetails is loaded)
                CountryOfCitizenship: companyDetails?.countryOfCitizenship,
                FullBirthName: companyDetails?.fullBirthName,
                CountryOfBirth: companyDetails?.countryOfBirth,
                BirthDate: companyDetails?.birthDate,
                IdentityDocumentType: companyDetails?.identityDocumentType,
                IdentityDocument: companyDetails?.identityDocument,
                CompanyType: companyDetails?.companyType
            };

            // Call UpdateMyCompany API
            const updateResponse = await ApiClient.put(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Company/UpdateMyCompany`,
                updateRequest
            );

            if (!updateResponse.ok) {
                const errorText = await updateResponse.text();
                console.error(`Failed to update company ${selectedCompany.id}: ${updateResponse.status} ${updateResponse.statusText}`, errorText);
                showError(`Failed to update company: ${errorText || updateResponse.statusText}`);

                // Roll back logo-related client state so UI matches persisted company data
                setFormData(prev => ({
                    ...prev,
                    logo: selectedCompany.logo || ''
                }));
                setPreviewUrl(
                    selectedCompany.logo
                        ? toAbsoluteUrl(selectedCompany.logo)
                        : ''
                );
                return;
            }

            const updateResult: CompanyDetailsResponse = await updateResponse.json();
            console.log('Company updated successfully:', updateResult);

            // Update the company state with the new data from the server response
            const updatedCompany = {
                id: updateResult.id,
                ownerID: updateResult.ownerID,
                name: updateResult.name,
                description: updateResult.description,
                logo: updateResult.logo,
                createdAt: updateResult.createdAt,
                updatedAt: updateResult.updatedAt
            };
            setSelectedCompany(updatedCompany);

            // Update company details with the full response
            setCompanyDetails(updateResult);

            // Notify parent component of the update
            if (onCompanyUpdate) {
                onCompanyUpdate(updatedCompany);
            }

            showSuccess('Company information updated successfully!');
            setExpandedCard(null);
        } catch (error) {
            console.error('Error saving company data:', error);
            showError('An error occurred while saving company data');
        }
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
                    Created: {formatDate(selectedCompany.createdAt)}
                </p>
            </div>

            <div className="company-cards-container">
                {/* Basic Information Card */}
                <div 
                    className={`company-card ${expandedCard === 'basic' ? 'expanded' : ''}`}
                    role={expandedCard === 'basic' ? 'dialog' : undefined}
                    aria-modal={expandedCard === 'basic' ? 'true' : undefined}
                    aria-labelledby={expandedCard === 'basic' ? 'basic-info-title' : undefined}
                >
                    <div className="company-card-header" onClick={() => toggleCard('basic')}>
                        <div className="company-card-title">
                            <h3 id="basic-info-title">Basic Information</h3>
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
                <div 
                    className={`company-card ${expandedCard === 'contact' ? 'expanded' : ''}`}
                    role={expandedCard === 'contact' ? 'dialog' : undefined}
                    aria-modal={expandedCard === 'contact' ? 'true' : undefined}
                    aria-labelledby={expandedCard === 'contact' ? 'contact-info-title' : undefined}
                >
                    <div className="company-card-header" onClick={() => toggleCard('contact')}>
                        <div className="company-card-title">
                            <h3 id="contact-info-title">Contact Information</h3>
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
                <div 
                    className={`company-card ${expandedCard === 'address' ? 'expanded' : ''}`}
                    role={expandedCard === 'address' ? 'dialog' : undefined}
                    aria-modal={expandedCard === 'address' ? 'true' : undefined}
                    aria-labelledby={expandedCard === 'address' ? 'address-info-title' : undefined}
                >
                    <div className="company-card-header" onClick={() => toggleCard('address')}>
                        <div className="company-card-title">
                            <h3 id="address-info-title">Business Address</h3>
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
                <div 
                    className={`company-card ${expandedCard === 'owner' ? 'expanded' : ''}`}
                    role={expandedCard === 'owner' ? 'dialog' : undefined}
                    aria-modal={expandedCard === 'owner' ? 'true' : undefined}
                    aria-labelledby={expandedCard === 'owner' ? 'owner-info-title' : undefined}
                >
                    <div className="company-card-header" onClick={() => toggleCard('owner')}>
                        <div className="company-card-title">
                            <h3 id="owner-info-title">Owner Information</h3>
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
            
            {/* Modal backdrop - rendered last for proper z-index layering */}
            {expandedCard && (
                <div 
                    className="company-backdrop" 
                    onClick={handleCancel}
                    role="presentation"
                    aria-hidden="true"
                />
            )}
        </div>
    );
}

export default CompanySection;