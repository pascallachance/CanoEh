import { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';
import './Filters.css';
import './Browse.css';
import { toAbsoluteUrl } from '../utils/urlUtils';

interface OffersProps {
    isAuthenticated?: boolean;
    onLogout?: () => void;
}

interface ItemVariantDto {
    id: string;
    price: number;
    stockQuantity: number;
    sku: string;
    imageUrls?: string;
    thumbnailUrl?: string;
    deleted: boolean;
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
}

interface GetItemResponse {
    id: string;
    name_en: string;
    name_fr: string;
    variants: ItemVariantDto[];
    deleted: boolean;
}

interface ApiResult<T> {
    isSuccess: boolean;
    value?: T;
    error?: string;
    errorCode?: number;
}

interface OfferProduct {
    id: string;
    name_en: string;
    name_fr: string;
    originalPrice: number;
    offerPercentage: number;
    discountedPrice: number;
    imageUrl: string;
}

const OFFERS_FETCH_COUNT = 100;
const PRIMARY_IMAGE_PATTERN = /_1\.(jpg|jpeg|png|gif|webp)$/i;

function isOfferActive(variant: ItemVariantDto): boolean {
    if (!variant.offer || variant.offer <= 0) return false;
    if (variant.offerStart && new Date(variant.offerStart) > new Date()) return false;
    if (variant.offerEnd && new Date(variant.offerEnd) < new Date()) return false;
    return true;
}

function Offers({ isAuthenticated = false, onLogout }: OffersProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);
    const [products, setProducts] = useState<OfferProduct[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    // Filter/sort state
    const [sortBy, setSortBy] = useState<string>('discount-desc');
    const [minPrice, setMinPrice] = useState<string>('');
    const [maxPrice, setMaxPrice] = useState<string>('');
    const [minDiscount, setMinDiscount] = useState<string>('');

    const getText = (en: string, fr: string) => language === 'fr' ? fr : en;

    // Derive filtered & sorted products synchronously (no extra render cycle)
    const filteredProducts = useMemo(() => {
        let result = [...products];

        const minPriceNum = minPrice !== '' ? parseFloat(minPrice) : null;
        const maxPriceNum = maxPrice !== '' ? parseFloat(maxPrice) : null;
        const minDiscountNum = minDiscount !== '' ? parseFloat(minDiscount) : null;

        if (minPriceNum !== null && !isNaN(minPriceNum)) {
            result = result.filter(p => p.discountedPrice >= minPriceNum);
        }
        if (maxPriceNum !== null && !isNaN(maxPriceNum)) {
            result = result.filter(p => p.discountedPrice <= maxPriceNum);
        }
        if (minDiscountNum !== null && !isNaN(minDiscountNum)) {
            result = result.filter(p => p.offerPercentage >= minDiscountNum);
        }

        switch (sortBy) {
            case 'discount-desc':
                result.sort((a, b) => b.offerPercentage - a.offerPercentage);
                break;
            case 'discount-asc':
                result.sort((a, b) => a.offerPercentage - b.offerPercentage);
                break;
            case 'price-asc':
                result.sort((a, b) => a.discountedPrice - b.discountedPrice);
                break;
            case 'price-desc':
                result.sort((a, b) => b.discountedPrice - a.discountedPrice);
                break;
            case 'name-asc':
                result.sort((a, b) => {
                    const nameA = language === 'fr' ? a.name_fr : a.name_en;
                    const nameB = language === 'fr' ? b.name_fr : b.name_en;
                    return nameA.localeCompare(nameB);
                });
                break;
            case 'name-desc':
                result.sort((a, b) => {
                    const nameA = language === 'fr' ? a.name_fr : a.name_en;
                    const nameB = language === 'fr' ? b.name_fr : b.name_en;
                    return nameB.localeCompare(nameA);
                });
                break;
        }

        return result;
    }, [products, sortBy, minPrice, maxPrice, minDiscount, language]);

    useEffect(() => {
        const browserLang = navigator.language.toLowerCase();
        setLanguage(browserLang.includes('fr') ? 'fr' : 'en');
        setCartItemsCount(0);
        fetchProducts();
    }, []);

    const fetchProducts = async () => {
        try {
            setLoading(true);
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            const response = await fetch(
                `${apiBaseUrl}/api/Item/GetProductsWithOffers?count=${OFFERS_FETCH_COUNT}`
            );
            if (!response.ok) {
                console.error('Failed to fetch products with offers');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                const offerProducts: OfferProduct[] = [];

                for (const product of result.value) {
                    // Find the variant with the highest active offer
                    let bestVariant: ItemVariantDto | null = null;
                    let bestOffer = 0;

                    for (const variant of product.variants) {
                        if (isOfferActive(variant) && (variant.offer ?? 0) > bestOffer) {
                            bestVariant = variant;
                            bestOffer = variant.offer!;
                        }
                    }

                    if (!bestVariant) continue;

                    // Find the best image for the variant
                    let imageUrl: string | null = null;
                    if (bestVariant.imageUrls) {
                        const urls = bestVariant.imageUrls
                            .split(',')
                            .map((u: string) => u.trim())
                            .filter((u: string) => u.length > 0);
                        const primaryImage = urls.find((u: string) =>
                            PRIMARY_IMAGE_PATTERN.test(u)
                        );
                        imageUrl = primaryImage ?? urls[0] ?? null;
                    }
                    if (!imageUrl && bestVariant.thumbnailUrl) {
                        imageUrl = bestVariant.thumbnailUrl;
                    }
                    if (!imageUrl) continue;

                    offerProducts.push({
                        id: product.id,
                        name_en: product.name_en,
                        name_fr: product.name_fr,
                        originalPrice: bestVariant.price,
                        offerPercentage: bestOffer,
                        discountedPrice: bestVariant.price * (1 - bestOffer / 100),
                        imageUrl: toAbsoluteUrl(imageUrl),
                    });
                }

                setProducts(offerProducts);
            }
        } catch (error) {
            console.error('Error fetching products with offers:', error);
        } finally {
            setLoading(false);
        }
    };


    const handleClearFilters = () => {
        setSortBy('discount-desc');
        setMinPrice('');
        setMaxPrice('');
        setMinDiscount('');
    };

    const handleConnectClick = () => {
        if (isAuthenticated) {
            if (onLogout) {
                onLogout();
            }
        } else {
            navigate('/login');
        }
    };

    const handleLogoClick = () => {
        navigate('/');
    };

    const handleCartClick = () => {
        navigate('/cart');
    };

    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        console.log('Search query:', searchQuery);
    };

    const offerCount = filteredProducts.length;
    const offerCountLabel = language === 'fr'
        ? `${offerCount} offre${offerCount !== 1 ? 's' : ''} trouvée${offerCount !== 1 ? 's' : ''}`
        : `${offerCount} offer${offerCount !== 1 ? 's' : ''} found`;

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
                    <select
                        value={language}
                        onChange={(e) => setLanguage(e.target.value)}
                        aria-label={getText("Select language", "Sélectionner la langue")}
                    >
                        <option value="en">English</option>
                        <option value="fr">Français</option>
                    </select>
                </div>
                <div className="nav-item connect-button">
                    <button onClick={handleConnectClick}>
                        {isAuthenticated
                            ? getText('Logout', 'Déconnexion')
                            : getText('Sign In', 'Connexion')}
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
                        <span className="nav-cart-line1">{cartItemsCount}</span>
                        <span className="nav-cart-line2">{getText("Cart", "Panier")}</span>
                    </div>
                </button>
            </nav>

            {/* Store Content Container */}
            <div className="store-content">
                {/* Offers Page Header */}
                <div className="browse-header">
                    <h1 className="browse-title">
                        {getText("Products with Offers", "Produits en promotion")}
                    </h1>
                    {!loading && (
                        <p className="browse-subtitle">{offerCountLabel}</p>
                    )}
                </div>

                <div className="browse-layout">
                    <aside className="browse-filters" aria-label={getText("Filters", "Filtres")}>
                        <div className="filters-section">
                            <h2 className="filters-title">
                                {getText("Sort & Filter", "Trier et filtrer")}
                            </h2>

                            {/* Sort */}
                            <div className="filter-group">
                                <label className="filter-label" htmlFor="offers-sort">
                                    {getText("Sort by", "Trier par")}
                                </label>
                                <select
                                    id="offers-sort"
                                    className="filter-select"
                                    value={sortBy}
                                    onChange={(e) => setSortBy(e.target.value)}
                                >
                                    <option value="discount-desc">
                                        {getText("Discount: High to Low", "Rabais : élevé à faible")}
                                    </option>
                                    <option value="discount-asc">
                                        {getText("Discount: Low to High", "Rabais : faible à élevé")}
                                    </option>
                                    <option value="price-asc">
                                        {getText("Price: Low to High", "Prix : faible à élevé")}
                                    </option>
                                    <option value="price-desc">
                                        {getText("Price: High to Low", "Prix : élevé à faible")}
                                    </option>
                                    <option value="name-asc">
                                        {getText("Name: A to Z", "Nom : A à Z")}
                                    </option>
                                    <option value="name-desc">
                                        {getText("Name: Z to A", "Nom : Z à A")}
                                    </option>
                                </select>
                            </div>

                            {/* Price Range */}
                            <div className="filter-group">
                                <label className="filter-label">
                                    {getText("Price Range ($)", "Fourchette de prix ($)")}
                                </label>
                                <div className="filter-range">
                                    <input
                                        type="number"
                                        className="filter-input"
                                        placeholder={getText("Min", "Min")}
                                        value={minPrice}
                                        onChange={(e) => setMinPrice(e.target.value)}
                                        min="0"
                                        step="0.01"
                                        aria-label={getText("Minimum price", "Prix minimum")}
                                    />
                                    <span className="filter-range-sep" aria-hidden="true">—</span>
                                    <input
                                        type="number"
                                        className="filter-input"
                                        placeholder={getText("Max", "Max")}
                                        value={maxPrice}
                                        onChange={(e) => setMaxPrice(e.target.value)}
                                        min="0"
                                        step="0.01"
                                        aria-label={getText("Maximum price", "Prix maximum")}
                                    />
                                </div>
                            </div>

                            {/* Minimum Discount */}
                            <div className="filter-group">
                                <label className="filter-label" htmlFor="offers-min-discount">
                                    {getText("Minimum Discount", "Rabais minimum")}
                                </label>
                                <div className="filter-discount-input">
                                    <input
                                        id="offers-min-discount"
                                        type="number"
                                        className="filter-input"
                                        placeholder="0"
                                        value={minDiscount}
                                        onChange={(e) => setMinDiscount(e.target.value)}
                                        min="0"
                                        max="100"
                                        step="1"
                                        aria-label={getText(
                                            "Minimum discount percentage",
                                            "Pourcentage de rabais minimum"
                                        )}
                                    />
                                    <span className="filter-percent" aria-hidden="true">%</span>
                                </div>
                            </div>

                            {/* Clear Filters */}
                            <button
                                type="button"
                                className="filter-clear-btn"
                                onClick={handleClearFilters}
                            >
                                {getText("Clear Filters", "Effacer les filtres")}
                            </button>
                        </div>
                    </aside>

                    {/* Products Grid – Right 3/4 */}
                    <main className="browse-products">
                        {loading ? (
                            <div className="browse-loading" role="status">
                                <p>{getText("Loading offers...", "Chargement des offres...")}</p>
                            </div>
                        ) : filteredProducts.length === 0 ? (
                            <div className="browse-empty">
                                <p>
                                    {getText(
                                        "No offers found matching your criteria.",
                                        "Aucune offre trouvée correspondant à vos critères."
                                    )}
                                </p>
                                <button
                                    type="button"
                                    className="filter-clear-btn browse-empty-btn"
                                    onClick={handleClearFilters}
                                >
                                    {getText("Clear Filters", "Effacer les filtres")}
                                </button>
                            </div>
                        ) : (
                            <div className="browse-grid">
                                {filteredProducts.map((product) => (
                                    <OfferProductCard
                                        key={product.id}
                                        product={product}
                                        language={language}
                                        onNavigate={(productId) => navigate(`/product/${productId}`)}
                                    />
                                ))}
                            </div>
                        )}
                    </main>
                </div>
            </div>

            {/* Store Footer */}
            <footer className="store-footer">
                <p>
                    {getText(
                        "© 2025 CanoEh! All rights reserved.",
                        "© 2025 CanoEh! Tous droits réservés."
                    )}
                </p>
            </footer>
        </div>
    );
}

