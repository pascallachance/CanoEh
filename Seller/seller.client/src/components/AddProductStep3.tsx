import { useState, useEffect, useMemo } from 'react';
import './AddProductStep3.css';
import { ApiClient } from '../utils/apiClient';
import { formatVariantAttribute } from '../utils/bilingualArrayUtils';
import { toAbsoluteUrl, toAbsoluteUrlArray } from '../utils/urlUtils';
import { useNotifications } from '../contexts/useNotifications';
import type { AddProductStep1Data } from './AddProductStep1';
import type { AddProductStep2Data } from './AddProductStep2';
import StepIndicator from './StepIndicator';

interface ItemVariant {
    id: string;
    attributes_en: Record<string, string>;
    attributes_fr: Record<string, string>;
    features_en: Record<string, string>;
    features_fr: Record<string, string>;
    sku: string;
    price: number;
    stock: number;
    productIdentifierType?: string;
    productIdentifierValue?: string;
    thumbnailUrl?: string;
    imageUrls?: string[];
    thumbnailFile?: File;
    imageFiles?: File[];
}

interface ApiResponseVariant {
    id: string;
    sku: string;
    price: number;
    stockQuantity: number;
}

interface AddProductStep3Props {
    onSubmit: () => void;
    onBack: () => void;
    onCancel: () => void;
    step1Data: AddProductStep1Data;
    step2Data: AddProductStep2Data;
    companies: Array<{ id: string; ownerID: string; name: string }>;
    editMode?: boolean;
    itemId?: string;
    existingVariants?: any[];
    onStepNavigate?: (step: number) => void;
    completedSteps?: number[];
}

