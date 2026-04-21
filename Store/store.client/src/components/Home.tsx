import { useState, useEffect, useMemo, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';
import { toAbsoluteUrl } from '../utils/urlUtils';
import { mapleLeavesFromRating } from '../utils/ratingUtils';

/** Tolerance (px) used when comparing scrollLeft to the maximum scroll position to account for sub-pixel rounding. */
const SCROLL_TOLERANCE = 1;

/** Fraction of the card's rendered height used as the chevron button size. */
const CHEVRON_SIZE_RATIO = 0.25;

interface HomeProps {
    isAuthenticated?: boolean;
    onLogout?: () => void;
}

// API Response Types
interface ItemVariantAttributeDto {
    id: string;
    attributeName_en: string;
    attributeName_fr?: string;
    attributes_en: string;
    attributes_fr?: string;
}

interface ItemVariantDto {
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
    itemVariantAttributes: ItemVariantAttributeDto[];
    deleted: boolean;
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
}

interface ItemAttributeDto {
    id: string;
    attributeName_en: string;
    attributeName_fr?: string;
    attributes_en: string;
    attributes_fr?: string;
}

interface GetItemResponse {
    id: string;
    sellerID: string;
    name_en: string;
    name_fr: string;
    description_en?: string;
    description_fr?: string;
    imageUrl?: string;
    categoryNodeID: string;
    categoryName_en?: string;
    categoryName_fr?: string;
    variants: ItemVariantDto[];
    itemAttributes: ItemAttributeDto[];
    createdAt: string;
    updatedAt?: string;
    averageRating: number;
    ratingCount: number;
    deleted: boolean;
}

interface ApiResult<T> {
    isSuccess: boolean;
    value?: T;
    error?: string;
    errorCode?: number;
}

interface ProductPreviewItem {
    id: string;
    imageUrl: string;
    name: string;
    offer: number;
    averageRating: number;
    ratingCount: number;
    categoryNodeId?: string;
}

interface OfferPreviewSource {
    variant: ItemVariantDto;
    productName_en: string;
    productName_fr: string;
    productId: string;
    averageRating: number;
    ratingCount: number;
}

const ITEM_PLACEHOLDER_ARRAY = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
const RECENT_ITEMS_DISPLAY_COUNT = 16;
const RECENT_ITEMS_FETCH_COUNT = 24; // Fetch more to ensure we get enough with images
const SUGGESTED_ITEMS_COUNT = 16;
const SUGGESTED_ITEMS_FETCH_COUNT = 24; // Fetch more to ensure we get enough with images
const OFFERS_COUNT = 16;
const SUGGESTED_CATEGORIES_FETCH_COUNT = 24;
const SUGGESTED_CATEGORIES_DISPLAY_COUNT = 16;

/**
 * Returns true if the variant has an active (non-expired and started) offer.
 * An offer is considered not yet active when offerStart is set and is in the future,
 * and expired when offerEnd is set and is in the past.
 */
function isOfferActive(variant: ItemVariantDto): boolean {
    if (!variant.offer || variant.offer <= 0) return false;
    if (variant.offerStart && new Date(variant.offerStart) > new Date()) return false;
    if (variant.offerEnd && new Date(variant.offerEnd) < new Date()) return false;
    return true;
}

/**
 * Extracts product preview data from a list of products.
 * Prefers imageUrls on variants, falls back to thumbnailUrl.
 * @param products List of products to extract from
 * @param language Current display language ('fr' or other)
 * @param maxCount Optional maximum number of entries to return
 * @param useCategoryName When true, use category name instead of item name
 * @returns Array of ProductPreviewItem objects each containing the product id, resolved image URL, display name and active offer percentage
 */
function extractProductImages(
    products: GetItemResponse[],
    language: string,
    maxCount?: number,
    useCategoryName?: boolean
): ProductPreviewItem[] {
    const result: ProductPreviewItem[] = [];
    for (const product of products) {
        if (maxCount !== undefined && result.length >= maxCount) {
            break;
        }
        if (product.variants && product.variants.length > 0) {
            let imageUrl: string | null = null;
            let selectedVariant: ItemVariantDto = product.variants[0];

            // Prefer imageUrls: scan all variants before falling back to thumbnailUrl
            for (const variant of product.variants) {
                if (variant.imageUrls) {
                    const urls = variant.imageUrls.split(',').filter((url: string) => url.trim());
                    if (urls.length > 0) {
                        imageUrl = urls[0].trim();
                        selectedVariant = variant;
                        break;
                    }
                }
            }

            // Fall back to thumbnailUrl if no imageUrls found on any variant
            if (!imageUrl) {
                for (const variant of product.variants) {
                    if (variant.thumbnailUrl) {
                        imageUrl = variant.thumbnailUrl;
                        selectedVariant = variant;
                        break;
                    }
                }
            }

            if (imageUrl) {
                const name = useCategoryName
                    ? (language === 'fr' ? (product.categoryName_fr || product.name_fr) : (product.categoryName_en || product.name_en))
                    : (language === 'fr' ? product.name_fr : product.name_en);
                result.push({
                    id: product.id,
                    imageUrl: toAbsoluteUrl(imageUrl),
                    name,
                    offer: isOfferActive(selectedVariant) ? (selectedVariant.offer || 0) : 0,
                    averageRating: product.averageRating ?? 0,
                    ratingCount: product.ratingCount ?? 0,
                    ...(useCategoryName ? { categoryNodeId: product.categoryNodeID } : {}),
                });
            }
        }
    }
    return result;
}

function Home({ isAuthenticated = false, onLogout }: HomeProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);
    const [rawRecentProducts, setRawRecentProducts] = useState<GetItemResponse[]>([]);
    const [rawSuggestedProducts, setRawSuggestedProducts] = useState<GetItemResponse[]>([]);
    const [rawOfferProducts, setRawOfferProducts] = useState<GetItemResponse[]>([]);
    const [rawCategoriesProducts, setRawCategoriesProducts] = useState<GetItemResponse[]>([]);

    const recentProducts = useMemo(
        () => extractProductImages(rawRecentProducts, language, RECENT_ITEMS_DISPLAY_COUNT),
        [rawRecentProducts, language]
    );
    const suggestedProducts = useMemo(
        () => extractProductImages(rawSuggestedProducts, language, SUGGESTED_ITEMS_COUNT),
        [rawSuggestedProducts, language]
    );
    const offerProducts = useMemo((): ProductPreviewItem[] => {
        const productsWithOffers: OfferPreviewSource[] = [];
        for (const product of rawOfferProducts) {
            if (productsWithOffers.length >= OFFERS_COUNT) break;
            if (product.variants && product.variants.length > 0) {
                const variantWithOffer = product.variants.find(v => isOfferActive(v));
                if (variantWithOffer) {
                    productsWithOffers.push({
                        variant: variantWithOffer,
                        productName_en: product.name_en,
                        productName_fr: product.name_fr,
                        productId: product.id,
                        averageRating: product.averageRating ?? 0,
                        ratingCount: product.ratingCount ?? 0,
                    });
                }
            }
        }
        const items: ProductPreviewItem[] = [];
        for (const { variant, productName_en, productName_fr, productId, averageRating, ratingCount } of productsWithOffers) {
            let imageUrl: string | null = null;
            if (variant.imageUrls) {
                const urls = variant.imageUrls.split(',').filter((url: string) => url.trim());
                imageUrl = urls.length > 0 ? urls[0].trim() : null;
            }
            if (!imageUrl && variant.thumbnailUrl) {
                imageUrl = variant.thumbnailUrl;
            }
            if (imageUrl) {
                items.push({
                    id: productId,
                    imageUrl: toAbsoluteUrl(imageUrl),
                    name: language === 'fr' ? productName_fr : productName_en,
                    offer: variant.offer!,
                    averageRating,
                    ratingCount,
                });
            }
        }
        return items;
    }, [rawOfferProducts, language]);
    const categoriesProducts = useMemo(
        () => extractProductImages(rawCategoriesProducts, language, SUGGESTED_CATEGORIES_DISPLAY_COUNT, true),
        [rawCategoriesProducts, language]
    );

    useEffect(() => {
        // Set language based on user or system settings
        const browserLang = navigator.language.toLowerCase();
        setLanguage(browserLang.includes('fr') ? 'fr' : 'en');

        // TODO: Fetch cart items count from API/state management
        // For now, this is hardcoded for demonstration
        setCartItemsCount(0); // Will be updated when cart state management is implemented

        // Fetch recently added products and suggested products
        fetchRecentlyAddedProducts();
        fetchSuggestedProducts();
        fetchProductsWithOffers();
        fetchSuggestedCategoriesProducts();
    }, [isAuthenticated]);

    const fetchRecentlyAddedProducts = async () => {
        try {
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            // Fetch more products than needed to ensure we get enough with images
            const response = await fetch(`${apiBaseUrl}/api/Item/GetRecentlyAddedProducts?count=${RECENT_ITEMS_FETCH_COUNT}`);
            if (!response.ok) {
                console.error('Failed to fetch recently added products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setRawRecentProducts(result.value);
            }
        } catch (error) {
            console.error('Error fetching recently added products:', error);
        }
    };

    const fetchSuggestedProducts = async () => {
        try {
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            // Fetch more products than needed to ensure we get enough with images
            const response = await fetch(`${apiBaseUrl}/api/Item/GetSuggestedProducts?count=${SUGGESTED_ITEMS_FETCH_COUNT}`);
            if (!response.ok) {
                console.error('Failed to fetch suggested products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setRawSuggestedProducts(result.value);
            }
        } catch (error) {
            console.error('Error fetching suggested products:', error);
        }
    };

    const fetchProductsWithOffers = async () => {
        try {
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            const response = await fetch(`${apiBaseUrl}/api/Item/GetProductsWithOffers?count=${OFFERS_COUNT}`);
            if (!response.ok) {
                console.error('Failed to fetch products with offers');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setRawOfferProducts(result.value);
            }
        } catch (error) {
            console.error('Error fetching products with offers:', error);
        }
    };

    const fetchSuggestedCategoriesProducts = async () => {
        try {
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            const response = await fetch(`${apiBaseUrl}/api/Item/GetSuggestedCategoriesProducts?count=${SUGGESTED_CATEGORIES_FETCH_COUNT}`);
            if (!response.ok) {
                console.error('Failed to fetch suggested categories products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setRawCategoriesProducts(result.value);
            }
        } catch (error) {
            console.error('Error fetching suggested categories products:', error);
        }
    };

    const handleConnectClick = () => {
        if (isAuthenticated) {
            // User is authenticated, perform logout if handler is available
            if (onLogout) {
                onLogout();
            }
        } else {
            // User is not authenticated, navigate to login
            navigate('/login');
        }
    };

    const handleLogoClick = () => {
        navigate('/');
    };

    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        // TODO: Implement search functionality
        console.log('Search query:', searchQuery);
    };

    const handleCartClick = () => {
        navigate('/cart');
    };

    const handleCardClick = (title: string) => {
        if (title === 'offers') {
            navigate('/offers');
            return;
        }
        if (title === 'categories') {
            navigate('/categories');
            return;
        }
        if (title === 'suggested') {
            navigate('/suggested-items');
            return;
        }
        if (title === 'recentlyadded') {
            navigate('/recently-added');
            return;
        }
        // TODO: Implement navigation to respective category/search pages
        console.log('Card clicked:', title);
    };

    // Get text based on selected language
    const getText = (en: string, fr: string) => language === 'fr' ? fr : en;

    return (
        <div className="home-container">
            {/* Top Navigation Bar */}
            <nav className="top-nav">
                <button
                    type="button"
                    className="nav-item logo"
                    onClick={handleLogoClick}
                    aria-label={getText("Go to home page", "Aller à la page d'accueil")}
                >
                    CanoEh!
                </button>
                <form className="nav-item search-bar" onSubmit={handleSearchSubmit}>
                    <div className="nav-search-field">
                        <input
                            type="text"
                            placeholder={getText("Search items...", "Rechercher des articles...")}
                            aria-label={getText("Search for items", "Rechercher des articles")}
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                        />
                    </div>
                    <div className="nav-right">
                        <button
                            type="submit"
                            className="search-icon"
                            aria-label={getText("Search", "Rechercher")}
                        >
                            🔍
                        </button>
                    </div>
                </form>
                <div className="nav-item language-selector">
                    <select value={language} onChange={(e) => setLanguage(e.target.value)} aria-label={getText("Select language", "Sélectionner la langue")}>
                        <option value="en">English</option>
                        <option value="fr">Français</option>
                    </select>
                </div>
                <div className="nav-item connect-button">
                    <button onClick={handleConnectClick}>
                        {isAuthenticated ? getText('Logout', 'Déconnexion') : getText('Sign In', 'Connexion')}
                    </button>
                </div>
                <button
                    type="button"
                    className="nav-item cart-button"
                    onClick={handleCartClick}
                    aria-label={getText("Items in cart", "Articles dans le panier")}
                >
                    <div>
                        <span className="nav-cart-icon"></span>
                    </div>
                    <div>
                        <span className="nav-cart-line1">
                            {cartItemsCount}
                        </span>
                        <span className="nav-cart-line2">
                            {getText("Items", "Articles")}
                        </span>
                    </div>
                </button>
            </nav>

            {/* Store Content Container */}
            <div className="store-content">
                {/* Page Content Section - Banner */}
                <section className="banner-section">
                    <div className="banner-widget">
                        <h2>{getText("Welcome to CanoEh!", "Bienvenue chez CanoEh!")}</h2>
                        <p>{getText("Discover amazing deals and products from across Canada!", "Découvrez des offres et des produits incroyables de partout au Canada!")}</p>
                    </div>
                </section>

                {/* Cards Section */}
                <section className="cards-section">
                <ItemPreviewCard
                    title={getText("Suggested items", "Articles suggérés")}
                    products={suggestedProducts}
                    language={language}
                    onClick={() => handleCardClick('suggested')}
                    onItemClick={(product) => navigate(`/product/${product.id}`)}
                />
                <ItemPreviewCard
                    title={getText("Offers", "Offres")}
                    products={offerProducts}
                    language={language}
                    onClick={() => handleCardClick('offers')}
                    onItemClick={(product) => navigate(`/product/${product.id}`)}
                />
                <ItemPreviewCard
                    title={getText("Explore Categories", "Explorer les catégories")}
                    products={categoriesProducts}
                    language={language}
                    onClick={() => handleCardClick('categories')}
                    onItemClick={(product) => navigate(`/categories${product.categoryNodeId ? `?nodeId=${encodeURIComponent(product.categoryNodeId)}` : ''}`)}
                />
                <ItemPreviewCard
                    title={getText("Best Sellers", "Meilleures ventes")}
                    onClick={() => handleCardClick('bestsellers')}
                />
                <ItemPreviewCard
                    title={getText("Best Rated", "Mieux notés")}
                    onClick={() => handleCardClick('rated')}
                />
                <ItemPreviewCard
                    title={getText("Recently added items", "Articles récemment ajoutés")}
                    products={recentProducts}
                    language={language}
                    onClick={() => handleCardClick('recentlyadded')}
                    onItemClick={(product) => navigate(`/product/${product.id}`)}
                />
                {isAuthenticated && (
                    <>
                        <ItemPreviewCard
                            title={getText("Last Viewed Items", "Derniers articles consultés")}
                            items={ITEM_PLACEHOLDER_ARRAY}
                            onClick={() => handleCardClick('viewed')}
                        />
                        <ItemPreviewCard
                            title={getText("Local Products", "Produits locaux")}
                            items={ITEM_PLACEHOLDER_ARRAY}
                            onClick={() => handleCardClick('local')}
                        />
                        <ItemPreviewCard
                            title={getText("Buy Again", "Acheter à nouveau")}
                            items={ITEM_PLACEHOLDER_ARRAY}
                            onClick={() => handleCardClick('buyagain')}
                        />
                    </>
                )}
                </section>
            </div>

            {/* Store Footer */}
            <footer className="store-footer">
                <p>{getText("© 2025 CanoEh! All rights reserved.", "© 2025 CanoEh! Tous droits réservés.")}</p>
            </footer>
        </div>
    );
}

/**
 * Props for ItemPreviewCard.
 *
 * Rendering follows a two-mode design:
 * - **Dynamic mode** (`products` provided): iterates over `products` and renders each as an
 *   `<img>`-backed item. When `onItemClick` is also provided, each item is wrapped in a
 *   `<button>` with the `item-placeholder-clickable` class.
 * - **Static placeholder mode** (`products` undefined): iterates over `items` and renders
 *   labelled placeholder divs. Used by cards (e.g. Best Sellers, Best Rated) that do not
 *   yet have real product data. `items` defaults to `ITEM_PLACEHOLDER_ARRAY` when omitted.
 */
interface ItemPreviewCardProps {
    title: string;
    /** Numeric keys for static placeholder rendering. Defaults to [1,2,3,4] when omitted. */
    items?: number[];
    /** Actual product data. When provided, takes precedence over `items`. */
    products?: ProductPreviewItem[];
    language?: string;
    onClick?: () => void;
    /** Called with the clicked product when `products` is provided. */
    onItemClick?: (product: ProductPreviewItem) => void;
}

function ItemPreviewCard({ title, items = ITEM_PLACEHOLDER_ARRAY, products, language = 'en', onClick, onItemClick }: ItemPreviewCardProps) {
    const [imageErrors, setImageErrors] = useState<Set<number>>(new Set());
    const [isHovered, setIsHovered] = useState(false);
    const [canScrollLeft, setCanScrollLeft] = useState(false);
    const [canScrollRight, setCanScrollRight] = useState(false);
    const [chevronHeight, setChevronHeight] = useState(0);
    const itemsGridRef = useRef<HTMLDivElement>(null);
    const cardRef = useRef<HTMLDivElement>(null);

    const updateScrollState = useCallback(() => {
        const el = itemsGridRef.current;
        if (!el) return;
        const maxScrollLeft = el.scrollWidth - el.clientWidth;
        setCanScrollLeft(el.scrollLeft > 0);
        setCanScrollRight(maxScrollLeft > SCROLL_TOLERANCE && el.scrollLeft < maxScrollLeft - SCROLL_TOLERANCE);
    }, []);

    const updateSize = useCallback(() => {
        const card = cardRef.current;
        if (card) setChevronHeight(card.offsetHeight * CHEVRON_SIZE_RATIO);
    }, []);

    useEffect(() => {
        const el = itemsGridRef.current;
        const card = cardRef.current;
        if (!el) return;

        const handleResize = () => {
            updateSize();
            updateScrollState();
        };

        el.addEventListener('scroll', updateScrollState);

        if (typeof ResizeObserver !== 'undefined') {
            const resizeObserver = new ResizeObserver(handleResize);
            if (card) resizeObserver.observe(card);
            handleResize();
            return () => {
                el.removeEventListener('scroll', updateScrollState);
                resizeObserver.disconnect();
            };
        }

        // Fallback when ResizeObserver is unavailable: measure the card height directly
        updateSize();
        updateScrollState();
        return () => {
            el.removeEventListener('scroll', updateScrollState);
        };
    }, [updateScrollState, updateSize]);

    useEffect(() => {
        // Recompute scroll state when the number of products/items changes,
        // without tearing down and re-attaching listeners/observers.
        updateScrollState();
    }, [updateScrollState, products?.length, items.length]);

    useEffect(() => {
        // Remeasure card height when hover starts so the chevron size is always
        // up-to-date when displayed. This also ensures the correct height is picked
        // up in test environments where ResizeObserver is stubbed as a no-op.
        if (isHovered) {
            updateSize();
            updateScrollState();
        }
    }, [isHovered, updateSize, updateScrollState]);

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (onClick && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            onClick();
        }
    };

    const getHorizontalGap = (el: HTMLDivElement) => {
        const computedStyle = getComputedStyle(el);
        const parseGapValue = (value: string) => value === 'normal' ? 0 : parseFloat(value);

        const columnGap = parseGapValue(computedStyle.columnGap);
        if (!Number.isNaN(columnGap)) return columnGap;

        const fallbackGap = parseGapValue(computedStyle.gap);
        return Number.isNaN(fallbackGap) ? 0 : fallbackGap;
    };

    const getScrollMetrics = (el: HTMLDivElement) => {
        const firstItem = el.querySelector<HTMLElement>('.item-placeholder');
        const itemWidth = firstItem?.offsetWidth ?? 0;
        const gap = getHorizontalGap(el);
        if (itemWidth <= 0) {
            return null;
        }
        const itemStep = itemWidth + gap;

        if (itemStep <= 0) {
            return null;
        }

        const visibleItemsPerPage = Math.max(1, Math.floor((el.clientWidth + gap) / itemStep));
        const maxScrollLeft = Math.max(0, el.scrollWidth - el.clientWidth);
        const maxItemIndex = Math.max(0, Math.ceil(maxScrollLeft / itemStep));
        const currentItemIndex = Math.round(el.scrollLeft / itemStep);

        return { itemStep, visibleItemsPerPage, maxScrollLeft, maxItemIndex, currentItemIndex };
    };

    const handleScrollLeft = (e: React.MouseEvent) => {
        e.stopPropagation();
        const el = itemsGridRef.current;
        if (!el) return;
        const metrics = getScrollMetrics(el);

        if (!metrics) {
            const gap = getHorizontalGap(el);
            el.scrollBy({ left: -(el.clientWidth + gap), behavior: 'smooth' });
            return;
        }

        const targetItemIndex = Math.max(0, metrics.currentItemIndex - metrics.visibleItemsPerPage);
        const targetLeft = targetItemIndex * metrics.itemStep;
        el.scrollTo({ left: targetLeft, behavior: 'smooth' });
    };

    const handleScrollRight = (e: React.MouseEvent) => {
        e.stopPropagation();
        const el = itemsGridRef.current;
        if (!el) return;
        const metrics = getScrollMetrics(el);

        if (!metrics) {
            const gap = getHorizontalGap(el);
            el.scrollBy({ left: el.clientWidth + gap, behavior: 'smooth' });
            return;
        }

        const targetItemIndex = Math.min(metrics.maxItemIndex, metrics.currentItemIndex + metrics.visibleItemsPerPage);
        const targetLeft = Math.min(metrics.maxScrollLeft, targetItemIndex * metrics.itemStep);
        el.scrollTo({ left: targetLeft, behavior: 'smooth' });
    };

    const handleImageError = (index: number) => {
        setImageErrors((prev) => new Set(prev).add(index));
    };

    // Helper to get translated text for offer badges
    const getOfferText = (percentage: number) => {
        return language === 'fr' ? `Rabais ${percentage}%` : `${percentage}% OFF`;
    };

    // When individual items are clickable (onItemClick), the card container must NOT also be
    // interactive to avoid nested button semantics. Instead, the card title becomes its own
    // button to preserve "See all" navigation, and each product item is a <button>.
    // Only consider items interactive when there are actual products loaded.
    const hasItemClickHandler = Boolean(onItemClick) && Boolean(products?.length);

    return (
        <div
            ref={cardRef}
            className="item-preview-card"
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
            onFocus={() => setIsHovered(true)}
            onBlur={() => setIsHovered(false)}
            onClick={hasItemClickHandler ? undefined : onClick}
            onKeyDown={hasItemClickHandler ? undefined : (onClick ? handleKeyDown : undefined)}
            tabIndex={hasItemClickHandler ? undefined : (onClick ? 0 : undefined)}
            role={hasItemClickHandler ? undefined : (onClick ? 'button' : undefined)}
            aria-label={hasItemClickHandler ? undefined : (onClick ? title : undefined)}
        >
            {hasItemClickHandler && onClick ? (
                <button type="button" className="card-title card-title-btn" onClick={onClick}>
                    {title}
                </button>
            ) : (
                <h3 className="card-title">{title}</h3>
            )}
            <div className="items-grid-wrapper">
                {/* chevronHeight > 0 ensures the button has a visible size before ResizeObserver has measured the card */}
                {isHovered && canScrollLeft && chevronHeight > 0 && (
                    <button
                        type="button"
                        className="carousel-chevron carousel-chevron-left"
                        style={{ height: chevronHeight, width: chevronHeight }}
                        onClick={handleScrollLeft}
                        aria-label={language === 'fr' ? 'Articles précédents' : 'Previous items'}
                    >
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" style={{ width: '100%', height: '100%' }}>
                            <polyline points="15 18 9 12 15 6" />
                        </svg>
                    </button>
                )}
                <div className="items-grid" ref={itemsGridRef}>
                {products !== undefined ? (
                    products.map((product, index) => {
                        if (imageErrors.has(index)) return null;

                        const itemContent = (
                            <>
                                <img
                                    src={product.imageUrl}
                                    alt={product.name}
                                    className="item-image"
                                    onError={() => handleImageError(index)}
                                />
                                <div className="maple-rating-badge-home">
                                    {mapleLeavesFromRating(product.averageRating)}
                                </div>
                                {product.offer > 0 && (
                                    <div className="offer-badge">{getOfferText(product.offer)}</div>
                                )}
                                <div className="item-name">{product.name}</div>
                            </>
                        );

                        if (onItemClick) {
                            return (
                                <button
                                    key={product.id}
                                    type="button"
                                    className="item-placeholder item-placeholder-clickable"
                                    onClick={() => onItemClick(product)}
                                    aria-label={product.name}
                                >
                                    {itemContent}
                                </button>
                            );
                        }

                        return (
                            <div key={product.id} className="item-placeholder">
                                {itemContent}
                            </div>
                        );
                    })
                ) : (
                    items.map((item) => {
                        const itemLabel = language === 'fr' ? `Article ${item}` : `Item ${item}`;
                        return (
                            <div key={item} className="item-placeholder">
                                <div className="item-image-placeholder">{itemLabel}</div>
                            </div>
                        );
                    })
                )}
            </div>
                {isHovered && canScrollRight && chevronHeight > 0 && (
                    <button
                        type="button"
                        className="carousel-chevron carousel-chevron-right"
                        style={{ height: chevronHeight, width: chevronHeight }}
                        onClick={handleScrollRight}
                        aria-label={language === 'fr' ? 'Articles suivants' : 'Next items'}
                    >
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" style={{ width: '100%', height: '100%' }}>
                            <polyline points="9 6 15 12 9 18" />
                        </svg>
                    </button>
                )}
            </div>
        </div>
    );
}

export default Home;
