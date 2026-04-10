import { useState, useEffect, useMemo, Fragment } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import './Home.css';
import './Product.css';
import { toAbsoluteUrl } from '../utils/urlUtils';

interface ProductProps {
    isAuthenticated?: boolean;
    onLogout?: () => void;
}

interface ItemVariantAttributeDto {
    id: string;
    attributeName_en: string;
    attributeName_fr?: string;
    attributes_en: string;
    attributes_fr?: string;
    isMain?: boolean;
}

interface ItemVariantFeaturesDto {
    id: string;
    attributeName_en: string;
    attributeName_fr?: string | null;
    attributes_en: string;
    attributes_fr?: string | null;
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
    itemVariantFeatures: ItemVariantFeaturesDto[];
    deleted: boolean;
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
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
    itemAttributes: { id: string; attributeName_en: string; attributeName_fr?: string; attributes_en: string; attributes_fr?: string }[];
    itemVariantFeatures: ItemVariantFeaturesDto[];
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

/**
 * An attribute group for rendering variant options.
 * nameKey / option.valueKey are language-invariant (always EN).
 * displayName / option.displayLabel are localized.
 * isMain reflects the IsMain flag on the underlying ItemVariantAttribute (main group is shown first).
 */
interface AttributeGroup {
    nameKey: string;
    displayName: string;
    isMain: boolean;
    options: { valueKey: string; displayLabel: string; thumbnailUrl?: string }[];
}

function isOfferActive(variant: ItemVariantDto): boolean {
    if (!variant.offer || variant.offer <= 0) return false;
    if (variant.offerStart && new Date(variant.offerStart) > new Date()) return false;
    if (variant.offerEnd && new Date(variant.offerEnd) < new Date()) return false;
    return true;
}

/**
 * Parses comma-separated imageUrls string into an array of absolute URLs.
 */
function parseImageUrls(imageUrlsStr?: string, thumbnailUrl?: string): string[] {
    const urls: string[] = [];
    if (imageUrlsStr) {
        const parsed = imageUrlsStr
            .split(',')
            .map((u) => u.trim())
            .filter((u) => u.length > 0)
            .map((u) => toAbsoluteUrl(u));
        urls.push(...parsed);
    }
    if (urls.length === 0 && thumbnailUrl) {
        urls.push(toAbsoluteUrl(thumbnailUrl));
    }
    return urls;
}

/**
 * Builds a list of attribute groups from the product variants for rendering the variant selector.
 * nameKey and valueKey are always English (language-invariant) so they can safely be used as
 * keys in selectedAttributes without de-syncing when the display language is changed.
 * Groups are sorted so that the IsMain group appears first.
 * Each option carries the thumbnailUrl of the first variant that has that attribute value.
 */
function buildAttributeGroups(variants: ItemVariantDto[], language: string): AttributeGroup[] {
    const groupMap = new Map<string, AttributeGroup>();
    for (const variant of variants) {
        if (variant.deleted) continue;
        for (const attr of variant.itemVariantAttributes) {
            const nameKey = attr.attributeName_en;
            const displayName = language === 'fr'
                ? (attr.attributeName_fr || attr.attributeName_en)
                : attr.attributeName_en;
            const valueKey = attr.attributes_en;
            const displayLabel = language === 'fr'
                ? (attr.attributes_fr || attr.attributes_en)
                : attr.attributes_en;

            if (!groupMap.has(nameKey)) {
                groupMap.set(nameKey, { nameKey, displayName, isMain: attr.isMain ?? false, options: [] });
            }
            const group = groupMap.get(nameKey)!;
            group.displayName = displayName; // update in case language changed
            if (attr.isMain) group.isMain = true;
            if (!group.options.some((o) => o.valueKey === valueKey)) {
                const thumbnailUrl = variant.thumbnailUrl ? toAbsoluteUrl(variant.thumbnailUrl) : undefined;
                group.options.push({ valueKey, displayLabel, thumbnailUrl });
            } else {
                const opt = group.options.find((o) => o.valueKey === valueKey)!;
                const thumbnailUrl = variant.thumbnailUrl ? toAbsoluteUrl(variant.thumbnailUrl) : undefined;
                opt.displayLabel = displayLabel; // update localized label
                if (!opt.thumbnailUrl && thumbnailUrl) {
                    opt.thumbnailUrl = thumbnailUrl;
                }
            }
        }
    }
    const groups = Array.from(groupMap.values());
    // Place the IsMain group first; preserve original order for the rest
    groups.sort((a, b) => Number(b.isMain) - Number(a.isMain));
    return groups;
}

/**
 * Returns true when the attribute matches the given language-invariant name/value keys.
 */
function attributeMatchesKeys(attribute: ItemVariantAttributeDto, nameKey: string, valueKey: string): boolean {
    return attribute.attributeName_en === nameKey && attribute.attributes_en === valueKey;
}

/**
 * Returns true if the variant has an attribute matching the given nameKey and valueKey.
 */
function variantHasAttribute(variant: ItemVariantDto, nameKey: string, valueKey: string): boolean {
    return variant.itemVariantAttributes.some(
        (a) => attributeMatchesKeys(a, nameKey, valueKey)
    );
}

/**
 * Computes which attribute option buttons should appear out-of-stock (greyed out).
 *
 * Groups are ordered with the main (primary) group first, followed by secondary, tertiary, etc.
 *
 * - The main group (`isMain === true`) is evaluated independently: an option is out-of-stock
 *   when NO non-deleted variant with that main attribute value has stockQuantity > 0.
 * - Each non-main group at array position i is constrained by the selected values of the groups
 *   at positions 0..i-1 (those that appear before it in the sorted array). Since `buildAttributeGroups`
 *   places the isMain group first, the main group's selection always acts as a constraint for
 *   non-main groups. Consequently a secondary option is only greyed out when ALL combinations
 *   with the currently selected main value are out of stock, not just the currently selected
 *   tertiary value.
 *
 * Returns a Set of JSON-serialized [nameKey, valueKey] pairs for options that should be greyed out.
 * JSON.stringify is used to avoid key collisions when either value contains the delimiter character.
 */
function computeOutOfStockOptions(
    variants: ItemVariantDto[],
    attributeGroups: AttributeGroup[],
    selectedAttributes: Record<string, string>
): Set<string> {
    const outOfStock = new Set<string>();
    const activeVariants = variants.filter((v) => !v.deleted);

    for (let i = 0; i < attributeGroups.length; i++) {
        const group = attributeGroups[i];
        // The main group is evaluated independently (no prior constraints): it is greyed when
        // no variant with this main value has stock, regardless of other selections.
        // Non-main groups use only the groups that appear before them in the sorted array as
        // constraints, so a secondary group is greyed only when ALL combinations involving the
        // groups that follow it are also OOS, not just the currently selected value.
        const priorGroups = group.isMain ? [] : attributeGroups.slice(0, i);
        for (const option of group.options) {
            const key = JSON.stringify([group.nameKey, option.valueKey]);
            const hasStock = activeVariants.some(
                (v) =>
                    v.stockQuantity > 0 &&
                    variantHasAttribute(v, group.nameKey, option.valueKey) &&
                    priorGroups.every(
                        (g) =>
                            !selectedAttributes[g.nameKey] ||
                            variantHasAttribute(v, g.nameKey, selectedAttributes[g.nameKey])
                    )
            );
            if (!hasStock) outOfStock.add(key);
        }
    }
    return outOfStock;
}

/**
 * Finds a variant that matches all selected attribute values.
 * selectedAttributes uses language-invariant keys (attributeName_en → attributes_en).
 * Returns the first matching non-deleted variant, or null if not found.
 */
function findMatchingVariant(
    variants: ItemVariantDto[],
    selectedAttributes: Record<string, string>
): ItemVariantDto | null {
    const selectedEntries = Object.entries(selectedAttributes);
    if (selectedEntries.length === 0) {
        return variants.find((v) => !v.deleted) ?? null;
    }

    for (const variant of variants) {
        if (variant.deleted) continue;
        const matches = selectedEntries.every(([nameKey, valueKey]) =>
            variant.itemVariantAttributes.some(
                (a) => attributeMatchesKeys(a, nameKey, valueKey)
            )
        );
        if (matches) return variant;
    }
    return null;
}

/**
 * Builds the ancestor path (root → leaf) for a given category node ID.
 * Accepts a pre-built node map to avoid rebuilding it on every call.
 * Guards against cycles in the category graph using a visited-ID set.
 */
function buildCategoryPath(nodeMap: Map<string, CategoryNodeDto>, categoryNodeId: string): CategoryNodeDto[] {
    const path: CategoryNodeDto[] = [];
    const visitedIds = new Set<string>();
    let current = nodeMap.get(categoryNodeId);
    while (current) {
        // Detect cycles in the category graph to avoid infinite loops.
        if (visitedIds.has(current.id)) {
            break;
        }
        visitedIds.add(current.id);
        path.unshift(current);
        current = current.parentId ? nodeMap.get(current.parentId) : undefined;
    }
    return path;
}

function getCategoryNodeName(node: CategoryNodeDto, language: string): string {
    return language === 'fr' ? node.name_fr : node.name_en;
}

function Product({ isAuthenticated = false, onLogout }: ProductProps) {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [cartItemsCount] = useState<number>(0);

    const [product, setProduct] = useState<GetItemResponse | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    // Category nodes for breadcrumb path
    const [categoryNodes, setCategoryNodes] = useState<CategoryNodeDto[]>([]);

    // Selected variant – keys are always attributeName_en, values are always attributes_en
    const [selectedAttributes, setSelectedAttributes] = useState<Record<string, string>>({});
    const [selectedVariant, setSelectedVariant] = useState<ItemVariantDto | null>(null);

    // Image gallery
    const [variantImages, setVariantImages] = useState<string[]>([]);
    const [mainImageIndex, setMainImageIndex] = useState<number>(0);
    const [mainImageError, setMainImageError] = useState<boolean>(false);

    const getText = (en: string, fr: string) => language === 'fr' ? fr : en;

    useEffect(() => {
        const browserLang = navigator.language.toLowerCase();
        setLanguage(browserLang.includes('fr') ? 'fr' : 'en');
        fetchCategoryNodes();
    }, []);

    useEffect(() => {
        if (!id) {
            setError(getText('Product ID is missing.', 'Identifiant de produit manquant.'));
            setLoading(false);
            return;
        }
        fetchProduct(id);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [id]);

    const fetchProduct = async (productId: string) => {
        try {
            setLoading(true);
            setError(null);
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                setError(getText('API not configured.', 'API non configurée.'));
                return;
            }
            const response = await fetch(`${apiBaseUrl}/api/Item/GetItemById/${productId}`);
            if (!response.ok) {
                setError(getText('Product not found.', 'Produit introuvable.'));
                return;
            }
            const result: ApiResult<GetItemResponse> = await response.json();
            if (result.isSuccess && result.value) {
                setProduct(result.value);
                initializeVariantSelection(result.value);
            } else {
                setError(getText('Product not found.', 'Produit introuvable.'));
            }
        } catch (err) {
            console.error('Error fetching product:', err);
            setError(getText('Failed to load product.', 'Impossible de charger le produit.'));
        } finally {
            setLoading(false);
        }
    };

    const fetchCategoryNodes = async () => {
        try {
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('VITE_API_STORE_BASE_URL is not set; skipping category nodes fetch and breadcrumb construction.');
                return;
            }
            const response = await fetch(`${apiBaseUrl}/api/CategoryNode/GetAllCategoryNodes`);
            if (!response.ok) {
                console.warn(`Failed to fetch category nodes: ${response.status} ${response.statusText}`);
                return;
            }
            const result: ApiResult<CategoryNodeDto[]> = await response.json();
            if (result.isSuccess && result.value) {
                setCategoryNodes(result.value);
            }
        } catch (err) {
            console.error('Error fetching category nodes:', err);
        }
    };

