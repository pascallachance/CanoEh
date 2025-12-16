import { useState } from 'react';
import './AddProductStep3.css';
import type { AddProductStep1Data } from './AddProductStep1';
import type { AddProductStep2Data } from './AddProductStep2';
import StepIndicator from './StepIndicator';
import BilingualTagInput, { type BilingualValue } from './BilingualTagInput';

export interface ItemAttribute {
    name_en: string;
    name_fr: string;
    values: BilingualValue[];
    // Legacy support for old data structure
    values_en?: string[];
    values_fr?: string[];
}

export interface AddProductStep3Data {
    attributes: ItemAttribute[];
}

interface AddProductStep3Props {
    onNext: (data: AddProductStep3Data) => void;
    onBack: () => void;
    step1Data: AddProductStep1Data;
    step2Data: AddProductStep2Data;
    initialData?: AddProductStep3Data;
    editMode?: boolean;
    onStepNavigate?: (step: number) => void;
    completedSteps?: number[];
}

function AddProductStep3({ onNext, onBack, initialData, editMode = false, onStepNavigate, completedSteps }: AddProductStep3Props) {
    // Constants for validation messages
    const REQUIRED_ATTRIBUTE_ERROR = 'Please add at least one variant attribute to continue.';
    
    const [formData, setFormData] = useState<AddProductStep3Data>(initialData || {
        attributes: []
    });

    const [newAttribute, setNewAttribute] = useState({
        name_en: '',
        name_fr: '',
        values: [] as BilingualValue[]
    });

    const [attributeError, setAttributeError] = useState('');
    
    // State to track if we're editing an existing attribute
    const [editingIndex, setEditingIndex] = useState<number | null>(null);

    const addAttribute = () => {
        // Clear any previous error
        setAttributeError('');

        if (!newAttribute.name_en || !newAttribute.name_fr) {
            setAttributeError('Attribute names in both languages are required');
            return;
        }

        if (newAttribute.values.length === 0) {
            setAttributeError('At least one value pair is required');
            return;
        }

        // Validate all values have both en and fr
        const hasIncompleteValues = newAttribute.values.some(v => !v.en || !v.fr);
        if (hasIncompleteValues) {
            setAttributeError('All value pairs must have both English and French values');
            return;
        }

        // Check for duplicate attribute names (case-insensitive), excluding the attribute being edited
        const isDuplicate = formData.attributes.some((attr, index) =>
            index !== editingIndex && (
                attr.name_en.toLowerCase() === newAttribute.name_en.toLowerCase() ||
                attr.name_fr.toLowerCase() === newAttribute.name_fr.toLowerCase()
            )
        );

        if (isDuplicate) {
            setAttributeError(`Attribute "${newAttribute.name_en}" or "${newAttribute.name_fr}" already exists. Please use different names.`);
            return;
        }

        if (editingIndex !== null) {
            // Update existing attribute
            setFormData(prev => ({
                ...prev,
                attributes: prev.attributes.map((attr, i) =>
                    i === editingIndex ? {
                        name_en: newAttribute.name_en,
                        name_fr: newAttribute.name_fr,
                        values: newAttribute.values
                    } : attr
                )
            }));
            setEditingIndex(null);
        } else {
            // Add new attribute
            setFormData(prev => ({
                ...prev,
                attributes: [...prev.attributes, {
                    name_en: newAttribute.name_en,
                    name_fr: newAttribute.name_fr,
                    values: newAttribute.values
                }]
            }));
        }
        
        setNewAttribute({
            name_en: '',
            name_fr: '',
            values: []
        });
    };

    const removeAttribute = (index: number) => {
        // If we're editing this attribute, cancel the edit
        if (editingIndex === index) {
            setEditingIndex(null);
            setNewAttribute({
                name_en: '',
                name_fr: '',
                values: []
            });
            setAttributeError('');
        } else if (editingIndex !== null && index < editingIndex) {
            // Adjust editingIndex if removing an attribute before the one being edited
            setEditingIndex(editingIndex - 1);
        }
        
        setFormData(prev => ({
            ...prev,
            attributes: prev.attributes.filter((_, i) => i !== index)
        }));
    };
    
    const editAttribute = (index: number) => {
        const attr = formData.attributes[index];
        
        // Clear any existing error messages
        setAttributeError('');
        
        setNewAttribute({
            name_en: attr.name_en,
            name_fr: attr.name_fr,
            values: attr.values
        });
        
        setEditingIndex(index);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        
        // Validate that at least one variant attribute has been added (only in add mode)
        if (formData.attributes.length === 0 && !editMode) {
            setAttributeError(REQUIRED_ATTRIBUTE_ERROR);
            return;
        }
        
        onNext(formData);
    };

    return (
        <div className="add-product-step3-container">
            <div className="add-product-step3-content">
                <header className="step-header">
                    <h1>{editMode ? 'Edit Product' : 'Add New Product'}</h1>
                    <StepIndicator 
                        currentStep={3}
                        totalSteps={4}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || [1, 2]}
                    />
                    <h2>Step 3: Variant Attributes *</h2>
                    <p>Define attributes that vary between product versions (e.g., Size, Color). At least one attribute is required.</p>
                </header>

                <form className="product-form" onSubmit={handleSubmit}>
                    <div className="variant-attributes-section">
                        <div className="section-info">
                            <p><strong>At least one variant attribute is required.</strong> Add attributes that create different variants of your product. Each combination of values will generate a unique variant in the next step.</p>
                            <p className="example-text">Example: Add "Size" with values "S, M, L" and "Color" with values "Red, Blue" to create 6 variants.</p>
                        </div>

                        <div className="attribute-input-container">
                            <div className="attribute-names">
                                <div className="attribute-input-group">
                                    <label>Attribute Name (English)</label>
                                    <input
                                        type="text"
                                        value={newAttribute.name_en}
                                        onChange={(e) => {
                                            setNewAttribute(prev => ({ ...prev, name_en: e.target.value }));
                                            if (attributeError) {
                                                setAttributeError('');
                                            }
                                        }}
                                        placeholder="e.g., Size"
                                    />
                                </div>
                                <div className="attribute-input-group">
                                    <label>Attribute Name (French)</label>
                                    <input
                                        type="text"
                                        value={newAttribute.name_fr}
                                        onChange={(e) => {
                                            setNewAttribute(prev => ({ ...prev, name_fr: e.target.value }));
                                            if (attributeError) {
                                                setAttributeError('');
                                            }
                                        }}
                                        placeholder="e.g., Taille"
                                    />
                                </div>
                            </div>

                            {attributeError && (
                                <div className="error-message" role="alert">
                                    {attributeError}
                                </div>
                            )}

                            <div className="attribute-values">
                                <BilingualTagInput
                                    values={newAttribute.values}
                                    onValuesChange={(values) => setNewAttribute(prev => ({ ...prev, values }))}
                                    placeholderEn="e.g., Small, Medium, Large"
                                    placeholderFr="e.g., Petit, Moyen, Grand"
                                    labelEn="Values (English)"
                                    labelFr="Values (French)"
                                    id="attribute_values"
                                />
                            </div>

                            <div className="attribute-actions">
                                <button
                                    type="button"
                                    onClick={addAttribute}
                                    className="add-attribute-btn"
                                >
                                    {editingIndex !== null ? 'Update Attribute' : 'Add Attribute'}
                                </button>
                                {editingIndex !== null && (
                                    <button
                                        type="button"
                                        onClick={() => {
                                            setEditingIndex(null);
                                            setNewAttribute({
                                                name_en: '',
                                                name_fr: '',
                                                values: []
                                            });
                                            setAttributeError('');
                                        }}
                                        className="cancel-edit-btn"
                                    >
                                        Cancel
                                    </button>
                                )}
                            </div>
                        </div>

                        {formData.attributes.length > 0 && (
                            <div className="added-attributes">
                                <h4>Added Variant Attributes</h4>
                                {formData.attributes.map((attr, index) => (
                                    <div key={index} className="attribute-display">
                                        <div className="attribute-info">
                                            <div className="attribute-lang-pair">
                                                (en) {attr.name_en}: {attr.values.map(v => v.en).join(',')}
                                            </div>
                                            <div className="attribute-lang-pair">
                                                (fr) {attr.name_fr}: {attr.values.map(v => v.fr).join(',')}
                                            </div>
                                        </div>
                                        <div className="attribute-action-buttons">
                                            <button
                                                type="button"
                                                onClick={() => editAttribute(index)}
                                                className="edit-attribute-btn"
                                                disabled={editingIndex === index}
                                            >
                                                {editingIndex === index ? 'Editing...' : 'Edit'}
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => removeAttribute(index)}
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

                    <div className="form-actions">
                        <button type="button" className="back-btn" onClick={onBack}>
                            Back
                        </button>
                        <button type="submit" className="next-btn">
                            {formData.attributes.length > 0 ? 'Generate Variants' : 'Add Attribute to Continue'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default AddProductStep3;
