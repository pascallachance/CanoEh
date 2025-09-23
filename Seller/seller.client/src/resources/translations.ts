// Translation resources for the Seller application
export interface TranslationResources {
  [key: string]: {
    en: string;
    fr: string;
  };
}

export const translations: TranslationResources = {
  // Navigation
  'nav.brand': {
    en: 'CanoEh! Seller',
    fr: 'CanoEh! Vendeur'
  },
  'nav.dashboard': {
    en: 'Dashboard',
    fr: 'Tableau de bord'
  },
  'nav.products': {
    en: 'Products',
    fr: 'Produits'
  },
  'nav.orders': {
    en: 'Orders',
    fr: 'Commandes'
  },
  'nav.company': {
    en: 'Company',
    fr: 'Entreprise'
  },
  'nav.logout': {
    en: 'Logout',
    fr: 'Déconnexion'
  },
  'nav.language': {
    en: 'Language',
    fr: 'Langue'
  },

  // Product Actions
  'products.listProducts': {
    en: 'List Products',
    fr: 'Liste des produits'
  },
  'products.addProduct': {
    en: 'Add Product',
    fr: 'Ajouter un produit'
  },

  // Product Form Labels
  'products.itemName': {
    en: 'Item Name',
    fr: 'Nom de l’article'
  },
  'products.itemNameFr': {
    en: 'Item Name (French)',
    fr: 'Nom de l’article (français)'
  },
  'products.description': {
    en: 'Description',
    fr: 'Description'
  },
  'products.descriptionFr': {
    en: 'Description (French)',
    fr: 'Description (français)'
  },
  'products.category': {
    en: 'Category',
    fr: 'Catégorie'
  },
  'products.selectCategory': {
    en: 'Select a category',
    fr: 'Sélectionner une catégorie'
  },
  'products.itemAttributes': {
    en: 'Item Attributes',
    fr: 'Attributs de l\'article'
  },
  'products.attributeName': {
    en: 'Attribute Name',
    fr: 'Nom de l\'attribut'
  },
  'products.attributeValues': {
    en: 'Attribute Values',
    fr: 'Valeurs d\'attribut'
  },
  'products.addValue': {
    en: 'Add Value',
    fr: 'Ajouter une valeur'
  },
  'products.addAttribute': {
    en: 'Add Attribute',
    fr: 'Ajouter un attribut'
  },
  'products.addItem': {
    en: 'Add Item',
    fr: 'Ajouter l\'article'
  },
  'products.deleteItem': {
    en: 'Delete',
    fr: 'Supprimer'
  },
  'products.attributes': {
    en: 'Attributes:',
    fr: 'Attributs :'
  },

  // Form Placeholders
  'placeholder.itemName': {
    en: 'Enter item name',
    fr: 'Entrer le nom de l\'article'
  },
  'placeholder.itemNameFr': {
    en: 'Enter item name in French',
    fr: 'Entrer le nom de l\'article en français'
  },
  'placeholder.description': {
    en: 'Enter item description',
    fr: 'Entrer la description de l\'article'
  },
  'placeholder.descriptionFr': {
    en: 'Enter item description in French',
    fr: 'Entrer la description de l\'article en français'
  },
  'placeholder.attributeName': {
    en: 'Enter attribute name',
    fr: 'Entrer le nom de l\'attribut'
  },
  'placeholder.attributeValue': {
    en: 'Enter value',
    fr: 'Entrer la valeur'
  },

  // Footer
  'footer.copyright': {
    en: '© 2024 CanoEh! Seller Platform. All rights reserved.',
    fr: '© 2024 Plateforme vendeur CanoEh! Tous droits réservés.'
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