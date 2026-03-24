import { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';
import { toAbsoluteUrl } from '../utils/urlUtils';

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
}

const ITEM_PLACEHOLDER_ARRAY = [1, 2, 3, 4];
const RECENT_ITEMS_DISPLAY_COUNT = 4;
const RECENT_ITEMS_FETCH_COUNT = 20; // Fetch more to ensure we get enough with images
const SUGGESTED_ITEMS_COUNT = 4;
const SUGGESTED_ITEMS_FETCH_COUNT = 20; // Fetch more to ensure we get enough with images
const OFFERS_COUNT = 4;
const PRIMARY_IMAGE_PATTERN = /_1\.(jpg|jpeg|png|gif|webp)$/i; // Pattern to match primary product images ending with _1

// Default CSS variable values (must match :root in Home.css)
const DEFAULT_CARD_WIDTH = 345;
const DEFAULT_CARDS_GAP = 20;
const DEFAULT_CAROUSEL_GAP = 0;
const RESIZE_DEBOUNCE_MS = 150;

/**
 * Helper function to read CSS card dimension variables
 * @returns Object containing card width, gap, and carousel gap
 */
function getCSSCardDimensions() {
    const rootStyle = getComputedStyle(document.documentElement);
    return {
        cardWidth: parseInt(rootStyle.getPropertyValue('--card-width')) || DEFAULT_CARD_WIDTH,
        gap: parseInt(rootStyle.getPropertyValue('--cards-gap')) || DEFAULT_CARDS_GAP,
        carouselGap: parseInt(rootStyle.getPropertyValue('--carousel-gap')) || DEFAULT_CAROUSEL_GAP
    };
}

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
                });
            }
        }
    }
    return result;
}

/**
 * Debounce utility to limit function execution frequency
 * @param func Function to debounce
 * @param wait Delay in milliseconds
 * @returns Debounced function
 */
function debounce<T extends (...args: any[]) => void>(func: T, wait: number): (...args: Parameters<T>) => void {
    let timeout: NodeJS.Timeout | null = null;
    return (...args: Parameters<T>) => {
        if (timeout) clearTimeout(timeout);
        timeout = setTimeout(() => func(...args), wait);
    };
}

