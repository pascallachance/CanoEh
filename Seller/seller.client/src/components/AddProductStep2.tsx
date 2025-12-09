import { useState, useEffect } from 'react';
import './AddProductStep2.css';
import { ApiClient } from '../utils/apiClient';
import type { AddProductStep1Data } from './AddProductStep1';

export interface BilingualItemAttribute {
    name_en: string;
    name_fr: string;
    value_en: string;
    value_fr: string;
}

export interface AddProductStep2Data {
    categoryId: string;
    itemAttributes: BilingualItemAttribute[];
}

interface AddProductStep2Props {
    onNext: (data: AddProductStep2Data) => void;
    onBack: () => void;
    step1Data: AddProductStep1Data;
    initialData?: AddProductStep2Data;
}

interface Category {
    id: string;
    name_en: string;
    name_fr: string;
    parentCategoryId?: string;
    createdAt: string;
    updatedAt?: string;
}

function AddProductStep2({ onNext, onBack, initialData }: AddProductStep2Props) {
    const [formData, setFormData] = useState<AddProductStep2Data>(initialData || {
        categoryId: '',
        itemAttributes: []
    });

    const [categories, setCategories] = useState<Category[]>([]);
    const [errors, setErrors] = useState<{ categoryId?: string }>({});
    
    // State for the new bilingual item attributes
    const [newItemAttribute, setNewItemAttribute] = useState({
        name_en: '',
        name_fr: '',
        value_en: '',
        value_fr: ''
    });

    // Fetch categories on component mount
    useEffect(() => {
        const fetchCategories = async () => {
            try {
                const response = await ApiClient.get(`${import.meta.env.VITE_API_SELLER_BASE_URL}/api/Category/GetAllCategories`);
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

        fetchCategories();
    }, []);

    const validateForm = (): boolean => {
        const newErrors: { categoryId?: string } = {};

        if (!formData.categoryId) {
            newErrors.categoryId = 'Category is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleCategoryChange = (value: string) => {
        setFormData(prev => ({ ...prev, categoryId: value }));
        if (errors.categoryId) {
            setErrors(prev => ({ ...prev, categoryId: undefined }));
        }
    };

    const addItemAttribute = () => {
        if (!newItemAttribute.name_en || !newItemAttribute.name_fr || 
            !newItemAttribute.value_en || !newItemAttribute.value_fr) {
            return;
        }

        setFormData(prev => ({
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
        setFormData(prev => ({
            ...prev,
            itemAttributes: prev.itemAttributes.filter((_, i) => i !== index)
        }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            onNext(formData);
        }
    };

    const isAddAttributeDisabled = !newItemAttribute.name_en || !newItemAttribute.name_fr || 
                                    !newItemAttribute.value_en || !newItemAttribute.value_fr;

    return (
        <div className="add-product-step2-container">
            <div className="add-product-step2-content">
                <header className="step-header">
                    <h1>Add New Product</h1>
                    <div className="step-indicator">
                        <span className="step completed">1</span>
                        <span className="step-divider"></span>
                        <span className="step active">2</span>
                        <span className="step-divider"></span>
                        <span className="step">3</span>
                        <span className="step-divider"></span>
                        <span className="step">4</span>
                    </div>
                    <h2>Step 2: Category and Item Attributes</h2>
                    <p>Select a category and add item-specific attributes (optional).</p>
                </header>

                <form className="product-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <div className="form-group full-width">
                            <label htmlFor="categoryId">Category *</label>
                            <select
                                id="categoryId"
                                value={formData.categoryId}
                                onChange={(e) => handleCategoryChange(e.target.value)}
                                className={errors.categoryId ? 'error' : ''}
                            >
                                <option value="">Select a category</option>
                                {categories.map(category => (
                                    <option key={category.id} value={category.id}>
                                        {category.name_en} / {category.name_fr}
                                    </option>
                                ))}
                            </select>
                            {errors.categoryId && (
                                <span className="error-message">{errors.categoryId}</span>
                            )}
                        </div>

                        <div className="item-attributes-section full-width">
                            <h4>Item Attributes (Optional)</h4>
                            <p className="section-description">
                                Add attributes that apply to all variants of this item (e.g., Brand, Material, Warranty).
                            </p>
                            
                            <div className="attribute-input-grid">
                                <div className="attribute-input-group">
                                    <label>Attribute Name (English)</label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.name_en}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_en: e.target.value }))}
                                        placeholder="e.g., Brand"
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label>Attribute Value (English)</label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.value_en}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, value_en: e.target.value }))}
                                        placeholder="e.g., Nike"
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label>Attribute Name (French)</label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.name_fr}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_fr: e.target.value }))}
                                        placeholder="e.g., Marque"
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label>Attribute Value (French)</label>
                                    <input
                                        type="text"
                                        value={newItemAttribute.value_fr}
                                        onChange={(e) => setNewItemAttribute(prev => ({ ...prev, value_fr: e.target.value }))}
                                        placeholder="e.g., Nike"
                                    />
                                </div>
                            </div>
                            
                            <div className="attribute-actions">
                                <button
                                    type="button"
                                    onClick={addItemAttribute}
                                    className="add-attribute-btn"
                                    disabled={isAddAttributeDisabled}
                                >
                                    Add Attribute
                                </button>
                            </div>

                            {formData.itemAttributes.length > 0 && (
                                <div className="added-item-attributes">
                                    <h5>Added Attributes</h5>
                                    {formData.itemAttributes.map((attr, index) => (
                                        <div key={index} className="item-attribute-display">
                                            <div className="attribute-display-content">
                                                <div className="attribute-lang-pair">
                                                    <strong>EN:</strong> {attr.name_en} = {attr.value_en}
                                                </div>
                                                <div className="attribute-lang-pair">
                                                    <strong>FR:</strong> {attr.name_fr} = {attr.value_fr}
                                                </div>
                                            </div>
                                            <button
                                                type="button"
                                                onClick={() => removeItemAttribute(index)}
                                                className="remove-attribute-btn"
                                            >
                                                Remove
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    <div className="form-actions">
                        <button type="button" className="back-btn" onClick={onBack}>
                            Back
                        </button>
                        <button type="submit" className="next-btn">
                            Next Step
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default AddProductStep2;
