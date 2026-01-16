import { useState, useEffect } from 'react';
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

function Home({ isAuthenticated = false, onLogout }: HomeProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [userPostalCode, setUserPostalCode] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);
    const [recentProductImages, setRecentProductImages] = useState<string[]>([]);
    const [suggestedProductImages, setSuggestedProductImages] = useState<string[]>([]);
    const [recentProductNames, setRecentProductNames] = useState<string[]>([]);
    const [suggestedProductNames, setSuggestedProductNames] = useState<string[]>([]);
    const [offerProductImages, setOfferProductImages] = useState<string[]>([]);
    const [offerProductNames, setOfferProductNames] = useState<string[]>([]);
    const [offerPercentages, setOfferPercentages] = useState<number[]>([]);

    useEffect(() => {
        // Set language based on user or system settings
        const browserLang = navigator.language.toLowerCase();
        setLanguage(browserLang.includes('fr') ? 'fr' : 'en');

        // TODO: Fetch user's postal code from their profile/session data
        // For now, this is a placeholder that would be replaced with actual API call
        if (isAuthenticated) {
            // Example: fetchUserPostalCode().then(code => setUserPostalCode(code));
            setUserPostalCode(''); // Empty until integrated with user profile API
        }

        // TODO: Fetch cart items count from API/state management
        // For now, this is hardcoded for demonstration
        setCartItemsCount(0); // Will be updated when cart state management is implemented

        // Fetch recently added products and suggested products
        fetchRecentlyAddedProducts();
        fetchSuggestedProducts();
        fetchProductsWithOffers();
        // eslint-disable-next-line react-hooks/exhaustive-deps
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
                // Extract images from products, but only include products that have valid images
                const images: string[] = [];
                const names: string[] = [];
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
                        }
                    }
                }
                setRecentProductImages(images);
                setRecentProductNames(names);
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
                        }
                    }
                }
                setSuggestedProductImages(images);
                setSuggestedProductNames(names);
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

    const handleLocationClick = () => {
        // TODO: Implement location update functionality
        console.log('Update location clicked');
    };

    const handleNavItemClick = (item: string) => {
        // TODO: Implement navigation to respective pages
        console.log('Navigate to:', item);
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
                <button
                    type="button"
                    className="nav-item address"
                    onClick={handleLocationClick}
                    aria-label={getText("Update location", "Mettre à jour l'emplacement")}
                >
                    <div className="location-icon"></div>
                    <div>
                        <span>
                            {getText("Deliver to", "Livrer à")}
                        </span>
                        <span>
                            {isAuthenticated && userPostalCode ? userPostalCode : getText("Update location", "Mettre à jour l'emplacement")}
                        </span>
                    </div>
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

            {/* Bottom Navigation Bar */}
            <nav className="bottom-nav">
                <button
                    type="button"
                    className="nav-item"
                    onClick={() => handleNavItemClick('category')}
                >
                    {getText("Shop by Category", "Magasiner par catégorie")}
                </button>
                <button
                    type="button"
                    className="nav-item"
                    onClick={() => handleNavItemClick('province')}
                >
                    {getText("Shop by Province", "Magasiner par province")}
                </button>
                <button
                    type="button"
                    className="nav-item"
                    onClick={() => handleNavItemClick('bestsellers')}
                >
                    {getText("Best Sellers", "Meilleures ventes")}
                </button>
                <button
                    type="button"
                    className="nav-item"
                    onClick={() => handleNavItemClick('offers')}
                >
                    {getText("Shop Offers", "Offres")}
                </button>
                <button
                    type="button"
                    className="nav-item"
                    onClick={() => handleNavItemClick('new')}
                >
                    {getText("New Products", "Nouveaux produits")}
                </button>
                {isAuthenticated && (
                    <>
                        <button
                            type="button"
                            className="nav-item"
                            onClick={() => handleNavItemClick('local')}
                        >
                            {getText("Local Products", "Produits locaux")}
                        </button>
                        <button
                            type="button"
                            className="nav-item"
                            onClick={() => handleNavItemClick('gifts')}
                        >
                            {getText("Gift Ideas", "Idées cadeaux")}
                        </button>
                        <button
                            type="button"
                            className="nav-item"
                            onClick={() => handleNavItemClick('history')}
                        >
                            {getText("Browsing History", "Historique de navigation")}
                        </button>
                        <button
                            type="button"
                            className="nav-item"
                            onClick={() => handleNavItemClick('service')}
                        >
                            {getText("Customer Service", "Service client")}
                        </button>
                    </>
                )}
            </nav>

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
                    items={suggestedItemsArray}
                    imageUrls={suggestedProductImages}
                    itemNames={suggestedProductNames}
                    onClick={() => handleCardClick('suggested')}
                />
                <ItemPreviewCard
                    title={getText("Offers", "Offres")}
                    items={offerItemsArray}
                    imageUrls={offerProductImages}
                    itemNames={offerProductNames}
                    offerPercentages={offerPercentages}
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
            </section>
        </div>
    );
}

interface ItemPreviewCardProps {
    title: string;
    items: number[];
    imageUrls?: string[];
    itemNames?: string[];
    offerPercentages?: number[];
    onClick?: () => void;
}

function ItemPreviewCard({ title, items, imageUrls, itemNames, offerPercentages, onClick }: ItemPreviewCardProps) {
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
                    // Only render the item-placeholder if there's an image to display
                    const hasImage = imageUrls && imageUrls[index] && !imageErrors.has(index);
                    const shouldShowDefaultPlaceholders = !imageUrls || imageUrls.length === 0;
                    
                    // Skip rendering if we have image data but this specific index has no image
                    if (!hasImage && !shouldShowDefaultPlaceholders) {
                        return null;
                    }
                    
                    return (
                        <div key={item} className="item-placeholder">
                            {hasImage ? (
                                <>
                                    <img 
                                        src={imageUrls[index]} 
                                        alt={itemNames?.[index] || `Item ${item}`} 
                                        className="item-image"
                                        onError={() => handleImageError(index)}
                                    />
                                    {offerPercentages && offerPercentages[index] !== undefined && (
                                        <div className="offer-badge">{offerPercentages[index]}% OFF</div>
                                    )}
                                    {itemNames && itemNames[index] && (
                                        <div className="item-name">{itemNames[index]}</div>
                                    )}
                                </>
                            ) : (
                                <div className="item-image-placeholder">
                                    Item {item}
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
