import { useState, useEffect, useMemo, useCallback, Fragment } from 'react';
import './ProductsSection.css';
import { useLanguage } from '../../contexts/LanguageContext';
import { useNotifications } from '../../contexts/useNotifications';
import { ApiClient } from '../../utils/apiClient';
import { 
    synchronizeBilingualArrays, 
    updateBilingualArrayValue, 
    removeBilingualArrayValue,
    validateBilingualArraySync,
    formatAttributeDisplay,
    formatAttributeName,
    formatVariantAttribute
} from '../../utils/bilingualArrayUtils';


interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

interface ProductsSectionProps {
    companies: Company[];
    viewMode?: 'list' | 'add';
    onViewModeChange?: (mode: 'list' | 'add') => void;
}

interface ItemAttribute {
    name_en: string;
    name_fr: string;
    values_en: string[];
    values_fr: string[];
}

interface BilingualItemAttribute {
    name_en: string;
    name_fr: string;
    value_en: string;
    value_fr: string;
}

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
    imageUrls?: string[]; // Array of image URLs (1-10 images)
}

interface Category {
    id: string;
    name_en: string;
    name_fr: string;
    parentCategoryId?: string;
    createdAt: string;
    updatedAt?: string;
}

// API response types for fetched items
interface ApiItemVariant {
    id: string;
    price: number;
    stockQuantity: number;
    sku: string;
    productIdentifierType?: string;
    productIdentifierValue?: string;
    imageUrls?: string;
    thumbnailUrl?: string;
    itemVariantName_en?: string;
    itemVariantName_fr?: string;
    deleted: boolean;
}

interface ApiItem {
    id: string;
    sellerID: string;
    name_en: string;
    name_fr: string;
    description_en?: string;
    description_fr?: string;
    categoryID: string;
    variants: ApiItemVariant[];
    createdAt: string;
    updatedAt?: string;
    deleted: boolean;
}

