import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';

interface HomeProps {
    isAuthenticated?: boolean;
}

const ITEM_PLACEHOLDER_ARRAY = [1, 2, 3, 4];

function Home({ isAuthenticated = false }: HomeProps) {
    const navigate = useNavigate();
    const [language, setLanguage] = useState<string>('en');
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [userPostalCode, setUserPostalCode] = useState<string>('');
    const [cartItemsCount, setCartItemsCount] = useState<number>(0);

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
    }, [isAuthenticated]);

    const handleConnectClick = () => {
        navigate('/login');
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
                    <input
                        type="text"
                        placeholder={getText("Search items...", "Rechercher des articles...")}
                        aria-label={getText("Search for items", "Rechercher des articles")}
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                    <button
                        type="submit"
                        className="search-icon"
                        aria-label={getText("Search", "Rechercher")}
                    >
                        🔍
                    </button>
                </form>
                <div className="nav-item language-selector">
                    <select value={language} onChange={(e) => setLanguage(e.target.value)} aria-label={getText("Select language", "Sélectionner la langue")}>
                        <option value="en">English</option>
                        <option value="fr">Français</option>
                    </select>
                </div>
                <div className="nav-item connect-button">
                    <button onClick={handleConnectClick}>
                        {isAuthenticated ? getText('Account', 'Compte') : getText('Connect', 'Connexion')}
                    </button>
                </div>
                <button
                    type="button"
                    className="nav-item cart-button"
                    onClick={handleCartClick}
                    aria-label={getText("Shopping cart", "Panier d'achat")}
                >
                    <div className="cart-icon-container">
                        <span className="cart-count">{cartItemsCount}</span>
                        <span className="nav-cart-icon"></span>
                    </div>
                    <div className="cart-text-container">
                        <span className="nav-cart-line1"></span>
                        <span className="nav-cart-line2">
                            {getText("Cart", "Panier")}
                            <span></span>
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
    onClick?: () => void;
}

function ItemPreviewCard({ title, items, onClick }: ItemPreviewCardProps) {
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
                {items.map((item) => (
                    <div key={item} className="item-placeholder">
                        <div className="item-image-placeholder">Item {item}</div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default Home;