    /**
     * Initializes variant selection from the fetched product.
     * Selects the first available (non-deleted) variant using language-invariant keys.
     */
    const initializeVariantSelection = (p: GetItemResponse) => {
        const activeVariants = p.variants.filter((v) => !v.deleted);
        if (activeVariants.length === 0) {
            setSelectedVariant(null);
            setVariantImages([]);
            return;
        }

        const firstVariant = activeVariants[0];

        // Use language-invariant EN keys/values for selectedAttributes
        const initialAttrs: Record<string, string> = {};
        for (const attr of firstVariant.itemVariantAttributes) {
            initialAttrs[attr.attributeName_en] = attr.attributes_en;
        }

        setSelectedAttributes(initialAttrs);
        setSelectedVariant(firstVariant);
        const images = parseImageUrls(firstVariant.imageUrls, firstVariant.thumbnailUrl);
        setVariantImages(images);
        setMainImageIndex(0);
        setMainImageError(false);
    };

    // Whenever selectedAttributes or product change, find the matching variant
    useEffect(() => {
        if (!product) return;
        const variant = findMatchingVariant(product.variants, selectedAttributes);
        setSelectedVariant(variant);
        if (variant) {
            const images = parseImageUrls(variant.imageUrls, variant.thumbnailUrl);
            setVariantImages(images);
            setMainImageIndex(0);
            setMainImageError(false);
        } else {
            setVariantImages([]);
            setMainImageIndex(0);
            setMainImageError(false);
        }
    }, [selectedAttributes, product]);

