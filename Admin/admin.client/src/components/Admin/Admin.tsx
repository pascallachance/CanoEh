import { useState } from 'react';
import './Admin.css';
import CategoryNodesSection from './CategoryNodesSection';
import { useLanguage } from '../../contexts/LanguageContext';

type AdminSection = 'categories';

function Admin({ onLogout }: { onLogout: () => void }) {
    const [activeSection, setActiveSection] = useState<AdminSection>('categories');
    const { language, setLanguage, t } = useLanguage();

    const renderContent = () => {
        switch (activeSection) {
            case 'categories':
                return <CategoryNodesSection />;
            default:
                return <CategoryNodesSection />;
        }
    };

    return (
        <div className="admin-container">
            <nav className="admin-nav">
                <div className="nav-logo">
                    <h1 className="nav-brand">{t('nav.brand')}</h1>
                </div>
                <div className="nav-tabs">
                    <button
                        className={`nav-tab ${activeSection === 'categories' ? 'active' : ''}`}
                        onClick={() => setActiveSection('categories')}
                    >
                        {t('nav.categories')}
                    </button>
                </div>
                <div className="nav-buttons">
                    <select
                        className="drop-down-language"
                        value={language}
                        onChange={(e) => setLanguage(e.target.value as 'en' | 'fr')}
                        title={t('nav.language')}
                    >
                        <option value="en">English</option>
                        <option value="fr">Français</option>
                    </select>
                    <button onClick={onLogout}>{t('nav.logout')}</button>
                </div>
            </nav>

            <main className="admin-content">
                {renderContent()}
            </main>

            <footer className="admin-footer">
                <p>{t('footer.copyright')}</p>
            </footer>
        </div>
    );
}

export default Admin;
