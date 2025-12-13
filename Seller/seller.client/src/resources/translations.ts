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
  'products.editProduct': {
    en: 'Edit Product',
    fr: 'Modifier le produit'
  },
  'products.edit': {
    en: 'Edit',
    fr: 'Modifier'
  },
  'products.updateItem': {
    en: 'Update Item',
    fr: 'Mettre à jour l\'article'
  },
  'products.updating': {
    en: 'Updating...',
    fr: 'Mise à jour...'
  },
  'products.delete': {
    en: 'Delete',
    fr: 'Supprimer'
  },
  'products.deleteConfirm': {
    en: 'Are you sure you want to delete this item? This item will be marked as deleted and can be restored later.',
    fr: 'Êtes-vous sûr de vouloir supprimer cet article ? Cet article sera marqué comme supprimé et pourra être restauré ultérieurement.'
  },
  'products.deleteSuccess': {
    en: 'Item deleted successfully',
    fr: 'Article supprimé avec succès'
  },
  'products.deleteError': {
    en: 'Failed to delete item',
    fr: 'Échec de la suppression de l\'article'
  },
  'products.actions': {
    en: 'Actions',
    fr: 'Actions'
  },
  'products.alreadyDeleted': {
    en: 'Item already deleted',
    fr: 'Article déjà supprimé'
  },
  'products.deleted': {
    en: 'Deleted',
    fr: 'Supprimé'
  },
  'products.undelete': {
    en: 'Undelete',
    fr: 'Restaurer'
  },
  'products.undeleteConfirm': {
    en: 'Are you sure you want to restore this item?',
    fr: 'Êtes-vous sûr de vouloir restaurer cet article?'
  },
  'products.undeleteSuccess': {
    en: 'Item restored successfully',
    fr: 'Article restauré avec succès'
  },
  'products.undeleteError': {
    en: 'Failed to restore item',
    fr: 'Échec de la restauration de l\'article'
  },

  // Product Form Labels
  'products.itemName': {
    en: 'Item Name (English)',
    fr: 'Nom de l\'article (anglais)'
  },
  'products.itemNameFr': {
    en: 'Item Name (French)',
    fr: 'Nom de l’article (français)'
  },
  'products.description': {
    en: 'Description (English)',
    fr: 'Description (anglais)'
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
  'products.saving': {
    en: 'Saving...',
    fr: 'Enregistrement...'
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

  // Product Identifier Fields
  'products.productIdentifierType': {
    en: 'Product ID Type',
    fr: 'Type d\'ID produit'
  },
  'products.productIdentifierValue': {
    en: 'Product ID Value',
    fr: 'Valeur d\'ID produit'
  },
  'products.thumbnailImage': {
    en: 'Thumbnail Image',
    fr: 'Image miniature'
  },
  'products.productImages': {
    en: 'Product Images',
    fr: 'Images du produit'
  },
  'products.chooseThumbnail': {
    en: 'Choose Thumbnail',
    fr: 'Choisir miniature'
  },
  'products.chooseImages': {
    en: 'Choose Images (1-10)',
    fr: 'Choisir images (1-10)'
  },
  'products.selectIdType': {
    en: 'Select ID Type',
    fr: 'Sélectionner type d\'ID'
  },

  // Form Placeholders
  'placeholder.itemName': {
    en: 'Enter item name in English',
    fr: 'Entrer le nom de l\'article en anglais'
  },
  'placeholder.itemNameFr': {
    en: 'Enter item name in French',
    fr: 'Entrer le nom de l\'article en français'
  },
  'placeholder.description': {
    en: 'Enter item description in English',
    fr: 'Entrer la description de l\'article en anglais'
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

  // Product List Section
  'products.list.itemName': {
    en: 'Item Name',
    fr: 'Nom de l\'article'
  },
  'products.list.itemCategory': {
    en: 'Item Category',
    fr: 'Catégorie de l\'article'
  },
  'products.list.creationDate': {
    en: 'Creation date',
    fr: 'Date de création'
  },
  'products.list.lastUpdated': {
    en: 'Last updated',
    fr: 'Dernière mise à jour'
  },
  'products.list.noItems': {
    en: 'No items found. Click "Add Product" to create your first product.',
    fr: 'Aucun article trouvé. Cliquez sur "Ajouter un produit" pour créer votre premier produit.'
  },
  'products.list.loading': {
    en: 'Loading items...',
    fr: 'Chargement des articles...'
  },
  'products.list.error': {
    en: 'Failed to load items. Please try again.',
    fr: 'Échec du chargement des articles. Veuillez réessayer.'
  },
  'products.list.currentItems': {
    en: 'Current Items',
    fr: 'Articles actuels'
  },
  'products.list.noVariants': {
    en: 'No variants available',
    fr: 'Aucune variante disponible'
  },
  'products.error.noSellerId': {
    en: 'Unable to determine seller ID.',
    fr: 'Impossible de déterminer l\'ID du vendeur.'
  },

  // Item Variants List
  'products.variant.name': {
    en: 'Variant Name',
    fr: 'Nom de la variante'
  },
  'products.variant.price': {
    en: 'Price',
    fr: 'Prix'
  },
  'products.variant.stockQty': {
    en: 'Stock Qty',
    fr: 'Qté en stock'
  },
  'products.variant.sku': {
    en: 'SKU',
    fr: 'SKU'
  },
  'products.variant.productIdType': {
    en: 'Product ID Type',
    fr: 'Type d\'ID produit'
  },
  'products.variant.productIdValue': {
    en: 'Product ID Value',
    fr: 'Valeur d\'ID produit'
  },

  // Product Filter Section
  'products.filter.title': {
    en: 'Filter Products',
    fr: 'Filtrer les produits'
  },
  'products.filter.itemName': {
    en: 'Item Name',
    fr: 'Nom de l\'article'
  },
  'products.filter.itemNamePlaceholder': {
    en: 'Search by item name...',
    fr: 'Rechercher par nom d\'article...'
  },
  'products.filter.category': {
    en: 'Category',
    fr: 'Catégorie'
  },
  'products.filter.allCategories': {
    en: 'All Categories',
    fr: 'Toutes les catégories'
  },
  'products.filter.variantName': {
    en: 'Variant Name',
    fr: 'Nom de variante'
  },
  'products.filter.variantNamePlaceholder': {
    en: 'Search by variant name...',
    fr: 'Rechercher par nom de variante...'
  },
  'products.filter.sku': {
    en: 'SKU',
    fr: 'SKU'
  },
  'products.filter.skuPlaceholder': {
    en: 'Search by SKU...',
    fr: 'Rechercher par SKU...'
  },
  'products.filter.productIdType': {
    en: 'Product ID Type',
    fr: 'Type d\'ID produit'
  },
  'products.filter.allIdTypes': {
    en: 'All ID Types',
    fr: 'Tous les types d\'ID'
  },
  'products.filter.productIdValue': {
    en: 'Product ID Value',
    fr: 'Valeur d\'ID produit'
  },
  'products.filter.productIdValuePlaceholder': {
    en: 'Search by product ID...',
    fr: 'Rechercher par ID produit...'
  },
  'products.filter.clearFilters': {
    en: 'Clear Filters',
    fr: 'Effacer les filtres'
  },
  'products.filter.clear': {
    en: 'Clear',
    fr: 'Effacer'
  },
  'products.filter.clearItemName': {
    en: 'Clear item name filter',
    fr: 'Effacer le filtre de nom d\'article'
  },
  'products.filter.clearVariantName': {
    en: 'Clear variant name filter',
    fr: 'Effacer le filtre de nom de variante'
  },
  'products.filter.clearSku': {
    en: 'Clear SKU filter',
    fr: 'Effacer le filtre SKU'
  },
  'products.filter.clearProductIdValue': {
    en: 'Clear product ID value filter',
    fr: 'Effacer le filtre de valeur d\'ID produit'
  },
  'products.filter.showDeleted': {
    en: 'Show Deleted',
    fr: 'Afficher les supprimés'
  },

  // Product Sort Section
  'products.sort.title': {
    en: 'Sort Options',
    fr: 'Options de tri'
  },
  'products.sort.orderBy': {
    en: 'Order By',
    fr: 'Trier par'
  },
  'products.sort.itemName': {
    en: 'Item Name',
    fr: 'Nom de l\'article'
  },
  'products.sort.itemCategory': {
    en: 'Item Category',
    fr: 'Catégorie de l\'article'
  },
  'products.sort.creationDate': {
    en: 'Creation Date',
    fr: 'Date de création'
  },
  'products.sort.lastUpdated': {
    en: 'Last Updated',
    fr: 'Dernière mise à jour'
  },
  'products.sort.direction': {
    en: 'Direction',
    fr: 'Direction'
  },
  'products.sort.ascending': {
    en: 'Ascending',
    fr: 'Croissant'
  },
  'products.sort.descending': {
    en: 'Descending',
    fr: 'Décroissant'
  },

  // Pagination
  'pagination.previous': {
    en: 'Previous',
    fr: 'Précédent'
  },
  'pagination.next': {
    en: 'Next',
    fr: 'Suivant'
  },
  'pagination.page': {
    en: 'Page',
    fr: 'Page'
  },
  'pagination.of': {
    en: 'of',
    fr: 'sur'
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