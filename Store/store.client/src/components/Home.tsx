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
    categoryID: string;
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

const ITEM_PLACEHOLDER_ARRAY = [1, 2, 3, 4];
const RECENT_ITEMS_DISPLAY_COUNT = 4;
const RECENT_ITEMS_FETCH_COUNT = 20; // Fetch more to ensure we get enough with images
const SUGGESTED_ITEMS_COUNT = 4;
const OFFERS_COUNT = 4;
const PRIMARY_IMAGE_PATTERN = /_1\.(jpg|jpeg|png|gif|webp)$/i; // Pattern to match primary product images ending with _1

// Default CSS variable values (must match :root in Home.css)
const DEFAULT_CARD_WIDTH = 350;
const DEFAULT_CARDS_GAP = 20;
const DEFAULT_CAROUSEL_GAP = 10;
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
    const [recentProductImages, setRecentProductImages] = useState<string[]>([]);
    const [suggestedProductImages, setSuggestedProductImages] = useState<string[]>([]);
    const [recentProductNames, setRecentProductNames] = useState<string[]>([]);
    const [suggestedProductNames, setSuggestedProductNames] = useState<string[]>([]);
    const [offerProductImages, setOfferProductImages] = useState<string[]>([]);
    const [offerProductNames, setOfferProductNames] = useState<string[]>([]);
    const [offerPercentages, setOfferPercentages] = useState<number[]>([]);
    const [suggestedOfferPercentages, setSuggestedOfferPercentages] = useState<number[]>([]);
    const [recentOfferPercentages, setRecentOfferPercentages] = useState<number[]>([]);
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
                // Extract images from products, but only include products that have valid images
                const images: string[] = [];
                const names: string[] = [];
                const offers: number[] = [];
                for (const product of result.value) {
                    // Stop if we already have enough images
                    if (images.length >= RECENT_ITEMS_DISPLAY_COUNT) {
                        break;
                    }

                    if (product.variants && product.variants.length > 0) {
                        const firstVariant: ItemVariantDto = product.variants[0];
                        let imageUrl: string | null = null;

                        // Try to get first image from ImageUrls
                        if (firstVariant.imageUrls) {
                            const urls = firstVariant.imageUrls.split(',').filter((url: string) => url.trim());
                            if (urls.length > 0) {
                                imageUrl = urls[0].trim();
                            }
                        }

                        // Fall back to ThumbnailUrl if no ImageUrls found
                        if (!imageUrl && firstVariant.thumbnailUrl) {
                            imageUrl = firstVariant.thumbnailUrl;
                        }

                        // Only add to images array if we found a valid image URL
                        if (imageUrl) {
                            // Use utility function to convert relative paths to absolute URLs
                            const fullImageUrl = toAbsoluteUrl(imageUrl);
                            images.push(fullImageUrl);
                            // Get product name based on language preference
                            const productName = language === 'fr' ? product.name_fr : product.name_en;
                            names.push(productName);
                            // Extract offer percentage if available
                            offers.push(firstVariant.offer || 0);
                        }
                    }
                }
                setRecentProductImages(images);
                setRecentProductNames(names);
                setRecentOfferPercentages(offers);
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

            const response = await fetch(`${apiBaseUrl}/api/Item/GetSuggestedProducts?count=${SUGGESTED_ITEMS_COUNT}`);
            if (!response.ok) {
                console.error('Failed to fetch suggested products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                // Extract the first image from each product
                const images: string[] = [];
                const names: string[] = [];
                const offers: number[] = [];
                for (const product of result.value) {
                    if (product.variants && product.variants.length > 0) {
                        const firstVariant: ItemVariantDto = product.variants[0];
                        let imageUrl: string | null = null;

                        // Try to get first image from ImageUrls
                        if (firstVariant.imageUrls) {
                            const urls = firstVariant.imageUrls.split(',').filter((url: string) => url.trim());
                            if (urls.length > 0) {
                                imageUrl = urls[0].trim();
                            }
                        }

                        // Fall back to ThumbnailUrl if no ImageUrls found
                        if (!imageUrl && firstVariant.thumbnailUrl) {
                            imageUrl = firstVariant.thumbnailUrl;
                        }

                        // Add to images array (should always have an image since the endpoint filters for items with images)
                        if (imageUrl) {
                            const fullImageUrl = toAbsoluteUrl(imageUrl);
                            images.push(fullImageUrl);
                            // Get product name based on language preference
                            const productName = language === 'fr' ? product.name_fr : product.name_en;
                            names.push(productName);
                            // Extract offer percentage if available
                            offers.push(firstVariant.offer || 0);
                        }
                    }
                }
                setSuggestedProductImages(images);
                setSuggestedProductNames(names);
                setSuggestedOfferPercentages(offers);
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
                const productsWithOffers: { variant: ItemVariantDto; productName_en: string; productName_fr: string }[] = [];
                
                for (const product of result.value) {
                    // Stop if we already have enough products
                    if (productsWithOffers.length >= OFFERS_COUNT) {
                        break;
                    }

                    if (product.variants && product.variants.length > 0) {
                        // Find the first variant with an offer
                        const variantWithOffer = product.variants.find(v => v.offer && v.offer > 0);
                        
                        if (variantWithOffer) {
                            productsWithOffers.push({
                                variant: variantWithOffer,
                                productName_en: product.name_en,
                                productName_fr: product.name_fr
                            });
                        }
                    }
                }

                // Extract images, names, and offer percentages
                const images: string[] = [];
                const names: string[] = [];
                const percentages: number[] = [];
                
                for (const { variant, productName_en, productName_fr } of productsWithOffers) {
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
                        const fullImageUrl = toAbsoluteUrl(imageUrl);
                        images.push(fullImageUrl);
                        const productName = language === 'fr' ? productName_fr : productName_en;
                        names.push(productName);
                        percentages.push(variant.offer!);
                    }
                }
                setOfferProductImages(images);
                setOfferProductNames(names);
                setOfferPercentages(percentages);
            }
        } catch (error) {
            console.error('Error fetching products with offers:', error);
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

    // Generate array for displaying recently added items
    // If we have images, create an array matching the number of images (up to 4)
    // Otherwise, use the default placeholder array.
    // NOTE: The numeric values here (1, 2, 3, ...) are only used as simple render keys
    // and placeholder labels ("Item 1", "Item 2", etc.). When actual images are shown,
    // these numbers do not represent real item IDs and are not meaningful domain data.
    const recentItemsArray = recentProductImages.length > 0 
        ? Array.from({ length: recentProductImages.length }, (_, i) => i + 1)
        : ITEM_PLACEHOLDER_ARRAY;

    // Generate array for displaying suggested items
    const suggestedItemsArray = suggestedProductImages.length > 0 
        ? Array.from({ length: suggestedProductImages.length }, (_, i) => i + 1)
        : ITEM_PLACEHOLDER_ARRAY;

    // Generate array for displaying offer items
    const offerItemsArray = offerProductImages.length > 0 
        ? Array.from({ length: offerProductImages.length }, (_, i) => i + 1)
        : ITEM_PLACEHOLDER_ARRAY;

    const handleCardClick = (title: string) => {
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
        
        // Calculate page width based on the number of visible cards
        // This matches the visible cards count set by updateVisibleCardsCount
        const pageWidth = (visibleCardsCount * cardWidth) + ((visibleCardsCount - 1) * gap);

        const currentScroll = container.scrollLeft;
        const maxScrollLeft = container.scrollWidth - container.clientWidth;
        
        // Use direction-aware rounding to avoid skipping pages
        // Floor for next: if at 0.8 pages, floor to 0, so next goes to page 1
        // Ceil for prev: if at 1.2 pages, ceil to 2, so prev goes to page 1
        const currentPage = direction === 'next' 
            ? Math.floor(currentScroll / pageWidth)
            : Math.ceil(currentScroll / pageWidth);
        
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

    const canScrollPrev = carouselScrollPosition > 10;

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
                    items={suggestedItemsArray}
                    imageUrls={suggestedProductImages}
                    itemNames={suggestedProductNames}
                    offerPercentages={suggestedOfferPercentages}
                    language={language}
                    onClick={() => handleCardClick('suggested')}
                />
                <ItemPreviewCard
                    title={getText("Offers", "Offres")}
                    items={offerItemsArray}
                    imageUrls={offerProductImages}
                    itemNames={offerProductNames}
                    offerPercentages={offerPercentages}
                    language={language}
                    onClick={() => handleCardClick('offers')}
                />
                <ItemPreviewCard
                    title={getText("Explore Categories", "Explorer les catégories")}
                    items={ITEM_PLACEHOLDER_ARRAY}
                    onClick={() => handleCardClick('categories')}
                />
                <ItemPreviewCard
                    title={getText("Best Sellers", "Meilleures ventes")}
                    items={ITEM_PLACEHOLDER_ARRAY}
                    onClick={() => handleCardClick('bestsellers')}
                />
                <ItemPreviewCard
                    title={getText("Best Rated", "Mieux notés")}
                    items={ITEM_PLACEHOLDER_ARRAY}
                    onClick={() => handleCardClick('rated')}
                />
                <ItemPreviewCard
                    title={getText("Recently added items", "Articles récemment ajoutés")}
                    items={recentItemsArray}
                    imageUrls={recentProductImages}
                    itemNames={recentProductNames}
                    offerPercentages={recentOfferPercentages}
                    language={language}
                    onClick={() => handleCardClick('recentlyadded')}
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

interface ItemPreviewCardProps {
    title: string;
    items: number[];
    imageUrls?: string[];
    itemNames?: string[];
    offerPercentages?: number[];
    language?: string;
    onClick?: () => void;
}

function ItemPreviewCard({ title, items, imageUrls, itemNames, offerPercentages, language = 'en', onClick }: ItemPreviewCardProps) {
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

    return (
        <div
            className="item-preview-card"
            onClick={onClick}
            onKeyDown={onClick ? handleKeyDown : undefined}
            tabIndex={onClick ? 0 : undefined}
            role={onClick ? 'button' : undefined}
            aria-label={onClick ? title : undefined}
        >
            <h3 className="card-title">{title}</h3>
            <div className="items-grid">
                {items.map((item, index) => {
                    // Check if this item has an actual image to display
                    const hasImage = imageUrls && imageUrls[index] && !imageErrors.has(index);
                    
                    // For cards that fetch data (have imageUrls prop defined), only render items with images
                    // For static cards (imageUrls prop undefined), always show placeholders
                    if (imageUrls !== undefined && !hasImage) {
                        return null;
                    }
                    
                    return (
                        <div key={item} className="item-placeholder">
                            {hasImage ? (
                                <>
                                    <img 
                                        src={imageUrls[index]!} 
                                        alt={itemNames?.[index] || `Item ${item}`} 
                                        className="item-image"
                                        onError={() => handleImageError(index)}
                                    />
                                    {offerPercentages && offerPercentages[index] !== undefined && offerPercentages[index] > 0 && (
                                        <div className="offer-badge">{getOfferText(offerPercentages[index])}</div>
                                    )}
                                    {itemNames && itemNames[index] && (
                                        <div className="item-name">{itemNames[index]}</div>
                                    )}
                                </>
                            ) : (
                                <div className="item-image-placeholder">
                                    {itemNames?.[index] ?? (language === 'fr' ? `Article ${item}` : `Item ${item}`)}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

export default Home;
