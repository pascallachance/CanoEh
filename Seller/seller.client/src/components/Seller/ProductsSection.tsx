import { useState, useEffect, useMemo, useCallback, Fragment, useRef, useImperativeHandle, forwardRef } from 'react';
import './ProductsSection.css';
import { useLanguage } from '../../contexts/LanguageContext';
import { useNotifications } from '../../contexts/useNotifications';
import { ApiClient } from '../../utils/apiClient';
import { 
    formatAttributeName,
    formatVariantAttribute
} from '../../utils/bilingualArrayUtils';
import { formatDate, toUTCISOString } from '../../utils/dateUtils';
import type { AddProductStep1Data } from '../AddProductStep1';
import type { AddProductStep2Data } from '../AddProductStep2';
import type { ItemAttribute } from '../AddProductStep2';
import BilingualTagInput from '../BilingualTagInput';
import AddProductStep1 from '../AddProductStep1';
import AddProductStep2 from '../AddProductStep2';
import AddProductStep3 from '../AddProductStep3';


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
    viewMode?: 'list' | 'add' | 'edit';
    onViewModeChange?: (mode: 'list' | 'add' | 'edit') => void;
    onManageOffersStateChange?: (isLoading: boolean, hasItems: boolean) => void;
}

export interface ProductsSectionRef {
    openManageOffers: () => void;
    openAddProduct: () => void;
    openEditProduct: (itemId: string) => void;
    isLoadingItems: boolean;
    hasItems: boolean;
}

interface QuickProductAttribute {
    name_en: string;
    name_fr: string;
    values: BilingualValue[];
}

interface BilingualValue {
    en: string;
    fr: string;
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
    thumbnailFile?: File;
    imageFiles?: File[];
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
interface ApiItemVariantAttribute {
    id: string;
    attributeName_en: string;
    attributeName_fr?: string;
    attributes_en: string;
    attributes_fr?: string;
}

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
    itemVariantAttributes: ApiItemVariantAttribute[];
    itemVariantFeatures?: ApiItemVariantAttribute[]; // Same structure as attributes
    deleted: boolean;
    offer?: number;
    offerStart?: string;
    offerEnd?: string;
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

const ProductsSection = forwardRef<ProductsSectionRef, ProductsSectionProps>(
    ({ companies, viewMode = 'list', onViewModeChange, onManageOffersStateChange }, ref) => {
    const [categories, setCategories] = useState<Category[]>([]);
    const { language, t } = useLanguage();
    const { showError, showSuccess } = useNotifications();
    const showAddForm = viewMode === 'add';
    const showEditForm = viewMode === 'edit';
    const showListSection = viewMode === 'list';
    const [editingItemId, setEditingItemId] = useState<string | null>(null);

    // State for fetched seller items from API
    const [sellerItems, setSellerItems] = useState<ApiItem[]>([]);
    const [isLoadingItems, setIsLoadingItems] = useState(false);
    const [loadItemsError, setLoadItemsError] = useState<string>('');
    const [expandedItemId, setExpandedItemId] = useState<string | null>(null);
    const [currentPage, setCurrentPage] = useState(1);
    const ITEMS_PER_PAGE = 25;

    // Filter and Sort State
    const [filters, setFilters] = useState({
        itemName: '',
        categoryId: '',
        variantName: '',
        sku: '',
        productIdType: '',
        productIdValue: '',
        showDeleted: false
    });
    const [sortBy, setSortBy] = useState<'itemName' | 'itemCategory' | 'creationDate' | 'lastUpdated'>('itemName');
    const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

    const [newItem, setNewItem] = useState({
        name: '',
        name_fr: '',
        description: '',
        description_fr: '',
        categoryId: '',
        attributes: [] as QuickProductAttribute[]
    });
    const [newAttribute, setNewAttribute] = useState({ 
        name_en: '', 
        name_fr: '', 
        values: [] as BilingualValue[]
    });
    const [attributeError, setAttributeError] = useState('');
    
    const [variants, setVariants] = useState<ItemVariant[]>([]);
    const [isSaving, setIsSaving] = useState(false);
    
    // State for undelete confirmation modal
    const [showUndeleteModal, setShowUndeleteModal] = useState(false);
    const [itemToUndelete, setItemToUndelete] = useState<ApiItem | null>(null);
    
    // State for delete confirmation modal
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [itemToDelete, setItemToDelete] = useState<ApiItem | null>(null);
    
    // State for manage offers (inline mode)
    const [showManageOffers, setShowManageOffers] = useState(false);
    const [offerChanges, setOfferChanges] = useState<Map<string, { offer?: number; offerStart?: string; offerEnd?: string }>>(new Map());
    const [isSavingOffers, setIsSavingOffers] = useState(false);
    
    // State for inline add/edit product workflow
    const [inlineProductMode, setInlineProductMode] = useState<'none' | 'add' | 'edit'>('none');
    const [productWorkflowStep, setProductWorkflowStep] = useState<number>(1);
    const [productStep1Data, setProductStep1Data] = useState<AddProductStep1Data | null>(null);
    const [productStep2Data, setProductStep2Data] = useState<AddProductStep2Data | null>(null);
    const [editingItemIdInline, setEditingItemIdInline] = useState<string | null>(null);
    const [editProductExistingVariants, setEditProductExistingVariants] = useState<any[] | null>(null);
    
    // Refs for accessibility
    const modalRef = useRef<HTMLDivElement>(null);
    const deleteModalRef = useRef<HTMLDivElement>(null);
    const previousActiveElementForUndelete = useRef<HTMLElement | null>(null);
    const previousActiveElementForDelete = useRef<HTMLElement | null>(null);

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

    // Accessibility: Focus management for undelete modal
    useEffect(() => {
        if (showUndeleteModal) {
            // Store the currently focused element
            previousActiveElementForUndelete.current = document.activeElement as HTMLElement;
            
            // Focus the modal content
            const timer = setTimeout(() => {
                modalRef.current?.focus();
            }, 100);

            // Prevent body scroll when modal is open
            document.body.style.overflow = 'hidden';

            return () => {
                clearTimeout(timer);
                document.body.style.overflow = '';
            };
        } else if (previousActiveElementForUndelete.current) {
            // Return focus to the element that opened the modal
            previousActiveElementForUndelete.current.focus();
            previousActiveElementForUndelete.current = null;
        }
    }, [showUndeleteModal]);

    // Accessibility: Focus management for delete modal
    useEffect(() => {
        if (showDeleteModal) {
            // Store the currently focused element
            previousActiveElementForDelete.current = document.activeElement as HTMLElement;
            
            // Focus the modal content
            const timer = setTimeout(() => {
                deleteModalRef.current?.focus();
            }, 100);

            // Prevent body scroll when modal is open
            document.body.style.overflow = 'hidden';

            return () => {
                clearTimeout(timer);
                document.body.style.overflow = '';
            };
        } else if (previousActiveElementForDelete.current) {
            // Return focus to the element that opened the modal
            previousActiveElementForDelete.current.focus();
            previousActiveElementForDelete.current = null;
        }
    }, [showDeleteModal]);

    // Handle escape key for undelete modal
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && showUndeleteModal) {
                setShowUndeleteModal(false);
                setItemToUndelete(null);
            }
        };

