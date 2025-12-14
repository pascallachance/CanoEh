import { useState, useMemo } from 'react';
import './AddProductStep3.css';
import type { AddProductStep1Data } from './AddProductStep1';
import type { AddProductStep2Data } from './AddProductStep2';
import StepIndicator from './StepIndicator';
import {
    synchronizeBilingualArrays,
    updateBilingualArrayValue,
    removeBilingualArrayValue,
    validateBilingualArraySync
} from '../utils/bilingualArrayUtils';

export interface ItemAttribute {
    name_en: string;
    name_fr: string;
    values_en: string[];
    values_fr: string[];
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
        values_en: [''],
        values_fr: ['']
    });

    const [attributeError, setAttributeError] = useState('');

    // Memoized synchronized attribute values to avoid redundant computation
    const synchronizedAttributeValues = useMemo(() => {
        return synchronizeBilingualArrays(newAttribute.values_en, newAttribute.values_fr);
    }, [newAttribute.values_en, newAttribute.values_fr]);

    // Memoized disabled state for "Add Value" button
    const isAddValueDisabled = useMemo(() => {
        return synchronizedAttributeValues.values_en.some(value => value.trim() === '') ||
            synchronizedAttributeValues.values_fr.some(value => value.trim() === '');
    }, [synchronizedAttributeValues.values_en, synchronizedAttributeValues.values_fr]);

    const addAttributeValue = () => {
        setNewAttribute(prev => ({
            ...prev,
            values_en: [...prev.values_en, ''],
            values_fr: [...prev.values_fr, '']
        }));
    };

    const removeAttributeValue = (index: number) => {
        setNewAttribute(prev => {
            const { values_en, values_fr } = removeBilingualArrayValue(prev.values_en, prev.values_fr, index);
            return {
                ...prev,
                values_en,
                values_fr
            };
        });
    };

    const updateAttributeValue = (index: number, value: string, language: 'en' | 'fr') => {
        setNewAttribute(prev => {
            const { values_en, values_fr } = updateBilingualArrayValue(prev.values_en, prev.values_fr, index, value, language);
            return {
                ...prev,
                values_en,
                values_fr
            };
        });
    };

    const addAttribute = () => {
        // Clear any previous error
        setAttributeError('');

        if (!newAttribute.name_en || !newAttribute.name_fr) {
            setAttributeError('Attribute names in both languages are required');
            return;
        }

        // Validate synchronized arrays for non-empty values
        const validation = validateBilingualArraySync(
            newAttribute.values_en,
            newAttribute.values_fr,
            {
                filterEmpty: true,
                errorType: 'user',
                customUserErrorMessage: 'Bilingual values must match in both languages',
                allowEmpty: false
            }
        );

        if (!validation.isValid) {
            setAttributeError(validation.errorMessage || "Array synchronization failed.");
            return;
        }

        // Check for duplicate attribute names (case-insensitive)
        const isDuplicate = formData.attributes.some(attr =>
            attr.name_en.toLowerCase() === newAttribute.name_en.toLowerCase() ||
            attr.name_fr.toLowerCase() === newAttribute.name_fr.toLowerCase()
        );

        if (isDuplicate) {
            setAttributeError(`Attribute "${newAttribute.name_en}" or "${newAttribute.name_fr}" already exists. Please use different names.`);
            return;
        }

        setFormData(prev => ({
            ...prev,
            attributes: [...prev.attributes, {
                name_en: newAttribute.name_en,
                name_fr: newAttribute.name_fr,
                values_en: validation.values_en!,
                values_fr: validation.values_fr!
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
        setFormData(prev => ({
            ...prev,
            attributes: prev.attributes.filter((_, i) => i !== index)
        }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        
        // Validate that at least one variant attribute has been added
        if (formData.attributes.length === 0) {
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
                                <div className="values-column">
                                    <label>Values (English)</label>
                                    {synchronizedAttributeValues.values_en.map((value, index) => (
                                        <div key={index} className="value-input-row">
                                            <input
                                                type="text"
                                                value={value}
                                                onChange={(e) => updateAttributeValue(index, e.target.value, 'en')}
                                                placeholder="e.g., Small"
                                            />
                                            {synchronizedAttributeValues.values_en.length > 1 && (
                                                <button
                                                    type="button"
                                                    onClick={() => removeAttributeValue(index)}
                                                    className="remove-value-btn"
                                                >
                                                    Remove
                                                </button>
                                            )}
                                        </div>
                                    ))}
                                </div>
                                <div className="values-column">
                                    <label>Values (French)</label>
                                    {synchronizedAttributeValues.values_fr.map((value, index) => (
                                        <div key={index} className="value-input-row">
                                            <input
                                                type="text"
                                                value={value}
                                                onChange={(e) => updateAttributeValue(index, e.target.value, 'fr')}
                                                placeholder="e.g., Petit"
                                            />
                                            {synchronizedAttributeValues.values_fr.length > 1 && (
                                                <button
                                                    type="button"
                                                    onClick={() => removeAttributeValue(index)}
                                                    className="remove-value-btn"
                                                >
                                                    Remove
                                                </button>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>

                            <div className="attribute-actions">
                                <button
                                    type="button"
                                    onClick={addAttributeValue}
                                    className="add-value-btn"
                                    disabled={isAddValueDisabled}
                                >
                                    Add Value
                                </button>
                                <button
                                    type="button"
                                    onClick={addAttribute}
                                    className="add-attribute-btn"
                                >
                                    Add Attribute
                                </button>
                            </div>
                        </div>

                        {formData.attributes.length > 0 && (
                            <div className="added-attributes">
                                <h4>Added Variant Attributes</h4>
                                {formData.attributes.map((attr, index) => (
                                    <div key={index} className="attribute-display">
                                        <div className="attribute-info">
                                            <div className="attribute-name">
                                                <strong>EN:</strong> {attr.name_en} | <strong>FR:</strong> {attr.name_fr}
                                            </div>
                                            <div className="attribute-values-display">
                                                <div><strong>EN values:</strong> {attr.values_en.join(', ')}</div>
                                                <div><strong>FR values:</strong> {attr.values_fr.join(', ')}</div>
                                            </div>
                                        </div>
                                        <button
                                            type="button"
                                            onClick={() => removeAttribute(index)}
                                            className="remove-attribute-btn"
                                        >
                                            Remove
                                        </button>
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
