export interface TranslationResources {
  [key: string]: {
    en: string;
    fr: string;
  };
}

export const translations: TranslationResources = {
  // Navigation
  'nav.brand': {
    en: 'CanoEh! Admin',
    fr: 'CanoEh! Admin'
  },
  'nav.categories': {
    en: 'Categories',
    fr: 'Catégories'
  },
  'nav.logout': {
    en: 'Logout',
    fr: 'Déconnexion'
  },
  'nav.language': {
    en: 'Language',
    fr: 'Langue'
  },

  // Category Nodes
  'categories.title': {
    en: 'Category Nodes',
    fr: 'Nœuds de catégorie'
  },
  'categories.addNode': {
    en: 'Add Node',
    fr: 'Ajouter un nœud'
  },
  'categories.loading': {
    en: 'Loading category nodes...',
    fr: 'Chargement des nœuds de catégorie...'
  },
  'categories.error': {
    en: 'Failed to load category nodes. Please try again.',
    fr: 'Échec du chargement des nœuds de catégorie. Veuillez réessayer.'
  },
  'categories.empty': {
    en: 'No category nodes found. Click "Add Node" to create the first one.',
    fr: 'Aucun nœud de catégorie trouvé. Cliquez sur "Ajouter un nœud" pour en créer un.'
  },
  'categories.nodeType': {
    en: 'Node Type',
    fr: 'Type de nœud'
  },
  'categories.nodeName_en': {
    en: 'Name (English)',
    fr: 'Nom (anglais)'
  },
  'categories.nodeName_fr': {
    en: 'Name (French)',
    fr: 'Nom (français)'
  },
  'categories.parent': {
    en: 'Parent Node',
    fr: 'Nœud parent'
  },
  'categories.noParent': {
    en: 'No parent (root)',
    fr: 'Aucun parent (racine)'
  },
  'categories.sortOrder': {
    en: 'Sort Order',
    fr: 'Ordre de tri'
  },
  'categories.isActive': {
    en: 'Active',
    fr: 'Actif'
  },
  'categories.create': {
    en: 'Create Node',
    fr: 'Créer le nœud'
  },
  'categories.creating': {
    en: 'Creating...',
    fr: 'Création...'
  },
  'categories.createSuccess': {
    en: 'Category node created successfully.',
    fr: 'Nœud de catégorie créé avec succès.'
  },
  'categories.createError': {
    en: 'Failed to create category node.',
    fr: 'Échec de la création du nœud de catégorie.'
  },
  'categories.delete': {
    en: 'Delete',
    fr: 'Supprimer'
  },
  'categories.deleteConfirm': {
    en: 'Are you sure you want to delete this node? This cannot be undone.',
    fr: 'Êtes-vous sûr de vouloir supprimer ce nœud ? Cette action est irréversible.'
  },
  'categories.deleteSuccess': {
    en: 'Category node deleted successfully.',
    fr: 'Nœud de catégorie supprimé avec succès.'
  },
  'categories.deleteError': {
    en: 'Failed to delete category node.',
    fr: 'Échec de la suppression du nœud de catégorie.'
  },
  'categories.move': {
    en: 'Move',
    fr: 'Déplacer'
  },
  'categories.moveTitle': {
    en: 'Move Node',
    fr: 'Déplacer le nœud'
  },
  'categories.moveSuccess': {
    en: 'Category node moved successfully.',
    fr: 'Nœud de catégorie déplacé avec succès.'
  },
  'categories.moveError': {
    en: 'Failed to move category node.',
    fr: 'Échec du déplacement du nœud de catégorie.'
  },
  'categories.newParent': {
    en: 'New Parent Node',
    fr: 'Nouveau nœud parent'
  },
  'categories.selectParent': {
    en: 'Select parent node...',
    fr: 'Sélectionner le nœud parent...'
  },
  'categories.confirmMove': {
    en: 'Move Node',
    fr: 'Déplacer le nœud'
  },
  'categories.departement': {
    en: 'Department',
    fr: 'Département'
  },
  'categories.navigation': {
    en: 'Navigation',
    fr: 'Navigation'
  },
  'categories.category': {
    en: 'Category',
    fr: 'Catégorie'
  },

  // Common
  'common.cancel': {
    en: 'Cancel',
    fr: 'Annuler'
  },
  'common.save': {
    en: 'Save',
    fr: 'Enregistrer'
  },

  // Footer
  'footer.copyright': {
    en: '© 2024 CanoEh! Admin Platform. All rights reserved.',
    fr: '© 2024 Plateforme admin CanoEh! Tous droits réservés.'
  }
};

export type Language = 'en' | 'fr';

export const getTranslation = (key: string, language: Language): string => {
  const translation = translations[key];
  if (!translation) {
    console.warn(`Translation missing for key: ${key}`);
    return key;
  }
  return translation[language] || translation.en || key;
};
