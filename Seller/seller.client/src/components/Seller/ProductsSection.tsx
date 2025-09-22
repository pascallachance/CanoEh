import { useState, useEffect, useMemo } from 'react';
import './ProductsSection.css';

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
    name: string;
    values: string[];
}

interface ItemVariant {
    id: string;
    attributes: Record<string, string>;
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
    variants: ItemVariant[];
}

function ProductsSection({ viewMode = 'list', onViewModeChange }: ProductsSectionProps) {
    const [items, setItems] = useState<Item[]>([]);
    const [categories, setCategories] = useState<Category[]>([]);
    const showAddForm = viewMode === 'add';
    const showListSection = viewMode === 'list';
    const [newItem, setNewItem] = useState({
        name: '',
        name_fr: '',
        description: '',
        description_fr: '',
        categoryId: '',
        attributes: [] as ItemAttribute[]
    });
    const [newAttribute, setNewAttribute] = useState({ name: '', values: [''] });
    const [attributeError, setAttributeError] = useState('');

    // Validation logic for save button
    const isFormInvalid = !newItem.name || !newItem.name_fr || !newItem.description || !newItem.description_fr || !newItem.categoryId;

    // Memoized helper function to get category display for an item
    const getCategoryDisplay = useMemo(() => {
        return (categoryId: string) => {
            const category = categories.find(c => c.id === categoryId);
            return {
                name_en: category?.name_en || 'Unknown',
                name_fr: category?.name_fr || 'Unknown'
            };
        };
    }, [categories]);

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
            values: [...prev.values, '']
        }));
    };

    const removeAttributeValue = (index: number) => {
        setNewAttribute(prev => ({
            ...prev,
            values: prev.values.filter((_, i) => i !== index)
        }));
    };

    const updateAttributeValue = (index: number, value: string) => {
        setNewAttribute(prev => ({
            ...prev,
            values: prev.values.map((v, i) => i === index ? value : v)
        }));
    };

    const addAttribute = () => {
        // Clear any previous error
        setAttributeError('');
        
        if (!newAttribute.name || newAttribute.values.filter(v => v.trim()).length === 0) {
            return;
        }

        // Check for duplicate attribute names (case-insensitive)
        const isDuplicate = newItem.attributes.some(attr => 
            attr.name.toLowerCase() === newAttribute.name.toLowerCase()
        );

        if (isDuplicate) {
            setAttributeError(`Attribute "${newAttribute.name}" already exists. Please use a different name.`);
            return;
        }

        setNewItem(prev => ({
            ...prev,
            attributes: [...prev.attributes, {
                name: newAttribute.name,
                values: newAttribute.values.filter(v => v.trim())
            }]
        }));
        setNewAttribute({ name: '', values: [''] });
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
                attributes: {},
                sku: '',
                price: 0,
                stock: 0
            }];
        }

        const combinations: Record<string, string>[] = [];
        
        const generateCombinations = (attrIndex: number, current: Record<string, string>) => {
            if (attrIndex >= newItem.attributes.length) {
                combinations.push({ ...current });
                return;
            }
            
            const attribute = newItem.attributes[attrIndex];
            for (const value of attribute.values) {
                generateCombinations(attrIndex + 1, { ...current, [attribute.name]: value });
            }
        };

        generateCombinations(0, {});

        return combinations.map((combo, index) => ({
            id: `variant-${index + 1}`,
            attributes: combo,
            sku: '',
            price: 0,
            stock: 0
        }));
    };

    const [variants, setVariants] = useState<ItemVariant[]>([]);

    const updateVariant = (variantId: string, field: keyof Omit<ItemVariant, 'id' | 'attributes'>, value: string | number) => {
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
                variants: variants
            };
            setItems(prev => [...prev, item]);
            setNewItem({ name: '', name_fr: '', description: '', description_fr: '', categoryId: '', attributes: [] });
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
                    <h3>Add New Item</h3>
                    
                    <div className="products-form-group">
                        <label className="products-form-label">
                            Item Name (English)
                        </label>
                        <input
                            type="text"
                            value={newItem.name}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name: e.target.value }))}
                            className="products-form-input"
                            placeholder="Enter item name in English"
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            Item Name (French)
                        </label>
                        <input
                            type="text"
                            value={newItem.name_fr}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name_fr: e.target.value }))}
                            className="products-form-input"
                            placeholder="Enter item name in French"
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            Description (English)
                        </label>
                        <textarea
                            value={newItem.description}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description: e.target.value }))}
                            className="products-form-textarea"
                            placeholder="Enter item description in English"
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            Description (French)
                        </label>
                        <textarea
                            value={newItem.description_fr}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description_fr: e.target.value }))}
                            className="products-form-textarea"
                            placeholder="Enter item description in French"
                        />
                    </div>

                    <div className="products-form-group">
                        <label className="products-form-label">
                            Category
                        </label>
                        <select
                            value={newItem.categoryId}
                            onChange={(e) => setNewItem(prev => ({ ...prev, categoryId: e.target.value }))}
                            className="products-form-input"
                        >
                            <option value="">Select a category</option>
                            {categories.map(category => (
                                <option key={category.id} value={category.id}>
                                    {category.name_en} / {category.name_fr}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="products-attributes-section">
                        <h4>Item Attributes</h4>
                        <div className="products-attribute-input">
                            <div className="products-attribute-name">
                                <label className="products-form-label">
                                    Attribute Name
                                </label>
                                <input
                                    type="text"
                                    value={newAttribute.name}
                                    onChange={(e) => {
                                        setNewAttribute(prev => ({ ...prev, name: e.target.value }));
                                        // Clear error when user starts typing
                                        if (attributeError) {
                                            setAttributeError('');
                                        }
                                    }}
                                    className="products-form-input"
                                    placeholder="e.g., Color"
                                    aria-invalid={!!attributeError}
                                    aria-describedby={attributeError ? "attribute-name-error" : undefined}
                                />
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
                            <div className="products-attribute-values">
                                <label className="products-form-label">
                                    Possible Values
                                </label>
                                {newAttribute.values.map((value, index) => (
                                    <div key={index} className="products-attribute-value-row">
                                        <input
                                            type="text"
                                            value={value}
                                            onChange={(e) => updateAttributeValue(index, e.target.value)}
                                            className="products-attribute-value-input"
                                            placeholder="e.g., Red"
                                        />
                                        {newAttribute.values.length > 1 && (
                                            <button
                                                onClick={() => removeAttributeValue(index)}
                                                className="products-remove-value-button"
                                            >
                                                Remove
                                            </button>
                                        )}
                                    </div>
                                ))}
                                <button
                                    onClick={addAttributeValue}
                                    className="products-add-value-button"
                                >
                                    Add Value
                                </button>
                            </div>
                            <div className="products-attribute-actions">
                                <button
                                    onClick={addAttribute}
                                    className="products-add-attribute-button"
                                >
                                    Add Attribute
                                </button>
                            </div>
                        </div>

                        {newItem.attributes.length > 0 && (
                            <div className="products-added-attributes">
                                <h5>Added Attributes:</h5>
                                {newItem.attributes.map((attr, index) => (
                                    <div key={index} className="products-attribute-item">
                                        <span>
                                            <strong>{attr.name}:</strong> {attr.values.join(', ')}
                                        </span>
                                        <button
                                            onClick={() => removeAttribute(index)}
                                            className="products-remove-attribute-button"
                                        >
                                            Remove
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
                                                <th key={attr.name}>
                                                    {attr.name}
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
                                                    <td key={attr.name}>
                                                        {variant.attributes[attr.name] || '-'}
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
                            Save Item
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
                                                    <strong>Category:</strong> {categoryDisplay.name_en} / {categoryDisplay.name_fr}
                                                </p>
                                            </div>
                                        <button
                                            onClick={() => deleteItem(item.id)}
                                            className="products-delete-button"
                                        >
                                            Delete
                                        </button>
                                    </div>
                                    
                                    {item.attributes.length > 0 && (
                                        <div className="products-item-attributes">
                                            <h5>Attributes:</h5>
                                            {item.attributes.map((attr, index) => (
                                                <span key={index} className="products-attribute-badge">
                                                    <strong>{attr.name}:</strong> {attr.values.join(', ')}
                                                </span>
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
                                                            <th key={attr.name}>
                                                                {attr.name}
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
                                                                <td key={attr.name}>
                                                                    {variant.attributes[attr.name] || '-'}
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