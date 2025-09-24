import { useState, useEffect, useMemo } from 'react';
import './ProductsSection.css';
import { useLanguage } from '../../contexts/LanguageContext';

interface Company {
    id: string;
    ownerID: string;
    name: string;
    description?: string;
    logo?: string;
    createdAt: string;
    updatedAt?: string;
}

interface ProductsSectionProps {
    companies: Company[];
    viewMode?: 'list' | 'add';
    onViewModeChange?: (mode: 'list' | 'add') => void;
}

interface ItemAttribute {
    name_en: string;
    name_fr: string;
    values_en: string[];
    values_fr: string[];
}

interface BilingualItemAttribute {
    name_en: string;
    name_fr: string;
    value_en: string;
    value_fr: string;
}

interface ItemVariant {
    id: string;
    attributes_en: Record<string, string>;
    attributes_fr: Record<string, string>;
    sku: string;
    price: number;
    stock: number;
}

interface Category {
    id: string;
    name_en: string;
    name_fr: string;
    parentCategoryId?: string;
    createdAt: string;
    updatedAt?: string;
}

interface Item {
    id: string;
    name: string;
    name_fr: string;
    description: string;
    description_fr: string;
    categoryId: string;
    attributes: ItemAttribute[];
    itemAttributes: BilingualItemAttribute[];
    variants: ItemVariant[];
}