        if (showUndeleteModal) {
            document.addEventListener('keydown', handleEscape);
            return () => document.removeEventListener('keydown', handleEscape);
        }
    }, [showUndeleteModal]);

    // Handle escape key for delete modal
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape' && showDeleteModal) {
                setShowDeleteModal(false);
                setItemToDelete(null);
            }
        };

        if (showDeleteModal) {
            document.addEventListener('keydown', handleEscape);
            return () => document.removeEventListener('keydown', handleEscape);
        }
    }, [showDeleteModal]);

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
            // Add includeDeleted query parameter based on the checkbox state
            const queryParams = filters.showDeleted ? '?includeDeleted=true' : '';
            const response = await ApiClient.get(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/GetSellerItems/${sellerId}${queryParams}`,
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
    }, [companies, t, filters.showDeleted]);

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

    // Reset to page 1 when filters or sort options change
    useEffect(() => {
        setCurrentPage(1);
    }, [filters, sortBy, sortDirection]);

    // Reset expanded state when navigating to a different page
    useEffect(() => {
        setExpandedItemId(null);
    }, [currentPage]);

    // Get category name by ID
    const getCategoryName = useCallback((categoryId: string): string => {
        const category = categories.find(c => c.id === categoryId);
        if (!category) return t('common.unknown');
        return language === 'fr' ? category.name_fr : category.name_en;
    }, [categories, language, t]);

    // Filter and sort items
    const filteredAndSortedItems = useMemo(() => {
        // First, filter the items
        const filtered = sellerItems.filter(item => {
            // Note: Deleted items are now filtered at the API level based on the includeDeleted parameter
            // No need to filter by deleted status here since the backend handles it

            // Filter by item name
            if (filters.itemName) {
                const itemName = (language === 'fr' ? item.name_fr : item.name_en).toLowerCase();
                if (!itemName.includes(filters.itemName.toLowerCase())) {
                    return false;
                }
            }

            // Filter by category
            if (filters.categoryId && item.categoryID !== filters.categoryId) {
                return false;
            }

            // Filter by variant name, SKU, product ID type, or product ID value
            if (filters.variantName || filters.sku || filters.productIdType || filters.productIdValue) {
                const hasMatchingVariant = item.variants.some(variant => {
                    if (!filters.showDeleted && variant.deleted) return false;

                    // Check variant name
                    if (filters.variantName) {
                        const variantName = (language === 'fr' ? variant.itemVariantName_fr : variant.itemVariantName_en) || '';
                        if (!variantName.toLowerCase().includes(filters.variantName.toLowerCase())) {
                            return false;
                        }
                    }

                    // Check SKU
                    if (filters.sku && !variant.sku.toLowerCase().includes(filters.sku.toLowerCase())) {
                        return false;
                    }

                    // Check product ID type
                    if (filters.productIdType && variant.productIdentifierType !== filters.productIdType) {
                        return false;
                    }

                    // Check product ID value
                    if (filters.productIdValue) {
                        const idValue = variant.productIdentifierValue || '';
                        if (!idValue.toLowerCase().includes(filters.productIdValue.toLowerCase())) {
                            return false;
                        }
                    }

                    return true;
                });

                if (!hasMatchingVariant) {
                    return false;
                }
            }

            return true;
        });

        // Then, sort the filtered items
        const sorted = [...filtered].sort((a, b) => {
            let compareResult = 0;

            switch (sortBy) {
                case 'itemName': {
                    const nameA = language === 'fr' ? a.name_fr : a.name_en;
                    const nameB = language === 'fr' ? b.name_fr : b.name_en;
                    compareResult = nameA.localeCompare(nameB, language === 'fr' ? 'fr' : 'en', { sensitivity: 'base' });
                    break;
                }
                case 'itemCategory': {
                    const categoryA = getCategoryName(a.categoryID);
                    const categoryB = getCategoryName(b.categoryID);
                    compareResult = categoryA.localeCompare(categoryB, language === 'fr' ? 'fr' : 'en', { sensitivity: 'base' });
                    break;
                }
                case 'creationDate': {
                    compareResult = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
                    break;
                }
                case 'lastUpdated': {
                    const dateA = a.updatedAt ? new Date(a.updatedAt).getTime() : new Date(a.createdAt).getTime();
                    const dateB = b.updatedAt ? new Date(b.updatedAt).getTime() : new Date(b.createdAt).getTime();
                    compareResult = dateA - dateB;
                    break;
                }
            }

            return sortDirection === 'asc' ? compareResult : -compareResult;
        });

        return sorted;
    }, [sellerItems, language, filters, sortBy, sortDirection, getCategoryName]);

    // Pagination calculations
    const totalPages = Math.ceil(filteredAndSortedItems.length / ITEMS_PER_PAGE);
    const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
    const endIndex = startIndex + ITEMS_PER_PAGE;
    const paginatedItems = filteredAndSortedItems.slice(startIndex, endIndex);

    // Toggle expanded row
    const toggleExpandedRow = (itemId: string) => {
        setExpandedItemId(prev => prev === itemId ? null : itemId);
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
    // Handle sort column click
    const handleSortClick = (column: 'itemName' | 'itemCategory' | 'creationDate' | 'lastUpdated', direction: 'asc' | 'desc') => {
        setSortBy(column);
        setSortDirection(direction);
    };

    // Render sortable header with arrow buttons
    const renderSortableHeader = (column: 'itemName' | 'itemCategory' | 'creationDate' | 'lastUpdated', labelKey: string) => {
        return (
            <th>
                <div className="products-header-cell">
                    <span>{t(labelKey)}</span>
                    <div className="products-sort-arrows">
                        <button
                            type="button"
                            className={`products-sort-arrow ${sortBy === column && sortDirection === 'desc' ? 'active' : ''}`}
                            onClick={() => handleSortClick(column, 'desc')}
                            title={t('products.sort.descending')}
                            aria-label={`${t(`products.sort.${column}`)} ${t('products.sort.descending')}`}
                        >
                            ▲
                        </button>
                        <button
                            type="button"
                            className={`products-sort-arrow ${sortBy === column && sortDirection === 'asc' ? 'active' : ''}`}
                            onClick={() => handleSortClick(column, 'asc')}
                            title={t('products.sort.ascending')}
                            aria-label={`${t(`products.sort.${column}`)} ${t('products.sort.ascending')}`}
                        >
                            ▼
                        </button>
                    </div>
                </div>
            </th>
        );
    };

    // Handle editing an item
    const handleEditItem = (item: ApiItem) => {
        // Use inline edit mode instead of navigating to separate route
        handleOpenEditProduct(item.id);
    };

    // Handle deleting an item
    const handleDeleteItem = (item: ApiItem) => {
        // Validate item ID
        if (!item.id || typeof item.id !== 'string') {
            showError(t('products.invalidItemId'));
            return;
        }

        // Check if item is already deleted (defense in depth)
        if (item.deleted) {
            showError(t('products.alreadyDeleted'));
            return;
        }
        
        // Show confirmation modal
        setItemToDelete(item);
        setShowDeleteModal(true);
    };

    const confirmDeleteItem = async () => {
        if (!itemToDelete) return;

        try {
            // Encode the ID to ensure URL safety (though GUID should be safe)
            const encodedId = encodeURIComponent(itemToDelete.id);
            const response = await ApiClient.delete(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/DeleteItem/${encodedId}`
            );

            if (response.ok) {
                showSuccess(t('products.deleteSuccess'));
                // Refresh the seller items list
                await fetchSellerItems();
            } else {
                // Do not expose backend error details to the user
                showError(t('products.deleteError'));
            }
        } catch (error) {
            console.error('Error deleting item:', error);
            showError(t('products.deleteError'));
        } finally {
            // Close modal
            setShowDeleteModal(false);
            setItemToDelete(null);
        }
    };

    const cancelDeleteItem = () => {
        setShowDeleteModal(false);
        setItemToDelete(null);
    };

    // Handlers for inline add/edit product workflow
    const handleOpenAddProduct = useCallback(() => {
        setInlineProductMode('add');
        setProductWorkflowStep(1);
        setProductStep1Data(null);
        setProductStep2Data(null);
        setEditingItemIdInline(null);
        setEditProductExistingVariants(null);
    }, []);

    const handleOpenEditProduct = useCallback((itemId: string) => {
        // Find the item to edit
        const item = sellerItems.find(i => i.id === itemId);
        if (!item) {
            showError(t('products.itemNotFound'));
            return;
        }

        // Prepare step1 data
        const step1Data = {
            name: item.name_en,
            name_fr: item.name_fr,
            description: item.description_en || '',
            description_fr: item.description_fr || ''
        };

        // Extract variant attributes and features (same logic as handleEditItem)
        const attributeOrderMap = new Map<string, number>();
        const attributesMap = new Map<string, {
            name_en: string;
            name_fr: string;
            values: Array<{ en: string; fr: string }>;
        }>();

        const activeVariants = item.variants.filter(v => !v.deleted);

        activeVariants.forEach((variant, variantIndex) => {
            variant.itemVariantAttributes.forEach((attr, attrIndex) => {
                const key = attr.attributeName_en;
                
                if (variantIndex === 0 && !attributeOrderMap.has(key)) {
                    attributeOrderMap.set(key, attrIndex);
                }
                
                if (!attributesMap.has(key)) {
                    attributesMap.set(key, {
                        name_en: attr.attributeName_en,
                        name_fr: attr.attributeName_fr || '',
                        values: []
                    });
                }
                
                const attrData = attributesMap.get(key)!;
                const valuePair = {
                    en: attr.attributes_en,
                    fr: attr.attributes_fr || ''
                };
                
                const alreadyExists = attrData.values.some(
                    v => v.en === valuePair.en && v.fr === valuePair.fr
                );
                if (!alreadyExists) {
                    attrData.values.push(valuePair);
                }
            });
        });

        const variantAttributes: ItemAttribute[] = Array.from(attributesMap.entries())
            .sort(([keyA], [keyB]) => {
                const orderA = attributeOrderMap.get(keyA) ?? 999;
                const orderB = attributeOrderMap.get(keyB) ?? 999;
                return orderA - orderB;
            })
            .map(([, attr]) => ({
                name_en: attr.name_en,
                name_fr: attr.name_fr,
                values: attr.values
            }));

        const featuresMap = new Map<string, {
            name_en: string;
            name_fr: string;
            values: Array<{ en: string; fr: string }>;
        }>();
        const featureOrderMap = new Map<string, number>();

        activeVariants.forEach((variant, variantIndex) => {
            (variant.itemVariantFeatures || []).forEach((feature, featureIndex) => {
                const key = feature.attributeName_en;
                
                if (variantIndex === 0 && !featureOrderMap.has(key)) {
                    featureOrderMap.set(key, featureIndex);
                }
                
                if (!featuresMap.has(key)) {
                    featuresMap.set(key, {
                        name_en: feature.attributeName_en,
                        name_fr: feature.attributeName_fr || '',
                        values: []
                    });
                }
            });
        });

        const variantFeatures: ItemAttribute[] = Array.from(featuresMap.entries())
            .sort(([keyA], [keyB]) => {
                const orderA = featureOrderMap.get(keyA) ?? 999;
                const orderB = featureOrderMap.get(keyB) ?? 999;
                return orderA - orderB;
            })
            .map(([, feature]) => ({
                name_en: feature.name_en,
                name_fr: feature.name_fr,
                values: []
            }));

        const step2Data = {
            categoryId: item.categoryID,
            variantAttributes,
            variantFeatures
        };

        const existingVariants = activeVariants.map(variant => ({
            id: variant.id,
            sku: variant.sku,
            price: variant.price,
            stockQuantity: variant.stockQuantity,
            productIdentifierType: variant.productIdentifierType,
            productIdentifierValue: variant.productIdentifierValue,
            thumbnailUrl: variant.thumbnailUrl,
            imageUrls: variant.imageUrls,
            itemVariantAttributes: variant.itemVariantAttributes,
            itemVariantFeatures: variant.itemVariantFeatures || []
        }));

        setInlineProductMode('edit');
        setProductWorkflowStep(1);
        setProductStep1Data(step1Data);
        setProductStep2Data(step2Data);
        setEditingItemIdInline(itemId);
        setEditProductExistingVariants(existingVariants);
    }, [sellerItems, showError, t]);

    const handleProductStep1Next = (data: AddProductStep1Data) => {
        setProductStep1Data(data);
        setProductWorkflowStep(2);
    };

    const handleProductStep1Cancel = () => {
        setInlineProductMode('none');
        setProductWorkflowStep(1);
        setProductStep1Data(null);
        setProductStep2Data(null);
        setEditingItemIdInline(null);
        setEditProductExistingVariants(null);
    };

    const handleProductStep2Next = (data: AddProductStep2Data) => {
        setProductStep2Data(data);
        setProductWorkflowStep(3);
    };

    const handleProductStep2Back = () => {
        setProductWorkflowStep(1);
    };

    const handleProductStep3Back = () => {
        setProductWorkflowStep(2);
    };

    const handleProductSubmit = async () => {
        // Product was successfully saved, refresh and return to list
        await fetchSellerItems();
        setInlineProductMode('none');
        setProductWorkflowStep(1);
        setProductStep1Data(null);
        setProductStep2Data(null);
        setEditingItemIdInline(null);
        setEditProductExistingVariants(null);
    };

    const handleProductStepNavigate = (step: number) => {
        // Allow navigation between steps only in edit mode and only to completed steps
        if (inlineProductMode !== 'edit') {
            return;
        }

        const completedSteps = getCompletedSteps();

        if (step >= 1 && step <= 3 && completedSteps.includes(step)) {
            setProductWorkflowStep(step);
        }
    };

    const getCompletedSteps = (): number[] => {
        const completed: number[] = [];
        if (productStep1Data) completed.push(1);
        if (productStep2Data) completed.push(2);
        if (productStep1Data && productStep2Data) {
            completed.push(3);
        }
        return completed;
    };

    // Handle opening manage offers inline view
    const handleOpenManageOffers = useCallback(() => {
        void (async () => {
            try {
                // Refresh data to ensure we show the latest offers
                await fetchSellerItems();
                setShowManageOffers(true);
                setOfferChanges(new Map());
            } catch (error) {
                console.error('Error fetching seller items for manage offers view:', error);
                // Provide user feedback and do not open on failure
                showError(t('products.list.error'));
            }
        })();
    }, [fetchSellerItems, showError, t]);
    
    // Expose methods and state to parent component
    useImperativeHandle(ref, () => ({
        openManageOffers: handleOpenManageOffers,
        openAddProduct: handleOpenAddProduct,
        openEditProduct: handleOpenEditProduct,
        isLoadingItems,
        hasItems: sellerItems.length > 0
    }), [isLoadingItems, sellerItems.length, handleOpenManageOffers, handleOpenAddProduct, handleOpenEditProduct]);

    // Notify parent of state changes for managing button disabled state
    useEffect(() => {
        if (onManageOffersStateChange) {
            onManageOffersStateChange(isLoadingItems, sellerItems.length > 0);
        }
    }, [isLoadingItems, sellerItems.length, onManageOffersStateChange]);

    // Handle closing manage offers
    const handleCloseManageOffers = () => {
        setShowManageOffers(false);
        setOfferChanges(new Map());
    };

    // Handle offer field change for a variant
    const handleOfferChange = (variantId: string, field: 'offer' | 'offerStart' | 'offerEnd', value: string) => {
        setOfferChanges(prev => {
            const newChanges = new Map(prev);
            const current = newChanges.get(variantId) || {};
            
            if (field === 'offer') {
                const numValue = value === '' ? undefined : parseFloat(value);
                if (numValue !== undefined && (isNaN(numValue) || numValue < 0 || numValue > 100)) {
                    showError(t('products.offer.invalidRange'));
                    return prev;
                }
                newChanges.set(variantId, { ...current, offer: numValue });
            } else if (field === 'offerStart' || field === 'offerEnd') {
                newChanges.set(variantId, { ...current, [field]: value || undefined });
            }
            
            return newChanges;
        });
    };

    // Get current offer value for a variant (from changes or original)
    const getCurrentOffer = (variant: ApiItemVariant, field: 'offer' | 'offerStart' | 'offerEnd') => {
        const changes = offerChanges.get(variant.id);
        if (changes && changes[field] !== undefined) {
            return changes[field];
        }
        
        if (field === 'offer') {
            return variant.offer !== undefined && variant.offer !== null ? variant.offer : '';
        } else if (field === 'offerStart') {
            return variant.offerStart ? variant.offerStart.split('T')[0] : '';
        } else if (field === 'offerEnd') {
            return variant.offerEnd ? variant.offerEnd.split('T')[0] : '';
        }
        return '';
    };

    // Helper function to convert date string to ISO format with validation
    // Uses the centralized UTC conversion utility
    const toISODateOrUndefined = (dateString?: string): string | undefined => {
        return toUTCISOString(dateString);
    };

    // Handle saving all offer changes
    const handleSaveOffers = async () => {
        if (offerChanges.size === 0) {
            showError(t('products.offers.noChanges'));
            return;
        }

        setIsSavingOffers(true);

        try {
            // Prepare batch request with all offer updates
            const offerUpdates = Array.from(offerChanges.entries()).map(([variantId, changes]) => ({
                variantId,
                offer: changes.offer,
                offerStart: toISODateOrUndefined(changes.offerStart),
                offerEnd: toISODateOrUndefined(changes.offerEnd)
            }));

            const batchRequest = {
                offerUpdates
            };

            const response = await ApiClient.put(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/BatchUpdateItemVariantOffers`,
                batchRequest
            );

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Failed to update offers: ${errorText}`);
            }
            
            showSuccess(t('products.offers.saveSuccess'));
            setShowManageOffers(false);
            setOfferChanges(new Map());
            await fetchSellerItems();
        } catch (error) {
            console.error('Error saving offers:', error);
            showError(t('products.offers.saveError'));
        } finally {
            setIsSavingOffers(false);
        }
    };

    // Handle clearing offer for a variant
    const handleClearOffer = (variantId: string) => {
        setOfferChanges(prev => {
            const newChanges = new Map(prev);
            newChanges.set(variantId, { offer: undefined, offerStart: undefined, offerEnd: undefined });
            return newChanges;
        });
    };

    // Handle undeleting an item
    const handleUndeleteItem = (item: ApiItem) => {
        // Validate item ID
        if (!item.id || typeof item.id !== 'string') {
            showError(t('products.invalidItemId'));
            return;
        }

        // Check if item is not deleted
        if (!item.deleted) {
            showError(t('products.itemNotDeleted'));
            return;
        }

        // Show confirmation modal
        setItemToUndelete(item);
        setShowUndeleteModal(true);
    };

    const confirmUndeleteItem = async () => {
        if (!itemToUndelete) return;

        try {
            // Encode the ID to ensure URL safety (though GUID should be safe)
            const encodedId = encodeURIComponent(itemToUndelete.id);
            const response = await ApiClient.put(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UnDeleteItem/${encodedId}`,
                {}
            );

            if (response.ok) {
                showSuccess(t('products.undeleteSuccess'));
                // Refresh the seller items list
                await fetchSellerItems();
            } else {
                // Do not expose backend error details to the user
                showError(t('products.undeleteError'));
            }
        } catch (error) {
            console.error('Error undeleting item:', error);
            showError(t('products.undeleteError'));
        } finally {
            // Close modal
            setShowUndeleteModal(false);
            setItemToUndelete(null);
        }
    };

    const cancelUndeleteItem = () => {
        setShowUndeleteModal(false);
        setItemToUndelete(null);
    };

    // Focus trapping within undelete modal
    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (!showUndeleteModal || !modalRef.current) return;

        if (event.key === 'Tab') {
            const focusableElements = modalRef.current.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            );
            const firstElement = focusableElements[0] as HTMLElement;
            const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

            if (event.shiftKey) {
                // Shift + Tab
                if (document.activeElement === firstElement) {
                    event.preventDefault();
                    lastElement?.focus();
                }
            } else {
                // Tab
                if (document.activeElement === lastElement) {
                    event.preventDefault();
                    firstElement?.focus();
                }
            }
        }
    };

    // Focus trapping within delete modal
    const handleDeleteKeyDown = (event: React.KeyboardEvent) => {
        if (!showDeleteModal || !deleteModalRef.current) return;

        if (event.key === 'Tab') {
            const focusableElements = deleteModalRef.current.querySelectorAll(
                'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
            );
            const firstElement = focusableElements[0] as HTMLElement;
            const lastElement = focusableElements[focusableElements.length - 1] as HTMLElement;

            if (event.shiftKey) {
                // Shift + Tab
                if (document.activeElement === firstElement) {
                    event.preventDefault();
                    lastElement?.focus();
                }
            } else {
                // Tab
                if (document.activeElement === lastElement) {
                    event.preventDefault();
                    firstElement?.focus();
                }
            }
        }
    };

    const addAttribute = () => {
        // Clear any previous error
        setAttributeError('');
        
        if (!newAttribute.name_en || !newAttribute.name_fr) {
            setAttributeError(t('error.bilingualNamesMissing'));
            return;
        }

        // Clear any previous error
        setAttributeError('');

        if (!newAttribute.name_en || !newAttribute.name_fr) {
            setAttributeError('Attribute names in both languages are required');
            return;
        }

        if (newAttribute.values.length === 0) {
            setAttributeError('At least one value pair is required');
            return;
        }

        // Validate all values have both en and fr
        const hasIncompleteValues = newAttribute.values.some(v => !v.en || !v.fr);
        if (hasIncompleteValues) {
            setAttributeError('All value pairs must have both English and French values');
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
                values: newAttribute.values
            }]
        }));
        setNewAttribute({ 
            name_en: '', 
            name_fr: '', 
            values: []
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
                imageUrls: [],
                thumbnailFile: undefined,
                imageFiles: []
            }];
        }

        const combinations: { en: Record<string, string>, fr: Record<string, string> }[] = [];
        
        const generateCombinations = (attrIndex: number, currentEn: Record<string, string>, currentFr: Record<string, string>) => {
            if (attrIndex >= newItem.attributes.length) {
                combinations.push({ en: { ...currentEn }, fr: { ...currentFr } });
                return;
            }
            
            const attribute = newItem.attributes[attrIndex];
            
            // Skip if no values
            if (!attribute.values || attribute.values.length === 0) {
                console.warn(`Attribute "${attribute.name_en}" has no values, skipping variant generation`);
                return;
            }
            
            // Use paired values - this automatically maintains synchronization
            for (const value of attribute.values) {
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
            const urls: string[] = [];
            try {
                fileArray.forEach(file => {
                    urls.push(URL.createObjectURL(file));
                });
                setVariants(prev => prev.map(v => 
                    v.id === variantId ? { ...v, imageUrls: urls, imageFiles: fileArray } : v
                ));
            } catch (error) {
                // If there's an error creating object URLs, clean up any that were created
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
            const isEditMode = editingItemId !== null;
            
            // For edit mode, we'll need to call an update endpoint (if available)
            // For now, we'll only support create mode
            if (isEditMode) {
                showError('Edit functionality is not yet fully implemented. Please contact support.');
                setIsSaving(false);
                return;
            }

            // Transform frontend data to match backend CreateItemRequest format
            // Images will be uploaded after item creation
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
                    ImageUrls: null, // Will be set after uploading images
                    ThumbnailUrl: null, // Will be set after uploading thumbnail
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
                }))
            };

            // Call the API to create the item
            const response = await ApiClient.post(
                `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/CreateItem`,
                createItemRequest
            );

            if (response.ok) {
                const result = await response.json();
                const createdItem = result.value;

                // Upload images for variants that have files
                // Match variants by SKU to avoid issues if backend reorders them
                if (createdItem && createdItem.variants) {
                    for (const variant of variants) {
                        // Find the corresponding created variant by SKU
                        const createdVariant = createdItem.variants.find((v: any) => v.sku === variant.sku);

                        if (!createdVariant || !createdVariant.id) {
                            console.warn(`Created variant with SKU "${variant.sku}" not found, skipping image upload`);
                            continue;
                        }

                        // Upload thumbnail if present
                        if (variant.thumbnailFile) {
                            try {
                                const formData = new FormData();
                                formData.append('file', variant.thumbnailFile);

                                const uploadResponse = await fetch(
                                    `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UploadImage?variantId=${createdVariant.id}&imageType=thumbnail`,
                                    {
                                        method: 'POST',
                                        credentials: 'include',
                                        body: formData,
                                    }
                                );

                                if (!uploadResponse.ok) {
                                    const errorText = await uploadResponse.text();
                                    console.error(`Failed to upload thumbnail for variant ${createdVariant.id}: ${uploadResponse.status} ${uploadResponse.statusText}`, errorText);
                                }
                            } catch (error) {
                                console.error(`Error uploading thumbnail for variant ${createdVariant.id}:`, error);
                            }
                        }

                        // Upload product images if present
                        if (variant.imageFiles && variant.imageFiles.length > 0) {
                            for (let imageIndex = 0; imageIndex < variant.imageFiles.length; imageIndex++) {
                                try {
                                    const formData = new FormData();
                                    formData.append('file', variant.imageFiles[imageIndex]);

                                    const uploadResponse = await fetch(
                                        `${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Item/UploadImage?variantId=${createdVariant.id}&imageType=image&imageNumber=${imageIndex + 1}`,
                                        {
                                            method: 'POST',
                                            credentials: 'include',
                                            body: formData,
                                        }
                                    );

                                    if (!uploadResponse.ok) {
                                        const errorText = await uploadResponse.text();
                                        console.error(`Failed to upload image ${imageIndex + 1} for variant ${createdVariant.id}: ${uploadResponse.status} ${uploadResponse.statusText}`, errorText);
                                    }
                                } catch (error) {
                                    console.error(`Error uploading image ${imageIndex + 1} for variant ${createdVariant.id}:`, error);
                                }
                            }
                        }
                    }
                }

                // Refresh the seller items list from API
                await fetchSellerItems();
                
                // Reset form
                setNewItem({ 
                    name: '', 
                    name_fr: '', 
                    description: '', 
                    description_fr: '', 
                    categoryId: '', 
                    attributes: []
                });
                setVariants([]);
                setEditingItemId(null);
                
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
            console.error('Error saving item:', error);
            showError('An unexpected error occurred while saving the item.');
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="section-container">
            {/* Show inline add/edit product workflow */}
            {inlineProductMode !== 'none' && productWorkflowStep === 1 && (
                <AddProductStep1
                    onNext={handleProductStep1Next}
                    onCancel={handleProductStep1Cancel}
                    initialData={productStep1Data || undefined}
                    editMode={inlineProductMode === 'edit'}
                    onStepNavigate={handleProductStepNavigate}
                    completedSteps={getCompletedSteps()}
                />
            )}
            {inlineProductMode !== 'none' && productWorkflowStep === 2 && productStep1Data && (
                <AddProductStep2
                    onNext={handleProductStep2Next}
                    onBack={handleProductStep2Back}
                    onCancel={handleProductStep1Cancel}
                    step1Data={productStep1Data}
                    initialData={productStep2Data || undefined}
                    editMode={inlineProductMode === 'edit'}
                    onStepNavigate={handleProductStepNavigate}
                    completedSteps={getCompletedSteps()}
                />
            )}
            {inlineProductMode !== 'none' && productWorkflowStep === 3 && productStep1Data && productStep2Data && (
                <AddProductStep3
                    onSubmit={handleProductSubmit}
                    onBack={handleProductStep3Back}
                    onCancel={handleProductStep1Cancel}
                    step1Data={productStep1Data}
                    step2Data={productStep2Data}
                    companies={companies}
                    editMode={inlineProductMode === 'edit'}
                    itemId={editingItemIdInline || undefined}
                    existingVariants={editProductExistingVariants || undefined}
                    onStepNavigate={handleProductStepNavigate}
                    completedSteps={getCompletedSteps()}
                />
            )}

            {/* Show product list when not in add/edit mode */}
            {inlineProductMode === 'none' && (showAddForm || showEditForm) && (
                <div className="products-add-form">
                    <h3>{showEditForm ? t('products.editProduct') : t('products.addProduct')}</h3>
                    
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
                                <BilingualTagInput
                                    values={newAttribute.values}
                                    onValuesChange={(values) => setNewAttribute(prev => ({ ...prev, values }))}
                                    placeholderEn={t('placeholder.attributeValue')}
                                    placeholderFr={t('placeholder.attributeValueFrVariant')}
                                    labelEn={t('products.attributeValues')}
                                    labelFr={t('products.attributeValuesFr')}
                                    id="quick_add_attribute_values"
                                />
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
                                {newItem.attributes.map((attr, index) => (
                                    <div key={index} className="products-attribute-item">
                                        <span>
                                            <div><strong>EN:</strong> {attr.name_en}: {attr.values?.map(v => v?.en).filter(Boolean).join(', ')}</div>
                                            <div><strong>FR:</strong> {attr.name_fr}: {attr.values?.map(v => v?.fr).filter(Boolean).join(', ')}</div>
                                        </span>
                                        <button
                                            onClick={() => removeAttribute(index)}
                                            className="products-remove-attribute-button"
                                        >
                                            {t('products.deleteItem')}
                                        </button>
                                    </div>
                                ))}
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
                            {isSaving 
                                ? (showEditForm ? t('products.updating') : t('products.saving'))
                                : (showEditForm ? t('products.updateItem') : t('products.addItem'))}
                        </button>
                        <button
                            onClick={() => {
                                setEditingItemId(null);
                                setNewItem({ 
                                    name: '', 
                                    name_fr: '', 
                                    description: '', 
                                    description_fr: '', 
                                    categoryId: '', 
                                    attributes: []
                                });
                                setVariants([]);
                                if (onViewModeChange) {
                                    onViewModeChange('list');
                                }
                            }}
                            className="products-action-button products-action-button--cancel"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {/* Show inline manage offers view */}
            {inlineProductMode === 'none' && showManageOffers && (
                <div className="products-manage-offers-section">
                    <h3>{t('products.manageOffers')}</h3>
                    
                    <div className="products-offers-container">
                        {isLoadingItems ? (
                            <p>{t('products.list.loading')}</p>
                        ) : sellerItems.length === 0 ? (
                            <p>{t('products.list.noItems')}</p>
                        ) : (
                            <div className="products-offers-table-wrapper">
                                <table className="products-offers-table">
                                    <thead>
                                        <tr>
                                            <th>{t('products.itemName')}</th>
                                            <th>{t('products.variant.name')}</th>
                                            <th>{t('products.offers.offer')} (%)</th>
                                            <th>{t('products.offers.offerStart')}</th>
                                            <th>{t('products.offers.offerEnd')}</th>
                                            <th>{t('products.actions')}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {sellerItems.filter(item => !item.deleted).map(item => {
                                            const activeVariants = item.variants.filter(v => !v.deleted);
                                            return activeVariants.map((variant, index) => (
                                                <tr key={variant.id}>
                                                    {index === 0 && (
                                                        <td rowSpan={activeVariants.length}>
                                                            {getItemName(item)}
                                                        </td>
                                                    )}
                                                    <td>{getVariantName(variant)}</td>
                                                    <td>
                                                        <input
                                                            type="number"
                                                            min="0"
                                                            max="100"
                                                            step="0.01"
                                                            value={getCurrentOffer(variant, 'offer')}
                                                            onChange={(e) => handleOfferChange(variant.id, 'offer', e.target.value)}
                                                            className="products-offer-input"
                                                            placeholder="0-100"
                                                        />
                                                    </td>
                                                    <td>
                                                        <input
                                                            type="date"
                                                            value={getCurrentOffer(variant, 'offerStart')}
                                                            onChange={(e) => handleOfferChange(variant.id, 'offerStart', e.target.value)}
                                                            className="products-offer-input"
                                                        />
                                                    </td>
                                                    <td>
                                                        <input
                                                            type="date"
                                                            value={getCurrentOffer(variant, 'offerEnd')}
                                                            onChange={(e) => handleOfferChange(variant.id, 'offerEnd', e.target.value)}
                                                            className="products-offer-input"
                                                        />
                                                    </td>
                                                    <td>
                                                        <button
                                                            onClick={() => handleClearOffer(variant.id)}
                                                            className="products-clear-offer-button"
                                                            title={t('products.offers.clearOffer')}
                                                        >
                                                            {t('products.offers.clear')}
                                                        </button>
                                                    </td>
                                                </tr>
                                            ));
                                        })}
                                    </tbody>
                                </table>
                            </div>
                        )}
                    </div>

                    <div className="products-form-actions">
                        <button
                            className="products-action-button products-action-button--cancel"
                            onClick={handleCloseManageOffers}
                            disabled={isSavingOffers}
                        >
                            {t('common.cancel')}
                        </button>
                        <button
                            className="products-action-button products-action-button--save"
                            onClick={handleSaveOffers}
                            disabled={isSavingOffers || offerChanges.size === 0}
                        >
                            {isSavingOffers ? t('products.saving') : t('products.offers.save')}
                        </button>
                    </div>
                </div>
            )}

            {inlineProductMode === 'none' && !showManageOffers && showListSection && (
                <div className="products-list-section">
                    {/* Filter and Sort Section */}
                    <div className="products-filter-section">
                        <h4>{t('products.filter.title')}</h4>
                        
                        <div className="products-filter-grid">
                            {/* Item Name Search */}
                            <div className="products-filter-field">
                                <label className="products-filter-label" htmlFor="filter-item-name">
                                    {t('products.filter.itemName')}
                                </label>
                                <div className="products-filter-input-wrapper">
                                    <input
                                        id="filter-item-name"
                                        type="text"
                                        value={filters.itemName}
                                        onChange={(e) => setFilters(prev => ({ ...prev, itemName: e.target.value }))}
                                        className="products-filter-input"
                                        placeholder={t('products.filter.itemNamePlaceholder')}
                                    />
                                    {filters.itemName && (
                                        <button
                                            type="button"
                                            className="products-filter-clear-button"
                                            onClick={() => setFilters(prev => ({ ...prev, itemName: '' }))}
                                            aria-label={t('products.filter.clearItemName')}
                                            title={t('products.filter.clear')}
                                        >
                                            ×
                                        </button>
                                    )}
                                </div>
                            </div>

                            {/* Category Dropdown */}
                            <div className="products-filter-field">
                                <label className="products-filter-label" htmlFor="filter-category">
                                    {t('products.filter.category')}
                                </label>
                                <select
                                    id="filter-category"
                                    value={filters.categoryId}
                                    onChange={(e) => setFilters(prev => ({ ...prev, categoryId: e.target.value }))}
                                    className="products-filter-input"
                                >
                                    <option value="">{t('products.filter.allCategories')}</option>
                                    {categories.map(category => (
                                        <option key={category.id} value={category.id}>
                                            {language === 'fr' ? category.name_fr : category.name_en}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            {/* Variant Name Search */}
                            <div className="products-filter-field">
                                <label className="products-filter-label" htmlFor="filter-variant-name">
                                    {t('products.filter.variantName')}
                                </label>
                                <div className="products-filter-input-wrapper">
                                    <input
                                        id="filter-variant-name"
                                        type="text"
                                        value={filters.variantName}
                                        onChange={(e) => setFilters(prev => ({ ...prev, variantName: e.target.value }))}
                                        className="products-filter-input"
                                        placeholder={t('products.filter.variantNamePlaceholder')}
                                    />
                                    {filters.variantName && (
                                        <button
                                            type="button"
                                            className="products-filter-clear-button"
                                            onClick={() => setFilters(prev => ({ ...prev, variantName: '' }))}
                                            aria-label={t('products.filter.clearVariantName')}
                                            title={t('products.filter.clear')}
                                        >
                                            ×
                                        </button>
                                    )}
                                </div>
                            </div>

                            {/* SKU Search */}
                            <div className="products-filter-field">
                                <label className="products-filter-label" htmlFor="filter-sku">
                                    {t('products.filter.sku')}
                                </label>
                                <div className="products-filter-input-wrapper">
                                    <input
                                        id="filter-sku"
                                        type="text"
                                        value={filters.sku}
                                        onChange={(e) => setFilters(prev => ({ ...prev, sku: e.target.value }))}
                                        className="products-filter-input"
                                        placeholder={t('products.filter.skuPlaceholder')}
                                    />
                                    {filters.sku && (
                                        <button
                                            type="button"
                                            className="products-filter-clear-button"
                                            onClick={() => setFilters(prev => ({ ...prev, sku: '' }))}
                                            aria-label={t('products.filter.clearSku')}
                                            title={t('products.filter.clear')}
                                        >
                                            ×
                                        </button>
                                    )}
                                </div>
                            </div>

                            {/* Product ID Type Dropdown */}
                            <div className="products-filter-field">
                                <label className="products-filter-label" htmlFor="filter-product-id-type">
                                    {t('products.filter.productIdType')}
                                </label>
                                <select
                                    id="filter-product-id-type"
                                    value={filters.productIdType}
                                    onChange={(e) => setFilters(prev => ({ ...prev, productIdType: e.target.value }))}
                                    className="products-filter-input"
                                >
                                    <option value="">{t('products.filter.allIdTypes')}</option>
                                    <option value="UPC">UPC</option>
                                    <option value="EAN">EAN</option>
                                    <option value="GTIN">GTIN</option>
                                    <option value="ISBN">ISBN</option>
                                    <option value="ASIN">ASIN</option>
                                    <option value="SKU">SKU</option>
                                    <option value="MPN">MPN</option>
                                </select>
                            </div>

                            {/* Product ID Value Search */}
                            <div className="products-filter-field">
                                <label className="products-filter-label" htmlFor="filter-product-id-value">
                                    {t('products.filter.productIdValue')}
                                </label>
                                <div className="products-filter-input-wrapper">
                                    <input
                                        id="filter-product-id-value"
                                        type="text"
                                        value={filters.productIdValue}
                                        onChange={(e) => setFilters(prev => ({ ...prev, productIdValue: e.target.value }))}
                                        className="products-filter-input"
                                        placeholder={t('products.filter.productIdValuePlaceholder')}
                                    />
                                    {filters.productIdValue && (
                                        <button
                                            type="button"
                                            className="products-filter-clear-button"
                                            onClick={() => setFilters(prev => ({ ...prev, productIdValue: '' }))}
                                            aria-label={t('products.filter.clearProductIdValue')}
                                            title={t('products.filter.clear')}
                                        >
                                            ×
                                        </button>
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Show Deleted Checkbox */}
                        <div className="products-filter-checkbox">
                            <label className="products-filter-checkbox-label" htmlFor="filter-show-deleted">
                                <input
                                    id="filter-show-deleted"
                                    type="checkbox"
                                    checked={filters.showDeleted}
                                    onChange={(e) => setFilters(prev => ({ ...prev, showDeleted: e.target.checked }))}
                                    className="products-filter-checkbox-input"
                                />
                                <span>{t('products.filter.showDeleted')}</span>
                            </label>
                        </div>

                        {/* Clear Filters Button and Current Items Count */}
                        <div className="products-filter-actions">
                            <span className="products-current-items-count" role="status" aria-live="polite">
                                {t('products.list.currentItems')}: {filteredAndSortedItems.length} / {sellerItems.length}
                            </span>
                            <button
                                onClick={() => {
                                    setFilters({
                                        itemName: '',
                                        categoryId: '',
                                        variantName: '',
                                        sku: '',
                                        productIdType: '',
                                        productIdValue: '',
                                        showDeleted: false
                                    });
                                    setSortBy('itemName');
                                    setSortDirection('asc');
                                }}
                                className="products-clear-filters-button"
                            >
                                {t('products.filter.clearFilters')}
                            </button>
                        </div>
                    </div>

                    
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
                                            {renderSortableHeader('itemName', 'products.list.itemName')}
                                            {renderSortableHeader('itemCategory', 'products.list.itemCategory')}
                                            {renderSortableHeader('creationDate', 'products.list.creationDate')}
                                            {renderSortableHeader('lastUpdated', 'products.list.lastUpdated')}
                                            <th className="products-actions-column">{t('products.actions')}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {paginatedItems.map(item => (
                                            <Fragment key={item.id}>
                                                <tr 
                                                    className={`products-list-row ${expandedItemId === item.id ? 'expanded' : ''} ${item.deleted ? 'products-list-row-deleted' : ''}`}
                                                >
                                                    <td 
                                                        onClick={() => toggleExpandedRow(item.id)}
                                                        role="button"
                                                        tabIndex={0}
                                                        style={{ cursor: 'pointer' }}
                                                        aria-expanded={expandedItemId === item.id}
                                                        onKeyDown={(e) => {
                                                            if (e.key === 'Enter' || e.key === ' ') {
                                                                e.preventDefault();
                                                                toggleExpandedRow(item.id);
                                                            }
                                                        }}
                                                    >
                                                        {item.deleted && <span className="products-sr-only">{t('products.deleted')} - </span>}
                                                        {getItemName(item)}
                                                    </td>
                                                    <td 
                                                        onClick={() => toggleExpandedRow(item.id)}
                                                        style={{ cursor: 'pointer' }}
                                                    >
                                                        {getCategoryName(item.categoryID)}
                                                    </td>
                                                    <td 
                                                        onClick={() => toggleExpandedRow(item.id)}
                                                        style={{ cursor: 'pointer' }}
                                                    >
                                                        {formatDate(item.createdAt)}
                                                    </td>
                                                    <td 
                                                        onClick={() => toggleExpandedRow(item.id)}
                                                        style={{ cursor: 'pointer' }}
                                                    >
                                                        {formatDate(item.updatedAt)}
                                                    </td>
                                                    <td className="products-actions-cell">
                                                        {item.deleted ? (
                                                            <button
                                                                className="products-undelete-button"
                                                                onClick={(e) => {
                                                                    e.stopPropagation();
                                                                    handleUndeleteItem(item);
                                                                }}
                                                                title={t('products.undelete')}
                                                                aria-label={`${t('products.undelete')} ${getItemName(item)}`}
                                                            >
                                                                <svg 
                                                                    className="products-undelete-icon" 
                                                                    width="20" 
                                                                    height="20" 
                                                                    viewBox="0 0 24 24" 
                                                                    fill="none" 
                                                                    stroke="currentColor" 
                                                                    strokeWidth="2"
                                                                    strokeLinecap="round" 
                                                                    strokeLinejoin="round"
                                                                >
                                                                    <path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"></path>
                                                                    <path d="M21 3v5h-5"></path>
                                                                    <path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"></path>
                                                                    <path d="M3 21v-5h5"></path>
                                                                </svg>
                                                            </button>
                                                        ) : (
                                                            <button
                                                                className="products-delete-button"
                                                                onClick={(e) => {
                                                                    e.stopPropagation();
                                                                    handleDeleteItem(item);
                                                                }}
                                                                title={t('products.delete')}
                                                                aria-label={`${t('products.delete')} ${getItemName(item)}`}
                                                            >
                                                                <svg 
                                                                    className="products-delete-icon" 
                                                                    width="20" 
                                                                    height="20" 
                                                                    viewBox="0 0 24 24" 
                                                                    fill="none" 
                                                                    stroke="currentColor" 
                                                                    strokeWidth="2"
                                                                    strokeLinecap="round" 
                                                                    strokeLinejoin="round"
                                                                >
                                                                    <polyline points="3 6 5 6 21 6"></polyline>
                                                                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                                                                    <line x1="10" y1="11" x2="10" y2="17"></line>
                                                                    <line x1="14" y1="11" x2="14" y2="17"></line>
                                                                </svg>
                                                            </button>
                                                        )}
                                                        <button
                                                            className="products-edit-button"
                                                            onClick={(e) => {
                                                                e.stopPropagation();
                                                                handleEditItem(item);
                                                            }}
                                                            title={t('products.edit')}
                                                            aria-label={`${t('products.edit')} ${getItemName(item)}`}
                                                        >
                                                            <svg 
                                                                className="products-edit-icon" 
                                                                width="20" 
                                                                height="20" 
                                                                viewBox="0 0 24 24" 
                                                                fill="none" 
                                                                stroke="currentColor" 
                                                                strokeWidth="2"
                                                                strokeLinecap="round" 
                                                                strokeLinejoin="round"
                                                            >
                                                                <path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"></path>
                                                            </svg>
                                                        </button>
                                                    </td>
                                                </tr>
                                                {expandedItemId === item.id && (
                                                    <tr className="products-variants-row">
                                                        <td colSpan={5}>
                                                            {item.variants && item.variants.filter(v => filters.showDeleted || !v.deleted).length > 0 ? (
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
                                                                            {item.variants.filter(v => filters.showDeleted || !v.deleted).map(variant => (
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
            
            {/* Undelete Confirmation Modal */}
            {showUndeleteModal && itemToUndelete && (
                <div 
                    className="products-modal-overlay"
                    onClick={(e) => {
                        // Close modal when clicking on overlay, but not on modal content
                        if (e.target === e.currentTarget) {
                            cancelUndeleteItem();
                        }
                    }}
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="undelete-modal-title"
                    aria-describedby="undelete-modal-description"
                >
                    <div 
                        className="products-modal-content"
                        ref={modalRef}
                        tabIndex={-1}
                        onKeyDown={handleKeyDown}
                    >
                        <h3 id="undelete-modal-title">{t('products.undelete')}</h3>
                        <p className="products-modal-message" id="undelete-modal-description">
                            {t('products.undeleteConfirm')}
                        </p>
                        <div className="products-modal-actions">
                            <button
                                className="products-modal-button products-modal-button--cancel"
                                onClick={cancelUndeleteItem}
                                aria-label="Cancel restore action"
                            >
                                {t('common.cancel')}
                            </button>
                            <button
                                className="products-modal-button products-modal-button--confirm"
                                onClick={confirmUndeleteItem}
                                aria-label="Confirm restore item"
                            >
                                {t('products.undelete')}
                            </button>
                        </div>
                    </div>
                </div>
            )}
            
            {/* Delete Confirmation Modal */}
            {showDeleteModal && itemToDelete && (
                <div 
                    className="products-modal-overlay"
                    onClick={(e) => {
                        // Close modal when clicking on overlay, but not on modal content
                        if (e.target === e.currentTarget) {
                            cancelDeleteItem();
                        }
                    }}
                    role="dialog"
                    aria-modal="true"
                    aria-labelledby="delete-modal-title"
                    aria-describedby="delete-modal-description"
                >
                    <div 
                        className="products-modal-content"
                        ref={deleteModalRef}
                        tabIndex={-1}
                        onKeyDown={handleDeleteKeyDown}
                    >
                        <h3 id="delete-modal-title">{t('products.deleteConfirmTitle')}</h3>
                        <p className="products-modal-message" id="delete-modal-description">
                            {t('products.deleteConfirm')}
                        </p>
                        <div className="products-modal-actions">
                            <button
                                className="products-modal-button products-modal-button--cancel"
                                onClick={cancelDeleteItem}
                                aria-label="Cancel delete action"
                            >
                                {t('common.cancel')}
                            </button>
                            <button
                                className="products-modal-button products-modal-button--confirm"
                                onClick={confirmDeleteItem}
                                aria-label="Confirm delete item"
                            >
                                {t('products.delete')}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
});

ProductsSection.displayName = 'ProductsSection';

export default ProductsSection;