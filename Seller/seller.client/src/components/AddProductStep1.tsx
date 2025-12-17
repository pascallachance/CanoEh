import { useState, useEffect } from 'react';
import './AddProductStep1.css';
import StepIndicator from './StepIndicator';

export interface AddProductStep1Data {
    name: string;
    name_fr: string;
    description: string;
    description_fr: string;
}

interface AddProductStep1Props {
    onNext: (data: AddProductStep1Data) => void;
    onCancel: () => void;
    initialData?: AddProductStep1Data;
    editMode?: boolean;
    onStepNavigate?: (step: number) => void;
    completedSteps?: number[];
}

function AddProductStep1({ onNext, onCancel, initialData, editMode = false, onStepNavigate, completedSteps }: AddProductStep1Props) {
    const [formData, setFormData] = useState<AddProductStep1Data>(initialData || {
        name: '',
        name_fr: '',
        description: '',
        description_fr: ''
    });

    const [errors, setErrors] = useState<Partial<AddProductStep1Data>>({});

    // Handle escape key to cancel
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape') {
                onCancel();
            }
        };

        document.addEventListener('keydown', handleEscape);
        return () => document.removeEventListener('keydown', handleEscape);
    }, [onCancel]);

    const validateForm = (): boolean => {
        const newErrors: Partial<AddProductStep1Data> = {};

        if (!formData.name.trim()) {
            newErrors.name = 'Item name (English) is required';
        }

        if (!formData.name_fr.trim()) {
            newErrors.name_fr = 'Item name (French) is required';
        }

        if (!formData.description.trim()) {
            newErrors.description = 'Description (English) is required';
        }

        if (!formData.description_fr.trim()) {
            newErrors.description_fr = 'Description (French) is required';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof AddProductStep1Data, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        // Clear error when user starts typing
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            onNext(formData);
        }
    };

    return (
        <div className="add-product-step1-container">
            <div className="add-product-step1-content">
                <header className="step-header">
                    <h1>{editMode ? 'Edit Product' : 'Add New Product'}</h1>
                    <StepIndicator 
                        currentStep={1}
                        totalSteps={4}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || []}
                    />
                    <h2>Step 1: Item Name and Description</h2>
                    <p>Provide the basic information about your product.</p>
                </header>

                <form className="product-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <div className="form-group full-width">
                            <label htmlFor="name">Item Name (English) *</label>
                            <input
                                type="text"
                                id="name"
                                value={formData.name}
                                onChange={(e) => handleInputChange('name', e.target.value)}
                                placeholder="Enter item name in English"
                                className={errors.name ? 'error' : ''}
                            />
                            {errors.name && (
                                <span className="error-message">{errors.name}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="name_fr">Item Name (French) *</label>
                            <input
                                type="text"
                                id="name_fr"
                                value={formData.name_fr}
                                onChange={(e) => handleInputChange('name_fr', e.target.value)}
                                placeholder="Entrez le nom de l'article en français"
                                className={errors.name_fr ? 'error' : ''}
                            />
                            {errors.name_fr && (
                                <span className="error-message">{errors.name_fr}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="description">Description (English) *</label>
                            <textarea
                                id="description"
                                value={formData.description}
                                onChange={(e) => handleInputChange('description', e.target.value)}
                                placeholder="Enter detailed product description in English"
                                className={errors.description ? 'error' : ''}
                                rows={5}
                            />
                            {errors.description && (
                                <span className="error-message">{errors.description}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="description_fr">Description (French) *</label>
                            <textarea
                                id="description_fr"
                                value={formData.description_fr}
                                onChange={(e) => handleInputChange('description_fr', e.target.value)}
                                placeholder="Entrez une description détaillée du produit en français"
                                className={errors.description_fr ? 'error' : ''}
                                rows={5}
                            />
                            {errors.description_fr && (
                                <span className="error-message">{errors.description_fr}</span>
                            )}
                        </div>
                    </div>

                    <div className="form-actions">
                        <button type="button" className="cancel-btn" onClick={onCancel}>
                            Cancel
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

export default AddProductStep1;