function ProductsSection({ viewMode = 'list', onViewModeChange }: ProductsSectionProps) {
    const [items, setItems] = useState<Item[]>([]);
    const [categories, setCategories] = useState<Category[]>([]);
    const { language, t } = useLanguage();
    const showAddForm = viewMode === 'add';
    const showListSection = viewMode === 'list';
    const [newItem, setNewItem] = useState({
        name: '',
        name_fr: '',
        description: '',
        description_fr: '',
        categoryId: '',
        attributes: [] as ItemAttribute[],
        itemAttributes: [] as BilingualItemAttribute[]
    });
    const [newAttribute, setNewAttribute] = useState({ 
        name_en: '', 
        name_fr: '', 
        values_en: [''], 
        values_fr: [''] 
    });
    const [attributeError, setAttributeError] = useState('');
    
    // State for the new bilingual item attributes
    const [newItemAttribute, setNewItemAttribute] = useState({
        name_en: '',
        name_fr: '',
        value_en: '',
        value_fr: ''
    });

    // Validation logic for save button
    const isFormInvalid = !newItem.name || !newItem.name_fr || !newItem.description || !newItem.description_fr || !newItem.categoryId;

    // Memoized helper function to get category display for an item
    const getCategoryDisplay = useMemo(() => {
        return (categoryId: string) => {
            const category = categories.find(c => c.id === categoryId);
            const displayName = language === 'fr' 
                ? category?.name_fr || t('common.unknown')
                : category?.name_en || t('common.unknown');
            return {
                name_en: category?.name_en || t('common.unknown'),
                name_fr: category?.name_fr || t('common.unknown'),
                displayName
            };
        };
    }, [categories, language, t]);

    // Fetch categories on component mount
    const fetchCategories = async () => {
        try {
            // For demo purposes, using mock categories when API is not available
            // Replace with your actual API endpoint when database is configured
            const response = await fetch('/api/Category/GetAllCategories');
            if (response.ok) {
                const result = await response.json();
                if (result.value) {
                    setCategories(result.value);
                    return;
                }
            }
        } catch (error) {
            console.error('Failed to fetch categories:', error);
        }
        
        // Mock categories for demo purposes
        setCategories([
            {
                id: '1',
                name_en: 'Electronics',
                name_fr: 'Électronique',
                createdAt: new Date().toISOString()
            },
            {
                id: '2',
                name_en: 'Clothing',
                name_fr: 'Vêtements',
                createdAt: new Date().toISOString()
            },
            {
                id: '3',
                name_en: 'Books',
                name_fr: 'Livres',
                createdAt: new Date().toISOString()
            },
            {
                id: '4',
                name_en: 'Home & Garden',
                name_fr: 'Maison et Jardin',
                createdAt: new Date().toISOString()
            }
        ]);
    };

    // Load categories when component mounts
    useEffect(() => {
        fetchCategories();
    }, []);

    const addAttributeValue = () => {
        setNewAttribute(prev => ({
            ...prev,
            values_en: [...prev.values_en, ''],
            values_fr: [...prev.values_fr, '']
        }));
    };

    const removeAttributeValue = (index: number) => {
        setNewAttribute(prev => ({
            ...prev,
            values_en: prev.values_en.filter((_, i) => i !== index),
            values_fr: prev.values_fr.filter((_, i) => i !== index)
        }));
    };

    const updateAttributeValue = (index: number, value: string, language: 'en' | 'fr') => {
        setNewAttribute(prev => ({
            ...prev,
            [language === 'en' ? 'values_en' : 'values_fr']: 
                prev[language === 'en' ? 'values_en' : 'values_fr'].map((v, i) => i === index ? value : v)
        }));
    };

    const addItemAttribute = () => {
        if (!newItemAttribute.name_en || !newItemAttribute.name_fr || 
            !newItemAttribute.value_en || !newItemAttribute.value_fr) {
            return; // Don't add if any field is empty
        }

        setNewItem(prev => ({
            ...prev,
            itemAttributes: [...prev.itemAttributes, { ...newItemAttribute }]
        }));
        setNewItemAttribute({
            name_en: '',
            name_fr: '',
            value_en: '',
            value_fr: ''
        });
    };

    const removeItemAttribute = (index: number) => {
        setNewItem(prev => ({
            ...prev,
            itemAttributes: prev.itemAttributes.filter((_, i) => i !== index)
        }));
    };

    const addAttribute = () => {
        // Clear any previous error
        setAttributeError('');
        
        const nonEmptyValuesEn = newAttribute.values_en.filter(v => v.trim());
        const nonEmptyValuesFr = newAttribute.values_fr.filter(v => v.trim());
        if (
            !newAttribute.name_en ||
            !newAttribute.name_fr ||
            nonEmptyValuesEn.length === 0 ||
            nonEmptyValuesFr.length === 0 ||
            nonEmptyValuesEn.length !== nonEmptyValuesFr.length
        ) {
            setAttributeError("Please ensure both English and French values are provided and have the same number of non-empty entries.");
            return;
        }

        // Check for duplicate attribute names (case-insensitive)
        const isDuplicate = newItem.attributes.some(attr => 
            attr.name_en.toLowerCase() === newAttribute.name_en.toLowerCase() ||
            attr.name_fr.toLowerCase() === newAttribute.name_fr.toLowerCase()
        );

        if (isDuplicate) {
            setAttributeError(`Attribute "${newAttribute.name_en}" or "${newAttribute.name_fr}" already exists. Please use different names.`);
            return;
        }

        setNewItem(prev => ({
            ...prev,
            attributes: [...prev.attributes, {
                name_en: newAttribute.name_en,
                name_fr: newAttribute.name_fr,
                values_en: newAttribute.values_en.filter(v => v.trim()),
                values_fr: newAttribute.values_fr.filter(v => v.trim())
            }]
        }));
        setNewAttribute({ 
            name_en: '', 
            name_fr: '', 
            values_en: [''], 
            values_fr: [''] 
        });
    };

    const removeAttribute = (index: number) => {
        setNewItem(prev => ({
            ...prev,
            attributes: prev.attributes.filter((_, i) => i !== index)
        }));
    };

    const generateVariants = (): ItemVariant[] => {
        if (newItem.attributes.length === 0) {
            return [{
                id: '1',
                attributes_en: {},
                attributes_fr: {},
                sku: '',
                price: 0,
                stock: 0
            }];
        }

        const combinations: { en: Record<string, string>, fr: Record<string, string> }[] = [];
        
        const generateCombinations = (attrIndex: number, currentEn: Record<string, string>, currentFr: Record<string, string>) => {
            if (attrIndex >= newItem.attributes.length) {
                combinations.push({ en: { ...currentEn }, fr: { ...currentFr } });
                return;
            }
            
            const attribute = newItem.attributes[attrIndex];
            const minLength = Math.min(attribute.values_en.length, attribute.values_fr.length);
            
            for (let i = 0; i < minLength; i++) {
                const valueEn = attribute.values_en[i];
                const valueFr = attribute.values_fr[i];
                generateCombinations(
                    attrIndex + 1, 
                    { ...currentEn, [attribute.name_en]: valueEn },
                    { ...currentFr, [attribute.name_fr]: valueFr }
                );
            }
        };

        generateCombinations(0, {}, {});

        return combinations.map((combo, index) => ({
            id: `variant-${index + 1}`,
            attributes_en: combo.en,
            attributes_fr: combo.fr,
            sku: '',
            price: 0,
            stock: 0
        }));
    };

    const [variants, setVariants] = useState<ItemVariant[]>([]);

    const updateVariant = (variantId: string, field: keyof Omit<ItemVariant, 'id' | 'attributes_en' | 'attributes_fr'>, value: string | number) => {
        setVariants(prev => prev.map(v => 
            v.id === variantId ? { ...v, [field]: value } : v
        ));
    };

    const handleGenerateVariants = () => {
        const newVariants = generateVariants();
        setVariants(newVariants);
    };

    const handleSaveItem = () => {
        if (newItem.name && newItem.name_fr && newItem.description && newItem.description_fr && newItem.categoryId) {
            const item: Item = {
                id: `item-${Date.now()}`,
                name: newItem.name,
                name_fr: newItem.name_fr,
                description: newItem.description,
                description_fr: newItem.description_fr,
                categoryId: newItem.categoryId,
                attributes: newItem.attributes,
                itemAttributes: newItem.itemAttributes,
                variants: variants
            };
            setItems(prev => [...prev, item]);
            setNewItem({ 
                name: '', 
                name_fr: '', 
                description: '', 
                description_fr: '', 
                categoryId: '', 
                attributes: [],
                itemAttributes: []
            });
            setVariants([]);
            // Switch back to list view after saving
            if (onViewModeChange) {
                onViewModeChange('list');
            }
        }
    };

    const deleteItem = (itemId: string) => {
        setItems(prev => prev.filter(item => item.id !== itemId));
    };

    return (
        <div className="section-container">
            {showAddForm && (
                <div className="products-add-form">
                    <h3>{t('products.addProduct')}</h3>
                    
                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.itemName')}
                        </label>
                        <input
                            type="text"
                            value={newItem.name}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name: e.target.value }))}
                            className="products-form-input"
                            placeholder={t('placeholder.itemName')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.itemNameFr')}
                        </label>
                        <input
                            type="text"
                            value={newItem.name_fr}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name_fr: e.target.value }))}
                            className="products-form-input"
                            placeholder={t('placeholder.itemNameFr')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.description')}
                        </label>
                        <textarea
                            value={newItem.description}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description: e.target.value }))}
                            className="products-form-textarea"
                            placeholder={t('placeholder.description')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.descriptionFr')}
                        </label>
                        <textarea
                            value={newItem.description_fr}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description_fr: e.target.value }))}
                            className="products-form-textarea"
                            placeholder={t('placeholder.descriptionFr')}
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            {t('products.category')}
                        </label>
                        <select
                            value={newItem.categoryId}
                            onChange={(e) => setNewItem(prev => ({ ...prev, categoryId: e.target.value }))}
                            className="products-form-input"
                        >
                            <option value="">{t('products.selectCategory')}</option>
                            {categories.map(category => (
                                <option key={category.id} value={category.id}>
                                    {language === 'fr' ? category.name_fr : category.name_en}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="item-attributes-section">
                        <h4>{t('products.itemAttributesTitle')}</h4>
                        <div className="products-form-group">
                            <div className="attribute-input-row">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeNameEn')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.name_en}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_en: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeNameEn')}
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeValueEn')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.value_en}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, value_en: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeValueEn')}
                                    />
                                </div>
                            </div>
                            <div className="attribute-input-row">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeNameFr')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.name_fr}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_fr: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeNameFr')}
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeValueFr')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.value_fr}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, value_fr: e.target.value }))}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeValueFr')}
                                    />
                                </div>
                            </div>
                            <div className="attribute-actions">
                                <button
                                    onClick={addItemAttribute}
                                    className="products-add-attribute-button"
                                    disabled={!newItemAttribute.name_en || !newItemAttribute.name_fr || 
                                             !newItemAttribute.value_en || !newItemAttribute.value_fr}
                                >
                                    {t('products.addNewAttribute')}
                                </button>
                            </div>
                        </div>

                        {newItem.itemAttributes.length > 0 && (
                            <div className="added-item-attributes">
                                <h5>{t('products.attributes')}</h5>
                                {newItem.itemAttributes.map((attr, index) => (
                                    <div key={index} className="item-attribute-display">
                                        <div className="attribute-display-content">
                                            <div className="attribute-lang-pair">
                                                <strong>{t('products.attributeNameEn')}:</strong> {attr.name_en} | 
                                                <strong> {t('products.attributeValueEn')}:</strong> {attr.value_en}
                                            </div>
                                            <div className="attribute-lang-pair">
                                                <strong>{t('products.attributeNameFr')}:</strong> {attr.name_fr} | 
                                                <strong> {t('products.attributeValueFr')}:</strong> {attr.value_fr}
                                            </div>
                                        </div>
                                        <button
                                            onClick={() => removeItemAttribute(index)}
                                            className="products-remove-attribute-button"
                                        >
                                            {t('products.removeAttribute')}
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    <div className="products-variants-section">
                        <h4>{t('products.itemAttributes')}</h4>
                        <div className="products-variant-input">
                            <div className="products-variant-name">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeName')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newAttribute.name_en}
                                        onChange={(e) => {
                                            setNewAttribute(prev => ({ ...prev, name_en: e.target.value }));
                                            // Clear error when user starts typing
                                            if (attributeError) {
                                                setAttributeError('');
                                            }
                                        }}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeName')}
                                        aria-invalid={!!attributeError}
                                        aria-describedby={attributeError ? "attribute-name-error" : undefined}
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeNameFrVariant')}
                                    </label>
                                    <input
                                        type="text"
                                        value={newAttribute.name_fr}
                                        onChange={(e) => {
                                            setNewAttribute(prev => ({ ...prev, name_fr: e.target.value }));
                                            // Clear error when user starts typing
                                            if (attributeError) {
                                                setAttributeError('');
                                            }
                                        }}
                                        className="products-form-input"
                                        placeholder={t('placeholder.attributeNameFrVariant')}
                                        aria-invalid={!!attributeError}
                                        aria-describedby={attributeError ? "attribute-name-error" : undefined}
                                    />
                                </div>
                                {attributeError && (
                                    <div
                                        id="attribute-name-error"
                                        className="products-error-message"
                                        style={{ color: 'red', fontSize: '14px', marginTop: '4px' }}
                                        role="alert"
                                    >
                                        {attributeError}
                                    </div>
                                )}
                            </div>
                            <div className="products-variant-values">
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeValues')}
                                    </label>
                                    {newAttribute.values_en.map((value, index) => (
                                        <div key={index} className="products-attribute-value-row">
                                            <input
                                                type="text"
                                                value={value}
                                                onChange={(e) => updateAttributeValue(index, e.target.value, 'en')}
                                                className="products-attribute-value-input"
                                                placeholder={t('placeholder.attributeValue')}
                                            />
                                            {newAttribute.values_en.length > 1 && (
                                                <button
                                                    onClick={() => removeAttributeValue(index)}
                                                    className="products-remove-value-button"
                                                >
                                                    {t('products.deleteItem')}
                                                </button>
                                            )}
                                        </div>
                                    ))}
                                </div>
                                <div className="attribute-input-group">
                                    <label className="products-form-label">
                                        {t('products.attributeValuesFr')}
                                    </label>
                                    {newAttribute.values_fr.map((value, index) => (
                                        <div key={index} className="products-attribute-value-row">
                                            <input
                                                type="text"
                                                value={value}
                                                onChange={(e) => updateAttributeValue(index, e.target.value, 'fr')}
                                                className="products-attribute-value-input"
                                                placeholder={t('placeholder.attributeValueFrVariant')}
                                            />
                                            {newAttribute.values_fr.length > 1 && (
                                                <button
                                                    onClick={() => removeAttributeValue(index)}
                                                    className="products-remove-value-button"
                                                >
                                                    {t('products.deleteItem')}
                                                </button>
                                            )}
                                        </div>
                                    ))}
                                </div>
                                <button
                                    onClick={addAttributeValue}
                                    className="products-add-value-button"
                                >
                                    {t('products.addValue')}
                                </button>
                            </div>
                            <div className="products-variant-actions">
                                <button
                                    onClick={addAttribute}
                                    className="products-add-attribute-button"
                                >
                                    {t('products.addAttribute')}
                                </button>
                            </div>
                        </div>

                        {newItem.attributes.length > 0 && (
                            <div className="products-added-attributes">
                                <h5>{t('products.attributes')}</h5>
                                {newItem.attributes.map((attr, index) => (
                                    <div key={index} className="products-attribute-item">
                                        <span>
                                            <div><strong>EN:</strong> {attr.name_en}: {attr.values_en.join(', ')}</div>
                                            <div><strong>FR:</strong> {attr.name_fr}: {attr.values_fr.join(', ')}</div>
                                        </span>
                                        <button
                                            onClick={() => removeAttribute(index)}
                                            className="products-remove-attribute-button"
                                        >
                                            {t('products.deleteItem')}
                                        </button>
                                    </div>
                                ))}
                                <button
                                    onClick={handleGenerateVariants}
                                    className="products-generate-variants-button"
                                >
                                    Generate Variants
                                </button>
                            </div>
                        )}
                    </div>

                    {variants.length > 0 && (
                        <div className="products-variants-section">
                            <h4>Item Variants</h4>
                            <div className="products-variants-table-container">
                                <table className="products-variants-table">
                                    <thead>
                                        <tr>
                                            {newItem.attributes.map(attr => (
                                                <th key={`${attr.name_en}-${attr.name_fr}`}>
                                                    <div>
                                                        <div><strong>EN:</strong> {attr.name_en}</div>
                                                        <div><strong>FR:</strong> {attr.name_fr}</div>
                                                    </div>
                                                </th>
                                            ))}
                                            <th>SKU</th>
                                            <th>Price</th>
                                            <th>Stock</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {variants.map(variant => (
                                            <tr key={variant.id}>
                                                {newItem.attributes.map(attr => (
                                                    <td key={`${attr.name_en}-${attr.name_fr}`}>
                                                        <div>
                                                            <div><strong>EN:</strong> {variant.attributes_en[attr.name_en] || '-'}</div>
                                                            <div><strong>FR:</strong> {variant.attributes_fr[attr.name_fr] || '-'}</div>
                                                        </div>
                                                    </td>
                                                ))}
                                                <td>
                                                    <input
                                                        type="text"
                                                        value={variant.sku}
                                                        onChange={(e) => updateVariant(variant.id, 'sku', e.target.value)}
                                                        className="products-variant-input products-variant-input--sku"
                                                        placeholder="SKU"
                                                    />
                                                </td>
                                                <td>
                                                    <input
                                                        type="number"
                                                        value={variant.price}
                                                        onChange={(e) => updateVariant(variant.id, 'price', parseFloat(e.target.value) || 0)}
                                                        className="products-variant-input"
                                                        step="0.01"
                                                        min="0"
                                                        placeholder="0.00"
                                                    />
                                                </td>
                                                <td>
                                                    <input
                                                        type="number"
                                                        value={variant.stock}
                                                        onChange={(e) => updateVariant(variant.id, 'stock', parseInt(e.target.value) || 0)}
                                                        className="products-variant-input products-variant-input--stock"
                                                        min="0"
                                                        placeholder="0"
                                                    />
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    )}

                    <div className="products-form-actions">
                        <button
                            onClick={handleSaveItem}
                            disabled={isFormInvalid}
                            className={`products-action-button products-action-button--save${isFormInvalid ? ' products-action-button--disabled' : ''}`}
                        >
                            {t('products.addItem')}
                        </button>
                        <button
                            onClick={() => onViewModeChange && onViewModeChange('list')}
                            className="products-action-button products-action-button--cancel"
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            {showListSection && (
                <div className="products-list-section">
                    <h3>Current Items ({items.length})</h3>
                    {items.length === 0 ? (
                        <p className="products-empty">
                            No items added yet. Click "Add New Item" to create your first product.
                        </p>
                    ) : (
                        <div className="products-items-grid">
                            {items.map(item => {
                                const categoryDisplay = getCategoryDisplay(item.categoryId);
                                return (
                                    <div key={item.id} className="products-item-card">
                                        <div className="products-item-header">
                                            <div className="products-item-info">
                                                <h4>{item.name} / {item.name_fr}</h4>
                                                <p className="products-item-description">
                                                    <strong>EN:</strong> {item.description}<br/>
                                                    <strong>FR:</strong> {item.description_fr}
                                                </p>
                                                <p className="products-item-category">
                                                    <strong>{t('products.category')}:</strong> {categoryDisplay.displayName}
                                                </p>
                                            </div>
                                        <button
                                            onClick={() => deleteItem(item.id)}
                                            className="products-delete-button"
                                        >
                                            {t('products.deleteItem')}
                                        </button>
                                    </div>
                                    
                                    {item.attributes.length > 0 && (
                                        <div className="products-item-attributes">
                                            <h5>{t('products.attributes')}</h5>
                                            {item.attributes.map((attr, index) => (
                                                <div key={index} className="products-attribute-badge">
                                                    <div><strong>EN:</strong> {attr.name_en}: {attr.values_en.join(', ')}</div>
                                                    <div><strong>FR:</strong> {attr.name_fr}: {attr.values_fr.join(', ')}</div>
                                                </div>
                                            ))}
                                        </div>
                                    )}

                                    {item.itemAttributes && item.itemAttributes.length > 0 && (
                                        <div className="products-item-attributes">
                                            <h5>{t('products.itemAttributesTitle')}</h5>
                                            {item.itemAttributes.map((attr, index) => (
                                                <div key={index} className="item-attribute-display">
                                                    <div className="attribute-lang-pair">
                                                        <strong>EN:</strong> {attr.name_en}: {attr.value_en}
                                                    </div>
                                                    <div className="attribute-lang-pair">
                                                        <strong>FR:</strong> {attr.name_fr}: {attr.value_fr}
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}

                                    <div>
                                        <h5>Variants ({item.variants.length}):</h5>
                                        <div className="products-variants-table-container">
                                            <table className="products-item-variants-table">
                                                <thead>
                                                    <tr>
                                                        {item.attributes.map(attr => (
                                                            <th key={`${attr.name_en}-${attr.name_fr}`}>
                                                                <div>
                                                                    <div><strong>EN:</strong> {attr.name_en}</div>
                                                                    <div><strong>FR:</strong> {attr.name_fr}</div>
                                                                </div>
                                                            </th>
                                                        ))}
                                                        <th>SKU</th>
                                                        <th>Price</th>
                                                        <th>Stock</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {item.variants.map(variant => (
                                                        <tr key={variant.id}>
                                                            {item.attributes.map(attr => (
                                                                <td key={`${attr.name_en}-${attr.name_fr}`}>
                                                                    <div>
                                                                        <div><strong>EN:</strong> {variant.attributes_en[attr.name_en] || '-'}</div>
                                                                        <div><strong>FR:</strong> {variant.attributes_fr[attr.name_fr] || '-'}</div>
                                                                    </div>
                                                                </td>
                                                            ))}
                                                            <td>
                                                                {variant.sku || '-'}
                                                            </td>
                                                            <td>
                                                                ${variant.price.toFixed(2)}
                                                            </td>
                                                            <td>
                                                                {variant.stock}
                                                            </td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                                );
                            })}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}

export default ProductsSection;