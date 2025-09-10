import { useState } from 'react';

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

interface Item {
    id: string;
    name: string;
    description: string;
    attributes: ItemAttribute[];
    variants: ItemVariant[];
}

function ProductsSection(_props: ProductsSectionProps) {
    const [items, setItems] = useState<Item[]>([]);
    const [showAddForm, setShowAddForm] = useState(false);
    const [newItem, setNewItem] = useState({
        name: '',
        description: '',
        attributes: [] as ItemAttribute[]
    });
    const [newAttribute, setNewAttribute] = useState({ name: '', values: [''] });

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
        if (newAttribute.name && newAttribute.values.filter(v => v.trim()).length > 0) {
            setNewItem(prev => ({
                ...prev,
                attributes: [...prev.attributes, {
                    name: newAttribute.name,
                    values: newAttribute.values.filter(v => v.trim())
                }]
            }));
            setNewAttribute({ name: '', values: [''] });
        }
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
        if (newItem.name && newItem.description) {
            const item: Item = {
                id: `item-${Date.now()}`,
                name: newItem.name,
                description: newItem.description,
                attributes: newItem.attributes,
                variants: variants
            };
            setItems(prev => [...prev, item]);
            setNewItem({ name: '', description: '', attributes: [] });
            setVariants([]);
            setShowAddForm(false);
        }
    };

    const deleteItem = (itemId: string) => {
        setItems(prev => prev.filter(item => item.id !== itemId));
    };

    return (
        <div className="section-container">
            <h2 className="section-title">Products Management</h2>
            <p className="section-description">
                Manage your product catalog. Add new items with their attributes and variants, 
                update existing products, and remove discontinued items.
            </p>

            <div style={{ marginBottom: '2rem' }}>
                <button 
                    onClick={() => setShowAddForm(!showAddForm)}
                    style={{
                        padding: '0.75rem 1.5rem',
                        background: '#007bff',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer',
                        fontSize: '1rem'
                    }}
                >
                    {showAddForm ? 'Cancel' : 'Add New Item'}
                </button>
            </div>

            {showAddForm && (
                <div style={{ 
                    background: '#f8f9fa', 
                    padding: '2rem', 
                    borderRadius: '8px', 
                    marginBottom: '2rem',
                    border: '1px solid #e1e5e9'
                }}>
                    <h3>Add New Item</h3>
                    
                    <div style={{ marginBottom: '1rem' }}>
                        <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                            Item Name *
                        </label>
                        <input
                            type="text"
                            value={newItem.name}
                            onChange={(e) => setNewItem(prev => ({ ...prev, name: e.target.value }))}
                            style={{
                                width: '100%',
                                padding: '0.75rem',
                                border: '1px solid #ced4da',
                                borderRadius: '4px',
                                fontSize: '1rem'
                            }}
                            placeholder="Enter item name"
                        />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                        <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                            Description *
                        </label>
                        <textarea
                            value={newItem.description}
                            onChange={(e) => setNewItem(prev => ({ ...prev, description: e.target.value }))}
                            style={{
                                width: '100%',
                                padding: '0.75rem',
                                border: '1px solid #ced4da',
                                borderRadius: '4px',
                                fontSize: '1rem',
                                minHeight: '100px',
                                resize: 'vertical'
                            }}
                            placeholder="Enter item description"
                        />
                    </div>

                    <div style={{ marginBottom: '1rem' }}>
                        <h4>Item Attributes</h4>
                        <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', alignItems: 'end' }}>
                            <div style={{ flex: 1 }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Attribute Name (e.g., Color, Size)
                                </label>
                                <input
                                    type="text"
                                    value={newAttribute.name}
                                    onChange={(e) => setNewAttribute(prev => ({ ...prev, name: e.target.value }))}
                                    style={{
                                        width: '100%',
                                        padding: '0.75rem',
                                        border: '1px solid #ced4da',
                                        borderRadius: '4px',
                                        fontSize: '1rem'
                                    }}
                                    placeholder="e.g., Color"
                                />
                            </div>
                            <div style={{ flex: 2 }}>
                                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '600' }}>
                                    Possible Values
                                </label>
                                {newAttribute.values.map((value, index) => (
                                    <div key={index} style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.5rem' }}>
                                        <input
                                            type="text"
                                            value={value}
                                            onChange={(e) => updateAttributeValue(index, e.target.value)}
                                            style={{
                                                flex: 1,
                                                padding: '0.5rem',
                                                border: '1px solid #ced4da',
                                                borderRadius: '4px',
                                                fontSize: '0.9rem'
                                            }}
                                            placeholder="e.g., Red"
                                        />
                                        {newAttribute.values.length > 1 && (
                                            <button
                                                onClick={() => removeAttributeValue(index)}
                                                style={{
                                                    padding: '0.5rem',
                                                    background: '#dc3545',
                                                    color: 'white',
                                                    border: 'none',
                                                    borderRadius: '4px',
                                                    cursor: 'pointer'
                                                }}
                                            >
                                                Remove
                                            </button>
                                        )}
                                    </div>
                                ))}
                                <button
                                    onClick={addAttributeValue}
                                    style={{
                                        padding: '0.5rem 1rem',
                                        background: '#28a745',
                                        color: 'white',
                                        border: 'none',
                                        borderRadius: '4px',
                                        cursor: 'pointer',
                                        fontSize: '0.9rem'
                                    }}
                                >
                                    Add Value
                                </button>
                            </div>
                            <button
                                onClick={addAttribute}
                                style={{
                                    padding: '0.75rem 1rem',
                                    background: '#007bff',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '4px',
                                    cursor: 'pointer'
                                }}
                            >
                                Add Attribute
                            </button>
                        </div>

                        {newItem.attributes.length > 0 && (
                            <div style={{ marginBottom: '1rem' }}>
                                <h5>Added Attributes:</h5>
                                {newItem.attributes.map((attr, index) => (
                                    <div key={index} style={{ 
                                        display: 'flex', 
                                        justifyContent: 'space-between', 
                                        alignItems: 'center',
                                        padding: '0.5rem',
                                        background: 'white',
                                        border: '1px solid #e1e5e9',
                                        borderRadius: '4px',
                                        marginBottom: '0.5rem'
                                    }}>
                                        <span>
                                            <strong>{attr.name}:</strong> {attr.values.join(', ')}
                                        </span>
                                        <button
                                            onClick={() => removeAttribute(index)}
                                            style={{
                                                padding: '0.25rem 0.5rem',
                                                background: '#dc3545',
                                                color: 'white',
                                                border: 'none',
                                                borderRadius: '4px',
                                                cursor: 'pointer',
                                                fontSize: '0.8rem'
                                            }}
                                        >
                                            Remove
                                        </button>
                                    </div>
                                ))}
                                <button
                                    onClick={handleGenerateVariants}
                                    style={{
                                        padding: '0.75rem 1rem',
                                        background: '#17a2b8',
                                        color: 'white',
                                        border: 'none',
                                        borderRadius: '4px',
                                        cursor: 'pointer',
                                        marginTop: '0.5rem'
                                    }}
                                >
                                    Generate Variants
                                </button>
                            </div>
                        )}
                    </div>

                    {variants.length > 0 && (
                        <div style={{ marginBottom: '1rem' }}>
                            <h4>Item Variants</h4>
                            <div style={{ overflowX: 'auto' }}>
                                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                                    <thead>
                                        <tr style={{ background: '#e9ecef' }}>
                                            {newItem.attributes.map(attr => (
                                                <th key={attr.name} style={{ padding: '0.75rem', border: '1px solid #dee2e6', textAlign: 'left' }}>
                                                    {attr.name}
                                                </th>
                                            ))}
                                            <th style={{ padding: '0.75rem', border: '1px solid #dee2e6', textAlign: 'left' }}>SKU</th>
                                            <th style={{ padding: '0.75rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Price</th>
                                            <th style={{ padding: '0.75rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Stock</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {variants.map(variant => (
                                            <tr key={variant.id}>
                                                {newItem.attributes.map(attr => (
                                                    <td key={attr.name} style={{ padding: '0.75rem', border: '1px solid #dee2e6' }}>
                                                        {variant.attributes[attr.name] || '-'}
                                                    </td>
                                                ))}
                                                <td style={{ padding: '0.75rem', border: '1px solid #dee2e6' }}>
                                                    <input
                                                        type="text"
                                                        value={variant.sku}
                                                        onChange={(e) => updateVariant(variant.id, 'sku', e.target.value)}
                                                        style={{
                                                            width: '100px',
                                                            padding: '0.5rem',
                                                            border: '1px solid #ced4da',
                                                            borderRadius: '4px',
                                                            fontSize: '0.9rem'
                                                        }}
                                                        placeholder="SKU"
                                                    />
                                                </td>
                                                <td style={{ padding: '0.75rem', border: '1px solid #dee2e6' }}>
                                                    <input
                                                        type="number"
                                                        value={variant.price}
                                                        onChange={(e) => updateVariant(variant.id, 'price', parseFloat(e.target.value) || 0)}
                                                        style={{
                                                            width: '100px',
                                                            padding: '0.5rem',
                                                            border: '1px solid #ced4da',
                                                            borderRadius: '4px',
                                                            fontSize: '0.9rem'
                                                        }}
                                                        step="0.01"
                                                        min="0"
                                                        placeholder="0.00"
                                                    />
                                                </td>
                                                <td style={{ padding: '0.75rem', border: '1px solid #dee2e6' }}>
                                                    <input
                                                        type="number"
                                                        value={variant.stock}
                                                        onChange={(e) => updateVariant(variant.id, 'stock', parseInt(e.target.value) || 0)}
                                                        style={{
                                                            width: '80px',
                                                            padding: '0.5rem',
                                                            border: '1px solid #ced4da',
                                                            borderRadius: '4px',
                                                            fontSize: '0.9rem'
                                                        }}
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

                    <div style={{ display: 'flex', gap: '1rem' }}>
                        <button
                            onClick={handleSaveItem}
                            disabled={!newItem.name || !newItem.description}
                            style={{
                                padding: '0.75rem 1.5rem',
                                background: newItem.name && newItem.description ? '#28a745' : '#6c757d',
                                color: 'white',
                                border: 'none',
                                borderRadius: '4px',
                                cursor: newItem.name && newItem.description ? 'pointer' : 'not-allowed',
                                fontSize: '1rem'
                            }}
                        >
                            Save Item
                        </button>
                        <button
                            onClick={() => setShowAddForm(false)}
                            style={{
                                padding: '0.75rem 1.5rem',
                                background: '#6c757d',
                                color: 'white',
                                border: 'none',
                                borderRadius: '4px',
                                cursor: 'pointer',
                                fontSize: '1rem'
                            }}
                        >
                            Cancel
                        </button>
                    </div>
                </div>
            )}

            <div>
                <h3>Current Items ({items.length})</h3>
                {items.length === 0 ? (
                    <p style={{ color: '#6c757d', fontStyle: 'italic' }}>
                        No items added yet. Click "Add New Item" to create your first product.
                    </p>
                ) : (
                    <div style={{ display: 'grid', gap: '1rem' }}>
                        {items.map(item => (
                            <div key={item.id} style={{
                                border: '1px solid #e1e5e9',
                                borderRadius: '8px',
                                padding: '1.5rem',
                                background: 'white'
                            }}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '1rem' }}>
                                    <div>
                                        <h4 style={{ margin: '0 0 0.5rem 0' }}>{item.name}</h4>
                                        <p style={{ margin: '0 0 1rem 0', color: '#6c757d' }}>{item.description}</p>
                                    </div>
                                    <button
                                        onClick={() => deleteItem(item.id)}
                                        style={{
                                            padding: '0.5rem 1rem',
                                            background: '#dc3545',
                                            color: 'white',
                                            border: 'none',
                                            borderRadius: '4px',
                                            cursor: 'pointer',
                                            fontSize: '0.9rem'
                                        }}
                                    >
                                        Delete
                                    </button>
                                </div>
                                
                                {item.attributes.length > 0 && (
                                    <div style={{ marginBottom: '1rem' }}>
                                        <h5>Attributes:</h5>
                                        {item.attributes.map((attr, index) => (
                                            <span key={index} style={{
                                                display: 'inline-block',
                                                background: '#e9ecef',
                                                padding: '0.25rem 0.5rem',
                                                borderRadius: '4px',
                                                margin: '0.25rem 0.5rem 0.25rem 0',
                                                fontSize: '0.9rem'
                                            }}>
                                                <strong>{attr.name}:</strong> {attr.values.join(', ')}
                                            </span>
                                        ))}
                                    </div>
                                )}

                                <div>
                                    <h5>Variants ({item.variants.length}):</h5>
                                    <div style={{ overflowX: 'auto' }}>
                                        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                                            <thead>
                                                <tr style={{ background: '#f8f9fa' }}>
                                                    {item.attributes.map(attr => (
                                                        <th key={attr.name} style={{ padding: '0.5rem', border: '1px solid #dee2e6', textAlign: 'left' }}>
                                                            {attr.name}
                                                        </th>
                                                    ))}
                                                    <th style={{ padding: '0.5rem', border: '1px solid #dee2e6', textAlign: 'left' }}>SKU</th>
                                                    <th style={{ padding: '0.5rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Price</th>
                                                    <th style={{ padding: '0.5rem', border: '1px solid #dee2e6', textAlign: 'left' }}>Stock</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {item.variants.map(variant => (
                                                    <tr key={variant.id}>
                                                        {item.attributes.map(attr => (
                                                            <td key={attr.name} style={{ padding: '0.5rem', border: '1px solid #dee2e6' }}>
                                                                {variant.attributes[attr.name] || '-'}
                                                            </td>
                                                        ))}
                                                        <td style={{ padding: '0.5rem', border: '1px solid #dee2e6' }}>
                                                            {variant.sku || '-'}
                                                        </td>
                                                        <td style={{ padding: '0.5rem', border: '1px solid #dee2e6' }}>
                                                            ${variant.price.toFixed(2)}
                                                        </td>
                                                        <td style={{ padding: '0.5rem', border: '1px solid #dee2e6' }}>
                                                            {variant.stock}
                                                        </td>
                                                    </tr>
                                                ))}
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}

export default ProductsSection;