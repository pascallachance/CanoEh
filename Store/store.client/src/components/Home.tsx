import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';

interface HomeProps {
    isAuthenticated?: boolean;
    onLogout?: () => void;
}

const ITEM_PLACEHOLDER_ARRAY = [1, 2, 3, 4];

function Home({ isAuthenticated = false, onLogout }: HomeProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [userPostalCode, setUserPostalCode] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);
    const [recentProductImages, setRecentProductImages] = useState<string[]>([]);

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

        // Fetch recently added products
        fetchRecentlyAddedProducts();
    }, [isAuthenticated]);

    const fetchRecentlyAddedProducts = async () => {
        try {
            const apiBaseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
            if (!apiBaseUrl) {
                console.warn('API base URL not configured');
                return;
            }

            const response = await fetch(`${apiBaseUrl}/api/Item/GetRecentlyAddedProducts?count=100`);
            if (!response.ok) {
                console.error('Failed to fetch recently added products');
                return;
            }

            const result = await response.json();
            if (result.isSuccess && result.value) {
                // Extract first image from first variant of the 4 most recent items
                const images: string[] = [];
                for (let i = 0; i < Math.min(4, result.value.length); i++) {
                    const product = result.value[i];
                    if (product.variants && product.variants.length > 0) {
                        const firstVariant = product.variants[0];
                        // Try to get first image from ImageUrls or fall back to ThumbnailUrl
                        if (firstVariant.imageUrls) {
                            const urls = firstVariant.imageUrls.split(',').filter((url: string) => url.trim());
                            if (urls.length > 0) {
                                images.push(urls[0].trim());
                                continue;
                            }
                        }
                        if (firstVariant.thumbnailUrl) {
                            images.push(firstVariant.thumbnailUrl);
                            continue;
                        }
                    }
                    // If no image found, add placeholder
                    images.push('');
                }
                setRecentProductImages(images);
            }
        } catch (error) {
            console.error('Error fetching recently added products:', error);
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
                    items={ITEM_PLACEHOLDER_ARRAY}
                    onClick={() => handleCardClick('suggested')}
                />
                <ItemPreviewCard
                    title={getText("Offers", "Offres")}
                    items={ITEM_PLACEHOLDER_ARRAY}
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
                    items={ITEM_PLACEHOLDER_ARRAY}
                    imageUrls={recentProductImages}
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
    onClick?: () => void;
}

function ItemPreviewCard({ title, items, imageUrls, onClick }: ItemPreviewCardProps) {
    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (onClick && (e.key === 'Enter' || e.key === ' ')) {
            e.preventDefault();
            onClick();
        }
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
                {items.map((item, index) => (
                    <div key={item} className="item-placeholder">
                        {imageUrls && imageUrls[index] ? (
                            <img 
                                src={imageUrls[index]} 
                                alt={`Item ${item}`} 
                                className="item-image"
                                onError={(e) => {
                                    // Fallback to placeholder on image load error
                                    e.currentTarget.style.display = 'none';
                                    e.currentTarget.nextElementSibling?.classList.remove('hidden');
                                }}
                            />
                        ) : null}
                        <div className={imageUrls && imageUrls[index] ? 'item-image-placeholder hidden' : 'item-image-placeholder'}>
                            Item {item}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default Home;