function Home({ isAuthenticated = false, onLogout }: HomeProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);
    const [recentProducts, setRecentProducts] = useState<ProductPreviewItem[]>([]);
    const [suggestedProducts, setSuggestedProducts] = useState<ProductPreviewItem[]>([]);
    const [offerProducts, setOfferProducts] = useState<ProductPreviewItem[]>([]);
    const [categoriesProducts, setCategoriesProducts] = useState<ProductPreviewItem[]>([]);
    const [carouselScrollPosition, setCarouselScrollPosition] = useState<number>(0);
    const [canScrollNext, setCanScrollNext] = useState<boolean>(false);
    const carouselRef = useRef<HTMLDivElement>(null);
    const cardsSectionRef = useRef<HTMLDivElement>(null);

    /**
     * Updates the carousel scroll state by reading current scroll position
     * and calculating button enable/disable states
     */
    const updateCarouselScrollState = () => {
        const container = carouselRef.current;
        if (container) {
            const scrollLeft = container.scrollLeft;
            const maxScrollLeft = container.scrollWidth - container.clientWidth;
            setCarouselScrollPosition(scrollLeft);
            setCanScrollNext(scrollLeft < maxScrollLeft - 10);
        }
    };

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

        // Add scroll listener to update carousel position
        const container = carouselRef.current;
        const handleScroll = () => {
            updateCarouselScrollState();
        };

        if (container) {
            container.addEventListener('scroll', handleScroll);
            // Initial state check
            handleScroll();
        }

        return () => {
            if (container) {
                container.removeEventListener('scroll', handleScroll);
            }
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [isAuthenticated]);

    // Effect to dynamically adjust visible cards count based on available space
    useEffect(() => {
        const updateVisibleCardsCount = () => {
            // Get card width and gap from CSS variables using helper function
            const { cardWidth, gap, carouselGap } = getCSSCardDimensions();

            // Calculate available width for cards (viewport width minus carousel gaps)
            const availableWidth = window.innerWidth - (2 * carouselGap);

            // Calculate how many complete cards can fit
            // Each card takes up (cardWidth + gap), except the last one which doesn't need a trailing gap
            // Formula: N * cardWidth + (N - 1) * gap <= availableWidth
            // Solving for N: N <= (availableWidth + gap) / (cardWidth + gap)
            let visibleCount = Math.floor((availableWidth + gap) / (cardWidth + gap));

            // Ensure at least 1 card is visible
            visibleCount = Math.max(1, visibleCount);

            // Update CSS variable with the calculated count
            document.documentElement.style.setProperty('--cards-visible-count', visibleCount.toString());

            // After updating the visible cards count, trigger scroll state update
            // This ensures the carousel buttons reflect the new layout
            // Use requestAnimationFrame to ensure CSS changes are applied and layout is recalculated
            // before reading scroll dimensions for the carousel
            window.requestAnimationFrame(() => {
                updateCarouselScrollState();
            });
        };

        // Debounced version for resize events to prevent excessive re-calculations
        const debouncedUpdate = debounce(updateVisibleCardsCount, RESIZE_DEBOUNCE_MS);

        // Use ResizeObserver to detect when the cards-section changes size
        // Check if ResizeObserver is available (not in all test environments)
        const cardsSection = cardsSectionRef.current;
        let resizeObserver: ResizeObserver | null = null;
        
        if (cardsSection && typeof ResizeObserver !== 'undefined') {
            resizeObserver = new ResizeObserver(debouncedUpdate);
            resizeObserver.observe(cardsSection);
        }

        // Update on window resize with debouncing
        window.addEventListener('resize', debouncedUpdate);

        // Initial update after setup, using requestAnimationFrame to ensure styles are computed
        requestAnimationFrame(updateVisibleCardsCount);

        return () => {
            window.removeEventListener('resize', debouncedUpdate);
            if (resizeObserver) {
                resizeObserver.disconnect();
            }
        };
    }, []);

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
                setRecentProducts(extractProductImages(result.value, language, RECENT_ITEMS_DISPLAY_COUNT));
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
                setSuggestedProducts(extractProductImages(result.value, language, SUGGESTED_ITEMS_COUNT));
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
                // Get one variant with offer from each product (4 different products)
                const productsWithOffers: { variant: ItemVariantDto; productName_en: string; productName_fr: string; productId: string }[] = [];
                
                for (const product of result.value) {
                    // Stop if we already have enough products
                    if (productsWithOffers.length >= OFFERS_COUNT) {
                        break;
                    }

                    if (product.variants && product.variants.length > 0) {
                        // Find the first variant with an active (non-expired) offer
                        const variantWithOffer = product.variants.find(v => isOfferActive(v));
                        
                        if (variantWithOffer) {
                            productsWithOffers.push({
                                variant: variantWithOffer,
                                productName_en: product.name_en,
                                productName_fr: product.name_fr,
                                productId: product.id
                            });
                        }
                    }
                }

                // Build unified ProductPreviewItem list
                const items: ProductPreviewItem[] = [];
                
                for (const { variant, productName_en, productName_fr, productId } of productsWithOffers) {
                    let imageUrl: string | null = null;

                    // Try to find image ending with _1 from ImageUrls
                    if (variant.imageUrls) {
                        const urls = variant.imageUrls.split(',').filter((url: string) => url.trim());
                        
                        // Look for image ending with _1 (before file extension)
                        const imageWith_1 = urls.find((url: string) => {
                            const trimmedUrl = url.trim();
                            // Match pattern like: /path/image_1.jpg or /path/image_1.png
                            return PRIMARY_IMAGE_PATTERN.test(trimmedUrl);
                        });
                        
                        if (imageWith_1) {
                            imageUrl = imageWith_1.trim();
                        } else if (urls.length > 0) {
                            // Fall back to first image if no _1 image found
                            imageUrl = urls[0].trim();
                        }
                    }

                    // Fall back to ThumbnailUrl if no ImageUrls found
                    if (!imageUrl && variant.thumbnailUrl) {
                        imageUrl = variant.thumbnailUrl;
                    }

                    if (imageUrl) {
                        items.push({
                            id: productId,
                            imageUrl: toAbsoluteUrl(imageUrl),
                            name: language === 'fr' ? productName_fr : productName_en,
                            offer: variant.offer!,
                        });
                    }
                }
                setOfferProducts(items);
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

            const response = await fetch(`${apiBaseUrl}/api/Item/GetSuggestedCategoriesProducts?count=4`);
            if (!response.ok) {
                console.error('Failed to fetch suggested categories products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setCategoriesProducts(extractProductImages(result.value, language, undefined, true));
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
        // TODO: Implement navigation to respective category/search pages
        console.log('Card clicked:', title);
    };

    const handleCarouselScroll = (direction: 'prev' | 'next') => {
        const container = carouselRef.current;
        if (!container) return;

        // Get card width, gap, and visible cards count from CSS variables
        const { cardWidth, gap } = getCSSCardDimensions();
        const rootStyle = getComputedStyle(document.documentElement);
        const visibleCardsCount = parseInt(rootStyle.getPropertyValue('--cards-visible-count')) || 1;
        
        // Calculate page width based on the number of visible cards.
        // Use visibleCardsCount * (cardWidth + gap) so the boundary aligns with
        // CSS scroll-snap points (each card occupies cardWidth + gap in the snap grid),
        // preventing the browser from snapping to a different position than targeted.
        const pageWidth = visibleCardsCount * (cardWidth + gap);

        const currentScroll = container.scrollLeft;
        const maxScrollLeft = container.scrollWidth - container.clientWidth;
        
        // Calculate current page in a direction-aware way to avoid skipping pages.
        // Use a small epsilon to handle floating-point drift near exact page boundaries.
        const rawPage = currentScroll / pageWidth;
        const EPSILON = 0.001;
        const nearestIntegerPage = Math.round(rawPage);
        let currentPage: number;
        if (Math.abs(rawPage - nearestIntegerPage) < EPSILON) {
            // Close enough to an integer page index: snap to it regardless of direction
            currentPage = nearestIntegerPage;
        } else if (direction === 'next') {
            // When moving forward, treat the user as being on the earlier page
            currentPage = Math.floor(rawPage + EPSILON);
        } else {
            // When moving backward, treat the user as being on the later page
            currentPage = Math.ceil(rawPage - EPSILON);
        }
        
        // Calculate target page
        const targetPage = direction === 'next' ? currentPage + 1 : currentPage - 1;
        
        // Calculate exact scroll position for target page
        const newScroll = targetPage * pageWidth;
        
        // Clamp to valid range
        const clampedScroll = Math.max(0, Math.min(newScroll, maxScrollLeft));

        container.scrollTo({
            left: clampedScroll,
            behavior: 'smooth'
        });

        // State will be updated by the scroll event listener
    };

    // Threshold in pixels used to determine when the carousel can scroll backwards.
    // Kept as a constant so it can be shared with scroll state logic that handles
    // fractional scrollLeft values.
    const SCROLL_BUTTON_THRESHOLD_PX = 10;

    const canScrollPrev = carouselScrollPosition > SCROLL_BUTTON_THRESHOLD_PX;
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
                    aria-label={getText("Shopping cart", "Panier d'achat")}
                >
                    <div>
                        <span className="nav-cart-icon"></span>
                    </div>
                    <div>
                        <span className="nav-cart-line1">
                            {cartItemsCount}
                        </span>
                        <span className="nav-cart-line2">
                            {getText("Cart", "Panier")}
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
                <section className="cards-section" ref={cardsSectionRef}>
                <button
                    type="button"
                    className="carousel-button prev"
                    onClick={() => handleCarouselScroll('prev')}
                    disabled={!canScrollPrev}
                    aria-label={getText("Previous cards", "Cartes précédentes")}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                        <path d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z"/>
                    </svg>
                </button>
                <div className="cards-container" ref={carouselRef}>
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
                </div>
                <button
                    type="button"
                    className="carousel-button next"
                    onClick={() => handleCarouselScroll('next')}
                    disabled={!canScrollNext}
                    aria-label={getText("Next cards", "Cartes suivantes")}
                >
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
                        <path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"/>
                    </svg>
                </button>
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

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (onClick && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            onClick();
        }
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
            className="item-preview-card"
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
            <div className="items-grid">
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
        </div>
    );
}

export default Home;
