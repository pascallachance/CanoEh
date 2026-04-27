import { useState, useEffect, useMemo, useRef, useCallback } from 'react';
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
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
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

interface ExistingItemVariantFeature {
    attributeName_en?: string | null;
    attributeName_fr?: string | null;
    attributes_en?: string | null;
    attributes_fr?: string | null;
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

function isPreviewOfferActive(variant: ItemVariant): boolean {
    if (!variant.offer || variant.offer <= 0) return false;
    if (variant.offerStart && new Date(variant.offerStart) > new Date()) return false;
    if (variant.offerEnd && new Date(variant.offerEnd) < new Date()) return false;
    return true;
}

function renderPreviewVariantPriceSection(
    variant: ItemVariant,
    offerActive: boolean,
    discountedPrice: number | null,
    language: string
) {
    const txt = (en: string, fr: string) => language === 'fr' ? fr : en;
    return offerActive && discountedPrice !== null ? (
        <>
            <span className="product-original-price">
                ${variant.price.toFixed(2)}
            </span>
            <span className="product-discounted-price">
                ${discountedPrice.toFixed(2)}
            </span>
            <span className="product-offer-badge">
                {txt(
                    `${variant.offer}% OFF`,
                    `Rabais ${variant.offer}%`
                )}
            </span>
        </>
    ) : (
        <span className="product-price">
            ${variant.price.toFixed(2)}
        </span>
    );
}

function AddProductStep3({ onSubmit, onBack, onCancel, step1Data, step2Data, companies, editMode = false, itemId, existingVariants, onStepNavigate, completedSteps }: AddProductStep3Props) {
    const { showSuccess, showError } = useNotifications();
    const { t, language } = useLanguage();
    const [variants, setVariants] = useState<ItemVariant[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState<string>('');
    const [isPreviewOpen, setIsPreviewOpen] = useState(false);
    const [previewSelectedAttributes, setPreviewSelectedAttributes] = useState<Record<string, string>>({});
    const [previewSelectedImageIndex, setPreviewSelectedImageIndex] = useState(0);
    const [previewIsVideoActive, setPreviewIsVideoActive] = useState(false);
    const [videoThumbnails, setVideoThumbnails] = useState<Record<string, string>>({});
    const processedVideoUrls = useRef<Record<string, string>>({});
    const previewModalRef = useRef<HTMLDivElement>(null);
    const variantsRef = useRef<ItemVariant[]>([]);

    // Keep ref in sync with variants state
    useEffect(() => {
        variantsRef.current = variants;
    }, [variants]);

    // Extract first frame from a video as a JPEG data-URL. Returns null on failure.
    const extractVideoFrame = useCallback((videoSrc: string): Promise<string | null> => {
        return new Promise((resolve) => {
            let settled = false;
            const settle = (value: string | null) => {
                if (settled) return;
                settled = true;
                video.src = '';
                clearTimeout(timeoutId);
                resolve(value);
            };

            const video = document.createElement('video');
            // For cross-origin videos, use anonymous CORS so canvas extraction can succeed
            // when the remote resource sends the appropriate CORS headers. Skip blob:/data:
            // URLs and same-origin URLs, which do not need crossOrigin.
            try {
                const resolvedUrl = new URL(videoSrc, window.location.href);
                const isSpecialScheme = resolvedUrl.protocol === 'blob:' || resolvedUrl.protocol === 'data:';
                const isCrossOrigin = resolvedUrl.origin !== window.location.origin;
                if (!isSpecialScheme && isCrossOrigin) {
                    video.crossOrigin = 'anonymous';
                }
            } catch {
                // Leave crossOrigin unset if the URL cannot be parsed.
            }
            video.muted = true;
            video.playsInline = true;
            video.preload = 'metadata';

            const drawFrame = () => {
                const canvas = document.createElement('canvas');
                canvas.width = video.videoWidth || 320;
                canvas.height = video.videoHeight || 180;
                const ctx = canvas.getContext('2d');
                if (ctx) {
                    try {
                        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                        settle(canvas.toDataURL('image/jpeg', 0.8));
                    } catch {
                        settle(null);
                    }
                } else {
                    settle(null);
                }
            };

            let metadataLoaded = false;
            video.onloadedmetadata = () => {
                metadataLoaded = true;
                // Guard against NaN/Infinity duration (some formats don't expose it from metadata alone)
                const duration = video.duration;
                const seekTime = (Number.isFinite(duration) && duration > 0) ? Math.min(0.5, duration / 4) : 0;
                if (seekTime === 0 || video.currentTime === seekTime) {
                    video.onloadeddata = () => drawFrame();
                } else {
                    video.currentTime = seekTime;
                }
            };
            // Guard against onseeked firing before onloadedmetadata (can happen in Chrome
            // with fast-loading sources like blob URLs), which would cause drawImage to throw.
            video.onseeked = () => { if (metadataLoaded) drawFrame(); };
            video.onerror = () => settle(null);

            // Safety net: resolve null if nothing fires within 8 seconds
            const timeoutId = setTimeout(() => settle(null), 8000);

            video.src = videoSrc;
        });
    }, []);

    // Extract video frames for variants whenever their videoUrl changes
    useEffect(() => {
        const currentIds = new Set(variants.map(v => v.id));

        // Prune entries for variants that no longer exist
        Object.keys(processedVideoUrls.current).forEach(id => {
            if (!currentIds.has(id)) {
                delete processedVideoUrls.current[id];
            }
        });
        setVideoThumbnails(prev => {
            const pruned = Object.keys(prev).filter(id => !currentIds.has(id));
            if (pruned.length === 0) return prev;
            const next = { ...prev };
            pruned.forEach(id => { delete next[id]; });
            return next;
        });

        variants.forEach(variant => {
            if (variant.videoUrl && processedVideoUrls.current[variant.id] !== variant.videoUrl) {
                // Clear the stale thumbnail immediately so the UI falls back to <video>
                setVideoThumbnails(prev => {
                    if (prev[variant.id] === undefined) return prev;
                    const { [variant.id]: _, ...rest } = prev;
                    return rest;
                });
                processedVideoUrls.current[variant.id] = variant.videoUrl;
                const capturedUrl = variant.videoUrl;
                extractVideoFrame(capturedUrl).then(thumbnail => {
                    // Only set if the variant still has the same URL (avoid stale updates)
                    if (processedVideoUrls.current[variant.id] === capturedUrl) {
                        if (thumbnail) {
                            setVideoThumbnails(prev => ({ ...prev, [variant.id]: thumbnail }));
                        } else {
                            setVideoThumbnails(prev => {
                                if (!(variant.id in prev)) return prev;
                                const { [variant.id]: _, ...rest } = prev;
                                return rest;
                            });
                        }
                    }
                });
            }
            if (!variant.videoUrl && processedVideoUrls.current[variant.id]) {
                delete processedVideoUrls.current[variant.id];
                setVideoThumbnails(prev => {
                    if (!(variant.id in prev)) return prev;
                    const { [variant.id]: _, ...rest } = prev;
                    return rest;
                });
            }
        });
    }, [variants, extractVideoFrame]);

    // Handle escape key to cancel
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            const target = event.target as HTMLElement;
            const isInputField =
                target.tagName === 'INPUT' ||
                target.tagName === 'TEXTAREA' ||
                target.tagName === 'SELECT' ||
                target.isContentEditable;
            if (event.key === 'Escape' && !isInputField) {
                if (isPreviewOpen) {
                    setIsPreviewOpen(false);
                    return;
                }
                onCancel();
            }
        };

        document.addEventListener('keydown', handleEscape);
        return () => document.removeEventListener('keydown', handleEscape);
    }, [onCancel, isPreviewOpen]);

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
                    (matchingExisting.itemVariantFeatures || []).forEach((feature: ExistingItemVariantFeature) => {
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
                        offer: matchingExisting.offer ?? genVariant.offer,
                        offerStart: matchingExisting.offerStart ?? genVariant.offerStart,
                        offerEnd: matchingExisting.offerEnd ?? genVariant.offerEnd,
                        thumbnailUrl: convertedThumbnailUrl || genVariant.thumbnailUrl,
                        imageUrls: convertedImageUrls.length > 0 ? convertedImageUrls : genVariant.imageUrls,
                        videoUrl: matchingExisting.videoUrl ? toAbsoluteUrl(matchingExisting.videoUrl) : genVariant.videoUrl,
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
                offer: null,
                offerStart: null,
                offerEnd: null,
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
            offer: null,
            offerStart: null,
            offerEnd: null,
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

    // Video upload handlers
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

    // Validation logic
    const isFormInvalid = useMemo(() => {
        if (variants.length > 0) {
            return variants.some(variant =>
                !variant.sku.trim() || variant.price <= 0 ||
                !variant.thumbnailUrl ||
                !variant.imageUrls || variant.imageUrls.length === 0 ||
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

                // Determine the video URL to preserve in DB.
                // If a new videoFile is provided the upload step will overwrite it, so pass null.
                let syncedVideoUrl: string | null;
                if (variant.videoFile) {
                    // New video file will be uploaded separately; DB update happens there
                    syncedVideoUrl = null;
                } else if (variant.videoUrl && !variant.videoUrl.startsWith('blob:') && !variant.videoUrl.startsWith('data:')) {
                    // Server-hosted URL: normalize to relative path to keep in DB
                    syncedVideoUrl = toRelativeUrl(variant.videoUrl);
                } else if (variant.videoUrl === '') {
                    // Explicitly cleared: pass empty string so the service removes it from DB
                    syncedVideoUrl = '';
                } else {
                    syncedVideoUrl = null;
                }

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
                            VideoUrl: syncedVideoUrl,
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

        // Upload video if a new video file is present
        if (variant.videoFile) {
            try {
                const formData = new FormData();
                formData.append('file', variant.videoFile);

                const uploadResponse = await fetch(
                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UploadVideo?variantId=${apiVariantId}`,
                    {
                        method: 'POST',
                        credentials: 'include',
                        body: formData,
                    }
                );

                if (!uploadResponse.ok) {
                    const errorText = await uploadResponse.text();
                    console.error(`Failed to upload video for variant ${apiVariantId}: ${uploadResponse.status} ${uploadResponse.statusText}`, errorText);
                }
            } catch (error) {
                console.error(`Error uploading video for variant ${apiVariantId}:`, error);
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

            const hasMissingVariantMedia = variants.some(variant =>
                !variant.thumbnailUrl || !variant.imageUrls || variant.imageUrls.length === 0
            );
            if (hasMissingVariantMedia) {
                const errorMessage = t('error.variantMediaRequired');
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

    const getPreviewText = (en: string, fr: string) => (language === 'fr' ? fr : en);

    const previewAttributeGroups = useMemo(
        () => step2Data.variantAttributes
            .map((attribute) => ({
                name_en: attribute.name_en,
                name_fr: attribute.name_fr,
                isMain: attribute.isMain ?? false,
                values: attribute.values.map((value) => ({
                    ...value,
                    thumbnailUrl: variants.find(
                        (variant) =>
                            variant.attributes_en[attribute.name_en] === value.en &&
                            !!variant.thumbnailUrl
                    )?.thumbnailUrl
                }))
            }))
            .sort((a, b) => Number(b.isMain) - Number(a.isMain)),
        [step2Data.variantAttributes, variants]
    );

    const findVariantForSelection = useCallback((
        selection: Record<string, string>,
        requireAllSelected: boolean
    ) => (
        variants.find(variant =>
            previewAttributeGroups.every(group => {
                const selectedValue = selection[group.name_en];
                if (!selectedValue) {
                    return !requireAllSelected;
                }
                return variant.attributes_en[group.name_en] === selectedValue;
            })
        ) || null
    ), [variants, previewAttributeGroups]);

    const getInitialPreviewAttributes = useCallback(() => {
        const firstVariant = variants[0];
        if (!firstVariant) {
            return {};
        }

        return previewAttributeGroups.reduce((acc, group) => {
            const selectedValue = firstVariant.attributes_en[group.name_en];
            if (selectedValue) {
                acc[group.name_en] = selectedValue;
            }
            return acc;
        }, {} as Record<string, string>);
    }, [variants, previewAttributeGroups]);

    const previewVariant = useMemo(() => {
        if (variants.length === 0) {
            return null;
        }

        if (previewAttributeGroups.length === 0) {
            return variants[0];
        }

        return findVariantForSelection(previewSelectedAttributes, true) || variants[0];
    }, [variants, previewAttributeGroups, previewSelectedAttributes, findVariantForSelection]);

    const previewImages = useMemo(() => {
        if (!previewVariant) {
            return [];
        }
        if (previewVariant.imageUrls && previewVariant.imageUrls.length > 0) {
            return previewVariant.imageUrls;
        }
        if (previewVariant.thumbnailUrl) {
            return [previewVariant.thumbnailUrl];
        }
        return [];
    }, [previewVariant]);

    const previewLastGroupPriceMap = useMemo(() => {
        if (previewAttributeGroups.length === 0) {
            return new Map<string, ItemVariant | null>();
        }

        const lastGroup = previewAttributeGroups[previewAttributeGroups.length - 1];
        const map = new Map<string, ItemVariant | null>();

        lastGroup.values.forEach((option) => {
            const selection = { ...previewSelectedAttributes, [lastGroup.name_en]: option.en };
            const matchedVariant =
                findVariantForSelection(selection, true) ||
                findVariantForSelection(selection, false);
            map.set(option.en, matchedVariant);
        });

        return map;
    }, [previewAttributeGroups, previewSelectedAttributes, findVariantForSelection]);

    const previewOfferActive = previewVariant ? isPreviewOfferActive(previewVariant) : false;
    const previewDiscountedPrice = previewOfferActive && previewVariant
        ? previewVariant.price * (1 - (previewVariant.offer ?? 0) / 100)
        : null;

    useEffect(() => {
        if (previewImages.length > 0 && previewSelectedImageIndex >= previewImages.length) {
            setPreviewSelectedImageIndex(0);
        }
    }, [previewImages, previewSelectedImageIndex]);

    useEffect(() => {
        if (!isPreviewOpen || !previewModalRef.current) {
            return;
        }

        const getFocusableElements = () =>
            Array.from(
                previewModalRef.current?.querySelectorAll<HTMLElement>(
                    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
                ) ?? []
            ).filter(el => !el.hasAttribute('disabled'));

        const focusable = getFocusableElements();
        if (focusable.length > 0) {
            focusable[0].focus();
        }

        const handleTabKey = (event: KeyboardEvent) => {
            if (event.key !== 'Tab') {
                return;
            }

            const currentFocusable = getFocusableElements();
            if (currentFocusable.length === 0) {
                return;
            }

            const firstElement = currentFocusable[0];
            const lastElement = currentFocusable[currentFocusable.length - 1];

            if (event.shiftKey) {
                if (document.activeElement === firstElement) {
                    event.preventDefault();
                    lastElement.focus();
                }
            } else if (document.activeElement === lastElement) {
                event.preventDefault();
                firstElement.focus();
            }
        };

        document.addEventListener('keydown', handleTabKey);
        return () => {
            document.removeEventListener('keydown', handleTabKey);
        };
    }, [isPreviewOpen]);

    const handleOpenPreview = () => {
        if (variants.length === 0) {
            return;
        }
        setPreviewSelectedAttributes(getInitialPreviewAttributes());
        setPreviewSelectedImageIndex(0);
        setPreviewIsVideoActive(false);
        setIsPreviewOpen(true);
    };

    const handlePreviewAttributeSelect = (attributeNameEn: string, valueEn: string) => {
        setPreviewSelectedAttributes(prev => {
            const tentativeSelection = {
                ...prev,
                [attributeNameEn]: valueEn
            };

            const matchedVariant = findVariantForSelection(tentativeSelection, true)
                || findVariantForSelection(tentativeSelection, false);

            if (!matchedVariant) {
                return tentativeSelection;
            }

            return previewAttributeGroups.reduce((acc, group) => {
                const variantValue = matchedVariant.attributes_en[group.name_en];
                if (variantValue) {
                    acc[group.name_en] = variantValue;
                }
                return acc;
            }, {} as Record<string, string>);
        });
        setPreviewSelectedImageIndex(0);
        setPreviewIsVideoActive(false);
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
                                        {/* Thumbnail + Product Images + Video side by side */}
                                        <div className="media-sections-row">
                                        {/* Thumbnail Section */}
                                        <div className="variant-field variant-field-media thumbnail-media-section">
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

                                        {/* Video Section */}
                                        <div className="variant-field variant-field-media video-media-section">
                                            <div className="media-upload-row">
                                                <label className="media-label" htmlFor={`video-${variant.id}`}>{t('variant.video')}</label>
                                                <div className="media-controls">
                                                    <input
                                                        type="file"
                                                        accept="video/mp4,video/quicktime,video/webm,video/avi,video/x-matroska"
                                                        onChange={(e) => handleVideoChange(variant.id, e.target.files?.[0] || null)}
                                                        className="file-input"
                                                        id={`video-${variant.id}`}
                                                        aria-label={t('variant.uploadVideoAriaLabel')}
                                                    />
                                                    <label htmlFor={`video-${variant.id}`} className="file-label">
                                                        {t('variant.chooseVideo')}
                                                    </label>
                                                </div>
                                                <div className="media-preview">
                                                    {variant.videoUrl && (
                                                        <div className="image-preview-item">
                                                            {videoThumbnails[variant.id] ? (
                                                                <img
                                                                    src={videoThumbnails[variant.id]}
                                                                    alt={t('variant.videoThumbnailAlt')}
                                                                    className="thumbnail-preview"
                                                                />
                                                            ) : (
                                                                <video
                                                                    src={variant.videoUrl}
                                                                    className="thumbnail-preview"
                                                                    muted
                                                                    playsInline
                                                                    preload="metadata"
                                                                    onLoadedMetadata={(e) => { e.currentTarget.currentTime = 0.1; }}
                                                                />
                                                            )}
                                                            <button
                                                                type="button"
                                                                onClick={() => handleRemoveVideo(variant.id)}
                                                                className="remove-media-btn"
                                                                title={t('variant.removeVideo')}
                                                                aria-label={t('variant.removeVideo')}
                                                            >
                                                                ×
                                                            </button>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        </div>
                                        </div>{/* end media-sections-row */}

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
                        className="preview-btn"
                        onClick={handleOpenPreview}
                        disabled={variants.length === 0 || isSaving}
                    >
                        {getPreviewText('Preview Product', 'Aperçu du produit')}
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

                {isPreviewOpen && (
                    <div
                        className="preview-modal-overlay"
                        role="dialog"
                        aria-modal="true"
                        aria-label={getPreviewText('Store product page preview', 'Aperçu de la page produit')}
                        onClick={() => setIsPreviewOpen(false)}
                    >
                        <div
                            className="preview-modal-content"
                            ref={previewModalRef}
                            onClick={(event) => event.stopPropagation()}
                        >
                            <div className="preview-modal-header">
                                <h3>{getPreviewText('Store product page preview', 'Aperçu de la page produit')}</h3>
                                <button
                                    type="button"
                                    className="preview-close-btn"
                                    onClick={() => setIsPreviewOpen(false)}
                                    aria-label={getPreviewText('Close preview', 'Fermer l’aperçu')}
                                >
                                    ×
                                </button>
                            </div>

                            <div className="preview-modal-body">
                                <div className="product-detail preview-product-detail">
                                    <div className="product-main">
                                        <section
                                            className="product-info"
                                            aria-label={getPreviewText('Product information', 'Informations sur le produit')}
                                        >
                                            <h4 className="product-name">{language === 'fr' ? step1Data.name_fr : step1Data.name}</h4>

                                            {previewAttributeGroups.length > 0 && (
                                                <div className="product-variants">
                                                    <h5 className="product-variants-title">{getPreviewText('Options', 'Options')}</h5>
                                                    {previewAttributeGroups.map((group, groupIndex) => (
                                                        <div key={group.name_en} className="product-attribute-group">
                                                            <p className="product-attribute-name">{language === 'fr' ? group.name_fr : group.name_en}</p>
                                                            <div className="product-attribute-options" role="group" aria-label={language === 'fr' ? group.name_fr : group.name_en}>
                                                                {group.values.map(value => {
                                                                    const selectedValue = previewSelectedAttributes[group.name_en];
                                                                    const optionValue = value.en;
                                                                    const hasThumbnail = group.isMain && !!value.thumbnailUrl;
                                                                    const button = (
                                                                        <button
                                                                            key={`${group.name_en}-${optionValue}`}
                                                                            type="button"
                                                                            className={`product-attribute-btn${selectedValue === optionValue ? ' selected' : ''}${hasThumbnail ? ' with-thumbnail' : ''}`}
                                                                            onClick={() => handlePreviewAttributeSelect(group.name_en, optionValue)}
                                                                            aria-pressed={selectedValue === optionValue}
                                                                        >
                                                                            {hasThumbnail && (
                                                                                <img
                                                                                    src={value.thumbnailUrl}
                                                                                    alt=""
                                                                                    aria-hidden="true"
                                                                                    className="product-attribute-btn-thumbnail"
                                                                                />
                                                                            )}
                                                                            {language === 'fr' ? value.fr : value.en}
                                                                        </button>
                                                                    );

                                                                    const isLastGroup = groupIndex === previewAttributeGroups.length - 1;
                                                                    if (!isLastGroup) {
                                                                        return button;
                                                                    }

                                                                    const optVariant = previewLastGroupPriceMap.get(optionValue);
                                                                    const optOfferActive = optVariant ? isPreviewOfferActive(optVariant) : false;
                                                                    const optEffectivePrice = optVariant
                                                                        ? (optOfferActive
                                                                            ? optVariant.price * (1 - (optVariant.offer ?? 0) / 100)
                                                                            : optVariant.price)
                                                                        : null;
                                                                    const optOriginalPrice = optOfferActive && optVariant
                                                                        ? optVariant.price
                                                                        : null;
                                                                    const formattedOptEffectivePrice = optEffectivePrice !== null
                                                                        ? `$${optEffectivePrice.toFixed(2)}`
                                                                        : '—';
                                                                    const optionLabel = language === 'fr' ? value.fr : value.en;
                                                                    const optionPriceAriaLabel = optEffectivePrice !== null
                                                                        ? `${optionLabel} price ${optOriginalPrice !== null ? `$${optOriginalPrice.toFixed(2)} original, ` : ''}${formattedOptEffectivePrice}${optOfferActive ? ' discounted' : ''}`
                                                                        : `${optionLabel} price unavailable`;

                                                                    return (
                                                                        <div
                                                                            key={`${group.name_en}-${optionValue}`}
                                                                            className={`product-option-with-price${optOfferActive ? ' has-offer' : ''}`}
                                                                        >
                                                                            {button}
                                                                            {optEffectivePrice !== null ? (
                                                                                optOriginalPrice !== null ? (
                                                                                    <div className="product-option-prices">
                                                                                        <span
                                                                                            className="product-option-original-price"
                                                                                            aria-label={getPreviewText(
                                                                                                `Original price $${optOriginalPrice.toFixed(2)}`,
                                                                                                `Prix original $${optOriginalPrice.toFixed(2)}`
                                                                                            )}
                                                                                        >
                                                                                            ${optOriginalPrice.toFixed(2)}
                                                                                        </span>
                                                                                        <span
                                                                                            className="product-option-price discounted"
                                                                                            aria-label={optionPriceAriaLabel}
                                                                                        >
                                                                                            {formattedOptEffectivePrice}
                                                                                        </span>
                                                                                    </div>
                                                                                ) : (
                                                                                    <span
                                                                                        className="product-option-price"
                                                                                        aria-label={optionPriceAriaLabel}
                                                                                    >
                                                                                        {formattedOptEffectivePrice}
                                                                                    </span>
                                                                                )
                                                                            ) : (
                                                                                <span
                                                                                    className="product-option-price unavailable"
                                                                                    aria-label={optionPriceAriaLabel}
                                                                                >
                                                                                    {formattedOptEffectivePrice}
                                                                                </span>
                                                                            )}
                                                                        </div>
                                                                    );
                                                                })}
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            )}

                                            {previewVariant ? (
                                                <>
                                                    <div className="product-price-section">
                                                        {renderPreviewVariantPriceSection(
                                                            previewVariant,
                                                            previewOfferActive,
                                                            previewDiscountedPrice,
                                                            language
                                                        )}
                                                    </div>
                                                    <p className={previewVariant.stock > 5 ? 'product-stock' : 'product-stock-low'}>
                                                        {previewVariant.stock > 0
                                                            ? (previewVariant.stock <= 5
                                                                ? getPreviewText(`${previewVariant.stock} in stock`, `${previewVariant.stock} en stock`)
                                                                : getPreviewText('In stock', 'En stock'))
                                                            : getPreviewText('Out of stock', 'Rupture de stock')}
                                                    </p>

                                                    {(previewVariant.sku || (previewVariant.productIdentifierType && previewVariant.productIdentifierValue)) && (
                                                        <div className="product-attributes">
                                                            <h5 className="product-attributes-title">{getPreviewText('Product Details', 'Détails du produit')}</h5>
                                                            {previewVariant.sku && (
                                                                <p className="product-attributes-row">
                                                                    {t('products.variant.sku')}: {previewVariant.sku}
                                                                </p>
                                                            )}
                                                            {previewVariant.productIdentifierType && previewVariant.productIdentifierValue && (
                                                                <p className="product-attributes-row">
                                                                    {previewVariant.productIdentifierType}: {previewVariant.productIdentifierValue}
                                                                </p>
                                                            )}
                                                        </div>
                                                    )}

                                                    {step2Data.variantFeatures.length > 0 && (
                                                        <div className="product-variant-features">
                                                            <h5 className="product-variant-features-title">{t('variant.features')}</h5>
                                                            <table className="product-variant-features-table">
                                                                <tbody>
                                                                    {step2Data.variantFeatures.map(feature => {
                                                                        const featureName = language === 'fr' ? feature.name_fr : feature.name_en;
                                                                        const featureValue = language === 'fr'
                                                                            ? (previewVariant.features_fr[feature.name_fr] || '')
                                                                            : (previewVariant.features_en[feature.name_en] || '');
                                                                        if (!featureValue) {
                                                                            return null;
                                                                        }
                                                                        return (
                                                                            <tr key={`${previewVariant.id}-${feature.name_en}`} className="product-variant-features-row">
                                                                                <th className="product-variant-features-name" scope="row">{featureName}</th>
                                                                                <td className="product-variant-features-value">{featureValue}</td>
                                                                            </tr>
                                                                        );
                                                                    })}
                                                                </tbody>
                                                            </table>
                                                        </div>
                                                    )}
                                                </>
                                            ) : (
                                                <span className="product-unavailable">
                                                    {getPreviewText('This combination is not available.', 'Cette combinaison n’est pas disponible.')}
                                                </span>
                                            )}
                                        </section>

                                        <section
                                            className="product-gallery"
                                            aria-label={getPreviewText('Product images', 'Images du produit')}
                                        >
                                            <div className="product-main-image-wrapper">
                                                {previewIsVideoActive && previewVariant?.videoUrl ? (
                                                    <video
                                                        src={previewVariant.videoUrl}
                                                        className="product-main-video"
                                                        controls
                                                        autoPlay
                                                    />
                                                ) : previewImages.length > 0 ? (
                                                    <img
                                                        src={previewImages[previewSelectedImageIndex]}
                                                        alt={language === 'fr' ? step1Data.name_fr : step1Data.name}
                                                        className="product-main-image"
                                                    />
                                                ) : (
                                                    <div className="product-main-image-placeholder">
                                                        {getPreviewText('No image available', 'Aucune image disponible')}
                                                    </div>
                                                )}
                                            </div>

                                            {(previewImages.length > 0 || previewVariant?.videoUrl) && (
                                                <ul className="product-thumbnails" aria-label={getPreviewText('Media thumbnails', 'Miniatures de médias')}>
                                                    {previewImages.map((imageUrl, index) => (
                                                        <li key={`${imageUrl}-${index}`}>
                                                            <button
                                                                type="button"
                                                                className={`product-thumbnail-btn${!previewIsVideoActive && previewSelectedImageIndex === index ? ' active' : ''}`}
                                                                onClick={() => { setPreviewSelectedImageIndex(index); setPreviewIsVideoActive(false); }}
                                                                aria-label={getPreviewText(`Select image ${index + 1}`, `Sélectionner l’image ${index + 1}`)}
                                                                aria-pressed={!previewIsVideoActive && previewSelectedImageIndex === index}
                                                            >
                                                                <img
                                                                    src={imageUrl}
                                                                    alt={`${language === 'fr' ? step1Data.name_fr : step1Data.name} ${index + 1}`}
                                                                    className="product-thumbnail-img"
                                                                />
                                                            </button>
                                                        </li>
                                                    ))}
                                                    {previewVariant?.videoUrl && (
                                                        <li>
                                                            <button
                                                                type="button"
                                                                className={`product-thumbnail-btn product-thumbnail-video-btn${previewIsVideoActive ? ' active' : ''}`}
                                                                onClick={() => setPreviewIsVideoActive(true)}
                                                                aria-label={getPreviewText('Play product video', 'Lire la vidéo du produit')}
                                                                aria-pressed={previewIsVideoActive}
                                                            >
                                                                {previewVariant.id && videoThumbnails[previewVariant.id] ? (
                                                                    <img
                                                                        src={videoThumbnails[previewVariant.id]}
                                                                        alt={t('variant.videoThumbnailAlt')}
                                                                        className="product-thumbnail-img"
                                                                    />
                                                                ) : (
                                                                    <video
                                                                        src={previewVariant.videoUrl}
                                                                        className="product-thumbnail-img"
                                                                        muted
                                                                        playsInline
                                                                        preload="metadata"
                                                                        onLoadedMetadata={(e) => { e.currentTarget.currentTime = 0.1; }}
                                                                    />
                                                                )}
                                                                <span className="product-video-play-icon" aria-hidden="true">▶</span>
                                                            </button>
                                                        </li>
                                                    )}
                                                </ul>
                                            )}

                                            {(language === 'fr' ? step1Data.description_fr : step1Data.description) && (
                                                <div className="product-description">
                                                    <h5 className="product-description-title">{getPreviewText('Description', 'Description')}</h5>
                                                    <p className="product-description-text">{language === 'fr' ? step1Data.description_fr : step1Data.description}</p>
                                                </div>
                                            )}
                                        </section>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
}

export default AddProductStep3;
