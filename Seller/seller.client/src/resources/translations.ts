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
  'products.deleteConfirmTitle': {
    en: 'Delete confirmation',
    fr: 'Confirmation de suppression'
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
    fr: "Échec de la restauration de l'article"
  },
  'products.invalidItemId': {
    en: 'Invalid item ID',
    fr: "ID d'article invalide"
  },
  'products.itemNotDeleted': {
    en: 'Item is not deleted',
    fr: "L'article n'est pas supprimé"
  },
  'products.itemNotFound': {
    en: 'Item not found',
    fr: 'Article non trouvé'
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
  'common.cancel': {
    en: 'Cancel',
    fr: 'Annuler'
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

  // Manage Offers
  'products.manageOffers': {
    en: 'Manage Offers',
    fr: 'Gérer les offres'
  },
  'products.offers.offer': {
    en: 'Offer',
    fr: 'Offre'
  },
  'products.offers.offerStart': {
    en: 'Offer Start',
    fr: 'Début de l\'offre'
  },
  'products.offers.offerEnd': {
    en: 'Offer End',
    fr: 'Fin de l\'offre'
  },
  'products.offers.save': {
    en: 'Save Offers',
    fr: 'Enregistrer les offres'
  },
  'products.offers.clear': {
    en: 'Clear',
    fr: 'Effacer'
  },
  'products.offer.invalidRange': {
    en: 'Offer must be a number between 0 and 100',
    fr: 'L\'offre doit être un nombre entre 0 et 100'
  },
  'products.offers.clearOffer': {
    en: 'Clear offer for this variant',
    fr: 'Effacer l\'offre pour cette variante'
  },
  'products.offers.noChanges': {
    en: 'No changes to save',
    fr: 'Aucune modification à enregistrer'
  },
  'products.offers.saveSuccess': {
    en: 'Offers updated successfully',
    fr: 'Offres mises à jour avec succès'
  },
  'products.offers.saveError': {
    en: 'Failed to update offers',
    fr: 'Échec de la mise à jour des offres'
  },
  'products.offers.variantsNotFound': {
    en: 'Some products have been updated or removed. Please refresh and try again.',
    fr: 'Certains produits ont été mis à jour ou supprimés. Veuillez actualiser et réessayer.'
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
  },

  // Common actions
  'common.back': {
    en: 'Back',
    fr: 'Retour'
  },
  'common.nextStep': {
    en: 'Next Step',
    fr: 'Étape suivante'
  },
  'common.editing': {
    en: 'Editing...',
    fr: 'Modification...'
  },

  // Add/Edit product header
  'products.addNewProduct': {
    en: 'Add New Product',
    fr: 'Ajouter un nouveau produit'
  },

  // Step 1
  'step1.title': {
    en: 'Step 1: Item Name and Description',
    fr: 'Étape 1 : Nom et description de l\'article'
  },
  'step1.subtitle': {
    en: 'Provide the basic information about your product.',
    fr: 'Fournissez les informations de base sur votre produit.'
  },

  // Step 2
  'step2.title': {
    en: 'Step 2: Category, Variant Attributes and Features',
    fr: 'Étape 2 : Catégorie, attributs de variante et caractéristiques'
  },
  'step2.subtitle': {
    en: 'Select a category, define variant attributes (required), and optionally add item attributes and variant features.',
    fr: 'Sélectionnez une catégorie, définissez les attributs de variante (requis) et ajoutez éventuellement des attributs d\'article et des caractéristiques de variante.'
  },

  // Step 3
  'step3.title': {
    en: 'Step 3: Configure Variants',
    fr: 'Étape 3 : Configurer les variantes'
  },
  'step3.subtitle': {
    en: 'Fill in SKU, price, stock, and variant features for each variant.',
    fr: 'Renseignez le SKU, le prix, le stock et les caractéristiques de variante pour chaque variante.'
  },

  // Validation errors
  'error.nameEnRequired': {
    en: 'Item name (English) is required',
    fr: 'Le nom de l\'article (anglais) est requis'
  },
  'error.nameFrRequired': {
    en: 'Item name (French) is required',
    fr: 'Le nom de l\'article (français) est requis'
  },
  'error.descriptionEnRequired': {
    en: 'Description (English) is required',
    fr: 'La description (anglais) est requise'
  },
  'error.descriptionFrRequired': {
    en: 'Description (French) is required',
    fr: 'La description (français) est requise'
  },
  'error.categoryRequired': {
    en: 'Category is required',
    fr: 'La catégorie est requise'
  },
  'error.variantAttributesRequired': {
    en: 'Please add at least one variant attribute to continue.',
    fr: 'Veuillez ajouter au moins un attribut de variante pour continuer.'
  },
  'error.variantAttributesIncomplete': {
    en: 'All variant attributes must have names (EN & FR) and at least one value.',
    fr: 'Tous les attributs de variante doivent avoir des noms (EN et FR) et au moins une valeur.'
  },
  'error.variantFeaturesIncomplete': {
    en: 'All variant features must have both English and French names.',
    fr: 'Toutes les caractéristiques de variante doivent avoir des noms en anglais et en français.'
  },
  'error.invalidVariants': {
    en: 'Please ensure all variants have a SKU and price greater than 0.',
    fr: 'Veuillez vous assurer que toutes les variantes ont un SKU et un prix supérieur à 0.'
  },
  'error.sellerIdMissing': {
    en: 'Unable to determine seller ID. Please ensure you are logged in.',
    fr: 'Impossible de déterminer l\'ID du vendeur. Veuillez vous assurer que vous êtes connecté.'
  },
  'error.skuTooLong': {
    en: 'SKU cannot exceed 100 characters.',
    fr: 'Le SKU ne peut pas dépasser 100 caractères.'
  },
  'error.productIdValueTooLong': {
    en: 'Product identifier value cannot exceed 100 characters.',
    fr: 'La valeur de l\'identifiant du produit ne peut pas dépasser 100 caractères.'
  },
  'error.nameEnTooLong': {
    en: 'Item name (English) cannot exceed 300 characters.',
    fr: 'Le nom de l\'article (anglais) ne peut pas dépasser 300 caractères.'
  },
  'error.nameFrTooLong': {
    en: 'Item name (French) cannot exceed 300 characters.',
    fr: 'Le nom de l\'article (français) ne peut pas dépasser 300 caractères.'
  },
  'error.descriptionEnTooLong': {
    en: 'Description (English) cannot exceed 3000 characters.',
    fr: 'La description (anglais) ne peut pas dépasser 3000 caractères.'
  },
  'error.descriptionFrTooLong': {
    en: 'Description (French) cannot exceed 3000 characters.',
    fr: 'La description (français) ne peut pas dépasser 3000 caractères.'
  },

  // Category navigator
  'category.selected': {
    en: 'Selected:',
    fr: 'Sélectionné :'
  },
  'category.change': {
    en: 'Change',
    fr: 'Modifier'
  },
  'category.all': {
    en: 'All',
    fr: 'Tout'
  },
  'category.empty': {
    en: 'No categories available.',
    fr: 'Aucune catégorie disponible.'
  },
  'category.selectLabel': {
    en: 'Select',
    fr: 'Sélectionner'
  },
  'category.navigateTo': {
    en: 'Navigate to subcategory',
    fr: 'Naviguer vers la sous-catégorie'
  },
  'category.selectCategoryLabel': {
    en: 'Select category',
    fr: 'Sélectionner la catégorie'
  },
  'category.doubleClickHint': {
    en: 'Double-click to change',
    fr: 'Double-cliquez pour modifier'
  },
  'category.changeHint': {
    en: 'Click (or press Enter/Space) to change',
    fr: 'Cliquez (ou appuyez sur Entrée/Espace) pour modifier'
  },

  // Variant attributes section
  'variantAttr.title': {
    en: 'Variant Attributes *',
    fr: 'Attributs de variante *'
  },
  'variantAttr.required': {
    en: 'At least one variant attribute is required.',
    fr: 'Au moins un attribut de variante est requis.'
  },
  'variantAttr.description': {
    en: 'Add attributes that create different variants of your product (e.g., Size, Color). Each combination of values will generate a unique variant in the next step.',
    fr: 'Ajoutez des attributs qui créent différentes variantes de votre produit (ex : Taille, Couleur). Chaque combinaison de valeurs générera une variante unique à l\'étape suivante.'
  },
  'variantAttr.namePlaceholderEn': {
    en: 'e.g., Size',
    fr: 'ex : Size'
  },
  'variantAttr.namePlaceholderFr': {
    en: 'e.g., Taille',
    fr: 'ex : Taille'
  },
  'variantAttr.valuesEn': {
    en: 'Values (English)',
    fr: 'Valeurs (anglais)'
  },
  'variantAttr.valuesFr': {
    en: 'Values (French)',
    fr: 'Valeurs (français)'
  },
  'variantAttr.addedTitle': {
    en: 'Added Variant Attributes',
    fr: 'Attributs de variante ajoutés'
  },
  'variantAttr.updateButton': {
    en: 'Update Attribute',
    fr: 'Mettre à jour l\'attribut'
  },
  'variantAttr.confirmSwitch': {
    en: 'You are currently editing another attribute. Switching will discard any unsaved changes to that attribute. Do you want to continue?',
    fr: 'Vous modifiez actuellement un autre attribut. Le changement supprimera toutes les modifications non enregistrées. Voulez-vous continuer ?'
  },
  'variantAttr.main': {
    en: 'Main',
    fr: 'Principal'
  },
  'variantAttr.setMainAriaLabel': {
    en: 'Set as main variant attribute',
    fr: 'Définir comme attribut de variante principal'
  },
  'variantAttr.maxReached': {
    en: 'Maximum of 3 variant attributes reached.',
    fr: 'Le maximum de 3 attributs de variante a été atteint.'
  },

  // Variant features section
  'variantFeature.title': {
    en: 'Variant Features (Optional)',
    fr: 'Caractéristiques de variante (facultatif)'
  },
  'variantFeature.description': {
    en: 'Add features that can vary by variant but don\'t create new variants (e.g., Weight, Dimensions). You can specify different values for each variant in the next step.',
    fr: 'Ajoutez des caractéristiques qui peuvent varier par variante mais ne créent pas de nouvelles variantes (ex : Poids, Dimensions). Vous pouvez spécifier des valeurs différentes pour chaque variante à l\'étape suivante.'
  },
  'variantFeature.nameEn': {
    en: 'Feature Name (English)',
    fr: 'Nom de la caractéristique (anglais)'
  },
  'variantFeature.nameFr': {
    en: 'Feature Name (French)',
    fr: 'Nom de la caractéristique (français)'
  },
  'variantFeature.namePlaceholderEn': {
    en: 'e.g., Weight',
    fr: 'ex : Weight'
  },
  'variantFeature.namePlaceholderFr': {
    en: 'e.g., Poids',
    fr: 'ex : Poids'
  },
  'variantFeature.addButton': {
    en: 'Add Feature',
    fr: 'Ajouter une caractéristique'
  },
  'variantFeature.updateButton': {
    en: 'Update Feature',
    fr: 'Mettre à jour la caractéristique'
  },
  'variantFeature.addedTitle': {
    en: 'Added Variant Features',
    fr: 'Caractéristiques de variante ajoutées'
  },
  'variantFeature.confirmSwitch': {
    en: 'You are currently editing another feature. Switching will discard any unsaved changes to that feature. Do you want to continue?',
    fr: 'Vous modifiez actuellement une autre caractéristique. Le changement supprimera toutes les modifications non enregistrées. Voulez-vous continuer ?'
  },

  // Step 3 variant cards
  'variant.attributes': {
    en: 'Attributes',
    fr: 'Attributs'
  },
  'variant.features': {
    en: 'Features',
    fr: 'Caractéristiques'
  },
  'variant.thumbnail': {
    en: 'Thumbnail',
    fr: 'Miniature'
  },
  'variant.chooseImage': {
    en: 'Choose Image',
    fr: 'Choisir une image'
  },
  'variant.chooseImages': {
    en: 'Choose Images',
    fr: 'Choisir des images'
  },
  'variant.stock': {
    en: 'Stock',
    fr: 'Stock'
  },
  'variant.idValuePlaceholder': {
    en: 'ID Value',
    fr: 'Valeur d\'ID'
  },
  'variant.createProduct': {
    en: 'Create Product',
    fr: 'Créer le produit'
  },
  'variant.creatingProduct': {
    en: 'Creating Product...',
    fr: 'Création du produit...'
  },
  'variant.updateProduct': {
    en: 'Update Product',
    fr: 'Mettre à jour le produit'
  },
  'variant.updatingProduct': {
    en: 'Updating Product...',
    fr: 'Mise à jour du produit...'
  },
  'variant.productCreated': {
    en: 'Product created successfully!',
    fr: 'Produit créé avec succès !'
  },
  'variant.productUpdated': {
    en: 'Product updated successfully!',
    fr: 'Produit mis à jour avec succès !'
  },
  'products.idType.mpn': {
    en: 'MPN (Manufacturer Part Number)',
    fr: 'MPN (Numéro de pièce du fabricant)'
  },

  // Media upload aria-labels and button titles
  'variant.uploadThumbnailAriaLabel': {
    en: 'Upload thumbnail image for variant',
    fr: 'Télécharger l\'image miniature pour la variante'
  },
  'variant.thumbnailAlt': {
    en: 'Thumbnail',
    fr: 'Miniature'
  },
  'variant.removeThumbnail': {
    en: 'Remove thumbnail',
    fr: 'Supprimer la miniature'
  },
  'variant.uploadImagesAriaLabel': {
    en: 'Upload product images for variant',
    fr: 'Télécharger les images du produit pour la variante'
  },
  'variant.removeImageTitle': {
    en: 'Remove image',
    fr: 'Supprimer l\'image'
  },
  'variant.moveLeft': {
    en: 'Move left',
    fr: 'Déplacer à gauche'
  },
  'variant.moveRight': {
    en: 'Move right',
    fr: 'Déplacer à droite'
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