interface OfferProductCardProps {
    product: OfferProduct;
    language: string;
    onNavigate: (productId: string) => void;
}

function OfferProductCard({ product, language, onNavigate }: OfferProductCardProps) {
    const [imageError, setImageError] = useState<boolean>(false);

    const name = language === 'fr' ? product.name_fr : product.name_en;
    const offerText =
        language === 'fr'
            ? `Rabais ${product.offerPercentage}%`
            : `${product.offerPercentage}% OFF`;

    return (
        <div
            className="browse-product-card browse-product-card-clickable"
            onClick={() => onNavigate(product.id)}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onNavigate(product.id); } }}
            aria-label={name}
        >
            <div className="browse-product-image-wrapper">
                {product.imageUrl && !imageError ? (
                    <img
                        src={product.imageUrl}
                        alt={name}
                        className="browse-product-image"
                        onError={() => setImageError(true)}
                    />
                ) : (
                    <div className="browse-product-image-placeholder">
                        {language === 'fr' ? 'Image non disponible' : 'No image'}
                    </div>
                )}
                <div className="offer-badge">{offerText}</div>
            </div>
            <div className="browse-product-info">
                <p className="browse-product-name" title={name}>{name}</p>
                <div className="browse-product-prices">
                    <span className="browse-original-price">
                        ${product.originalPrice.toFixed(2)}
                    </span>
                    <span className="browse-discounted-price">
                        ${product.discountedPrice.toFixed(2)}
                    </span>
                </div>
            </div>
        </div>
    );
}

export default Offers;
