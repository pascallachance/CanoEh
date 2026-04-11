import { useState, useEffect, useMemo, useRef } from 'react';
import './AddProductStep3.css';
import { ApiClient } from '../utils/apiClient';
import { formatVariantAttribute } from '../utils/bilingualArrayUtils';
import { toAbsoluteUrl, toAbsoluteUrlArray, toRelativeUrl } from '../utils/urlUtils';
import { useNotifications } from '../contexts/useNotifications';
import type { AddProductStep1Data } from './AddProductStep1';
import type { AddProductStep2Data } from './AddProductStep2';
import StepIndicator from './StepIndicator';
import { useLanguage } from '../contexts/LanguageContext';

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
    imageFiles?: (File | null)[]; // Allow null for server-hosted images to keep alignment with imageUrls
    videoUrl?: string;
    videoFile?: File;
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
    const { t, language } = useLanguage();
    const [variants, setVariants] = useState<ItemVariant[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState<string>('');
    const variantsRef = useRef<ItemVariant[]>([]);

    // Keep ref in sync with variants state
    useEffect(() => {
        variantsRef.current = variants;
    }, [variants]);

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
        { value: '', label: t('products.selectIdType') },
        { value: 'UPC', label: 'UPC' },
        { value: 'EAN', label: 'EAN' },
        { value: 'GTIN', label: 'GTIN' },
        { value: 'ISBN', label: 'ISBN' },
        { value: 'ASIN', label: 'ASIN' },
        { value: 'SKU', label: 'SKU' },
        { value: 'MPN', label: t('products.idType.mpn') }
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

                    // Restore features from the existing variant so they survive an edit round-trip.
                    // Use != null checks (not truthiness) so that legitimate empty-string values are preserved.
                    const features_en: Record<string, string> = {};
                    const features_fr: Record<string, string> = {};
                    (matchingExisting.itemVariantFeatures || []).forEach((feature) => {
                        if (feature.attributeName_en != null) {
                            features_en[feature.attributeName_en] = feature.attributes_en ?? '';
                        }
                        if (feature.attributeName_fr != null) {
                            features_fr[feature.attributeName_fr] = feature.attributes_fr ?? '';
                        }
                    });
                    
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
                        imageUrls: convertedImageUrls.length > 0 ? convertedImageUrls : genVariant.imageUrls,
                        features_en,
                        features_fr
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

    // Cleanup object URLs on component unmount only
    useEffect(() => {
        return () => {
            variantsRef.current.forEach(variant => {
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
                if (variant.videoUrl && variant.videoUrl.startsWith('blob:')) {
                    URL.revokeObjectURL(variant.videoUrl);
                }
            });
        };
    }, []); // Empty deps - only runs on unmount

    const generateVariants = (): ItemVariant[] => {
        if (step2Data.variantAttributes.length === 0) {
            return [{
                id: 'variant-1',
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
                imageFiles: [],
                videoUrl: '',
                videoFile: undefined
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
            imageFiles: [],
            videoUrl: '',
            videoFile: undefined
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
        if (files && files.length > 0) {
            const currentVariant = variants.find(v => v.id === variantId);
            const existingUrls = currentVariant?.imageUrls || [];
            const existingFiles = currentVariant?.imageFiles || [];
            
            // Calculate how many more images we can add (max 10 total)
            const remainingSlots = 10 - existingUrls.length;
            if (remainingSlots <= 0) {
                console.warn('Maximum of 10 images already reached');
                showError('You can upload a maximum of 10 images for this product variant.');
                return;
            }
            
            const fileArray = Array.from(files).slice(0, remainingSlots);
            
            // If more files were selected than could be added, notify the seller that some were ignored
            if (files.length > remainingSlots) {
                showError(`You can upload a maximum of 10 images for this product variant. Only ${remainingSlots} of the selected images were added.`);
            }
            
            const newUrls: string[] = [];
            try {
                fileArray.forEach(file => {
                    newUrls.push(URL.createObjectURL(file));
                });
                
                // Append new images to existing ones
                // Keep imageFiles aligned with imageUrls by padding with null for server-hosted images
                // This ensures handleRemoveImage, handleMoveImage, and uploadVariantImages work correctly
                const paddedExistingFiles: (File | null)[] = existingUrls.map((_url, idx) => 
                    existingFiles[idx] || null
                );
                
                setVariants(prev => prev.map(v => 
                    v.id === variantId ? { 
                        ...v, 
                        imageUrls: [...existingUrls, ...newUrls], 
                        imageFiles: [...paddedExistingFiles, ...fileArray] 
                    } : v
                ));
            } catch (error) {
                // Clean up created URLs on error
                newUrls.forEach(url => {
                    if (url.startsWith('blob:')) {
                        URL.revokeObjectURL(url);
                    }
                });
                console.error('Error creating object URLs:', error);
            }
        }
    };

    // Helper function to remove a specific image from product images
    const handleRemoveImage = (variantId: string, imageIndex: number) => {
        setVariants(prev => prev.map(v => {
            if (v.id === variantId) {
                const newImageUrls = [...(v.imageUrls || [])];
                const newImageFiles = [...(v.imageFiles || [])];
                
                // Revoke the blob URL before removing
                if (newImageUrls[imageIndex]?.startsWith('blob:')) {
                    URL.revokeObjectURL(newImageUrls[imageIndex]);
                }
                
                newImageUrls.splice(imageIndex, 1);
                newImageFiles.splice(imageIndex, 1);
                
                return { ...v, imageUrls: newImageUrls, imageFiles: newImageFiles };
            }
            return v;
        }));
    };

    // Helper function to remove thumbnail
    const handleRemoveThumbnail = (variantId: string) => {
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.thumbnailUrl && currentVariant.thumbnailUrl.startsWith('blob:')) {
            URL.revokeObjectURL(currentVariant.thumbnailUrl);
        }
        setVariants(prev => prev.map(v => 
            v.id === variantId ? { ...v, thumbnailUrl: '', thumbnailFile: undefined } : v
        ));
    };

    // Helper function to reorder images
    const handleMoveImage = (variantId: string, fromIndex: number, toIndex: number) => {
        setVariants(prev => prev.map(v => {
            if (v.id === variantId) {
                const newImageUrls = [...(v.imageUrls || [])];
                const newImageFiles = [...(v.imageFiles || [])];
                
                // Move URL (always present)
                const [movedUrl] = newImageUrls.splice(fromIndex, 1);
                newImageUrls.splice(toIndex, 0, movedUrl);
                
                // Move corresponding file (may be null for server-hosted images)
                // Since arrays are now aligned 1:1, we can safely move at the same index
                if (newImageFiles.length > fromIndex) {
                    const [movedFile] = newImageFiles.splice(fromIndex, 1);
                    newImageFiles.splice(toIndex, 0, movedFile);
                }
                
                return { ...v, imageUrls: newImageUrls, imageFiles: newImageFiles };
            }
            return v;
        }));
    };

    // Video upload handlers - disabled pending backend support
    // Uncomment when backend supports video uploads via /api/Item/UploadVideo endpoint
    /*
    const handleVideoChange = (variantId: string, file: File | null) => {
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.videoUrl && currentVariant.videoUrl.startsWith('blob:')) {
            URL.revokeObjectURL(currentVariant.videoUrl);
        }

        if (file) {
            const url = URL.createObjectURL(file);
            setVariants(prev => prev.map(v => 
                v.id === variantId ? { ...v, videoUrl: url, videoFile: file } : v
            ));
        } else {
            setVariants(prev => prev.map(v => 
                v.id === variantId ? { ...v, videoUrl: '', videoFile: undefined } : v
            ));
        }
    };

    const handleRemoveVideo = (variantId: string) => {
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.videoUrl && currentVariant.videoUrl.startsWith('blob:')) {
            URL.revokeObjectURL(currentVariant.videoUrl);
        }
        setVariants(prev => prev.map(v => 
            v.id === variantId ? { ...v, videoUrl: '', videoFile: undefined } : v
        ));
    };
    */

    // Validation logic
    const isFormInvalid = useMemo(() => {
        if (variants.length > 0) {
            return variants.some(variant =>
                !variant.sku.trim() || variant.price <= 0 ||
                variant.sku.length > 100 ||
                (variant.productIdentifierValue != null && variant.productIdentifierValue.length > 100)
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
            CategoryNodeID: step2Data.categoryId,
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
                        Attributes_fr: attrValueFr,
                        IsMain: foundAttribute?.isMain ?? false
                    };
                }) : [],
                ItemVariantFeatures: variant.features_en ? Object.entries(variant.features_en).map(([featureNameEn, featureValueEn]) => {
                    const foundFeature = step2Data.variantFeatures.find(feat => feat.name_en === featureNameEn);
                    const featureNameFr = foundFeature?.name_fr || null;
                    const featureValueFr = featureNameFr && variant.features_fr ? variant.features_fr[featureNameFr] : null;
                    return {
                        AttributeName_en: featureNameEn,
                        AttributeName_fr: featureNameFr,
                        Attributes_en: featureValueEn,
                        Attributes_fr: featureValueFr
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
                Id: variants[index].id.startsWith('variant-') ? '00000000-0000-0000-0000-000000000000' : variants[index].id
            }));
        }

        return request;
    };

    // Helper function to upload images for a variant (shared between create and update)
    const uploadVariantImages = async (variant: ItemVariant, apiVariantId: string) => {
        // In edit mode, sync the DB image URLs to match the current UI state before uploading.
        // This ensures that images removed or reordered in the UI are persisted correctly.
        if (editMode) {
            try {
                // Build the list of relative URLs for server-hosted images, preserving position.
                // Positions occupied by a new File upload are sent as empty string so that the
                // subsequent UploadImage call can fill them in at the correct index.
                const syncedImageUrls = (variant.imageUrls || []).map((url, i) => {
                    const isServerHosted = !variant.imageFiles || variant.imageFiles[i] === null || variant.imageFiles[i] === undefined;
                    return isServerHosted ? toRelativeUrl(url) : '';
                });

                // Determine the thumbnail that should be preserved in the DB.
                // If a new thumbnailFile is provided the upload step will overwrite it, so pass null.
                const syncedThumbnailUrl =
                    variant.thumbnailFile
                        ? null
                        : variant.thumbnailUrl &&
                          !variant.thumbnailUrl.startsWith('blob:') &&
                          !variant.thumbnailUrl.startsWith('data:')
                            ? toRelativeUrl(variant.thumbnailUrl)
                            : null;

                const syncResponse = await fetch(
                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UpdateVariantImageUrls`,
                    {
                        method: 'PUT',
                        credentials: 'include',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            VariantId: apiVariantId,
                            ThumbnailUrl: syncedThumbnailUrl,
                            ImageUrls: syncedImageUrls,
                        }),
                    }
                );

                if (!syncResponse.ok) {
                    const errorText = await syncResponse.text();
                    console.error(`Failed to sync image URLs for variant ${apiVariantId}: ${syncResponse.status} ${syncResponse.statusText}`, errorText);
                    showError('Some image changes could not be saved. Please try again.');
                }
            } catch (error) {
                console.error(`Error syncing image URLs for variant ${apiVariantId}:`, error);
                showError('Some image changes could not be saved. Please try again.');
            }
        }

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
        // imageFiles array is aligned 1:1 with imageUrls, with null entries for server-hosted images
        // Only upload non-null File entries, using their index in imageUrls as imageNumber
        if (variant.imageFiles && variant.imageFiles.length > 0) {
            for (let imageIndex = 0; imageIndex < variant.imageFiles.length; imageIndex++) {
                const file = variant.imageFiles[imageIndex];
                
                // Skip null entries (server-hosted images that don't need re-uploading)
                if (!file) {
                    continue;
                }
                
                try {
                    const formData = new FormData();
                    formData.append('file', file);

                    // Use imageIndex + 1 as imageNumber to match the position in imageUrls array
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
                const errorMessage = t('error.invalidVariants');
                setError(errorMessage);
                showError(errorMessage);
                return;
            }

            const hasSkuTooLong = variants.some(variant => variant.sku.length > 100);
            if (hasSkuTooLong) {
                const errorMessage = t('error.skuTooLong');
                setError(errorMessage);
                showError(errorMessage);
                return;
            }

            const hasProductIdValueTooLong = variants.some(variant =>
                variant.productIdentifierValue != null && variant.productIdentifierValue.length > 100
            );
            if (hasProductIdValueTooLong) {
                const errorMessage = t('error.productIdValueTooLong');
                setError(errorMessage);
                showError(errorMessage);
                return;
            }
        }

        if ((step1Data.name?.length ?? 0) > 300) {
            const errorMessage = t('error.nameEnTooLong');
            setError(errorMessage);
            showError(errorMessage);
            return;
        }

        if ((step1Data.name_fr?.length ?? 0) > 300) {
            const errorMessage = t('error.nameFrTooLong');
            setError(errorMessage);
            showError(errorMessage);
            return;
        }

        if ((step1Data.description?.length ?? 0) > 3000) {
            const errorMessage = t('error.descriptionEnTooLong');
            setError(errorMessage);
            showError(errorMessage);
            return;
        }

        if ((step1Data.description_fr?.length ?? 0) > 3000) {
            const errorMessage = t('error.descriptionFrTooLong');
            setError(errorMessage);
            showError(errorMessage);
            return;
        }

        const sellerId = companies.length > 0 ? companies[0].ownerID : null;
        if (!sellerId) {
            const errorMessage = t('error.sellerIdMissing');
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
                showSuccess(editMode ? t('variant.productUpdated') : t('variant.productCreated'));
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
                    <h1>{editMode ? t('products.editProduct') : t('products.addNewProduct')}</h1>
                    <StepIndicator 
                        currentStep={3}
                        totalSteps={3}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || [1, 2]}
                    />
                    <h2>{t('step3.title')}</h2>
                    <p>{t('step3.subtitle')}</p>
                </header>

                {error && (
                    <div className="error-banner">
                        {error}
                    </div>
                )}

                <div className="variants-section">
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
                                                <strong>{language === 'fr' ? attr.name_fr : attr.name_en}</strong> ({language === 'fr' ? formatted.fr : formatted.en})
                                            </span>
                                        );
                                    })}
                                </div>

                                {/* Attributes Section */}
                                <div className="variant-section">
                                    <h4 className="variant-section-title">{t('variant.attributes')}</h4>
                                    <div className="variant-fields">
                                        <div className="variant-field">
                                            <label className="variant-field-label" htmlFor={`sku-${variant.id}`}>{t('products.variant.sku')} *</label>
                                            <div className="variant-field-content">
                                                <input
                                                    type="text"
                                                    id={`sku-${variant.id}`}
                                                    value={variant.sku}
                                                    onChange={(e) => updateVariant(variant.id, 'sku', e.target.value)}
                                                    className={`variant-input ${!variant.sku.trim() || variant.sku.length > 100 ? 'invalid' : ''}`}
                                                    placeholder={`${t('products.variant.sku')} *`}
                                                    required
                                                    maxLength={100}
                                                />
                                            </div>
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label" htmlFor={`product-id-type-${variant.id}`}>{t('products.variant.productIdType')}</label>
                                            <div className="variant-field-content">
                                                <select
                                                    id={`product-id-type-${variant.id}`}
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
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label" htmlFor={`product-id-value-${variant.id}`}>{t('products.variant.productIdValue')}</label>
                                            <div className="variant-field-content">
                                                <input
                                                    type="text"
                                                    id={`product-id-value-${variant.id}`}
                                                    value={variant.productIdentifierValue || ''}
                                                    onChange={(e) => updateVariant(variant.id, 'productIdentifierValue', e.target.value)}
                                                    className="variant-input"
                                                    placeholder={t('variant.idValuePlaceholder')}
                                                    disabled={!variant.productIdentifierType}
                                                    maxLength={100}
                                                />
                                            </div>
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label" htmlFor={`price-${variant.id}`}>{t('products.variant.price')} *</label>
                                            <div className="variant-field-content">
                                                <input
                                                    type="number"
                                                    id={`price-${variant.id}`}
                                                    value={variant.price}
                                                    onChange={(e) => updateVariant(variant.id, 'price', parseFloat(e.target.value) || 0)}
                                                    className={`variant-input ${variant.price <= 0 ? 'invalid' : ''}`}
                                                    step="0.01"
                                                    min="0.01"
                                                    placeholder="0.01"
                                                />
                                            </div>
                                        </div>
                                        <div className="variant-field">
                                            <label className="variant-field-label" htmlFor={`stock-${variant.id}`}>{t('variant.stock')}</label>
                                            <div className="variant-field-content">
                                                <input
                                                    type="number"
                                                    id={`stock-${variant.id}`}
                                                    value={variant.stock}
                                                    onChange={(e) => updateVariant(variant.id, 'stock', parseInt(e.target.value) || 0)}
                                                    className="variant-input"
                                                    min="0"
                                                    placeholder="0"
                                                />
                                            </div>
                                        </div>
                                        {/* Thumbnail Section */}
                                        <div className="variant-field variant-field-media">
                                            <div className="media-upload-row">
                                                <label className="media-label" htmlFor={`thumbnail-${variant.id}`}>{t('variant.thumbnail')}</label>
                                                <div className="media-controls">
                                                    <input
                                                        type="file"
                                                        accept="image/*"
                                                        onChange={(e) => handleThumbnailChange(variant.id, e.target.files?.[0] || null)}
                                                        className="file-input"
                                                        id={`thumbnail-${variant.id}`}
                                                        aria-label={t('variant.uploadThumbnailAriaLabel')}
                                                    />
                                                    <label htmlFor={`thumbnail-${variant.id}`} className="file-label">
                                                        {t('variant.chooseImage')}
                                                    </label>
                                                </div>
                                                <div className="media-preview">
                                                    {variant.thumbnailUrl && (
                                                        <div className="image-preview-item">
                                                            <img 
                                                                src={variant.thumbnailUrl} 
                                                                alt={t('variant.thumbnailAlt')} 
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
                                                            <button
                                                                type="button"
                                                                onClick={() => handleRemoveThumbnail(variant.id)}
                                                                className="remove-media-btn"
                                                                title={t('variant.removeThumbnail')}
                                                                aria-label={t('variant.removeThumbnail')}
                                                            >
                                                                ×
                                                            </button>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>
                                        
                                        {/* Product Images Section */}
                                        <div className="variant-field variant-field-media">
                                            <div className="media-upload-row">
                                                <label className="media-label" htmlFor={`images-${variant.id}`}>{t('products.productImages')}</label>
                                                <div className="media-controls">
                                                    <input
                                                        type="file"
                                                        accept="image/*"
                                                        multiple
                                                        onChange={(e) => handleImagesChange(variant.id, e.target.files)}
                                                        className="file-input"
                                                        id={`images-${variant.id}`}
                                                        aria-label={t('variant.uploadImagesAriaLabel')}
                                                    />
                                                    <label htmlFor={`images-${variant.id}`} className="file-label">
                                                        {t('variant.chooseImages')}
                                                    </label>
                                                </div>
                                                <div className="media-preview">
                                                    {variant.imageUrls && variant.imageUrls.length > 0 && (
                                                        <div className="images-grid">
                                                            {variant.imageUrls.map((url, index) => (
                                                                <div key={index} className="image-preview-item">
                                                                    <img 
                                                                        src={url} 
                                                                        alt={`${t('products.productImages')} ${index + 1}`} 
                                                                        className="thumbnail-preview"
                                                                    />
                                                                    {index === 0 && (
                                                                        <span className="main-image-label">{t('variant.mainImageLabel')}</span>
                                                                    )}
                                                                    <button
                                                                        type="button"
                                                                        onClick={() => handleRemoveImage(variant.id, index)}
                                                                        className="remove-media-btn"
                                                                        title={t('variant.removeImageTitle')}
                                                                        aria-label={`${t('variant.removeImageTitle')} ${index + 1}`}
                                                                    >
                                                                        ×
                                                                    </button>
                                                                    <div className="image-actions">
                                                                        {index > 0 && (
                                                                            <button
                                                                                type="button"
                                                                                onClick={() => handleMoveImage(variant.id, index, index - 1)}
                                                                                className="move-btn"
                                                                                title={t('variant.moveLeft')}
                                                                                aria-label={`${t('variant.moveLeft')} ${index + 1}`}
                                                                            >
                                                                                ←
                                                                            </button>
                                                                        )}
                                                                        {index < variant.imageUrls!.length - 1 && (
                                                                            <button
                                                                                type="button"
                                                                                onClick={() => handleMoveImage(variant.id, index, index + 1)}
                                                                                className="move-btn"
                                                                                title={t('variant.moveRight')}
                                                                                aria-label={`${t('variant.moveRight')} ${index + 1}`}
                                                                            >
                                                                                →
                                                                            </button>
                                                                        )}
                                                                    </div>
                                                                </div>
                                                            ))}
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>

                                        {/* Video Section - DISABLED: Backend video upload endpoint not yet implemented */}
                                        {/* Uncomment when backend supports video uploads via /api/Item/UploadVideo endpoint */}
                                        {/* 
                                        <div className="variant-field variant-field-media">
                                            <div className="media-upload-row">
                                                <label className="media-label" htmlFor={`video-${variant.id}`}>Video</label>
                                                <div className="media-controls">
                                                    <input
                                                        type="file"
                                                        accept="video/*"
                                                        onChange={(e) => handleVideoChange(variant.id, e.target.files?.[0] || null)}
                                                        className="file-input"
                                                        id={`video-${variant.id}`}
                                                        aria-label="Upload video for variant"
                                                    />
                                                    <label htmlFor={`video-${variant.id}`} className="file-label">
                                                        Choose Video
                                                    </label>
                                                </div>
                                                <div className="media-preview">
                                                    {variant.videoUrl && (
                                                        <div className="video-preview-item">
                                                            <video 
                                                                src={variant.videoUrl} 
                                                                className="video-preview"
                                                                controls
                                                            />
                                                            <button
                                                                type="button"
                                                                onClick={() => handleRemoveVideo(variant.id)}
                                                                className="remove-media-btn"
                                                                title="Remove video"
                                                                aria-label="Remove video"
                                                            >
                                                                ×
                                                            </button>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>
                                        */}
                                    </div>
                                </div>

                                {/* Features Section */}
                                {step2Data.variantFeatures.length > 0 && (
                                    <div className="variant-section">
                                        <h4 className="variant-section-title">{t('variant.features')}</h4>
                                        <div className="variant-fields">
                                            {step2Data.variantFeatures.map(feature => (
                                                <div key={`${feature.name_en}-${feature.name_fr}`} className="variant-field">
                                                    <label className="variant-field-label" id={`feature-label-${variant.id}-${feature.name_en}`}>
                                                        {language === 'fr' ? feature.name_fr : feature.name_en}
                                                    </label>
                                                    <div className="variant-field-content">
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
                                                                aria-describedby={`feature-label-${variant.id}-${feature.name_en}`}
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
                                                                aria-describedby={`feature-label-${variant.id}-${feature.name_en}`}
                                                            />
                                                        </div>
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
                        {t('common.back')}
                    </button>
                    <button
                        type="button"
                        onClick={handleSaveItem}
                        disabled={isFormInvalid || isSaving}
                        className={`submit-btn${(isFormInvalid || isSaving) ? ' disabled' : ''}`}
                    >
                        {isSaving 
                            ? (editMode ? t('variant.updatingProduct') : t('variant.creatingProduct')) 
                            : (editMode ? t('variant.updateProduct') : t('variant.createProduct'))}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default AddProductStep3;
