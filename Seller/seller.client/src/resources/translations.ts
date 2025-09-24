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
    en: 'Item Name (English)',
    fr: 'Nom de l’article'
  },
  'products.itemNameFr': {
    en: 'Item Name (French)',
    fr: 'Nom de l’article (français)'
  },
  'products.description': {
    en: 'Description (English)',
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
    en: 'Item Variants',
    fr: 'Variantes de l\'article'
  },
  'products.attributeName': {
    en: 'Variant Attribute Name (English)',
    fr: 'Nom de l\'attribut de variante (anglais)'
  },
  'products.attributeNameFrVariant': {
    en: 'Variant Attribute Name (French)',
    fr: 'Nom de l\'attribut de variante (français)'
  },
  'products.attributeValues': {
    en: 'Variant Attribute Value (English)',
    fr: 'Valeur d\'attribut de variante (anglais)'
  },
  'products.attributeValuesFr': {
    en: 'Variant Attribute Value (French)',
    fr: 'Valeur d\'attribut de variante (français)'
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

  // Item Attributes Section
  'products.itemAttributesTitle': {
    en: 'Item Attributes',
    fr: 'Attributs de l\'article'
  },
  'products.attributeNameEn': {
    en: 'Attribute Name (English)',
    fr: 'Nom d\'attribut (anglais)'
  },
  'products.attributeNameFr': {
    en: 'Attribute Name (French)',
    fr: 'Nom d\'attribut (français)'
  },
  'products.attributeValueEn': {
    en: 'Attribute Value (English)',
    fr: 'Valeur d\'attribut (anglais)'
  },
  'products.attributeValueFr': {
    en: 'Attribute Value (French)',
    fr: 'Valeur d\'attribut (français)'
  },
  'products.addNewAttribute': {
    en: 'Add Another Attribute',
    fr: 'Ajouter un autre attribut'
  },
  'products.removeAttribute': {
    en: 'Remove',
    fr: 'Supprimer'
  },

  // Form Placeholders
  'placeholder.itemName': {
    en: 'Enter item name in English',
    fr: 'Entrer le nom de l\'article'
  },
  'placeholder.itemNameFr': {
    en: 'Enter item name in French',
    fr: 'Entrer le nom de l\'article en français'
  },
  'placeholder.description': {
    en: 'Enter item description in English',
    fr: 'Entrer la description de l\'article'
  },
  'placeholder.descriptionFr': {
    en: 'Enter item description in French',
    fr: 'Entrer la description de l\'article en français'
  },
  'placeholder.attributeName': {
    en: 'Enter variant attribute name in English',
    fr: 'Entrer le nom de l\'attribut de variante en anglais'
  },
  'placeholder.attributeNameFrVariant': {
    en: 'Enter variant attribute name in French',
    fr: 'Entrer le nom de l\'attribut de variante en français'
  },
  'placeholder.attributeValue': {
    en: 'Enter value in English',
    fr: 'Entrer la valeur en anglais'
  },
  'placeholder.attributeValueFrVariant': {
    en: 'Enter value in French',
    fr: 'Entrer la valeur en français'
  },
  'placeholder.attributeNameEn': {
    en: 'e.g., Brand',
    fr: 'ex: Marque'
  },
  'placeholder.attributeNameFr': {
    en: 'e.g., Marque',
    fr: 'ex: Brand'
  },
  'placeholder.attributeValueEn': {
    en: 'e.g., Brand Name',
    fr: 'ex: Nom de la marque'
  },
  'placeholder.attributeValueFr': {
    en: 'e.g., Nom de la marque',
    fr: 'ex: Brand Name'
  },

  // Common
  'common.unknown': {
    en: 'Unknown',
    fr: 'Inconnu'
  },

  // Error Messages
  'error.bilingualValuesMismatch': {
    en: 'Please ensure both English and French values are provided and have the same number of non-empty entries.',
    fr: 'Veuillez vous assurer que les valeurs anglaises et françaises sont fournies et qu\'elles ont le même nombre d\'entrées non vides.'
  },
  'error.bilingualNamesMissing': {
    en: 'Please ensure both English and French names are provided.',
    fr: 'Veuillez vous assurer que les noms anglais et français sont fournis.'
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