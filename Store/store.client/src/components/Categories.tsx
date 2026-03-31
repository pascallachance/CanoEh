import { useState, useEffect, useMemo, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import './Home.css';
import './Filters.css';
import './Browse.css';
import './Categories.css';
import { toAbsoluteUrl } from '../utils/urlUtils';

interface CategoriesProps {
    isAuthenticated?: boolean;
    onLogout?: () => void;
}

interface CategoryNodeDto {
    id: string;
    name_en: string;
    name_fr: string;
    nodeType: string;
    parentId: string | null;
    isActive: boolean;
    sortOrder: number | null;
    children: CategoryNodeDto[];
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
    categoryNodeID: string;
    categoryName_en?: string;
    categoryName_fr?: string;
    variants: ItemVariantDto[];
    deleted: boolean;
}

interface ApiResult<T> {
    isSuccess: boolean;
    value?: T;
    error?: string;
    errorCode?: number;
}

interface CategoryProduct {
    id: string;
    name_en: string;
    name_fr: string;
    price: number;
    hasOffer: boolean;
    offerPercentage: number;
    discountedPrice: number;
    imageUrl: string;
    categoryNodeID: string;
    categoryName_en: string;
    categoryName_fr: string;
}

const PRIMARY_IMAGE_PATTERN = /_1\.(jpg|jpeg|png|gif|webp)$/i;

function isOfferActive(variant: ItemVariantDto): boolean {
    if (!variant.offer || variant.offer <= 0) return false;
    if (variant.offerStart && new Date(variant.offerStart) > new Date()) return false;
    if (variant.offerEnd && new Date(variant.offerEnd) < new Date()) return false;
    return true;
}

function extractBestImageUrl(variant: ItemVariantDto): string | null {
    if (variant.imageUrls) {
        const urls = variant.imageUrls
            .split(',')
            .map((u: string) => u.trim())
            .filter((u: string) => u.length > 0);
        const primary = urls.find((u: string) => PRIMARY_IMAGE_PATTERN.test(u));
        return primary ?? urls[0] ?? null;
    }
    return variant.thumbnailUrl ?? null;
}

function buildCategoryTree(nodes: CategoryNodeDto[]): CategoryNodeDto[] {
    const map = new Map<string, CategoryNodeDto>();
    const roots: CategoryNodeDto[] = [];

    for (const node of nodes) {
        map.set(node.id, { ...node, children: [] });
    }

    for (const node of map.values()) {
        if (node.parentId && map.has(node.parentId)) {
            map.get(node.parentId)!.children.push(node);
        } else {
            // Treat nodes whose parent is missing (or have no parentId) as roots
            roots.push(node);
        }
    }

    // Sort by sortOrder then name
    const sortNodes = (arr: CategoryNodeDto[]) => {
        arr.sort((a, b) => {
            if (a.sortOrder !== null && b.sortOrder !== null) return a.sortOrder - b.sortOrder;
            if (a.sortOrder !== null) return -1;
            if (b.sortOrder !== null) return 1;
            return a.name_en.localeCompare(b.name_en);
        });
        arr.forEach(n => sortNodes(n.children));
    };
    sortNodes(roots);

    return roots;
}

function mapItemsToCategoryProducts(items: GetItemResponse[]): CategoryProduct[] {
    const result: CategoryProduct[] = [];

    for (const item of items) {
        if (!item.variants || item.variants.length === 0) continue;

        // Pick the variant with the best (active) offer, or the first available
        let bestVariant: ItemVariantDto | null = null;
        let bestOffer = 0;

        for (const v of item.variants) {
            if (v.deleted) continue;
            if (isOfferActive(v) && (v.offer ?? 0) > bestOffer) {
                bestVariant = v;
                bestOffer = v.offer!;
            }
        }

        if (!bestVariant) {
            bestVariant = item.variants.find(v => !v.deleted) ?? null;
        }

        if (!bestVariant) continue;

        const imageUrl = extractBestImageUrl(bestVariant);
        if (!imageUrl) continue;

        const hasOffer = isOfferActive(bestVariant);
        const offerPct = hasOffer ? (bestVariant.offer ?? 0) : 0;

        result.push({
            id: item.id,
            name_en: item.name_en,
            name_fr: item.name_fr,
            price: bestVariant.price,
            hasOffer,
            offerPercentage: offerPct,
            discountedPrice: bestVariant.price * (1 - offerPct / 100),
            imageUrl: toAbsoluteUrl(imageUrl),
            categoryNodeID: item.categoryNodeID,
            categoryName_en: item.categoryName_en ?? '',
            categoryName_fr: item.categoryName_fr ?? '',
        });
    }

    return result;
}

function Categories({ isAuthenticated = false, onLogout }: CategoriesProps) {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [cartItemsCount] = useState<number>(0);

    // Category tree data
    const [categoryTree, setCategoryTree] = useState<CategoryNodeDto[]>([]);
    const [nodesError, setNodesError] = useState<boolean>(false);

    // Navigation state: stack of visited nodes (breadcrumb path)
    const [navPath, setNavPath] = useState<CategoryNodeDto[]>([]);

    // Products
    const [products, setProducts] = useState<CategoryProduct[]>([]);
    const [loadingNodes, setLoadingNodes] = useState<boolean>(true);
    const [loadingProducts, setLoadingProducts] = useState<boolean>(false);

    // Filter/sort state
    const [sortBy, setSortBy] = useState<string>('name-asc');
    const [minPrice, setMinPrice] = useState<string>('');
    const [maxPrice, setMaxPrice] = useState<string>('');

    // Pagination
    const PAGE_SIZE = 12;
    const [currentPage, setCurrentPage] = useState<number>(1);

    const getText = (en: string, fr: string) => language === 'fr' ? fr : en;

    const currentNode: CategoryNodeDto | null = navPath.length > 0 ? navPath[navPath.length - 1] : null;

    // Children of the current node (or root departments if no node selected)
    const currentChildren = useMemo<CategoryNodeDto[]>(() => {
        if (!currentNode) return categoryTree;
        return currentNode.children;
    }, [currentNode, categoryTree]);

    // Filtered & sorted products
    const filteredProducts = useMemo(() => {
        let result = [...products];

        const minPriceNum = minPrice !== '' ? parseFloat(minPrice) : null;
        const maxPriceNum = maxPrice !== '' ? parseFloat(maxPrice) : null;

        if (minPriceNum !== null && !isNaN(minPriceNum)) {
            result = result.filter(p => {
                const displayPrice = p.hasOffer ? p.discountedPrice : p.price;
                return displayPrice >= minPriceNum;
            });
        }
        if (maxPriceNum !== null && !isNaN(maxPriceNum)) {
            result = result.filter(p => {
                const displayPrice = p.hasOffer ? p.discountedPrice : p.price;
                return displayPrice <= maxPriceNum;
            });
        }

        switch (sortBy) {
            case 'price-asc':
                result.sort((a, b) => {
                    const pa = a.hasOffer ? a.discountedPrice : a.price;
                    const pb = b.hasOffer ? b.discountedPrice : b.price;
                    return pa - pb;
                });
                break;
            case 'price-desc':
                result.sort((a, b) => {
                    const pa = a.hasOffer ? a.discountedPrice : a.price;
                    const pb = b.hasOffer ? b.discountedPrice : b.price;
                    return pb - pa;
                });
                break;
            case 'name-desc':
                result.sort((a, b) => {
                    const nameA = language === 'fr' ? a.name_fr : a.name_en;
                    const nameB = language === 'fr' ? b.name_fr : b.name_en;
                    return nameB.localeCompare(nameA);
                });
                break;
            default: // 'name-asc'
                result.sort((a, b) => {
                    const nameA = language === 'fr' ? a.name_fr : a.name_en;
                    const nameB = language === 'fr' ? b.name_fr : b.name_en;
                    return nameA.localeCompare(nameB);
                });
        }

        return result;
    }, [products, sortBy, minPrice, maxPrice, language]);

    // Paginated slice of filtered products
    const totalPages = Math.max(1, Math.ceil(filteredProducts.length / PAGE_SIZE));
    const safePage = Math.min(currentPage, totalPages);
    const pagedProducts = filteredProducts.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE);

    const fetchCategoryNodes = useCallback(async () => {
        try {
            setLoadingNodes(true);
            setNodesError(false);
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                setNodesError(true);
                return;
            }

            const response = await fetch(`${apiBaseUrl}/api/CategoryNode/GetAllCategoryNodes`);
            if (!response.ok) {
                console.error('Failed to fetch category nodes');
                setNodesError(true);
                return;
            }

            const result: ApiResult<CategoryNodeDto[]> = await response.json();
            if (result.isSuccess && result.value) {
                const tree = buildCategoryTree(result.value);
                setCategoryTree(tree);
            } else {
                setNodesError(true);
            }
        } catch (error) {
            console.error('Error fetching category nodes:', error);
            setNodesError(true);
        } finally {
            setLoadingNodes(false);
        }
    }, []);

    const fetchAllProducts = useCallback(async () => {
        try {
            setLoadingProducts(true);
            setProducts([]);

            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            const response = await fetch(`${apiBaseUrl}/api/Item/GetAllItems`);
            if (!response.ok) {
                console.error('Failed to fetch all products');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setProducts(mapItemsToCategoryProducts(result.value));
            }
        } catch (error) {
            console.error('Error fetching all products:', error);
        } finally {
            setLoadingProducts(false);
        }
    }, []);

    const fetchProductsForNode = useCallback(async (nodeId: string) => {
        try {
            setLoadingProducts(true);
            setProducts([]);

            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            const response = await fetch(
                `${apiBaseUrl}/api/Item/GetItemsByCategoryNode/${nodeId}`
            );
            if (!response.ok) {
                console.error('Failed to fetch products for category node');
                return;
            }

            const result: ApiResult<GetItemResponse[]> = await response.json();
            if (result.isSuccess && result.value) {
                setProducts(mapItemsToCategoryProducts(result.value));
            }
        } catch (error) {
            console.error('Error fetching products:', error);
        } finally {
            setLoadingProducts(false);
        }
    }, []);

    // Reset pagination when sorting or price filters change
    useEffect(() => {
        setCurrentPage(1);
    }, [sortBy, minPrice, maxPrice]);

    useEffect(() => {
        const browserLang = navigator.language.toLowerCase();
        setLanguage(browserLang.includes('fr') ? 'fr' : 'en');
        fetchCategoryNodes();
    }, [fetchCategoryNodes]);

    // When no nodeId is present, fetch all products immediately (no need to wait for the category tree).
    useEffect(() => {
        const nodeId = searchParams.get('nodeId');
        if (!nodeId) {
            setNavPath([]);
            fetchAllProducts();
        }
    }, [searchParams, fetchAllProducts]);

    // When a nodeId is present, wait for the category tree to load, then resolve the path and fetch.
    useEffect(() => {
        const nodeId = searchParams.get('nodeId');
        if (!nodeId || categoryTree.length === 0) return;
        const path = buildPathToNode(categoryTree, nodeId);
        if (path.length > 0) {
            setNavPath(path);
            fetchProductsForNode(encodeURIComponent(nodeId));
        } else {
            // nodeId not found in the category tree – fall back to showing all products
            setNavPath([]);
            fetchAllProducts();
        }
    }, [categoryTree, searchParams, fetchProductsForNode, fetchAllProducts]);

    const handleNodeClick = (node: CategoryNodeDto) => {
        // Update breadcrumb path - if node is already in path, navigate to it
        const existingIdx = navPath.findIndex(n => n.id === node.id);
        let newPath: CategoryNodeDto[];

        if (existingIdx >= 0) {
            newPath = navPath.slice(0, existingIdx + 1);
        } else {
            // Merge with the stored tree node (so children are populated)
            const enrichedNode = findNodeInTree(categoryTree, node.id) ?? node;
            newPath = [...navPath, enrichedNode];
        }

        setNavPath(newPath);
        setCurrentPage(1);
        fetchProductsForNode(node.id);
    };

    const handleBreadcrumbClick = (idx: number) => {
        if (idx < 0) {
            // Root – show all products
            setNavPath([]);
            setCurrentPage(1);
            fetchAllProducts();
            return;
        }
        const newPath = navPath.slice(0, idx + 1);
        setNavPath(newPath);
        setCurrentPage(1);
        fetchProductsForNode(navPath[idx].id);
    };

    const handleClearFilters = () => {
        setSortBy('name-asc');
        setMinPrice('');
        setMaxPrice('');
        setCurrentPage(1);
    };

    const handleConnectClick = () => {
        if (isAuthenticated) {
            if (onLogout) onLogout();
        } else {
            navigate('/login');
        }
    };

    const productCount = filteredProducts.length;
    const productCountLabel = language === 'fr'
        ? `${productCount} produit${productCount !== 1 ? 's' : ''} trouvé${productCount !== 1 ? 's' : ''}`
        : `${productCount} product${productCount !== 1 ? 's' : ''} found`;

    const currentNodeName = currentNode
        ? (language === 'fr' ? currentNode.name_fr : currentNode.name_en)
        : getText('All Products', 'Tous les produits');

    return (
        <div className="home-container">
            {/* Top Navigation Bar */}
            <nav className="top-nav">
                <button
                    type="button"
                    className="nav-item logo"
                    onClick={() => navigate('/')}
                    aria-label={getText("Go to home page", "Aller à la page d'accueil")}
                >
                    CanoEh!
                </button>
                <form
                    className="nav-item search-bar"
                    onSubmit={(e) => { e.preventDefault(); console.log('Search:', searchQuery); }}
                >
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
                    onClick={() => navigate('/cart')}
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
                <div className="categories-header">
                    <h1 className="categories-title">
                        {getText("Browse by Category", "Parcourir par catégorie")}
                    </h1>
                    <p className="categories-subtitle">
                        {currentNodeName}
                        {!loadingProducts && ` — ${productCountLabel}`}
                    </p>
                </div>

                {/* Main Content */}
                <div className="categories-layout">
                    {/* Filter Panel – Left 20% */}
                    <aside className="categories-filters" aria-label={getText("Filters", "Filtres")}>
                        <div className="filters-section">
                            <h2 className="filters-title">
                                {getText("Sort & Filter", "Trier et filtrer")}
                            </h2>

                            {/* Sort */}
                            <div className="filter-group">
                                <label className="filter-label" htmlFor="categories-sort">
                                    {getText("Sort by", "Trier par")}
                                </label>
                                <select
                                    id="categories-sort"
                                    className="filter-select"
                                    value={sortBy}
                                    onChange={(e) => setSortBy(e.target.value)}
                                >
                                    <option value="name-asc">
                                        {getText("Name: A to Z", "Nom : A à Z")}
                                    </option>
                                    <option value="name-desc">
                                        {getText("Name: Z to A", "Nom : Z à A")}
                                    </option>
                                    <option value="price-asc">
                                        {getText("Price: Low to High", "Prix : faible à élevé")}
                                    </option>
                                    <option value="price-desc">
                                        {getText("Price: High to Low", "Prix : élevé à faible")}
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

                    {/* Products Area – Right 80% */}
                    <main className="categories-products">
                        {/* Breadcrumb */}
                        <nav
                            className="categories-breadcrumb"
                            aria-label={getText("Category navigation", "Navigation par catégorie")}
                        >
                            {navPath.length > 0 && (
                                <>
                                    <button
                                        type="button"
                                        className="breadcrumb-item"
                                        onClick={() => handleBreadcrumbClick(-1)}
                                    >
                                        {getText('All Products', 'Tous les produits')}
                                    </button>
                                    <span className="breadcrumb-sep" aria-hidden="true">›</span>
                                </>
                            )}
                            {navPath.map((node, idx) => (
                                <span key={node.id} style={{ display: 'contents' }}>
                                    {idx > 0 && <span className="breadcrumb-sep" aria-hidden="true">›</span>}
                                    {idx < navPath.length - 1 ? (
                                        <button
                                            type="button"
                                            className="breadcrumb-item"
                                            onClick={() => handleBreadcrumbClick(idx)}
                                        >
                                            {language === 'fr' ? node.name_fr : node.name_en}
                                        </button>
                                    ) : (
                                        <span className="breadcrumb-current">
                                            {language === 'fr' ? node.name_fr : node.name_en}
                                        </span>
                                    )}
                                </span>
                            ))}
                        </nav>

                        {/* Category Node Chips */}
                        {loadingNodes ? (
                            <div className="categories-loading" role="status">
                                <p>{getText("Loading categories...", "Chargement des catégories...")}</p>
                            </div>
                        ) : nodesError ? (
                            <div className="categories-empty">
                                <p>
                                    {getText(
                                        "Unable to load categories. Please try again later.",
                                        "Impossible de charger les catégories. Veuillez réessayer plus tard."
                                    )}
                                </p>
                            </div>
                        ) : currentChildren.length > 0 && (
                            <div className="category-nodes-section">
                                <div className="category-nodes-list">
                                    {currentChildren.map((node) => (
                                        <button
                                            key={node.id}
                                            type="button"
                                            className="category-node-chip"
                                            onClick={() => handleNodeClick(node)}
                                        >
                                            {language === 'fr' ? node.name_fr : node.name_en}
                                            {node.children.length > 0 && (
                                                <span className="category-node-chip-arrow">›</span>
                                            )}
                                        </button>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Products */}
                        {loadingProducts ? (
                            <div className="categories-loading" role="status">
                                <p>{getText("Loading products...", "Chargement des produits...")}</p>
                            </div>
                        ) : filteredProducts.length === 0 ? (
                            <div className="categories-empty">
                                <p>
                                    {getText(
                                        "No products found.",
                                        "Aucun produit trouvé."
                                    )}
                                </p>
                                {(minPrice || maxPrice) && (
                                    <button
                                        type="button"
                                        className="filter-clear-btn categories-empty-btn"
                                        onClick={handleClearFilters}
                                    >
                                        {getText("Clear Filters", "Effacer les filtres")}
                                    </button>
                                )}
                            </div>
                        ) : (
                            <>
                                <div className="categories-grid">
                                    {pagedProducts.map((product) => (
                                        <CategoryProductCard
                                            key={product.id}
                                            product={product}
                                            language={language}
                                            onNavigate={(id) => navigate(`/product/${id}`)}
                                        />
                                    ))}
                                </div>
                                {totalPages > 1 && (
                                    <div className="categories-pagination" aria-label={getText("Page navigation", "Navigation par page")}>
                                        <button
                                            type="button"
                                            className="pagination-btn"
                                            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                                            disabled={safePage <= 1}
                                            aria-label={getText("Previous page", "Page précédente")}
                                        >
                                            ‹
                                        </button>
                                        {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
                                            <button
                                                key={page}
                                                type="button"
                                                className={`pagination-btn${page === safePage ? ' pagination-btn-active' : ''}`}
                                                onClick={() => setCurrentPage(page)}
                                                aria-label={getText(`Page ${page}`, `Page ${page}`)}
                                                aria-current={page === safePage ? 'page' : undefined}
                                            >
                                                {page}
                                            </button>
                                        ))}
                                        <button
                                            type="button"
                                            className="pagination-btn"
                                            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                                            disabled={safePage >= totalPages}
                                            aria-label={getText("Next page", "Page suivante")}
                                        >
                                            ›
                                        </button>
                                    </div>
                                )}
                            </>
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

interface CategoryProductCardProps {
    product: CategoryProduct;
    language: string;
    onNavigate: (productId: string) => void;
}

function CategoryProductCard({ product, language, onNavigate }: CategoryProductCardProps) {
    const [imageError, setImageError] = useState<boolean>(false);

    const name = language === 'fr' ? product.name_fr : product.name_en;
    const offerText = language === 'fr'
        ? `Rabais ${product.offerPercentage}%`
        : `${product.offerPercentage}% OFF`;

    return (
        <div
            className="browse-product-card browse-product-card-clickable"
            onClick={() => onNavigate(product.id)}
            role="button"
            tabIndex={0}
            onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    onNavigate(product.id);
                }
            }}
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
                {product.hasOffer && (
                    <div className="offer-badge">{offerText}</div>
                )}
            </div>
            <div className="browse-product-info">
                <p className="browse-product-name" title={name}>{name}</p>
                <div className="browse-product-prices">
                    {product.hasOffer ? (
                        <>
                            <span className="browse-original-price">
                                ${product.price.toFixed(2)}
                            </span>
                            <span className="browse-discounted-price">
                                ${product.discountedPrice.toFixed(2)}
                            </span>
                        </>
                    ) : (
                        <span className="browse-product-price">
                            ${product.price.toFixed(2)}
                        </span>
                    )}
                </div>
            </div>
        </div>
    );
}

// Helper to find a node by ID in the built tree
function findNodeInTree(nodes: CategoryNodeDto[], id: string): CategoryNodeDto | null {
    for (const node of nodes) {
        if (node.id === id) return node;
        const found = findNodeInTree(node.children, id);
        if (found) return found;
    }
    return null;
}

// Helper to build the ancestor path (root → node) for a given node ID
function buildPathToNode(nodes: CategoryNodeDto[], id: string): CategoryNodeDto[] {
    for (const node of nodes) {
        if (node.id === id) return [node];
        const childPath = buildPathToNode(node.children, id);
        if (childPath.length > 0) return [node, ...childPath];
    }
    return [];
}

export default Categories;
