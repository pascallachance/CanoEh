import { useState, useEffect } from 'react';
import './AddProductStep2.css';
import { ApiClient } from '../utils/apiClient';
import type { AddProductStep1Data } from './AddProductStep1';
import StepIndicator from './StepIndicator';
import BilingualTagInput, { type BilingualValue } from './BilingualTagInput';

export interface BilingualItemAttribute {
    name_en: string;
    name_fr: string;
    value_en: string[];
    value_fr: string[];
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
    editMode?: boolean;
    onStepNavigate?: (step: number) => void;
    completedSteps?: number[];
}

interface Category {
    id: string;
    name_en: string;
    name_fr: string;
    parentCategoryId?: string;
    createdAt: string;
    updatedAt?: string;
}

function AddProductStep2({ onNext, onBack, initialData, editMode = false, onStepNavigate, completedSteps }: AddProductStep2Props) {
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
        values: [] as BilingualValue[]
    });
    
    // State to track if we're editing an existing attribute
    const [editingIndex, setEditingIndex] = useState<number | null>(null);

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
            newItemAttribute.values.length === 0) {
            return;
        }

        // Convert BilingualValue[] to separate arrays for storage
        const value_en = newItemAttribute.values.map(v => v.en);
        const value_fr = newItemAttribute.values.map(v => v.fr);

        if (editingIndex !== null) {
            // Update existing attribute
            setFormData(prev => ({
                ...prev,
                itemAttributes: prev.itemAttributes.map((attr, i) => 
                    i === editingIndex ? {
                        name_en: newItemAttribute.name_en,
                        name_fr: newItemAttribute.name_fr,
                        value_en,
                        value_fr
                    } : attr
                )
            }));
            setEditingIndex(null);
        } else {
            // Add new attribute
            setFormData(prev => ({
                ...prev,
                itemAttributes: [...prev.itemAttributes, {
                    name_en: newItemAttribute.name_en,
                    name_fr: newItemAttribute.name_fr,
                    value_en,
                    value_fr
                }]
            }));
        }
        
        setNewItemAttribute({
            name_en: '',
            name_fr: '',
            values: []
        });
    };

    const removeItemAttribute = (index: number) => {
        // If we're editing this attribute, cancel the edit
        if (editingIndex === index) {
            setEditingIndex(null);
            setNewItemAttribute({
                name_en: '',
                name_fr: '',
                values: []
            });
        } else if (editingIndex !== null && index < editingIndex) {
            // Adjust editingIndex if removing an attribute before the one being edited
            setEditingIndex(editingIndex - 1);
        }
        
        setFormData(prev => ({
            ...prev,
            itemAttributes: prev.itemAttributes.filter((_, i) => i !== index)
        }));
    };
    
    const editItemAttribute = (index: number) => {
        // If already editing a different attribute, confirm before switching
        if (editingIndex !== null && editingIndex !== index) {
            const confirmSwitch = window.confirm(
                'You are currently editing another attribute. Switching will discard any unsaved changes to that attribute. Do you want to continue?'
            );
            if (!confirmSwitch) {
                return;
            }
        }
        
        const attr = formData.itemAttributes[index];
        
        // Convert the separate arrays back to BilingualValue[]
        const values: BilingualValue[] = attr.value_en.map((en, i) => ({
            en,
            fr: attr.value_fr[i]
        }));
        
        setNewItemAttribute({
            name_en: attr.name_en,
            name_fr: attr.name_fr,
            values
        });
        
        setEditingIndex(index);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            onNext(formData);
        }
    };

    const isAddAttributeDisabled = !newItemAttribute.name_en || !newItemAttribute.name_fr || 
                                    newItemAttribute.values.length === 0;

    return (
        <div className="add-product-step2-container">
            <div className="add-product-step2-content">
                <header className="step-header">
                    <h1>{editMode ? 'Edit Product' : 'Add New Product'}</h1>
                    <StepIndicator 
                        currentStep={2}
                        totalSteps={4}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || [1]}
                    />
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
                            
                            <div className="attribute-input-container">
                                <div className="attribute-names">
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
                                        <label>Attribute Name (French)</label>
                                        <input
                                            type="text"
                                            value={newItemAttribute.name_fr}
                                            onChange={(e) => setNewItemAttribute(prev => ({ ...prev, name_fr: e.target.value }))}
                                            placeholder="e.g., Marque"
                                        />
                                    </div>
                                </div>
                                
                                <div className="attribute-values">
                                    <BilingualTagInput
                                        values={newItemAttribute.values}
                                        onValuesChange={(values) => setNewItemAttribute(prev => ({ ...prev, values }))}
                                        placeholderEn="e.g., Nike"
                                        placeholderFr="e.g., Nike"
                                        labelEn="Values (English)"
                                        labelFr="Values (French)"
                                        id="attribute_values"
                                    />
                                </div>
                                
                                <div className="attribute-actions">
                                    <button
                                        type="button"
                                        onClick={addItemAttribute}
                                        className="add-attribute-btn"
                                        disabled={isAddAttributeDisabled}
                                    >
                                        {editingIndex !== null ? 'Update Attribute' : 'Add Attribute'}
                                    </button>
                                    {editingIndex !== null && (
                                        <button
                                            type="button"
                                            onClick={() => {
                                                setEditingIndex(null);
                                                setNewItemAttribute({
                                                    name_en: '',
                                                    name_fr: '',
                                                    values: []
                                                });
                                            }}
                                            className="cancel-edit-btn"
                                        >
                                            Cancel
                                        </button>
                                    )}
                                </div>
                            </div>

                            {formData.itemAttributes.length > 0 && (
                                <div className="added-item-attributes">
                                    <h5>Added Attributes</h5>
                                    {formData.itemAttributes.map((attr, index) => (
                                        <div key={index} className="item-attribute-display">
                                            <div className="attribute-display-content">
                                                <div className="attribute-lang-pair">
                                                    (en) {attr.name_en}: {attr.value_en.join(',')}
                                                </div>
                                                <div className="attribute-lang-pair">
                                                    (fr) {attr.name_fr}: {attr.value_fr.join(',')}
                                                </div>
                                            </div>
                                            <div className="attribute-action-buttons">
                                                <button
                                                    type="button"
                                                    onClick={() => editItemAttribute(index)}
                                                    className="edit-attribute-btn"
                                                    disabled={editingIndex === index}
                                                >
                                                    {editingIndex === index ? 'Editing...' : 'Edit'}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => removeItemAttribute(index)}
                                                    className="remove-attribute-btn"
                                                >
                                                    Remove
                                                </button>
                                            </div>
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
