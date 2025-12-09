import { useState, useEffect, useMemo } from 'react';
import './AddProductStep4.css';
import { ApiClient } from '../utils/apiClient';
import { validateBilingualArraySync, formatVariantAttribute } from '../utils/bilingualArrayUtils';
import { useNotifications } from '../contexts/useNotifications';
import type { AddProductStep1Data } from './AddProductStep1';
import type { AddProductStep2Data } from './AddProductStep2';
import type { AddProductStep3Data } from './AddProductStep3';

interface ItemVariant {
    id: string;
    attributes_en: Record<string, string>;
    attributes_fr: Record<string, string>;
    sku: string;
    price: number;
    stock: number;
    productIdentifierType?: string;
    productIdentifierValue?: string;
    thumbnailUrl?: string;
    imageUrls?: string[];
}

interface AddProductStep4Props {
    onSubmit: () => void;
    onBack: () => void;
    step1Data: AddProductStep1Data;
    step2Data: AddProductStep2Data;
    step3Data: AddProductStep3Data;
    companies: Array<{ id: string; ownerID: string; name: string }>;
}

function AddProductStep4({ onSubmit, onBack, step1Data, step2Data, step3Data, companies }: AddProductStep4Props) {
    const { showSuccess, showError } = useNotifications();
    const [variants, setVariants] = useState<ItemVariant[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState<string>('');

    // Product identifier types
    const identifierTypes = [
        { value: '', label: 'Select ID Type' },
        { value: 'UPC', label: 'UPC' },
        { value: 'EAN', label: 'EAN' },
        { value: 'GTIN', label: 'GTIN' },
        { value: 'ISBN', label: 'ISBN' },
        { value: 'ASIN', label: 'ASIN' },
        { value: 'SKU', label: 'SKU' },
        { value: 'MPN', label: 'MPN (Manufacturer Part Number)' }
    ];

    // Generate variants on mount
    useEffect(() => {
        const generated = generateVariants();
        setVariants(generated);
    }, [step3Data]);

    // Cleanup object URLs on component unmount
    useEffect(() => {
        return () => {
            variants.forEach(variant => {
                if (variant.thumbnailUrl && variant.thumbnailUrl.startsWith('blob:')) {
                    URL.revokeObjectURL(variant.thumbnailUrl);
                }
                if (variant.imageUrls) {
                    variant.imageUrls.forEach(url => {
                        if (url.startsWith('blob:')) {
                            URL.revokeObjectURL(url);
                        }
                    });
                }
            });
        };
    }, [variants]);

    const generateVariants = (): ItemVariant[] => {
        if (step3Data.attributes.length === 0) {
            return [{
                id: '1',
                attributes_en: {},
                attributes_fr: {},
                sku: '',
                price: 0,
                stock: 0,
                productIdentifierType: '',
                productIdentifierValue: '',
                thumbnailUrl: '',
                imageUrls: []
            }];
        }

        const combinations: { en: Record<string, string>, fr: Record<string, string> }[] = [];

        const generateCombinations = (attrIndex: number, currentEn: Record<string, string>, currentFr: Record<string, string>) => {
            if (attrIndex >= step3Data.attributes.length) {
                combinations.push({ en: { ...currentEn }, fr: { ...currentFr } });
                return;
            }

            const attribute = step3Data.attributes[attrIndex];

            // Ensure synchronized arrays
            const validation = validateBilingualArraySync(
                attribute.values_en,
                attribute.values_fr,
                { attributeName: attribute.name_en, errorType: 'console' }
            );
            if (!validation.isValid) {
                return;
            }

            for (let i = 0; i < attribute.values_en.length; i++) {
                const valueEn = attribute.values_en[i];
                const valueFr = attribute.values_fr[i];
                generateCombinations(
                    attrIndex + 1,
                    { ...currentEn, [attribute.name_en]: valueEn },
                    { ...currentFr, [attribute.name_fr]: valueFr }
                );
            }
        };

        generateCombinations(0, {}, {});

        return combinations.map((combo, index) => ({
            id: `variant-${index + 1}`,
            attributes_en: combo.en,
            attributes_fr: combo.fr,
            sku: '',
            price: 0,
            stock: 0,
            productIdentifierType: '',
            productIdentifierValue: '',
            thumbnailUrl: '',
            imageUrls: []
        }));
    };

    const updateVariant = (variantId: string, field: keyof Omit<ItemVariant, 'id' | 'attributes_en' | 'attributes_fr'>, value: string | number | string[]) => {
        setVariants(prev => prev.map(v =>
            v.id === variantId ? { ...v, [field]: value } : v
        ));
    };

    // Helper function to handle thumbnail file selection
    const handleThumbnailChange = (variantId: string, file: File | null) => {
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.thumbnailUrl && currentVariant.thumbnailUrl.startsWith('blob:')) {
            URL.revokeObjectURL(currentVariant.thumbnailUrl);
        }

        if (file) {
            const url = URL.createObjectURL(file);
            updateVariant(variantId, 'thumbnailUrl', url);
        } else {
            updateVariant(variantId, 'thumbnailUrl', '');
        }
    };

    // Helper function to handle product images file selection
    const handleImagesChange = (variantId: string, files: FileList | null) => {
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.imageUrls) {
            currentVariant.imageUrls.forEach(url => {
                if (url.startsWith('blob:')) {
                    URL.revokeObjectURL(url);
                }
            });
        }

        if (files && files.length > 0) {
            const fileArray = Array.from(files).slice(0, 10);
            const urls: string[] = [];
            try {
                fileArray.forEach(file => {
                    urls.push(URL.createObjectURL(file));
                });
                updateVariant(variantId, 'imageUrls', urls);
            } catch (error) {
                urls.forEach(url => {
                    if (url.startsWith('blob:')) {
                        URL.revokeObjectURL(url);
                    }
                });
                console.error('Error creating object URLs:', error);
                updateVariant(variantId, 'imageUrls', []);
            }
        } else {
            updateVariant(variantId, 'imageUrls', []);
        }
    };

    // Validation logic
    const isFormInvalid = useMemo(() => {
        if (variants.length > 0) {
            return variants.some(variant =>
                !variant.sku.trim() || variant.price <= 0
            );
        }
        return false;
    }, [variants]);

    const handleSaveItem = async () => {
        // Validate variants
        if (variants.length > 0) {
            const hasInvalidVariants = variants.some(variant =>
                !variant.sku.trim() || variant.price <= 0
            );

            if (hasInvalidVariants) {
                const errorMessage = 'Please ensure all variants have a SKU and price greater than 0.';
                setError(errorMessage);
                showError(errorMessage);
                return;
            }
        }

        const sellerId = companies.length > 0 ? companies[0].ownerID : null;
        if (!sellerId) {
            const errorMessage = 'Unable to determine seller ID. Please ensure you are logged in.';
            setError(errorMessage);
            showError(errorMessage);
            return;
        }

        setIsSaving(true);
        setError('');

        try {
            // Transform frontend data to match backend CreateItemRequest format
            // NOTE: Image URLs are currently blob: URLs from URL.createObjectURL()
            // These are only valid in the current browser session and won't work when sent to the API.
            // TODO: Implement proper file upload handling:
            // - Convert files to base64 strings, OR
            // - Upload files separately to get actual URLs from server, OR
            // - Use FormData for multipart/form-data upload
            const createItemRequest = {
                SellerID: sellerId,
                Name_en: step1Data.name,
                Name_fr: step1Data.name_fr,
                Description_en: step1Data.description,
                Description_fr: step1Data.description_fr,
                CategoryID: step2Data.categoryId,
                Variants: variants.map(variant => ({
                    Price: variant.price,
                    StockQuantity: variant.stock,
                    Sku: variant.sku,
                    ProductIdentifierType: variant.productIdentifierType || null,
                    ProductIdentifierValue: variant.productIdentifierValue || null,
                    // WARNING: blob: URLs will not work on the server
                    ImageUrls: variant.imageUrls?.join(',') || null,
                    ThumbnailUrl: variant.thumbnailUrl || null,
                    ItemVariantName_en: variant.attributes_en ? Object.entries(variant.attributes_en).map(([k, v]) => `${k}: ${v}`).join(', ') : null,
                    ItemVariantName_fr: variant.attributes_fr ? Object.entries(variant.attributes_fr).map(([k, v]) => `${k}: ${v}`).join(', ') : null,
                    ItemVariantAttributes: variant.attributes_en ? Object.entries(variant.attributes_en).map(([attrNameEn, attrValueEn]) => {
                        const itemAttribute = step3Data.attributes.find(attr => attr.name_en === attrNameEn);
                        const attrNameFr = itemAttribute?.name_fr || null;
                        const attrValueFr = attrNameFr && variant.attributes_fr ? variant.attributes_fr[attrNameFr] : null;
                        return {
                            AttributeName_en: attrNameEn,
                            AttributeName_fr: attrNameFr,
                            Attributes_en: attrValueEn,
                            Attributes_fr: attrValueFr
                        };
                    }) : [],
                    Deleted: false
                })),
                ItemAttributes: step2Data.itemAttributes.map(attr => ({
                    AttributeName_en: attr.name_en,
                    AttributeName_fr: attr.name_fr,
                    Attributes_en: attr.value_en,
                    Attributes_fr: attr.value_fr
                }))
            };

            // Call the API
            const response = await ApiClient.post(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/CreateItem`,
                createItemRequest
            );

            if (response.ok) {
                // Show success message that will persist after navigation
                showSuccess('Product created successfully!');
                // Navigate to products list
                onSubmit();
            } else {
                const errorText = await response.text();
                const errorMessage = `Failed to create item: ${errorText}`;
                setError(errorMessage);
                showError(errorMessage);
            }

        } catch (error) {
            console.error('Error creating item:', error);
            const errorMessage = 'An unexpected error occurred while creating the item.';
            setError(errorMessage);
            showError(errorMessage);
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="add-product-step4-container">
            <div className="add-product-step4-content">
                <header className="step-header">
                    <h1>Add New Product</h1>
                    <div className="step-indicator">
                        <span className="step completed">1</span>
                        <span className="step-divider"></span>
                        <span className="step completed">2</span>
                        <span className="step-divider"></span>
                        <span className="step completed">3</span>
                        <span className="step-divider"></span>
                        <span className="step active">4</span>
                    </div>
                    <h2>Step 4: Configure Variants</h2>
                    <p>Fill in SKU, price, and stock for each variant.</p>
                </header>

                {error && (
                    <div className="error-banner">
                        {error}
                    </div>
                )}

                <div className="variants-section">
                    <div className="section-info">
                        <p><strong>{variants.length}</strong> variant{variants.length !== 1 ? 's' : ''} generated</p>
                    </div>

                    <div className="variants-table-container">
                        <table className="variants-table">
                            <thead>
                                <tr>
                                    {step3Data.attributes.map(attr => (
                                        <th key={`${attr.name_en}-${attr.name_fr}`}>
                                            <div className="header-bilingual">
                                                <div><strong>EN:</strong> {attr.name_en}</div>
                                                <div><strong>FR:</strong> {attr.name_fr}</div>
                                            </div>
                                        </th>
                                    ))}
                                    <th>SKU *</th>
                                    <th>Product ID Type</th>
                                    <th>Product ID Value</th>
                                    <th>Price *</th>
                                    <th>Stock</th>
                                    <th>Thumbnail</th>
                                    <th>Images</th>
                                </tr>
                            </thead>
                            <tbody>
                                {variants.map(variant => (
                                    <tr key={variant.id}>
                                        {step3Data.attributes.map(attr => {
                                            const formatted = formatVariantAttribute(attr.name_en, attr.name_fr, variant.attributes_en, variant.attributes_fr);
                                            return (
                                                <td key={`${attr.name_en}-${attr.name_fr}`}>
                                                    <div className="cell-bilingual">
                                                        <div><strong>EN:</strong> {formatted.en}</div>
                                                        <div><strong>FR:</strong> {formatted.fr}</div>
                                                    </div>
                                                </td>
                                            );
                                        })}
                                        <td>
                                            <input
                                                type="text"
                                                value={variant.sku}
                                                onChange={(e) => updateVariant(variant.id, 'sku', e.target.value)}
                                                className={`variant-input ${!variant.sku.trim() ? 'required' : ''}`}
                                                placeholder="SKU *"
                                                required
                                            />
                                        </td>
                                        <td>
                                            <select
                                                value={variant.productIdentifierType || ''}
                                                onChange={(e) => updateVariant(variant.id, 'productIdentifierType', e.target.value)}
                                                className="variant-input"
                                            >
                                                {identifierTypes.map(type => (
                                                    <option key={type.value} value={type.value}>
                                                        {type.label}
                                                    </option>
                                                ))}
                                            </select>
                                        </td>
                                        <td>
                                            <input
                                                type="text"
                                                value={variant.productIdentifierValue || ''}
                                                onChange={(e) => updateVariant(variant.id, 'productIdentifierValue', e.target.value)}
                                                className="variant-input"
                                                placeholder="ID Value"
                                                disabled={!variant.productIdentifierType}
                                            />
                                        </td>
                                        <td>
                                            <input
                                                type="number"
                                                value={variant.price}
                                                onChange={(e) => updateVariant(variant.id, 'price', parseFloat(e.target.value) || 0)}
                                                className={`variant-input ${variant.price <= 0 ? 'invalid' : ''}`}
                                                step="0.01"
                                                min="0.01"
                                                placeholder="0.01"
                                            />
                                        </td>
                                        <td>
                                            <input
                                                type="number"
                                                value={variant.stock}
                                                onChange={(e) => updateVariant(variant.id, 'stock', parseInt(e.target.value) || 0)}
                                                className="variant-input"
                                                min="0"
                                                placeholder="0"
                                            />
                                        </td>
                                        <td>
                                            <div className="file-input-container">
                                                <input
                                                    type="file"
                                                    accept="image/*"
                                                    onChange={(e) => handleThumbnailChange(variant.id, e.target.files?.[0] || null)}
                                                    className="file-input"
                                                    id={`thumbnail-${variant.id}`}
                                                    aria-label="Upload thumbnail image for variant"
                                                />
                                                <label htmlFor={`thumbnail-${variant.id}`} className="file-label">
                                                    Choose
                                                </label>
                                                {variant.thumbnailUrl && (
                                                    <div className="image-preview">
                                                        <img src={variant.thumbnailUrl} alt="Thumbnail" className="thumbnail-preview" />
                                                    </div>
                                                )}
                                            </div>
                                        </td>
                                        <td>
                                            <div className="file-input-container">
                                                <input
                                                    type="file"
                                                    accept="image/*"
                                                    multiple
                                                    onChange={(e) => handleImagesChange(variant.id, e.target.files)}
                                                    className="file-input"
                                                    id={`images-${variant.id}`}
                                                    aria-label="Upload product images for variant"
                                                />
                                                <label htmlFor={`images-${variant.id}`} className="file-label">
                                                    Choose
                                                </label>
                                                {variant.imageUrls && variant.imageUrls.length > 0 && (
                                                    <div className="images-preview">
                                                        <small>{variant.imageUrls.length} image{variant.imageUrls.length !== 1 ? 's' : ''}</small>
                                                    </div>
                                                )}
                                            </div>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </div>

                <div className="form-actions">
                    <button type="button" className="back-btn" onClick={onBack} disabled={isSaving}>
                        Back
                    </button>
                    <button
                        type="button"
                        onClick={handleSaveItem}
                        disabled={isFormInvalid || isSaving}
                        className={`submit-btn${(isFormInvalid || isSaving) ? ' disabled' : ''}`}
                    >
                        {isSaving ? 'Creating Product...' : 'Create Product'}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default AddProductStep4;
