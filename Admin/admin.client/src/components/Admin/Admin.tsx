import { useState, useRef } from 'react';
import './Admin.css';
import CategoryNodesSection from './CategoryNodesSection';
import type { CategoryNodesSectionRef } from './CategoryNodesSection';
import { useLanguage } from '../../contexts/LanguageContext';

type AdminSection = 'categories';

function Admin({ onLogout }: { onLogout: () => void }) {
    const [activeSection, setActiveSection] = useState<AdminSection>('categories');
    const { language, setLanguage, t } = useLanguage();
    const categoriesSectionRef = useRef<CategoryNodesSectionRef>(null);

    const renderContent = () => {
        return <CategoryNodesSection ref={categoriesSectionRef} />;
    };

    const renderActions = () => {
        switch (activeSection) {
            case 'categories':
                return (
                    <div className="action-buttons">
                        <button
                            className="action-button"
                            onClick={() => categoriesSectionRef.current?.openCreateModal()}
                        >
                            + {t('categories.addNode')}
                        </button>
                    </div>
                );
            default:
                return null;
        }
    };

    const actions = renderActions();

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

            {actions && (
                <div className="admin-content-actions">
                    {actions}
                </div>
            )}

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
