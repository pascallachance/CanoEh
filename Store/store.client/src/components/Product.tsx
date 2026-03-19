import { useState, useEffect } from 'react';
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
    itemVariantFeatures?: ItemVariantFeaturesDto[];
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

/**
 * An attribute group for rendering variant options.
 * nameKey / option.valueKey are language-invariant (always EN).
 * displayName / option.displayLabel are localized.
 */
interface AttributeGroup {
    nameKey: string;
    displayName: string;
    options: { valueKey: string; displayLabel: string }[];
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
                groupMap.set(nameKey, { nameKey, displayName, options: [] });
            }
            const group = groupMap.get(nameKey)!;
            group.displayName = displayName; // update in case language changed
            if (!group.options.some((o) => o.valueKey === valueKey)) {
                group.options.push({ valueKey, displayLabel });
            } else {
                const opt = group.options.find((o) => o.valueKey === valueKey)!;
                opt.displayLabel = displayLabel; // update localized label
            }
        }
    }
    return Array.from(groupMap.values());
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
                (a) => a.attributeName_en === nameKey && a.attributes_en === valueKey
            )
        );
        if (matches) return variant;
    }
    return null;
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
    const categoryName = product
        ? (language === 'fr' ? product.categoryName_fr : product.categoryName_en)
        : '';

    const offerActive = selectedVariant ? isOfferActive(selectedVariant) : false;
    const discountedPrice = offerActive && selectedVariant
        ? selectedVariant.price * (1 - (selectedVariant.offer ?? 0) / 100)
        : null;

    // Build localized attribute groups for rendering the variant selector
    const attributeGroups = product
        ? buildAttributeGroups(product.variants, language)
        : [];

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
                        {/* Breadcrumb */}
                        <nav className="product-breadcrumb" aria-label={getText('Breadcrumb', 'Fil d\'Ariane')}>
                            <button
                                type="button"
                                className="breadcrumb-link"
                                onClick={() => navigate('/')}
                            >
                                {getText('Home', 'Accueil')}
                            </button>
                            {categoryName && (
                                <>
                                    <span className="breadcrumb-sep" aria-hidden="true">›</span>
                                    <span className="breadcrumb-current">{categoryName}</span>
                                </>
                            )}
                            <span className="breadcrumb-sep" aria-hidden="true">›</span>
                            <span className="breadcrumb-current">{productName}</span>
                        </nav>

                        {/* Main product section */}
                        <div className="product-main">
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
                                {variantImages.length > 1 && (
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
                            </section>

                            {/* Product Info */}
                            <section
                                className="product-info"
                                aria-label={getText('Product information', 'Informations sur le produit')}
                            >
                                <h1 className="product-name">{productName}</h1>

                                {categoryName && (
                                    <p className="product-category">
                                        {getText('Category', 'Catégorie')}: {categoryName}
                                    </p>
                                )}

                                {/* Price */}
                                {selectedVariant ? (
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
                                )}

                                {/* Variant Attribute Selectors */}
                                {attributeGroups.length > 0 && (
                                    <div className="product-variants">
                                        <h2 className="product-variants-title">
                                            {getText('Options', 'Options')}
                                        </h2>
                                        {attributeGroups.map((group) => (
                                            <div key={group.nameKey} className="product-attribute-group">
                                                <p className="product-attribute-name">{group.displayName}</p>
                                                <div
                                                    className="product-attribute-options"
                                                    role="group"
                                                    aria-label={group.displayName}
                                                >
                                                    {group.options.map((option) => (
                                                        <button
                                                            key={option.valueKey}
                                                            type="button"
                                                            className={`product-attribute-btn${selectedAttributes[group.nameKey] === option.valueKey ? ' selected' : ''}`}
                                                            onClick={() => handleAttributeSelect(group.nameKey, option.valueKey)}
                                                            aria-pressed={selectedAttributes[group.nameKey] === option.valueKey}
                                                        >
                                                            {option.displayLabel}
                                                        </button>
                                                    ))}
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                )}

                                {/* Stock info */}
                                {selectedVariant && (
                                    <p className="product-stock">
                                        {selectedVariant.stockQuantity > 0
                                            ? getText(
                                                `${selectedVariant.stockQuantity} in stock`,
                                                `${selectedVariant.stockQuantity} en stock`
                                            )
                                            : getText('Out of stock', 'Rupture de stock')}
                                    </p>
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

                                {/* Variant Features */}
                                {selectedVariant?.itemVariantFeatures && selectedVariant.itemVariantFeatures.length > 0 && (
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

