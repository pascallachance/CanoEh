import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Home.css';

interface HomeProps {
    isAuthenticated?: boolean;
}

function Home({ isAuthenticated = false }: HomeProps) {
    const navigate = useNavigate();
    const [userAddress] = useState<string>('');
    const [language, setLanguage] = useState<string>('en');

    useEffect(() => {
        // Set language based on user or system settings
        const browserLang = navigator.language.toLowerCase();
        if (!isAuthenticated) {
            setLanguage(browserLang.includes('fr') ? 'fr' : 'en');
        }
    }, [isAuthenticated]);

    const handleConnectClick = () => {
        navigate('/login');
    };

    return (
        <div className="home-container">
            {/* Top Navigation Bar */}
            <nav className="top-nav">
                <div className="nav-item logo">CanoEh.ca</div>
                <div className="nav-item address">
                    <span className="location-icon">📍</span>
                    <span>{isAuthenticated && userAddress ? userAddress : 'Update Location'}</span>
                </div>
                <div className="nav-item search-bar">
                    <input type="text" placeholder="Search items..." />
                    <span className="search-icon">🔍</span>
                </div>
                <div className="nav-item language-selector">
                    <select value={language} onChange={(e) => setLanguage(e.target.value)}>
                        <option value="en">English</option>
                        <option value="fr">Français</option>
                    </select>
                </div>
                <div className="nav-item connect-button">
                    <button onClick={handleConnectClick}>
                        {isAuthenticated ? 'Account' : 'Connect'}
                    </button>
                </div>
            </nav>

            {/* Bottom Navigation Bar */}
            <nav className="bottom-nav">
                <div className="nav-item">Shop by Category</div>
                <div className="nav-item">Shop by Province</div>
                <div className="nav-item">Best Sellers</div>
                <div className="nav-item">Shop Offers</div>
                <div className="nav-item">New Products</div>
                {isAuthenticated && (
                    <>
                        <div className="nav-item">Local Products</div>
                        <div className="nav-item">Gift Ideas</div>
                        <div className="nav-item">Browsing History</div>
                        <div className="nav-item">Customer Service</div>
                    </>
                )}
            </nav>

            {/* Page Content Section - Banner */}
            <section className="banner-section">
                <div className="banner-widget">
                    <h2>Welcome to CanoEh.ca</h2>
                    <p>Discover amazing deals and products from across Canada!</p>
                </div>
            </section>

            {/* Cards Section */}
            <section className="cards-section">
                <ItemPreviewCard title="Suggested items" items={[1, 2, 3, 4]} />
                <ItemPreviewCard title="Offers" items={[1, 2, 3, 4]} />
                <ItemPreviewCard title="Explore Categories" items={[1, 2, 3, 4]} />
                <ItemPreviewCard title="Best Sellers" items={[1, 2, 3, 4]} />
                <ItemPreviewCard title="Best Rated" items={[1, 2, 3, 4]} />
                {isAuthenticated && (
                    <>
                        <ItemPreviewCard title="Last Viewed Items" items={[1, 2, 3, 4]} />
                        <ItemPreviewCard title="Local Products" items={[1, 2, 3, 4]} />
                        <ItemPreviewCard title="Buy Again" items={[1, 2, 3, 4]} />
                    </>
                )}
            </section>
        </div>
    );
}

interface ItemPreviewCardProps {
    title: string;
    items: number[];
}

function ItemPreviewCard({ title, items }: ItemPreviewCardProps) {
    return (
        <div className="item-preview-card">
            <h3 className="card-title">{title}</h3>
            <div className="items-grid">
                {items.map((item, index) => (
                    <div key={index} className="item-placeholder">
                        <div className="item-image-placeholder">Item {item}</div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default Home;
