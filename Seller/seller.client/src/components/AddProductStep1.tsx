import { useState, useEffect, useRef } from 'react';
import './AddProductStep1.css';
import StepIndicator from './StepIndicator';
import { useLanguage } from '../contexts/LanguageContext';

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
    onDataChange?: (data: AddProductStep1Data) => void;
}

function AddProductStep1({ onNext, onCancel, initialData, editMode = false, onStepNavigate, completedSteps, onDataChange }: AddProductStep1Props) {
    const { t } = useLanguage();
    const initialFormData = initialData || {
        name: '',
        name_fr: '',
        description: '',
        description_fr: ''
    };
    const [formData, setFormData] = useState<AddProductStep1Data>(initialFormData);
    // Ref always holds the latest form data so handleStepNavigate can read it without
    // closing over a stale state snapshot (avoids calling onDataChange on every keystroke).
    const latestFormData = useRef<AddProductStep1Data>(initialFormData);

    const [errors, setErrors] = useState<Partial<AddProductStep1Data>>({});

    // Handle escape key to cancel
    useEffect(() => {
        const handleEscape = (event: KeyboardEvent) => {
            const target = event.target as HTMLElement;
            const isInputField = target.tagName === 'INPUT' || target.tagName === 'TEXTAREA';
            if (event.key === 'Escape' && !isInputField) {
                onCancel();
            }
        };

        document.addEventListener('keydown', handleEscape);
        return () => document.removeEventListener('keydown', handleEscape);
    }, [onCancel]);

    const validateForm = (): boolean => {
        const newErrors: Partial<AddProductStep1Data> = {};

        if (!formData.name.trim()) {
            newErrors.name = t('error.nameEnRequired');
        } else if (formData.name.length > 255) {
            newErrors.name = t('error.nameEnTooLong');
        }

        if (!formData.name_fr.trim()) {
            newErrors.name_fr = t('error.nameFrRequired');
        } else if (formData.name_fr.length > 255) {
            newErrors.name_fr = t('error.nameFrTooLong');
        }

        if (!formData.description.trim()) {
            newErrors.description = t('error.descriptionEnRequired');
        }

        if (!formData.description_fr.trim()) {
            newErrors.description_fr = t('error.descriptionFrRequired');
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleInputChange = (field: keyof AddProductStep1Data, value: string) => {
        setFormData(prev => {
            const updated = { ...prev, [field]: value };
            // Keep the ref in sync so handleStepNavigate always has the latest data.
            latestFormData.current = updated;
            return updated;
        });
        // Clear error when user starts typing
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleStepNavigate = (step: number) => {
        // In edit mode, persist the latest form data before navigating to another step
        // so that changes made in step 1 are not lost when using the step indicator.
        // We read from the ref (not state) to avoid a stale closure and to avoid calling
        // onDataChange on every keystroke, which would cause expensive parent re-renders.
        if (editMode && onDataChange) {
            onDataChange(latestFormData.current);
        }
        onStepNavigate?.(step);
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
                    <h1>{editMode ? t('products.editProduct') : t('products.addNewProduct')}</h1>
                    <StepIndicator 
                        currentStep={1}
                        totalSteps={3}
                        onStepClick={handleStepNavigate}
                        completedSteps={completedSteps || []}
                    />
                    <h2>{t('step1.title')}</h2>
                    <p>{t('step1.subtitle')}</p>
                </header>

                <form className="product-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <div className="form-group full-width">
                            <label htmlFor="name">{t('products.itemName')} *</label>
                            <input
                                type="text"
                                id="name"
                                value={formData.name}
                                onChange={(e) => handleInputChange('name', e.target.value)}
                                placeholder={t('placeholder.itemName')}
                                className={errors.name ? 'error' : ''}
                                maxLength={255}
                            />
                            {errors.name && (
                                <span className="error-message">{errors.name}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="name_fr">{t('products.itemNameFr')} *</label>
                            <input
                                type="text"
                                id="name_fr"
                                value={formData.name_fr}
                                onChange={(e) => handleInputChange('name_fr', e.target.value)}
                                placeholder={t('placeholder.itemNameFr')}
                                className={errors.name_fr ? 'error' : ''}
                                maxLength={255}
                            />
                            {errors.name_fr && (
                                <span className="error-message">{errors.name_fr}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="description">{t('products.description')} *</label>
                            <textarea
                                id="description"
                                value={formData.description}
                                onChange={(e) => handleInputChange('description', e.target.value)}
                                placeholder={t('placeholder.description')}
                                className={errors.description ? 'error' : ''}
                                rows={5}
                            />
                            {errors.description && (
                                <span className="error-message">{errors.description}</span>
                            )}
                        </div>

                        <div className="form-group full-width">
                            <label htmlFor="description_fr">{t('products.descriptionFr')} *</label>
                            <textarea
                                id="description_fr"
                                value={formData.description_fr}
                                onChange={(e) => handleInputChange('description_fr', e.target.value)}
                                placeholder={t('placeholder.descriptionFr')}
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
                            {t('common.cancel')}
                        </button>
                        <button type="submit" className="next-btn">
                            {t('common.nextStep')}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default AddProductStep1;
