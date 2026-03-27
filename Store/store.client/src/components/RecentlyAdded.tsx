import { useState, useEffect, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';
import './Filters.css';
import './Offers.css';
import { toAbsoluteUrl } from '../utils/urlUtils';

interface RecentlyAddedProps {
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
}

interface GetItemResponse {
    id: string;
    name_en: string;
    name_fr: string;
    variants: ItemVariantDto[];
    createdAt: string;
    deleted: boolean;
}

interface ApiResult<T> {
    isSuccess: boolean;
    value?: T;
    error?: string;
    errorCode?: number;
}

interface RecentProduct {
    id: string;
    name_en: string;
    name_fr: string;
    price: number;
    imageUrl: string;
    createdAt: string;
}

const RECENTLY_ADDED_FETCH_COUNT = 100;
const PRIMARY_IMAGE_PATTERN = /_1\.(jpg|jpeg|png|gif|webp)$/i;

function RecentlyAdded({ isAuthenticated = false, onLogout }: RecentlyAddedProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);
    const [products, setProducts] = useState<RecentProduct[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    // Filter/sort state – default: most recently added first
    const [sortBy, setSortBy] = useState<string>('date-desc');
    const [minPrice, setMinPrice] = useState<string>('');
    const [maxPrice, setMaxPrice] = useState<string>('');

    const getText = (en: string, fr: string) => language === 'fr' ? fr : en;

    const filteredProducts = useMemo(() => {
        let result = [...products];

        const minPriceNum = minPrice !== '' ? parseFloat(minPrice) : null;
        const maxPriceNum = maxPrice !== '' ? parseFloat(maxPrice) : null;

        if (minPriceNum !== null && !isNaN(minPriceNum)) {
            result = result.filter(p => p.price >= minPriceNum);
        }
        if (maxPriceNum !== null && !isNaN(maxPriceNum)) {
            result = result.filter(p => p.price <= maxPriceNum);
        }

        switch (sortBy) {
            case 'date-desc':
                result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                break;
            case 'date-asc':
                result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
                break;
            case 'price-asc':
                result.sort((a, b) => a.price - b.price);
                break;
            case 'price-desc':
                result.sort((a, b) => b.price - a.price);
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
    }, [products, sortBy, minPrice, maxPrice, language]);

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
                `${apiBaseUrl}/api/Item/GetRecentlyAddedProducts?count=${RECENTLY_ADDED_FETCH_COUNT}`
            );
            if (!response.ok) {
                console.error('Failed to fetch recently added products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                const recentProducts: RecentProduct[] = [];

                for (const product of result.value) {
                    const activeVariants = product.variants.filter(v => !v.deleted);
                    if (activeVariants.length === 0) continue;

                    // Pick the cheapest variant for display price
                    const cheapestVariant = activeVariants.reduce((prev, curr) =>
                        curr.price < prev.price ? curr : prev
                    );

                    // Find the best image across all variants
                    let imageUrl: string | null = null;
                    for (const variant of activeVariants) {
                        if (variant.imageUrls) {
                            const urls = variant.imageUrls
                                .split(',')
                                .map((u: string) => u.trim())
                                .filter((u: string) => u.length > 0);
                            const primaryImage = urls.find((u: string) =>
                                PRIMARY_IMAGE_PATTERN.test(u)
                            );
                            imageUrl = primaryImage ?? urls[0] ?? null;
                        }
                        if (!imageUrl && variant.thumbnailUrl) {
                            imageUrl = variant.thumbnailUrl;
                        }
                        if (imageUrl) break;
                    }
                    if (!imageUrl) continue;

                    recentProducts.push({
                        id: product.id,
                        name_en: product.name_en,
                        name_fr: product.name_fr,
                        price: cheapestVariant.price,
                        imageUrl: toAbsoluteUrl(imageUrl),
                        createdAt: product.createdAt,
                    });
                }

                setProducts(recentProducts);
            }
        } catch (error) {
            console.error('Error fetching recently added products:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleClearFilters = () => {
        setSortBy('date-desc');
        setMinPrice('');
        setMaxPrice('');
    };

    const handleConnectClick = () => {
        if (isAuthenticated) {
            if (onLogout) onLogout();
        } else {
            navigate('/login');
        }
    };

    const handleLogoClick = () => navigate('/');
    const handleCartClick = () => navigate('/cart');

    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        console.log('Search query:', searchQuery);
    };

    const productCount = filteredProducts.length;
    const productCountLabel = language === 'fr'
        ? `${productCount} article${productCount !== 1 ? 's' : ''} trouvé${productCount !== 1 ? 's' : ''}`
        : `${productCount} item${productCount !== 1 ? 's' : ''} found`;

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
                {/* Page Header */}
                <div className="offers-header">
                    <h1 className="offers-title">
                        {getText("Recently Added Items", "Articles récemment ajoutés")}
                    </h1>
                    {!loading && (
                        <p className="offers-subtitle">{productCountLabel}</p>
                    )}
                </div>

                {/* Main Content: Filters (left) + Products (right) */}
                <div className="offers-layout">
                    {/* Filter Panel */}
                    <aside className="offers-filters" aria-label={getText("Filters", "Filtres")}>
                        <div className="filters-section">
                            <h2 className="filters-title">
                                {getText("Sort & Filter", "Trier et filtrer")}
                            </h2>

                            {/* Sort */}
                            <div className="filter-group">
                                <label className="filter-label" htmlFor="recently-added-sort">
                                    {getText("Sort by", "Trier par")}
                                </label>
                                <select
                                    id="recently-added-sort"
                                    className="filter-select"
                                    value={sortBy}
                                    onChange={(e) => setSortBy(e.target.value)}
                                >
                                    <option value="date-desc">
                                        {getText("Date: Newest First", "Date : plus récent d'abord")}
                                    </option>
                                    <option value="date-asc">
                                        {getText("Date: Oldest First", "Date : plus ancien d'abord")}
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

                    {/* Products Grid */}
                    <main className="offers-products">
                        {loading ? (
                            <div className="offers-loading" role="status">
                                <p>{getText("Loading recently added items...", "Chargement des articles récemment ajoutés...")}</p>
                            </div>
                        ) : filteredProducts.length === 0 ? (
                            <div className="offers-empty">
                                <p>
                                    {getText(
                                        "No items found matching your criteria.",
                                        "Aucun article trouvé correspondant à vos critères."
                                    )}
                                </p>
                                <button
                                    type="button"
                                    className="filter-clear-btn offers-empty-btn"
                                    onClick={handleClearFilters}
                                >
                                    {getText("Clear Filters", "Effacer les filtres")}
                                </button>
                            </div>
                        ) : (
                            <div className="offers-grid">
                                {filteredProducts.map((product) => (
                                    <RecentProductCard
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

interface RecentProductCardProps {
    product: RecentProduct;
    language: string;
    onNavigate: (productId: string) => void;
}

function RecentProductCard({ product, language, onNavigate }: RecentProductCardProps) {
    const [imageError, setImageError] = useState<boolean>(false);

    const name = language === 'fr' ? product.name_fr : product.name_en;
    const dateLabel = language === 'fr' ? 'Ajouté le' : 'Added';
    const formattedDate = new Date(product.createdAt).toLocaleDateString(
        language === 'fr' ? 'fr-CA' : 'en-CA',
        { year: 'numeric', month: 'short', day: 'numeric' }
    );

    return (
        <div
            className="offer-product-card offer-product-card-clickable"
            onClick={() => onNavigate(product.id)}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); onNavigate(product.id); } }}
            aria-label={name}
        >
            <div className="offer-product-image-wrapper">
                {product.imageUrl && !imageError ? (
                    <img
                        src={product.imageUrl}
                        alt={name}
                        className="offer-product-image"
                        onError={() => setImageError(true)}
                    />
                ) : (
                    <div className="offer-product-image-placeholder">
                        {language === 'fr' ? 'Image non disponible' : 'No image'}
                    </div>
                )}
            </div>
            <div className="offer-product-info">
                <p className="offer-product-name" title={name}>{name}</p>
                <div className="offer-product-prices">
                    <span className="browse-product-price">
                        ${product.price.toFixed(2)}
                    </span>
                </div>
                <p className="browse-product-date">
                    {dateLabel}: {formattedDate}
                </p>
            </div>
        </div>
    );
}

export default RecentlyAdded;