function AddProductStep3({ onSubmit, onBack, onCancel, step1Data, step2Data, companies, editMode = false, itemId, existingVariants, onStepNavigate, completedSteps }: AddProductStep3Props) {
    const { showSuccess, showError } = useNotifications();
    const [variants, setVariants] = useState<ItemVariant[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState<string>('');

    // Handle escape key to cancel
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            const target = event.target as HTMLElement;
            const isInputField = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA';
            if (event.key === 'Escape' && !isInputField) {
                onCancel();
            }
        };

        document.addEventListener('keydown', handleEscape);
        return () => document.removeEventListener('keydown', handleEscape);
    }, [onCancel]);

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

    // Generate variants on mount or when step2Data changes
    // In edit mode, merge with existing variant data
    useEffect(() => {
        if (import.meta.env.DEV) {
            console.log('[AddProductStep3] Generating variants - editMode:', editMode, 'existingVariants:', existingVariants);
        }
        
        const generated = generateVariants();
        if (import.meta.env.DEV) {
            console.log('[AddProductStep3] Generated variants:', generated);
        }
        
        if (editMode && existingVariants && existingVariants.length > 0) {
            if (import.meta.env.DEV) {
                console.log('[AddProductStep3] Edit mode detected, merging with existing variants');
            }
            
            // Match generated variants with existing ones by attribute combination
            const mergedVariants = generated.map(genVariant => {
                // Find matching existing variant by comparing attributes
                const matchingExisting = existingVariants.find(existing => {
                    if (!existing.itemVariantAttributes || existing.itemVariantAttributes.length === 0) {
                        return Object.keys(genVariant.attributes_en).length === 0;
                    }
                    
                    // Check if all attributes match
                    return existing.itemVariantAttributes.every((attr: any) => {
                        const attrNameEn = attr.attributeName_en;
                        const attrValueEn = attr.attributes_en;
                        return genVariant.attributes_en[attrNameEn] === attrValueEn;
                    }) && existing.itemVariantAttributes.length === Object.keys(genVariant.attributes_en).length;
                });
                
                if (matchingExisting) {
                    // Merge existing data with generated structure
                    // Convert relative URLs to absolute URLs for display
                    const convertedThumbnailUrl = toAbsoluteUrl(matchingExisting.thumbnailUrl);
                    const convertedImageUrls = toAbsoluteUrlArray(matchingExisting.imageUrls);
                    
                    if (import.meta.env.DEV) {
                        console.log('[AddProductStep3] Found matching existing variant:', {
                            id: matchingExisting.id,
                            thumbnailUrl: matchingExisting.thumbnailUrl,
                            imageUrls: matchingExisting.imageUrls
                        });
                        console.log('[AddProductStep3] Converted URLs - thumbnail:', convertedThumbnailUrl, 'images:', convertedImageUrls);
                    }
                    
                    return {
                        ...genVariant,
                        id: matchingExisting.id, // Use existing ID
                        sku: matchingExisting.sku || genVariant.sku,
                        price: matchingExisting.price || genVariant.price,
                        stock: matchingExisting.stockQuantity || genVariant.stock,
                        productIdentifierType: matchingExisting.productIdentifierType || genVariant.productIdentifierType,
                        productIdentifierValue: matchingExisting.productIdentifierValue || genVariant.productIdentifierValue,
                        thumbnailUrl: convertedThumbnailUrl || genVariant.thumbnailUrl,
                        imageUrls: convertedImageUrls.length > 0 ? convertedImageUrls : genVariant.imageUrls
                    };
                }
                
                if (import.meta.env.DEV) {
                    console.log('[AddProductStep3] No matching existing variant found for generated variant:', genVariant);
                }
                return genVariant;
            });
            
            if (import.meta.env.DEV) {
                console.log('[AddProductStep3] Final merged variants:', mergedVariants);
            }
            setVariants(mergedVariants);
        } else {
            if (import.meta.env.DEV) {
                console.log('[AddProductStep3] Not in edit mode or no existing variants, using generated variants');
            }
            setVariants(generated);
        }
    }, [step2Data, editMode, existingVariants]);

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
        if (step2Data.variantAttributes.length === 0) {
            return [{
                id: '1',
                attributes_en: {},
                attributes_fr: {},
                features_en: {},
                features_fr: {},
                sku: '',
                price: 0,
                stock: 0,
                productIdentifierType: '',
                productIdentifierValue: '',
                thumbnailUrl: '',
                imageUrls: [],
                thumbnailFile: undefined,
                imageFiles: []
            }];
        }

        const combinations: { en: Record<string, string>, fr: Record<string, string> }[] = [];

        const generateCombinations = (attrIndex: number, currentEn: Record<string, string>, currentFr: Record<string, string>) => {
            if (attrIndex >= step2Data.variantAttributes.length) {
                combinations.push({ en: { ...currentEn }, fr: { ...currentFr } });
                return;
            }

            const attribute = step2Data.variantAttributes[attrIndex];

            // Skip if no values
            if (attribute.values.length === 0) {
                console.warn(`Attribute "${attribute.name_en}" has no values, skipping variant generation`);
                return;
            }

            // Use paired values
            for (let i = 0; i < attribute.values.length; i++) {
                const value = attribute.values[i];
                generateCombinations(
                    attrIndex + 1,
                    { ...currentEn, [attribute.name_en]: value.en },
                    { ...currentFr, [attribute.name_fr]: value.fr }
                );
            }
        };

        generateCombinations(0, {}, {});

        return combinations.map((combo, index) => ({
            id: `variant-${index + 1}`,
            attributes_en: combo.en,
            attributes_fr: combo.fr,
            features_en: {},
            features_fr: {},
            sku: '',
            price: 0,
            stock: 0,
            productIdentifierType: '',
            productIdentifierValue: '',
            thumbnailUrl: '',
            imageUrls: [],
            thumbnailFile: undefined,
            imageFiles: []
        }));
    };

    const updateVariant = (variantId: string, field: keyof Omit<ItemVariant, 'id' | 'attributes_en' | 'attributes_fr' | 'features_en' | 'features_fr'>, value: string | number | string[]) => {
        setVariants(prev => prev.map(v =>
            v.id === variantId ? { ...v, [field]: value } : v
        ));
    };

    const updateVariantFeature = (variantId: string, featureName_en: string, featureName_fr: string, value_en: string, value_fr: string) => {
        setVariants(prev => prev.map(v =>
            v.id === variantId ? {
                ...v,
                features_en: { ...v.features_en, [featureName_en]: value_en },
                features_fr: { ...v.features_fr, [featureName_fr]: value_fr }
            } : v
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
            setVariants(prev => prev.map(v => 
                v.id === variantId ? { ...v, thumbnailUrl: url, thumbnailFile: file } : v
            ));
        } else {
            setVariants(prev => prev.map(v => 
                v.id === variantId ? { ...v, thumbnailUrl: '', thumbnailFile: undefined } : v
            ));
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
                setVariants(prev => prev.map(v => 
                    v.id === variantId ? { ...v, imageUrls: urls, imageFiles: fileArray } : v
                ));
            } catch (error) {
                urls.forEach(url => {
                    if (url.startsWith('blob:')) {
                        URL.revokeObjectURL(url);
                    }
                });
                console.error('Error creating object URLs:', error);
                setVariants(prev => prev.map(v => 
                    v.id === variantId ? { ...v, imageUrls: [], imageFiles: [] } : v
                ));
            }
        } else {
            setVariants(prev => prev.map(v => 
                v.id === variantId ? { ...v, imageUrls: [], imageFiles: [] } : v
            ));
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

    // Helper function to build item request (shared between create and update)
    const buildItemRequest = (sellerId: string, itemId?: string) => {
        const request: any = {
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
                ImageUrls: null, // Will be set after uploading images
                ThumbnailUrl: null, // Will be set after uploading thumbnail
                ItemVariantName_en: variant.attributes_en ? Object.entries(variant.attributes_en).map(([k, v]) => `${k}: ${v}`).join(', ') : null,
                ItemVariantName_fr: variant.attributes_fr ? Object.entries(variant.attributes_fr).map(([k, v]) => `${k}: ${v}`).join(', ') : null,
                ItemVariantAttributes: variant.attributes_en ? Object.entries(variant.attributes_en).map(([attrNameEn, attrValueEn]) => {
                    const foundAttribute = step2Data.variantAttributes.find(attr => attr.name_en === attrNameEn);
                    const attrNameFr = foundAttribute?.name_fr || null;
                    const attrValueFr = attrNameFr && variant.attributes_fr ? variant.attributes_fr[attrNameFr] : null;
                    return {
                        AttributeName_en: attrNameEn,
                        AttributeName_fr: attrNameFr,
                        Attributes_en: attrValueEn,
                        Attributes_fr: attrValueFr
                    };
                }) : [],
                ItemVariantFeatures: variant.features_en ? Object.entries(variant.features_en).map(([featureNameEn, featureValueEn]) => {
                    const foundFeature = step2Data.variantFeatures.find(feat => feat.name_en === featureNameEn);
                    const featureNameFr = foundFeature?.name_fr || null;
                    const featureValueFr = featureNameFr && variant.features_fr ? variant.features_fr[featureNameFr] : null;
                    return {
                        FeatureName_en: featureNameEn,
                        FeatureName_fr: featureNameFr,
                        Features_en: featureValueEn,
                        Features_fr: featureValueFr
                    };
                }) : [],
                Deleted: false
            }))
        };

        // Add Id and variant Ids for update mode
        if (itemId) {
            request.Id = itemId;
            request.Variants = request.Variants.map((variantData: any, index: number) => ({
                ...variantData,
                Id: variants[index].id.startsWith('variant-') ? null : variants[index].id
            }));
        }

        return request;
    };

    // Helper function to upload images for a variant (shared between create and update)
    const uploadVariantImages = async (variant: ItemVariant, apiVariantId: string) => {
        // Upload thumbnail if present
        if (variant.thumbnailFile) {
            try {
                const formData = new FormData();
                formData.append('file', variant.thumbnailFile);

                const uploadResponse = await fetch(
                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UploadImage?variantId=${apiVariantId}&imageType=thumbnail`,
                    {
                        method: 'POST',
                        credentials: 'include',
                        body: formData,
                    }
                );

                if (!uploadResponse.ok) {
                    const errorText = await uploadResponse.text();
                    console.error(`Failed to upload thumbnail for variant ${apiVariantId}: ${uploadResponse.status} ${uploadResponse.statusText}`, errorText);
                }
            } catch (error) {
                console.error(`Error uploading thumbnail for variant ${apiVariantId}:`, error);
            }
        }

        // Upload product images if present
        if (variant.imageFiles && variant.imageFiles.length > 0) {
            for (let imageIndex = 0; imageIndex < variant.imageFiles.length; imageIndex++) {
                try {
                    const formData = new FormData();
                    formData.append('file', variant.imageFiles[imageIndex]);

                    const uploadResponse = await fetch(
                        `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UploadImage?variantId=${apiVariantId}&imageType=image&imageNumber=${imageIndex + 1}`,
                        {
                            method: 'POST',
                            credentials: 'include',
                            body: formData,
                        }
                    );

                    if (!uploadResponse.ok) {
                        const errorText = await uploadResponse.text();
                        console.error(`Failed to upload image ${imageIndex + 1} for variant ${apiVariantId}: ${uploadResponse.status} ${uploadResponse.statusText}`, errorText);
                    }
                } catch (error) {
                    console.error(`Error uploading image ${imageIndex + 1} for variant ${apiVariantId}:`, error);
                }
            }
        }
    };

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
            // Build request using helper function
            const itemRequest = buildItemRequest(sellerId, editMode ? itemId : undefined);
            
            // Call the appropriate API endpoint
            const response = editMode
                ? await ApiClient.put(
                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UpdateItem`,
                    itemRequest
                )
                : await ApiClient.post(
                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/CreateItem`,
                    itemRequest
                );

            if (response.ok) {
                const result = await response.json();
                const savedItem = result.value;

                // Upload images for variants that have files
                if (savedItem && savedItem.variants) {
                    for (const variant of variants) {
                        // Find the corresponding saved variant by SKU
                        const savedVariant = savedItem.variants.find((v: ApiResponseVariant) => v.sku === variant.sku);

                        if (!savedVariant || !savedVariant.id) {
                            console.warn(`Saved variant with SKU "${variant.sku}" not found, skipping image upload`);
                            continue;
                        }

                        // Upload images using helper function
                        await uploadVariantImages(variant, savedVariant.id);
                    }
                }

                // Show success message
                showSuccess(`Product ${editMode ? 'updated' : 'created'} successfully!`);
                // Navigate to products list
                onSubmit();
            } else {
                const errorText = await response.text();
                const errorMessage = `Failed to ${editMode ? 'update' : 'create'} item: ${errorText}`;
                setError(errorMessage);
                showError(errorMessage);
            }

        } catch (error) {
            console.error(`Error ${editMode ? 'updating' : 'creating'} item:`, error);
            const errorMessage = `An unexpected error occurred while ${editMode ? 'updating' : 'creating'} the item.`;
            setError(errorMessage);
            showError(errorMessage);
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="add-product-step3-container">
            <div className="add-product-step3-content">
                <header className="step-header">
                    <h1>{editMode ? 'Edit Product' : 'Add New Product'}</h1>
                    <StepIndicator 
                        currentStep={3}
                        totalSteps={3}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || [1, 2]}
                    />
                    <h2>Step 3: Configure Variants</h2>
                    <p>Fill in SKU, price, stock, and variant features for each variant.</p>
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

                    <div className="variants-cards-container">
                        {variants.map(variant => (
                            <div key={variant.id} className="variant-card">
                                {/* Variant Attributes Header */}
                                <div className="variant-card-header">
                                    {step2Data.variantAttributes.map((attr, index) => {
                                        const formatted = formatVariantAttribute(attr.name_en, attr.name_fr, variant.attributes_en, variant.attributes_fr);
                                        return (
                                            <span key={`${attr.name_en}-${attr.name_fr}`}>
                                                {index > 0 && ' / '}
                                                <strong>{attr.name_en}</strong> ({formatted.en}) / <strong>{attr.name_fr}</strong> ({formatted.fr})
                                            </span>
                                        );
                                    })}
                                </div>

                                {/* Attributes Section */}
                                <div className="variant-section">
                                    <h4 className="variant-section-title">Attributes</h4>
                                    <div className="variant-fields">
                                        <div className="variant-field">
                                            <label className="variant-field-label">SKU *</label>
                                            <input
                                                type="text"
                                                value={variant.sku}
                                                onChange={(e) => updateVariant(variant.id, 'sku', e.target.value)}
                                                className={`variant-input ${!variant.sku.trim() ? 'required' : ''}`}
                                                placeholder="SKU *"
                                                required
                                            />
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label">Product ID Type</label>
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
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label">Product ID Value</label>
                                            <input
                                                type="text"
                                                value={variant.productIdentifierValue || ''}
                                                onChange={(e) => updateVariant(variant.id, 'productIdentifierValue', e.target.value)}
                                                className="variant-input"
                                                placeholder="ID Value"
                                                disabled={!variant.productIdentifierType}
                                            />
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label">Price *</label>
                                            <input
                                                type="number"
                                                value={variant.price}
                                                onChange={(e) => updateVariant(variant.id, 'price', parseFloat(e.target.value) || 0)}
                                                className={`variant-input ${variant.price <= 0 ? 'invalid' : ''}`}
                                                step="0.01"
                                                min="0.01"
                                                placeholder="0.01"
                                            />
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label">Stock</label>
                                            <input
                                                type="number"
                                                value={variant.stock}
                                                onChange={(e) => updateVariant(variant.id, 'stock', parseInt(e.target.value) || 0)}
                                                className="variant-input"
                                                min="0"
                                                placeholder="0"
                                            />
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label">Thumbnail</label>
                                            <div className="file-input-wrapper">
                                                <input
                                                    type="file"
                                                    accept="image/*"
                                                    onChange={(e) => handleThumbnailChange(variant.id, e.target.files?.[0] || null)}
                                                    className="file-input"
                                                    id={`thumbnail-${variant.id}`}
                                                    aria-label="Upload thumbnail image for variant"
                                                />
                                                <label htmlFor={`thumbnail-${variant.id}`} className="file-label">
                                                    Choose Image
                                                </label>
                                                {variant.thumbnailUrl && (
                                                    <div className="image-preview">
                                                        <img 
                                                            src={variant.thumbnailUrl} 
                                                            alt="Thumbnail" 
                                                            className="thumbnail-preview"
                                                            onLoad={() => {
                                                                if (import.meta.env.DEV) {
                                                                    console.log('[AddProductStep3] Thumbnail loaded successfully:', variant.thumbnailUrl);
                                                                }
                                                            }}
                                                            onError={(e) => {
                                                                if (import.meta.env.DEV) {
                                                                    console.error('[AddProductStep3] Thumbnail failed to load:', variant.thumbnailUrl);
                                                                    console.error('[AddProductStep3] Image error type:', e.type);
                                                                }
                                                            }}
                                                        />
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label">Images</label>
                                            <div className="file-input-wrapper">
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
                                                    Choose Images
                                                </label>
                                                {variant.imageUrls && variant.imageUrls.length > 0 && (
                                                    <div className="images-preview">
                                                        <small>{variant.imageUrls.length} image{variant.imageUrls.length !== 1 ? 's' : ''}</small>
                                                    </div>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                {/* Features Section */}
                                {step2Data.variantFeatures.length > 0 && (
                                    <div className="variant-section">
                                        <h4 className="variant-section-title">Features</h4>
                                        <div className="variant-fields">
                                            {step2Data.variantFeatures.map(feature => (
                                                <div key={`${feature.name_en}-${feature.name_fr}`} className="variant-field">
                                                    <label className="variant-field-label">
                                                        {feature.name_en} / {feature.name_fr}
                                                    </label>
                                                    <div className="feature-inputs-vertical">
                                                        <input
                                                            type="text"
                                                            value={variant.features_en[feature.name_en] || ''}
                                                            onChange={(e) => updateVariantFeature(
                                                                variant.id,
                                                                feature.name_en,
                                                                feature.name_fr,
                                                                e.target.value,
                                                                variant.features_fr[feature.name_fr] || ''
                                                            )}
                                                            className="variant-input"
                                                            placeholder={`EN: ${feature.name_en}`}
                                                            aria-label={`${feature.name_en} (English)`}
                                                        />
                                                        <input
                                                            type="text"
                                                            value={variant.features_fr[feature.name_fr] || ''}
                                                            onChange={(e) => updateVariantFeature(
                                                                variant.id,
                                                                feature.name_en,
                                                                feature.name_fr,
                                                                variant.features_en[feature.name_en] || '',
                                                                e.target.value
                                                            )}
                                                            className="variant-input"
                                                            placeholder={`FR: ${feature.name_fr}`}
                                                            aria-label={`${feature.name_fr} (French)`}
                                                        />
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </div>
                        ))}
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
                        {isSaving 
                            ? (editMode ? 'Updating Product...' : 'Creating Product...') 
                            : (editMode ? 'Update Product' : 'Create Product')}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default AddProductStep3;