    const handleAttributeSelect = (nameKey: string, valueKey: string) => {
        setSelectedAttributes((prev) => ({ ...prev, [nameKey]: valueKey }));
    };

    const handleThumbnailClick = (index: number) => {
        setMainImageIndex(index);
        setMainImageError(false);
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

    const productName = product
        ? (language === 'fr' ? product.name_fr : product.name_en)
        : '';
    const productDescription = product
        ? (language === 'fr' ? product.description_fr : product.description_en)
        : '';

    const categoryNodeMap = useMemo(() => {
        const map = new Map<string, CategoryNodeDto>();
        for (const node of categoryNodes) {
            map.set(node.id, node);
        }
        return map;
    }, [categoryNodes]);

    const categoryPath = useMemo(
        () => product ? buildCategoryPath(categoryNodeMap, product.categoryNodeID) : [],
        [categoryNodeMap, product]
    );

    const offerActive = selectedVariant ? isOfferActive(selectedVariant) : false;
    const discountedPrice = offerActive && selectedVariant
        ? selectedVariant.price * (1 - (selectedVariant.offer ?? 0) / 100)
        : null;

    // Build localized attribute groups for rendering the variant selector
    const attributeGroups = useMemo(
        () => product ? buildAttributeGroups(product.variants, language) : [],
        [product, language]
    );

    // Compute which option buttons should appear out-of-stock (greyed out / disabled)
    const outOfStockOptions = useMemo(
        () =>
            product
                ? computeOutOfStockOptions(product.variants, attributeGroups, selectedAttributes)
                : new Set<string>(),
        [product, attributeGroups, selectedAttributes]
    );

    // Compute the price map for the last attribute group's options.
    // Each entry maps an option valueKey to the matching variant given the current
    // selections for all other groups combined with that option.
    const lastGroupPriceMap = useMemo(() => {
        if (attributeGroups.length === 0 || !product) return new Map<string, ItemVariantDto | null>();
        const lastGrp = attributeGroups[attributeGroups.length - 1];
        const map = new Map<string, ItemVariantDto | null>();
        for (const option of lastGrp.options) {
            const testAttrs = { ...selectedAttributes, [lastGrp.nameKey]: option.valueKey };
            const variant = findMatchingVariant(product.variants, testAttrs);
            map.set(option.valueKey, variant);
        }
        return map;
    }, [attributeGroups, product, selectedAttributes]);

    const mainImage = variantImages[mainImageIndex] ?? null;

    const hasProductAttributes = !!(selectedVariant && (selectedVariant.sku || (selectedVariant.productIdentifierType && selectedVariant.productIdentifierValue)));

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
                {loading ? (
                    <div className="product-loading" role="status">
                        <p>{getText('Loading product...', 'Chargement du produit...')}</p>
                    </div>
                ) : error ? (
                    <div className="product-error">
                        <p>{error}</p>
                        <button type="button" onClick={() => navigate(-1)}>
                            {getText('Go Back', 'Retour')}
                        </button>
                    </div>
                ) : product ? (
                    <div className="product-detail">
                        {/* Main product section */}
                        <div className="product-main">
                            {/* Product Info */}
                            <section
                                className="product-info"
                                aria-label={getText('Product information', 'Informations sur le produit')}
                            >
                                <h1 className="product-name">{productName}</h1>

                                {categoryPath.length > 0 && (
                                    <div
                                        className="product-category"
                                        role="navigation"
                                        aria-label={getText('Product category breadcrumb', 'Fil d\'Ariane de la catégorie du produit')}
                                    >
                                        {getText('Category', 'Catégorie')}:{' '}
                                        {categoryPath.map((node, idx) => (
                                            <Fragment key={node.id}>
                                                {idx > 0 && <span className="breadcrumb-sep" aria-hidden="true">›</span>}
                                                <button
                                                    type="button"
                                                    className="breadcrumb-link"
                                                    onClick={() => navigate(`/categories?nodeId=${encodeURIComponent(node.id)}`)}
                                                >
                                                    {getCategoryNodeName(node, language)}
                                                </button>
                                            </Fragment>
                                        ))}
                                    </div>
                                )}

                                {/* Price – shown here only when there are no variant attribute groups */}
                                {attributeGroups.length === 0 && (
                                    selectedVariant ? (
                                        <div className="product-price-section">
                                            {offerActive && discountedPrice !== null ? (
                                                <>
                                                    <span className="product-original-price">
                                                        ${selectedVariant.price.toFixed(2)}
                                                    </span>
                                                    <span className="product-discounted-price">
                                                        ${discountedPrice.toFixed(2)}
                                                    </span>
                                                    <span className="product-offer-badge">
                                                        {getText(
                                                            `${selectedVariant.offer}% OFF`,
                                                            `Rabais ${selectedVariant.offer}%`
                                                        )}
                                                    </span>
                                                </>
                                            ) : (
                                                <span className="product-price">
                                                    ${selectedVariant.price.toFixed(2)}
                                                </span>
                                            )}
                                        </div>
                                    ) : (
                                        <div className="product-price-section">
                                            <span className="product-unavailable">
                                                {getText(
                                                    'This combination is not available.',
                                                    'Cette combinaison n\'est pas disponible.'
                                                )}
                                            </span>
                                        </div>
                                    )
                                )}

                                {/* Variant Attribute Selectors */}
                                {attributeGroups.length > 0 && (
                                    <div className="product-variants">
                                        <h2 className="product-variants-title">
                                            {getText('Options', 'Options')}
                                        </h2>
                                        {attributeGroups.map((group, groupIndex) => {
                                            const isLastGroup = groupIndex === attributeGroups.length - 1;
                                            return (
                                                <div key={group.nameKey} className="product-attribute-group">
                                                    <p className="product-attribute-name">{group.displayName}</p>
                                                    <div
                                                        className="product-attribute-options"
                                                        role="group"
                                                        aria-label={group.displayName}
                                                    >
                                                        {group.options.map((option) => {
                                                            const isSelected = selectedAttributes[group.nameKey] === option.valueKey;
                                                            const hasThumbnail = group.isMain && !!option.thumbnailUrl;
                                                            const isOutOfStock = outOfStockOptions.has(JSON.stringify([group.nameKey, option.valueKey]));
                                                            const btn = (
                                                                <button
                                                                    key={option.valueKey}
                                                                    type="button"
                                                                    className={`product-attribute-btn${isSelected ? ' selected' : ''}${hasThumbnail ? ' with-thumbnail' : ''}${isOutOfStock ? ' out-of-stock' : ''}`}
                                                                    onClick={() => handleAttributeSelect(group.nameKey, option.valueKey)}
                                                                    onMouseEnter={() => handleAttributeSelect(group.nameKey, option.valueKey)}
                                                                    aria-pressed={isSelected}
                                                                    aria-label={isOutOfStock ? `${option.displayLabel}, ${getText('out of stock', 'rupture de stock')}` : undefined}
                                                                >
                                                                    {hasThumbnail && (
                                                                        <img
                                                                            src={option.thumbnailUrl}
                                                                            alt=""
                                                                            aria-hidden="true"
                                                                            className="product-attribute-btn-thumbnail"
                                                                        />
                                                                    )}
                                                                    <span>{option.displayLabel}</span>
                                                                </button>
                                                            );
                                                            if (!isLastGroup) {
                                                                return btn;
                                                            }
                                                            const optVariant = lastGroupPriceMap.get(option.valueKey);
                                                            const optOfferActive = optVariant ? isOfferActive(optVariant) : false;
                                                            const optEffectivePrice = optVariant
                                                                ? (optOfferActive
                                                                    ? optVariant.price * (1 - (optVariant.offer ?? 0) / 100)
                                                                    : optVariant.price)
                                                                : null;
                                                            const formattedOptEffectivePrice = optEffectivePrice !== null
                                                                ? `$${optEffectivePrice.toFixed(2)}`
                                                                : '—';
                                                            const optionPriceTestId = `product-option-price-${group.nameKey}-${option.valueKey}`;
                                                            const optionPriceAriaLabel = optEffectivePrice !== null
                                                                ? `${option.displayLabel} price ${formattedOptEffectivePrice}${optOfferActive ? ' discounted' : ''}`
                                                                : `${option.displayLabel} price unavailable`;
                                                            return (
                                                                <div key={option.valueKey} className="product-option-with-price">
                                                                    {btn}
                                                                    {optEffectivePrice !== null ? (
                                                                        <span
                                                                            className={`product-option-price${optOfferActive ? ' discounted' : ''}`}
                                                                            data-testid={optionPriceTestId}
                                                                            data-option-value={option.valueKey}
                                                                            aria-label={optionPriceAriaLabel}
                                                                        >
                                                                            {formattedOptEffectivePrice}
                                                                        </span>
                                                                    ) : (
                                                                        <span
                                                                            className="product-option-price unavailable"
                                                                            data-testid={optionPriceTestId}
                                                                            data-option-value={option.valueKey}
                                                                            aria-label={optionPriceAriaLabel}
                                                                        >
                                                                            —
                                                                        </span>
                                                                    )}
                                                                </div>
                                                            );
                                                        })}
                                                    </div>
                                                </div>
                                            );
                                        })}

                                        {/* Unavailable combination message */}
                                        {!selectedVariant && (
                                            <div className="product-price-section">
                                                <span className="product-unavailable">
                                                    {getText(
                                                        'This combination is not available.',
                                                        'Cette combinaison n\'est pas disponible.'
                                                    )}
                                                </span>
                                            </div>
                                        )}
                                    </div>
                                )}

                                {/* Stock info */}
                                {selectedVariant && (
                                    <p className={selectedVariant.stockQuantity > 5 ? 'product-stock' : 'product-stock-low'}>
                                        {selectedVariant.stockQuantity > 0
                                            ? (selectedVariant.stockQuantity <= 5
                                                ? getText(
                                                    `${selectedVariant.stockQuantity} in stock`,
                                                    `${selectedVariant.stockQuantity} en stock`
                                                )
                                                : getText('In stock', 'En stock')
                                            )
                                            : getText('Out of Stock', 'Rupture de stock')}
                                    </p>
                                )}

                                {/* Variant Features */}
                                {selectedVariant && selectedVariant.itemVariantFeatures.length > 0 && (
                                    <div className="product-variant-features">
                                        <h2 className="product-variant-features-title">
                                            {getText('Features', 'Caractéristiques')}
                                        </h2>
                                        <table className="product-variant-features-table">
                                            <tbody>
                                                {selectedVariant.itemVariantFeatures.map((feature) => (
                                                    <tr key={feature.id} className="product-variant-features-row">
                                                        <th className="product-variant-features-name" scope="row">
                                                            {language === 'fr' && feature.attributeName_fr
                                                                ? feature.attributeName_fr
                                                                : feature.attributeName_en}
                                                        </th>
                                                        <td className="product-variant-features-value">
                                                            {language === 'fr' && feature.attributes_fr
                                                                ? feature.attributes_fr
                                                                : feature.attributes_en}
                                                        </td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    </div>
                                )}

                                {/* Product Attributes */}
                                {hasProductAttributes && selectedVariant && (
                                    <div className="product-attributes">
                                        <h2 className="product-attributes-title">
                                            {getText('Product Details', 'Détails du produit')}
                                        </h2>
                                        {selectedVariant.sku && (
                                            <p className="product-attributes-row">
                                                {getText('SKU', 'UGS')}: {selectedVariant.sku}
                                            </p>
                                        )}
                                        {selectedVariant.productIdentifierType && selectedVariant.productIdentifierValue && (
                                            <p className="product-attributes-row">
                                                {selectedVariant.productIdentifierType}: {selectedVariant.productIdentifierValue}
                                            </p>
                                        )}
                                    </div>
                                )}
                            </section>

                            {/* Image Gallery */}
                            <section
                                className="product-gallery"
                                aria-label={getText('Product images', 'Images du produit')}
                            >
                                <div className="product-main-image-wrapper">
                                    {mainImage && !mainImageError ? (
                                        <img
                                            src={mainImage}
                                            alt={productName}
                                            className="product-main-image"
                                            onError={() => setMainImageError(true)}
                                        />
                                    ) : (
                                        <div className="product-main-image-placeholder">
                                            {getText('No image available', 'Image non disponible')}
                                        </div>
                                    )}
                                </div>

                                {/* Thumbnails */}
                                {variantImages.length > 0 && (
                                    <ul
                                        className="product-thumbnails"
                                        aria-label={getText('Image thumbnails', 'Miniatures d\'images')}
                                    >
                                        {variantImages.map((imgUrl, idx) => (
                                            <li key={idx}>
                                                <button
                                                    type="button"
                                                    className={`product-thumbnail-btn${mainImageIndex === idx ? ' active' : ''}`}
                                                    onClick={() => handleThumbnailClick(idx)}
                                                    onMouseEnter={() => handleThumbnailClick(idx)}
                                                    aria-label={getText(`View image ${idx + 1}`, `Voir l'image ${idx + 1}`)}
                                                    aria-pressed={mainImageIndex === idx}
                                                >
                                                    <img
                                                        src={imgUrl}
                                                        alt={`${productName} ${idx + 1}`}
                                                        className="product-thumbnail-img"
                                                    />
                                                </button>
                                            </li>
                                        ))}
                                    </ul>
                                )}

                                {/* Description */}
                                {productDescription && (
                                    <div className="product-description">
                                        <h2 className="product-description-title">
                                            {getText('Description', 'Description')}
                                        </h2>
                                        <p className="product-description-text">{productDescription}</p>
                                    </div>
                                )}
                            </section>
                        </div>
                    </div>
                ) : null}
            </div>

            {/* Store Footer */}
            <footer className="store-footer">
                <p>
                    {getText(
                        '© 2025 CanoEh! All rights reserved.',
                        '© 2025 CanoEh! Tous droits réservés.'
                    )}
                </p>
            </footer>
        </div>
    );
}

export default Product;