function ProductsSection({ companies, viewMode = 'list', onViewModeChange }: ProductsSectionProps) {
    const [categories, setCategories] = useState<Category[]>([]);
    const { language, t } = useLanguage();
    const { showError, showSuccess } = useNotifications();
    const showAddForm = viewMode === 'add';
    const showListSection = viewMode === 'list';

    // State for fetched seller items from API
    const [sellerItems, setSellerItems] = useState<ApiItem[]>([]);
    const [isLoadingItems, setIsLoadingItems] = useState(false);
    const [loadItemsError, setLoadItemsError] = useState<string>('');
    const [expandedItemId, setExpandedItemId] = useState<string | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const ITEMS_PER_PAGE = 25;

    const [newItem, setNewItem] = useState({
        name: '',
        name_fr: '',
        description: '',
        description_fr: '',
        categoryId: '',
        attributes: [] as ItemAttribute[],
        itemAttributes: [] as BilingualItemAttribute[]
    });
    const [newAttribute, setNewAttribute] = useState({ 
        name_en: '', 
        name_fr: '', 
        values_en: [''], 
        values_fr: [''] 
    });
    const [attributeError, setAttributeError] = useState('');
    
    // State for the new bilingual item attributes
    const [newItemAttribute, setNewItemAttribute] = useState({
        name_en: '',
        name_fr: '',
        value_en: '',
        value_fr: ''
    });
    const [variants, setVariants] = useState<ItemVariant[]>([]);
    const [isSaving, setIsSaving] = useState(false);

    // Cleanup object URLs on component unmount
    useEffect(() => {
        return () => {
            // Clean up all object URLs from variants when component unmounts
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

    // Validation logic for save button
    const isFormInvalid = useMemo(() => {
        // Basic form validation
        if (!newItem.name || !newItem.name_fr || !newItem.description || !newItem.description_fr || !newItem.categoryId) {
            return true;
        }
        
        // Variant validation if variants exist
        if (variants.length > 0) {
            return variants.some(variant => 
                !variant.sku.trim() || variant.price <= 0
            );
        }
        
        return false;
    }, [newItem.name, newItem.name_fr, newItem.description, newItem.description_fr, newItem.categoryId, variants]);

    // Memoized synchronized attribute values to avoid redundant computation
    const synchronizedAttributeValues = useMemo(() => {
        return synchronizeBilingualArrays(newAttribute.values_en, newAttribute.values_fr);
    }, [newAttribute.values_en, newAttribute.values_fr]);

    // Memoized disabled state for "Add Attribute" button to avoid re-computation on every render
    const isAddAttributeDisabled = useMemo(() => {
        return !newItemAttribute.name_en || !newItemAttribute.name_fr || 
               !newItemAttribute.value_en || !newItemAttribute.value_fr;
    }, [newItemAttribute.name_en, newItemAttribute.name_fr, newItemAttribute.value_en, newItemAttribute.value_fr]);

    // Memoized disabled state for "Add Value" button to avoid array iterations on every render
    const isAddValueDisabled = useMemo(() => {
        return synchronizedAttributeValues.values_en.some(value => value.trim() === '') || 
               synchronizedAttributeValues.values_fr.some(value => value.trim() === '');
    }, [synchronizedAttributeValues.values_en, synchronizedAttributeValues.values_fr]);

    // Fetch categories on component mount
    const fetchCategories = async () => {
        try {
            // For demo purposes, using mock categories when API is not available
            // Replace with your actual API endpoint when database is configured
            const response = await ApiClient.get(`${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Category/GetAllCategories`);
            if (response.ok) {
                const result = await response.json();
                if (result.value) {
                    setCategories(result.value);
                    return;
                }
            }
        } catch (error) {
            console.error('Failed to fetch categories:', error);
        }
        
        // Mock categories for demo purposes
        setCategories([
            {
                id: '1',
                name_en: 'Electronics',
                name_fr: 'Électronique',
                createdAt: new Date().toISOString()
            },
            {
                id: '2',
                name_en: 'Clothing',
                name_fr: 'Vêtements',
                createdAt: new Date().toISOString()
            },
            {
                id: '3',
                name_en: 'Books',
                name_fr: 'Livres',
                createdAt: new Date().toISOString()
            },
            {
                id: '4',
                name_en: 'Home & Garden',
                name_fr: 'Maison et Jardin',
                createdAt: new Date().toISOString()
            }
        ]);
    };

    // Fetch seller items from API
    const fetchSellerItems = useCallback(async (signal?: AbortSignal) => {
        // Get seller ID from first company's ownerID. The ownerID is the user ID of the seller
        // who owns the company, which is the currently logged-in user.
        const sellerId = companies.length > 0 ? companies[0].ownerID : null;
        if (!sellerId) {
            setLoadItemsError(t('products.error.noSellerId'));
            return;
        }

        setIsLoadingItems(true);
        setLoadItemsError('');

        try {
            const response = await ApiClient.get(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/GetSellerItems/${sellerId}`,
                { signal }
            );

            // Check if the request was aborted
            if (signal?.aborted) {
                return;
            }

            if (response.ok) {
                const result = await response.json();
                // The API returns Result<List<GetItemResponse>> with value property
                const fetchedItems = Array.isArray(result?.value) ? result.value : Array.isArray(result) ? result : [];
                setSellerItems(fetchedItems);
            } else {
                const errorText = await response.text();
                setLoadItemsError(errorText || t('products.list.error'));
            }
        } catch (error) {
            // Don't log or set error if the request was aborted
            if (signal?.aborted) {
                return;
            }
            console.error('Failed to fetch seller items:', error);
            setLoadItemsError(t('products.list.error'));
        } finally {
            // Only update loading state if not aborted
            if (!signal?.aborted) {
                setIsLoadingItems(false);
            }
        }
    }, [companies, t]);

    // Load categories when component mounts
    useEffect(() => {
        fetchCategories();
    }, []);

    // Load seller items when component mounts or when fetchSellerItems changes
    // Uses AbortController to cancel pending requests and prevent race conditions
    useEffect(() => {
        if (companies.length > 0) {
            const abortController = new AbortController();
            fetchSellerItems(abortController.signal);
            return () => abortController.abort();
        }
    }, [fetchSellerItems, companies.length]);

    // Reset to page 1 when seller items change (e.g., after creating a new item)
    useEffect(() => {
        setCurrentPage(1);
    }, [sellerItems.length]);

    // Reset expanded state when navigating to a different page
    useEffect(() => {
        setExpandedItemId(null);
    }, [currentPage]);

    // Sort items by name based on current language
    const sortedItems = useMemo(() => {
        return [...sellerItems].sort((a, b) => {
            const nameA = language === 'fr' ? a.name_fr : a.name_en;
            const nameB = language === 'fr' ? b.name_fr : b.name_en;
            return nameA.localeCompare(nameB, language === 'fr' ? 'fr' : 'en', { sensitivity: 'base' });
        });
    }, [sellerItems, language]);

    // Pagination calculations
    const totalPages = Math.ceil(sortedItems.length / ITEMS_PER_PAGE);
    const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
    const endIndex = startIndex + ITEMS_PER_PAGE;
    const paginatedItems = sortedItems.slice(startIndex, endIndex);

    // Toggle expanded row
    const toggleExpandedRow = (itemId: string) => {
        setExpandedItemId(prev => prev === itemId ? null : itemId);
    };

    // Get category name by ID
    const getCategoryName = (categoryId: string): string => {
        const category = categories.find(c => c.id === categoryId);
        if (!category) return t('common.unknown');
        return language === 'fr' ? category.name_fr : category.name_en;
    };

    // Get item name based on language
    const getItemName = (item: ApiItem): string => {
        return language === 'fr' ? item.name_fr : item.name_en;
    };

    // Get variant name based on language
    const getVariantName = (variant: ApiItemVariant): string => {
        const name = language === 'fr' ? variant.itemVariantName_fr : variant.itemVariantName_en;
        return name || '-';
    };

    // Format date for display
    const formatDate = (dateString: string | undefined): string => {
        if (!dateString) return '-';
        const date = new Date(dateString);
        return date.toLocaleDateString(language === 'fr' ? 'fr-CA' : 'en-CA', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    };

    const addAttributeValue = () => {
        setNewAttribute(prev => ({
            ...prev,
            values_en: [...prev.values_en, ''],
            values_fr: [...prev.values_fr, '']
        }));
    };

    const removeAttributeValue = (index: number) => {
        setNewAttribute(prev => {
            const { values_en, values_fr } = removeBilingualArrayValue(prev.values_en, prev.values_fr, index);
            return {
                ...prev,
                values_en,
                values_fr
            };
        });
    };

    const updateAttributeValue = (index: number, value: string, language: 'en' | 'fr') => {
        setNewAttribute(prev => {
            const { values_en, values_fr } = updateBilingualArrayValue(prev.values_en, prev.values_fr, index, value, language);
            return {
                ...prev,
                values_en,
                values_fr
            };
        });
    };

    const addItemAttribute = () => {
        if (!newItemAttribute.name_en || !newItemAttribute.name_fr || 
            !newItemAttribute.value_en || !newItemAttribute.value_fr) {
            return; // Don't add if any field is empty
        }

        setNewItem(prev => ({
            ...prev,
            itemAttributes: [...prev.itemAttributes, { ...newItemAttribute }]
        }));
        setNewItemAttribute({
            name_en: '',
            name_fr: '',
            value_en: '',
            value_fr: ''
        });
    };

    const removeItemAttribute = (index: number) => {
        setNewItem(prev => ({
            ...prev,
            itemAttributes: prev.itemAttributes.filter((_, i) => i !== index)
        }));
    };

    const addAttribute = () => {
        // Clear any previous error
        setAttributeError('');
        
        if (!newAttribute.name_en || !newAttribute.name_fr) {
            setAttributeError(t('error.bilingualNamesMissing'));
            return;
        }

        // Validate synchronized arrays for non-empty values
        const validation = validateBilingualArraySync(
            newAttribute.values_en,
            newAttribute.values_fr,
            { 
                filterEmpty: true, 
                errorType: 'user',
                customUserErrorMessage: t('error.bilingualValuesMismatch'),
                allowEmpty: false
            }
        );
        
        if (!validation.isValid) {
            setAttributeError(validation.errorMessage || "Array synchronization failed.");
            return;
        }

        // Check for duplicate attribute names (case-insensitive)
        const isDuplicate = newItem.attributes.some(attr => 
            attr.name_en.toLowerCase() === newAttribute.name_en.toLowerCase() ||
            attr.name_fr.toLowerCase() === newAttribute.name_fr.toLowerCase()
        );

        if (isDuplicate) {
            setAttributeError(`Attribute "${newAttribute.name_en}" or "${newAttribute.name_fr}" already exists. Please use different names.`);
            return;
        }

        setNewItem(prev => ({
            ...prev,
            attributes: [...prev.attributes, {
                name_en: newAttribute.name_en,
                name_fr: newAttribute.name_fr,
                values_en: validation.values_en!,
                values_fr: validation.values_fr!
            }]
        }));
        setNewAttribute({ 
            name_en: '', 
            name_fr: '', 
            values_en: [''], 
            values_fr: [''] 
        });
    };

    const removeAttribute = (index: number) => {
        setNewItem(prev => ({
            ...prev,
            attributes: prev.attributes.filter((_, i) => i !== index)
        }));
    };

    const generateVariants = (): ItemVariant[] => {
        if (newItem.attributes.length === 0) {
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
            if (attrIndex >= newItem.attributes.length) {
                combinations.push({ en: { ...currentEn }, fr: { ...currentFr } });
                return;
            }
            
            const attribute = newItem.attributes[attrIndex];
            
            // Ensure synchronized arrays - validate that lengths match for safety
            const validation = validateBilingualArraySync(
                attribute.values_en, 
                attribute.values_fr, 
                { attributeName: attribute.name_en, errorType: 'console' }
            );
            if (!validation.isValid) {
                return; // Skip this attribute to prevent silent data loss
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
        // Find the current variant to revoke previous URL
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.thumbnailUrl && currentVariant.thumbnailUrl.startsWith('blob:')) {
            URL.revokeObjectURL(currentVariant.thumbnailUrl);
        }

        if (file) {
            // In a real app, you would upload the file and get a URL
            // For demo purposes, we'll create a local URL
            const url = URL.createObjectURL(file);
            updateVariant(variantId, 'thumbnailUrl', url);
        } else {
            updateVariant(variantId, 'thumbnailUrl', '');
        }
    };

    // Helper function to handle product images file selection
    const handleImagesChange = (variantId: string, files: FileList | null) => {
        // Find the current variant to revoke previous URLs
        const currentVariant = variants.find(v => v.id === variantId);
        if (currentVariant?.imageUrls) {
            currentVariant.imageUrls.forEach(url => {
                if (url.startsWith('blob:')) {
                    URL.revokeObjectURL(url);
                }
            });
        }

        if (files && files.length > 0) {
            // Limit to 10 images maximum
            const fileArray = Array.from(files).slice(0, 10);
            // In a real app, you would upload the files and get URLs
            // For demo purposes, we'll create local URLs
            const urls: string[] = [];
            try {
                fileArray.forEach(file => {
                    urls.push(URL.createObjectURL(file));
                });
                updateVariant(variantId, 'imageUrls', urls);
            } catch (error) {
                // If there's an error creating object URLs, clean up any that were created
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

    // Product identifier types
    const identifierTypes = [
        { value: '', label: t('products.selectIdType') },
        { value: 'UPC', label: 'UPC' },
        { value: 'EAN', label: 'EAN' },
        { value: 'GTIN', label: 'GTIN' },
        { value: 'ISBN', label: 'ISBN' },
        { value: 'ASIN', label: 'ASIN' },
        { value: 'SKU', label: 'SKU' },
        { value: 'MPN', label: 'MPN (Manufacturer Part Number)' }
    ];

    const handleGenerateVariants = () => {
        // Clean up any existing object URLs before generating new variants
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
        
        const newVariants = generateVariants();
        setVariants(newVariants);
    };

    const handleSaveItem = async () => {
        // Validate basic item fields
        if (!newItem.name || !newItem.name_fr || !newItem.description || !newItem.description_fr || !newItem.categoryId) {
            showError('Please fill in all required fields.');
            return;
        }

        // Get seller ID from first company (assuming user owns the companies)
        const sellerId = companies.length > 0 ? companies[0].ownerID : null;
        if (!sellerId) {
            showError('Unable to determine seller ID. Please ensure you are logged in.');
            return;
        }
        
        // Validate variants if they exist
        if (variants.length > 0) {
            const hasInvalidVariants = variants.some(variant => 
                !variant.sku.trim() || variant.price <= 0
            );
            
            if (hasInvalidVariants) {
                showError('Please ensure all variants have a SKU and price greater than 0.');
                return;
            }
        }

        setIsSaving(true);

        try {
            // Transform frontend data to match backend CreateItemRequest format
            const createItemRequest = {
                SellerID: sellerId,
                Name_en: newItem.name,
                Name_fr: newItem.name_fr,
                Description_en: newItem.description,
                Description_fr: newItem.description_fr,
                CategoryID: newItem.categoryId,
                Variants: variants.map(variant => ({
                    Price: variant.price,
                    StockQuantity: variant.stock,
                    Sku: variant.sku,
                    ProductIdentifierType: variant.productIdentifierType || null,
                    ProductIdentifierValue: variant.productIdentifierValue || null,
                    ImageUrls: variant.imageUrls?.join(',') || null,
                    ThumbnailUrl: variant.thumbnailUrl || null,
                    ItemVariantName_en: variant.attributes_en ? Object.entries(variant.attributes_en).map(([k, v]) => `${k}: ${v}`).join(', ') : null,
                    ItemVariantName_fr: variant.attributes_fr ? Object.entries(variant.attributes_fr).map(([k, v]) => `${k}: ${v}`).join(', ') : null,
                    ItemVariantAttributes: variant.attributes_en ? Object.entries(variant.attributes_en).map(([attrNameEn, attrValueEn]) => {
                        // Find the corresponding ItemAttribute to get the French attribute name
                        const itemAttribute = newItem.attributes.find(attr => attr.name_en === attrNameEn);
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
                ItemAttributes: newItem.itemAttributes.map(attr => ({
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
                // Refresh the seller items list from API
                await fetchSellerItems();
                
                // Reset form
                setNewItem({ 
                    name: '', 
                    name_fr: '', 
                    description: '', 
                    description_fr: '', 
                    categoryId: '', 
                    attributes: [],
                    itemAttributes: []
                });
                setVariants([]);
                
                // Switch back to list view after saving
                if (onViewModeChange) {
                    onViewModeChange('list');
                }
                
                // Show success message
                showSuccess('Item created successfully!');
                
            } else {
                const errorText = await response.text();
                showError(`Failed to create item: ${errorText}`);
            }
            
        } catch (error) {
            console.error('Error creating item:', error);
            showError('An unexpected error occurred while creating the item.');
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="section-container">
            {showAddForm && (
                <div className="products-add-form">
                    <h3>{t('products.addProduct')}</h3>
                    
                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.itemName')}
                        </label>
                        <input
                            type="text"
                            value={newItem.name}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name: e.target.value }))}
                            className="products-form-input"
                            placeholder={t('placeholder.itemName')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.itemNameFr')}
                        </label>
                        <input
                            type="text"
                            value={newItem.name_fr}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name_fr: e.target.value }))}
                            className="products-form-input"
                            placeholder={t('placeholder.itemNameFr')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.description')}
                        </label>
                        <textarea
                            value={newItem.description}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description: e.target.value }))}
                            className="products-form-textarea"
                            placeholder={t('placeholder.description')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.descriptionFr')}
                        </label>
                        <textarea
                            value={newItem.description_fr}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description_fr: e.target.value }))}
                            className="products-form-textarea"
                            placeholder={t('placeholder.descriptionFr')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.category')}
                        </label>
                        <select
                            value={newItem.categoryId}
                            onChange={(e) => setNewItem(prev => ({ ...prev, categoryId: e.target.value }))}
                            className="products-form-input"
                        >
                            <option value="">{t('products.selectCategory')}</option>
                            {categories.map(category => (
                                <option key={category.id} value={category.id}>
                                    {language === 'fr' ? category.name_fr : category.name_en}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="item-attributes-section">
                        <h4>{t('products.itemAttributesTitle')}</h4>
                        <div className="products-form-group">
                            <div className="attribute-input-row">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeNameEn')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.name_en}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_en: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeNameEn')}
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeValueEn')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.value_en}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, value_en: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeValueEn')}
                                    />
                                </div>
                            </div>
                            <div className="attribute-input-row">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeNameFr')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.name_fr}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_fr: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeNameFr')}
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeValueFr')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.value_fr}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, value_fr: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeValueFr')}
                                    />
                                </div>
                            </div>
                            <div className="attribute-actions">
                                <button
                                    onClick={addItemAttribute}
                                    className="products-add-attribute-button"
                                    disabled={isAddAttributeDisabled}
                                >
                                    {t('products.addNewAttribute')}
                                </button>
                            </div>
                        </div>

                        {newItem.itemAttributes.length > 0 && (
                            <div className="added-item-attributes">
                                <h5>{t('products.attributes')}</h5>
                                {newItem.itemAttributes.map((attr, index) => (
                                    <div key={index} className="item-attribute-display">
                                        <div className="attribute-display-content">
                                            <div className="attribute-lang-pair">
                                                <strong>{t('products.attributeNameEn')}:</strong> {attr.name_en} | 
                                                <strong> {t('products.attributeValueEn')}:</strong> {attr.value_en}
                                            </div>
                                            <div className="attribute-lang-pair">
                                                <strong>{t('products.attributeNameFr')}:</strong> {attr.name_fr} | 
                                                <strong> {t('products.attributeValueFr')}:</strong> {attr.value_fr}
                                            </div>
                                        </div>
                                        <button
                                            onClick={() => removeItemAttribute(index)}
                                            className="products-remove-attribute-button"
                                        >
                                            {t('products.removeAttribute')}
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    <div className="products-variants-section">
                        <h4>{t('products.itemAttributes')}</h4>
                        <div className="products-variant-input">
                            <div className="products-variant-name">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeName')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newAttribute.name_en}
                                        onChange={(e) => {
                                            setNewAttribute(prev => ({ ...prev, name_en: e.target.value }));
                                            // Clear error when user starts typing
                                            if (attributeError) {
                                                setAttributeError('');
                                            }
                                        }}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeName')}
                                        aria-invalid={!!attributeError}
                                        aria-describedby={attributeError ? "attribute-name-error" : undefined}
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeNameFrVariant')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newAttribute.name_fr}
                                        onChange={(e) => {
                                            setNewAttribute(prev => ({ ...prev, name_fr: e.target.value }));
                                            // Clear error when user starts typing
                                            if (attributeError) {
                                                setAttributeError('');
                                            }
                                        }}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeNameFrVariant')}
                                        aria-invalid={!!attributeError}
                                        aria-describedby={attributeError ? "attribute-name-error" : undefined}
                                    />
                                </div>
                                {attributeError && (
                                    <div
                                        id="attribute-name-error"
                                        className="products-error-message"
                                        style={{ color: 'red', fontSize: '14px', marginTop: '4px' }}
                                        role="alert"
                                    >
                                        {attributeError}
                                    </div>
                                )}
                            </div>
                            <div className="products-variant-values">
                                <>
                                    <div className="attribute-input-group">
                                        <label className="products-form-label">
                                            {t('products.attributeValues')}
                                        </label>
                                        {synchronizedAttributeValues.values_en.map((value, index) => (
                                            <div key={index} className="products-attribute-value-row">
                                                <input
                                                    type="text"
                                                    value={value}
                                                    onChange={(e) => updateAttributeValue(index, e.target.value, 'en')}
                                                    className="products-attribute-value-input"
                                                    placeholder={t('placeholder.attributeValue')}
                                                />
                                                {synchronizedAttributeValues.length > 1 && (
                                                    <button
                                                        onClick={() => removeAttributeValue(index)}
                                                        className="products-remove-value-button"
                                                    >
                                                        {t('products.deleteItem')}
                                                    </button>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                    <div className="attribute-input-group">
                                        <label className="products-form-label">
                                            {t('products.attributeValuesFr')}
                                        </label>
                                        {synchronizedAttributeValues.values_fr.map((value, index) => (
                                            <div key={index} className="products-attribute-value-row">
                                                <input
                                                    type="text"
                                                    value={value}
                                                    onChange={(e) => updateAttributeValue(index, e.target.value, 'fr')}
                                                    className="products-attribute-value-input"
                                                    placeholder={t('placeholder.attributeValueFrVariant')}
                                                />
                                                {synchronizedAttributeValues.length > 1 && (
                                                    <button
                                                        onClick={() => removeAttributeValue(index)}
                                                        className="products-remove-value-button"
                                                    >
                                                        {t('products.deleteItem')}
                                                    </button>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                </>
                                <button
                                    onClick={addAttributeValue}
                                    className="products-add-value-button"
                                    disabled={isAddValueDisabled}
                                >
                                    {t('products.addValue')}
                                </button>
                            </div>
                            <div className="products-variant-actions">
                                <button
                                    onClick={addAttribute}
                                    className="products-add-attribute-button"
                                >
                                    {t('products.addAttribute')}
                                </button>
                            </div>
                        </div>

                        {newItem.attributes.length > 0 && (
                            <div className="products-added-attributes">
                                <h5>{t('products.attributes')}</h5>
                                {newItem.attributes.map((attr, index) => {
                                    const formatted = formatAttributeDisplay(attr.name_en, attr.name_fr, attr.values_en, attr.values_fr);
                                    return (
                                        <div key={index} className="products-attribute-item">
                                            <span>
                                                <div><strong>EN:</strong> {formatted.en}</div>
                                                <div><strong>FR:</strong> {formatted.fr}</div>
                                            </span>
                                            <button
                                                onClick={() => removeAttribute(index)}
                                                className="products-remove-attribute-button"
                                            >
                                                {t('products.deleteItem')}
                                            </button>
                                        </div>
                                    );
                                })}
                                <button
                                    onClick={handleGenerateVariants}
                                    className="products-generate-variants-button"
                                >
                                    Generate Variants
                                </button>
                            </div>
                        )}
                    </div>

                    {variants.length > 0 && (
                        <div className="products-variants-section">
                            <h4>Item Variants</h4>
                            <div className="products-variants-table-container">
                                <table className="products-variants-table">
                                    <thead>
                                        <tr>
                                            {newItem.attributes.map(attr => {
                                                const formatted = formatAttributeName(attr.name_en, attr.name_fr);
                                                return (
                                                    <th key={`${attr.name_en}-${attr.name_fr}`}>
                                                        <div>
                                                            <div><strong>EN:</strong> {formatted.en}</div>
                                                            <div><strong>FR:</strong> {formatted.fr}</div>
                                                        </div>
                                                    </th>
                                                );
                                            })}
                                            <th>SKU</th>
                                            <th>{t('products.productIdentifierType')}</th>
                                            <th>{t('products.productIdentifierValue')}</th>
                                            <th>Price</th>
                                            <th>Stock</th>
                                            <th>{t('products.thumbnailImage')}</th>
                                            <th>{t('products.productImages')}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {variants.map(variant => (
                                            <tr key={variant.id}>
                                                {newItem.attributes.map(attr => {
                                                    const formatted = formatVariantAttribute(attr.name_en, attr.name_fr, variant.attributes_en, variant.attributes_fr);
                                                    return (
                                                        <td key={`${attr.name_en}-${attr.name_fr}`}>
                                                            <div>
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
                                                        className={`products-variant-input products-variant-input--sku ${!variant.sku.trim() ? 'products-variant-input--required' : ''}`}
                                                        placeholder="SKU *"
                                                        required
                                                    />
                                                </td>
                                                <td>
                                                    <select
                                                        value={variant.productIdentifierType || ''}
                                                        onChange={(e) => updateVariant(variant.id, 'productIdentifierType', e.target.value)}
                                                        className="products-variant-input products-variant-input--select"
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
                                                        className="products-variant-input products-variant-input--identifier"
                                                        placeholder="ID Value"
                                                        disabled={!variant.productIdentifierType}
                                                    />
                                                </td>
                                                <td>
                                                    <input
                                                        type="number"
                                                        value={variant.price}
                                                        onChange={(e) => updateVariant(variant.id, 'price', parseFloat(e.target.value) || 0)}
                                                        className={`products-variant-input ${variant.price <= 0 ? 'products-variant-input--invalid' : ''}`}
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
                                                        className="products-variant-input products-variant-input--stock"
                                                        min="0"
                                                        placeholder="0"
                                                    />
                                                </td>
                                                <td>
                                                    <div className="products-file-input-container">
                                                        <input
                                                            type="file"
                                                            accept="image/*"
                                                            onChange={(e) => handleThumbnailChange(variant.id, e.target.files?.[0] || null)}
                                                            className="products-file-input"
                                                            id={`thumbnail-${variant.id}`}
                                                        />
                                                        <label htmlFor={`thumbnail-${variant.id}`} className="products-file-label">
                                                            {t('products.chooseThumbnail')}
                                                        </label>
                                                        {variant.thumbnailUrl && (
                                                            <div className="products-image-preview">
                                                                <img src={variant.thumbnailUrl} alt="Thumbnail" className="products-thumbnail-preview" />
                                                            </div>
                                                        )}
                                                    </div>
                                                </td>
                                                <td>
                                                    <div className="products-file-input-container">
                                                        <input
                                                            type="file"
                                                            accept="image/*"
                                                            multiple
                                                            onChange={(e) => handleImagesChange(variant.id, e.target.files)}
                                                            className="products-file-input"
                                                            id={`images-${variant.id}`}
                                                        />
                                                        <label htmlFor={`images-${variant.id}`} className="products-file-label">
                                                            {t('products.chooseImages')}
                                                        </label>
                                                        {variant.imageUrls && variant.imageUrls.length > 0 && (
                                                            <div className="products-images-preview">
                                                                <small>{variant.imageUrls.length} {variant.imageUrls.length === 1 ? 'image' : 'images'} selected</small>
                                                                <div className="products-images-grid">
                                                                    {variant.imageUrls.slice(0, 3).map((url, index) => (
                                                                        <img key={index} src={url} alt={`Product ${index + 1}`} className="products-image-preview-small" />
                                                                    ))}
                                                                    {variant.imageUrls.length > 3 && (
                                                                        <div className="products-more-images">+{variant.imageUrls.length - 3}</div>
                                                                    )}
                                                                </div>
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
                    )}

                    <div className="products-form-actions">
                        <button
                            onClick={handleSaveItem}
                            disabled={isFormInvalid || isSaving}
                            className={`products-action-button products-action-button--save${(isFormInvalid || isSaving) ? ' products-action-button--disabled' : ''}`}
                        >
                            {isSaving ? t('products.saving') : t('products.addItem')}
                        </button>
                        <button
                            onClick={() => onViewModeChange && onViewModeChange('list')}
                            className="products-action-button products-action-button--cancel"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {showListSection && (
                <div className="products-list-section">
                    <h3>{t('products.list.currentItems')} ({sellerItems.length})</h3>
                    
                    {isLoadingItems ? (
                        <p className="products-loading">{t('products.list.loading')}</p>
                    ) : loadItemsError ? (
                        <p className="products-error">{loadItemsError}</p>
                    ) : sellerItems.length === 0 ? (
                        <p className="products-empty">{t('products.list.noItems')}</p>
                    ) : (
                        <>
                            <div className="products-list-table-container">
                                <table className="products-list-table">
                                    <thead>
                                        <tr>
                                            <th>{t('products.list.itemName')}</th>
                                            <th>{t('products.list.itemCategory')}</th>
                                            <th>{t('products.list.creationDate')}</th>
                                            <th>{t('products.list.lastUpdated')}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {paginatedItems.map(item => (
                                            <Fragment key={item.id}>
                                                <tr 
                                                    className={`products-list-row ${expandedItemId === item.id ? 'expanded' : ''}`}
                                                    onClick={() => toggleExpandedRow(item.id)}
                                                    role="button"
                                                    tabIndex={0}
                                                    aria-expanded={expandedItemId === item.id}
                                                    onKeyDown={(e) => {
                                                        if (e.key === 'Enter' || e.key === ' ') {
                                                            e.preventDefault();
                                                            toggleExpandedRow(item.id);
                                                        }
                                                    }}
                                                >
                                                    <td>{getItemName(item)}</td>
                                                    <td>{getCategoryName(item.categoryID)}</td>
                                                    <td>{formatDate(item.createdAt)}</td>
                                                    <td>{formatDate(item.updatedAt)}</td>
                                                </tr>
                                                {expandedItemId === item.id && (
                                                    <tr className="products-variants-row">
                                                        <td colSpan={4}>
                                                            {item.variants && item.variants.filter(v => !v.deleted).length > 0 ? (
                                                                <div className="products-variants-expanded">
                                                                    <table className="products-variants-inner-table">
                                                                        <thead>
                                                                            <tr>
                                                                                <th>{t('products.variant.name')}</th>
                                                                                <th>{t('products.variant.price')}</th>
                                                                                <th>{t('products.variant.stockQty')}</th>
                                                                                <th>{t('products.variant.sku')}</th>
                                                                                <th>{t('products.variant.productIdType')}</th>
                                                                                <th>{t('products.variant.productIdValue')}</th>
                                                                            </tr>
                                                                        </thead>
                                                                        <tbody>
                                                                            {item.variants.filter(v => !v.deleted).map(variant => (
                                                                                <tr key={variant.id}>
                                                                                    <td>{getVariantName(variant)}</td>
                                                                                    <td>${variant.price.toFixed(2)}</td>
                                                                                    <td>{variant.stockQuantity}</td>
                                                                                    <td>{variant.sku || '-'}</td>
                                                                                    <td>{variant.productIdentifierType || '-'}</td>
                                                                                    <td>{variant.productIdentifierValue || '-'}</td>
                                                                                </tr>
                                                                            ))}
                                                                        </tbody>
                                                                    </table>
                                                                </div>
                                                            ) : (
                                                                <div className="products-no-variants">
                                                                    {t('products.list.noVariants')}
                                                                </div>
                                                            )}
                                                        </td>
                                                    </tr>
                                                )}
                                            </Fragment>
                                        ))}
                                    </tbody>
                                </table>
                            </div>

                            {/* Pagination */}
                            {totalPages > 1 && (
                                <div className="products-pagination">
                                    <button
                                        className="products-pagination-btn"
                                        onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
                                        disabled={currentPage === 1}
                                    >
                                        {t('pagination.previous')}
                                    </button>
                                    <span className="products-pagination-info">
                                        {t('pagination.page')} {currentPage} {t('pagination.of')} {totalPages}
                                    </span>
                                    <button
                                        className="products-pagination-btn"
                                        onClick={() => setCurrentPage(prev => Math.min(prev + 1, totalPages))}
                                        disabled={currentPage === totalPages}
                                    >
                                        {t('pagination.next')}
                                    </button>
                                </div>
                            )}
                        </>
                    )}
                </div>
            )}
        </div>
    );
}

export default ProductsSection;