import { useState, useEffect } from 'react';
import './AddProductStep2.css';
import { ApiClient } from '../utils/apiClient';
import type { AddProductStep1Data } from './AddProductStep1';
import StepIndicator from './StepIndicator';
import BilingualTagInput, { type BilingualValue } from './BilingualTagInput';
import { useLanguage } from '../contexts/LanguageContext';

export interface ItemAttribute {
    name_en: string;
    name_fr: string;
    values: BilingualValue[];
    isMain?: boolean;
}

export interface AddProductStep2Data {
    categoryId: string;
    variantAttributes: ItemAttribute[];
    variantFeatures: ItemAttribute[];
}

interface AddProductStep2Props {
    onNext: (data: AddProductStep2Data) => void;
    onBack: () => void;
    onCancel: () => void;
    step1Data: AddProductStep1Data;
    initialData?: AddProductStep2Data;
    editMode?: boolean;
    onStepNavigate?: (step: number) => void;
    completedSteps?: number[];
}

interface CategoryNode {
    id: string;
    name_en: string;
    name_fr: string;
    nodeType: string;
    parentId?: string;
    isActive: boolean;
    children: CategoryNode[];
}

function AddProductStep2({ onNext, onBack, onCancel, initialData, editMode = false, onStepNavigate, completedSteps }: AddProductStep2Props) {
    const { t, language } = useLanguage();
    const [formData, setFormData] = useState<AddProductStep2Data>(initialData || {
        categoryId: '',
        variantAttributes: [],
        variantFeatures: []
    });

    const [allCategoryNodes, setAllCategoryNodes] = useState<CategoryNode[]>([]);
    const [navigationPath, setNavigationPath] = useState<CategoryNode[]>([]);
    const [errors, setErrors] = useState<{ categoryId?: string; variantAttributes?: string }>({});
    
    // State for variant attributes
    const [newVariantAttribute, setNewVariantAttribute] = useState({
        name_en: '',
        name_fr: '',
        values: [] as BilingualValue[]
    });
    
    // State for variant features
    const [newVariantFeature, setNewVariantFeature] = useState({
        name_en: '',
        name_fr: '',
        values: [] as BilingualValue[]
    });
    
    // State to track if we're editing an existing attribute
    const [editingVariantAttrIndex, setEditingVariantAttrIndex] = useState<number | null>(null);
    const [editingFeatureIndex, setEditingFeatureIndex] = useState<number | null>(null);

    // Fetch all category nodes on mount
    useEffect(() => {
        const fetchCategoryNodes = async () => {
            try {
                const response = await ApiClient.get(`${import.meta.env.VITE_API_SELLER_BASE_URL}/api/CategoryNode/GetAllCategoryNodes`);
                if (response.ok) {
                    const result = await response.json();
                    if (result.value) {
                        setAllCategoryNodes(result.value);
                        return;
                    }
                }
            } catch (error) {
                console.error('Failed to fetch category nodes:', error);
            }
            setAllCategoryNodes([]);
        };

        fetchCategoryNodes();
    }, []);

    // Normalize isMain: if variant attributes are loaded with no main selected (e.g. pre-migration
    // data where all IsMain=0), auto-promote the first attribute so the radio group always has a
    // selection and the form never saves all-false.
    const variantAttributesLength = formData.variantAttributes.length;
    const hasMainAttribute = formData.variantAttributes.some(a => a.isMain);
    useEffect(() => {
        if (variantAttributesLength > 0 && !hasMainAttribute) {
            setFormData(prev => ({
                ...prev,
                variantAttributes: prev.variantAttributes.map((attr, i) => ({ ...attr, isMain: i === 0 }))
            }));
        }
    }, [variantAttributesLength, hasMainAttribute]);

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
        const newErrors: { categoryId?: string; variantAttributes?: string } = {};

        if (!formData.categoryId) {
            newErrors.categoryId = t('error.categoryRequired');
        }

        if (formData.variantAttributes.length === 0 && !editMode) {
            newErrors.variantAttributes = t('error.variantAttributesRequired');
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    // Build a flat lookup map from all category nodes (flat list from API)
    const buildNodeMap = (nodes: CategoryNode[]): Map<string, CategoryNode> => {
        const map = new Map<string, CategoryNode>();
        nodes.forEach(node => map.set(node.id, node));
        return map;
    };

    // Get children of a node from the flat list
    const getChildren = (parentId: string): CategoryNode[] =>
        allCategoryNodes.filter(n => n.parentId === parentId);

    // Build path string for a given node id using the current UI language
    const getCategoryPath = (nodeId: string): string => {
        const nodeMap = buildNodeMap(allCategoryNodes);
        const parts: string[] = [];
        let current = nodeMap.get(nodeId);
        while (current) {
            parts.unshift(language === 'fr' ? current.name_fr : current.name_en);
            current = current.parentId ? nodeMap.get(current.parentId) : undefined;
        }
        return parts.join(' > ');
    };

    // Get current level nodes to display in the navigator
    const currentLevelNodes = navigationPath.length === 0
        ? allCategoryNodes.filter(n => !n.parentId)
        : getChildren(navigationPath[navigationPath.length - 1].id);

    const handleNodeClick = (node: CategoryNode) => {
        if (node.nodeType === 'Category') {
            // Select this category
            setFormData(prev => ({ ...prev, categoryId: node.id }));
            if (errors.categoryId) {
                setErrors(prev => ({ ...prev, categoryId: undefined }));
            }
        } else {
            // Drill into this node
            setNavigationPath(prev => [...prev, node]);
        }
    };

    const handleBreadcrumbClick = (index: number) => {
        setNavigationPath(prev => prev.slice(0, index));
    };


    // Variant Attributes handlers
    const addVariantAttribute = () => {
        if (!newVariantAttribute.name_en || !newVariantAttribute.name_fr || 
            newVariantAttribute.values.length === 0) {
            return;
        }

        // Validate all values have both en and fr
        const hasIncompleteValues = newVariantAttribute.values.some(v => !v.en || !v.fr);
        if (hasIncompleteValues) {
            return;
        }

        // Check for duplicate attribute names
        const isDuplicate = formData.variantAttributes.some((attr, index) =>
            index !== editingVariantAttrIndex && (
                attr.name_en.toLowerCase() === newVariantAttribute.name_en.toLowerCase() ||
                attr.name_fr.toLowerCase() === newVariantAttribute.name_fr.toLowerCase()
            )
        );

        if (isDuplicate) {
            return;
        }

        if (editingVariantAttrIndex !== null) {
            // Update existing attribute, preserve isMain
            setFormData(prev => ({
                ...prev,
                variantAttributes: prev.variantAttributes.map((attr, i) =>
                    i === editingVariantAttrIndex ? {
                        name_en: newVariantAttribute.name_en,
                        name_fr: newVariantAttribute.name_fr,
                        values: newVariantAttribute.values,
                        isMain: attr.isMain
                    } : attr
                )
            }));
            setEditingVariantAttrIndex(null);
        } else {
            // Add new attribute; mark as main if it's the first one
            setFormData(prev => {
                const isFirstAttribute = prev.variantAttributes.length === 0;
                return {
                    ...prev,
                    variantAttributes: [...prev.variantAttributes, {
                        name_en: newVariantAttribute.name_en,
                        name_fr: newVariantAttribute.name_fr,
                        values: newVariantAttribute.values,
                        isMain: isFirstAttribute
                    }]
                };
            });
        }
        
        // Clear error when an attribute is added
        if (errors.variantAttributes) {
            setErrors(prev => ({ ...prev, variantAttributes: undefined }));
        }
        
        setNewVariantAttribute({
            name_en: '',
            name_fr: '',
            values: []
        });
    };

    const removeVariantAttribute = (index: number) => {
        if (editingVariantAttrIndex === index) {
            setEditingVariantAttrIndex(null);
            setNewVariantAttribute({
                name_en: '',
                name_fr: '',
                values: []
            });
        } else if (editingVariantAttrIndex !== null && index < editingVariantAttrIndex) {
            setEditingVariantAttrIndex(editingVariantAttrIndex - 1);
        }
        
        setFormData(prev => {
            const removedAttr = prev.variantAttributes[index];
            const filtered = prev.variantAttributes.filter((_, i) => i !== index);
            // If removed attr was the main one, promote the first remaining as main
            if (removedAttr?.isMain && filtered.length > 0) {
                filtered[0] = { ...filtered[0], isMain: true };
            }
            return { ...prev, variantAttributes: filtered };
        });
    };
    
    const editVariantAttribute = (index: number) => {
        if (editingVariantAttrIndex !== null && editingVariantAttrIndex !== index) {
            const confirmSwitch = window.confirm(
                t('variantAttr.confirmSwitch')
            );
            if (!confirmSwitch) {
                return;
            }
        }
        
        const attr = formData.variantAttributes[index];
        
        setNewVariantAttribute({
            name_en: attr.name_en,
            name_fr: attr.name_fr,
            values: attr.values
        });
        
        setEditingVariantAttrIndex(index);
    };

    const setMainVariantAttribute = (index: number) => {
        setFormData(prev => ({
            ...prev,
            variantAttributes: prev.variantAttributes.map((attr, i) => ({
                ...attr,
                isMain: i === index
            }))
        }));
    };

    // Variant Features handlers
    const addVariantFeature = () => {
        if (!newVariantFeature.name_en || !newVariantFeature.name_fr) {
            return;
        }

        // Check for duplicate feature names
        const isDuplicate = formData.variantFeatures.some((feat, index) =>
            index !== editingFeatureIndex && (
                feat.name_en.toLowerCase() === newVariantFeature.name_en.toLowerCase() ||
                feat.name_fr.toLowerCase() === newVariantFeature.name_fr.toLowerCase()
            )
        );

        if (isDuplicate) {
            return;
        }

        if (editingFeatureIndex !== null) {
            // Update existing feature
            setFormData(prev => ({
                ...prev,
                variantFeatures: prev.variantFeatures.map((feat, i) =>
                    i === editingFeatureIndex ? {
                        name_en: newVariantFeature.name_en,
                        name_fr: newVariantFeature.name_fr,
                        values: newVariantFeature.values
                    } : feat
                )
            }));
            setEditingFeatureIndex(null);
        } else {
            // Add new feature
            setFormData(prev => ({
                ...prev,
                variantFeatures: [...prev.variantFeatures, {
                    name_en: newVariantFeature.name_en,
                    name_fr: newVariantFeature.name_fr,
                    values: newVariantFeature.values
                }]
            }));
        }
        
        setNewVariantFeature({
            name_en: '',
            name_fr: '',
            values: []
        });
    };

    const removeVariantFeature = (index: number) => {
        if (editingFeatureIndex === index) {
            setEditingFeatureIndex(null);
            setNewVariantFeature({
                name_en: '',
                name_fr: '',
                values: []
            });
        } else if (editingFeatureIndex !== null && index < editingFeatureIndex) {
            setEditingFeatureIndex(editingFeatureIndex - 1);
        }
        
        setFormData(prev => ({
            ...prev,
            variantFeatures: prev.variantFeatures.filter((_, i) => i !== index)
        }));
    };
    
    const editVariantFeature = (index: number) => {
        if (editingFeatureIndex !== null && editingFeatureIndex !== index) {
            const confirmSwitch = window.confirm(
                t('variantFeature.confirmSwitch')
            );
            if (!confirmSwitch) {
                return;
            }
        }
        
        const feat = formData.variantFeatures[index];
        
        setNewVariantFeature({
            name_en: feat.name_en,
            name_fr: feat.name_fr,
            values: feat.values
        });
        
        setEditingFeatureIndex(index);
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            onNext(formData);
        }
    };
    
    const MAX_VARIANT_ATTRIBUTES = 3;
    const variantAttributeLimitReached = editingVariantAttrIndex === null && formData.variantAttributes.length >= MAX_VARIANT_ATTRIBUTES;
    const isAddVariantAttributeDisabled = variantAttributeLimitReached || !newVariantAttribute.name_en || !newVariantAttribute.name_fr || 
                                           newVariantAttribute.values.length === 0;
    
    const isAddVariantFeatureDisabled = !newVariantFeature.name_en || !newVariantFeature.name_fr;

    return (
        <div className="add-product-step2-container">
            <div className="add-product-step2-content">
                <header className="step-header">
                    <h1>{editMode ? t('products.editProduct') : t('products.addNewProduct')}</h1>
                    <StepIndicator 
                        currentStep={2}
                        totalSteps={3}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || [1]}
                    />
                    <h2>{t('step2.title')}</h2>
                    <p>{t('step2.subtitle')}</p>
                </header>

                <form className="product-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        {/* Category Selection */}
                        <div className="form-group full-width">
                            <label>{t('products.category')} *</label>
                            {formData.categoryId && (
                                <div className="category-selected-path">
                                    <strong>{t('category.selected')}</strong> {getCategoryPath(formData.categoryId)}
                                    <button
                                        type="button"
                                        className="category-clear-btn"
                                        onClick={() => { setFormData(prev => ({ ...prev, categoryId: '' })); setNavigationPath([]); }}
                                    >
                                        {t('category.change')}
                                    </button>
                                </div>
                            )}
                            {!formData.categoryId && (
                                <div className="category-navigator">
                                    <div className="category-breadcrumb">
                                        <button
                                            type="button"
                                            className="category-breadcrumb-item"
                                            onClick={() => handleBreadcrumbClick(0)}
                                        >
                                            {t('category.all')}
                                        </button>
                                        {navigationPath.map((node, index) => (
                                            <span key={node.id}>
                                                <span className="category-breadcrumb-sep"> &gt; </span>
                                                <button
                                                    type="button"
                                                    className="category-breadcrumb-item"
                                                    onClick={() => handleBreadcrumbClick(index + 1)}
                                                >
                                                    {language === 'fr' ? node.name_fr : node.name_en}
                                                </button>
                                            </span>
                                        ))}
                                    </div>
                                    <div className="category-node-list">
                                        {currentLevelNodes.length === 0 && (
                                            <p className="category-empty">{t('category.empty')}</p>
                                        )}
                                        {currentLevelNodes.map(node => (
                                            <div
                                                key={node.id}
                                                className={`category-node-item category-node-type-${node.nodeType.toLowerCase()}`}
                                                role="button"
                                                tabIndex={0}
                                                aria-label={`${node.nodeType === 'Category' ? t('category.selectCategoryLabel') : t('category.navigateTo')}: ${language === 'fr' ? node.name_fr : node.name_en}`}
                                                onClick={() => handleNodeClick(node)}
                                                onKeyDown={(event) => {
                                                    if (event.key === 'Enter' || event.key === ' ') {
                                                        event.preventDefault();
                                                        handleNodeClick(node);
                                                    }
                                                }}
                                            >
                                                <span className="category-node-name">{language === 'fr' ? node.name_fr : node.name_en}</span>
                                                {node.nodeType !== 'Category' && (
                                                    <span className="category-node-arrow">›</span>
                                                )}
                                                {node.nodeType === 'Category' && (
                                                    <span className="category-node-select">{t('category.selectLabel')}</span>
                                                )}
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            )}
                            {errors.categoryId && (
                                <span className="error-message">{errors.categoryId}</span>
                            )}
                        </div>

                        {/* Variant Attributes Section */}
                        <div className="variant-attributes-section full-width">
                            <h4>{t('variantAttr.title')}</h4>
                            <p className="section-description">
                                <strong>{t('variantAttr.required')}</strong> {t('variantAttr.description')}
                            </p>
                            {errors.variantAttributes && (
                                <div className="error-message" role="alert">
                                    {errors.variantAttributes}
                                </div>
                            )}
                            
                            <div className="attribute-input-container">
                                <div className="attribute-names">
                                    <div className="attribute-input-group">
                                        <label>{t('products.attributeNameEn')}</label>
                                        <input
                                            type="text"
                                            value={newVariantAttribute.name_en}
                                            onChange={(e) => setNewVariantAttribute(prev => ({ ...prev, name_en: e.target.value }))}
                                            placeholder={t('variantAttr.namePlaceholderEn')}
                                            maxLength={255}
                                        />
                                    </div>
                                    <div className="attribute-input-group">
                                        <label>{t('products.attributeNameFr')}</label>
                                        <input
                                            type="text"
                                            value={newVariantAttribute.name_fr}
                                            onChange={(e) => setNewVariantAttribute(prev => ({ ...prev, name_fr: e.target.value }))}
                                            placeholder={t('variantAttr.namePlaceholderFr')}
                                            maxLength={255}
                                        />
                                    </div>
                                </div>
                                
                                <div className="attribute-values">
                                    <BilingualTagInput
                                        values={newVariantAttribute.values}
                                        onValuesChange={(values) => setNewVariantAttribute(prev => ({ ...prev, values }))}
                                        placeholderEn="e.g., Small, Medium, Large"
                                        placeholderFr="e.g., Petit, Moyen, Grand"
                                        labelEn={t('variantAttr.valuesEn')}
                                        labelFr={t('variantAttr.valuesFr')}
                                        id="variant_attribute_values"
                                    />
                                </div>
                                
                                <div className="attribute-actions">
                                    {variantAttributeLimitReached && (
                                        <p className="attribute-limit-message" role="status" aria-live="polite">{t('variantAttr.maxReached')}</p>
                                    )}
                                    <button
                                        type="button"
                                        onClick={addVariantAttribute}
                                        className="add-attribute-btn"
                                        disabled={isAddVariantAttributeDisabled}
                                    >
                                        {editingVariantAttrIndex !== null ? t('variantAttr.updateButton') : t('products.addAttribute')}
                                    </button>
                                    {editingVariantAttrIndex !== null && (
                                        <button
                                            type="button"
                                            onClick={() => {
                                                setEditingVariantAttrIndex(null);
                                                setNewVariantAttribute({
                                                    name_en: '',
                                                    name_fr: '',
                                                    values: []
                                                });
                                            }}
                                            className="cancel-edit-btn"
                                        >
                                            {t('common.cancel')}
                                        </button>
                                    )}
                                </div>
                            </div>

                            {formData.variantAttributes.length > 0 && (
                                <div className="added-attributes">
                                    <h5>{t('variantAttr.addedTitle')}</h5>
                                    {formData.variantAttributes.map((attr, index) => (
                                        <div key={index} className={`attribute-display${attr.isMain ? ' attribute-display-main' : ''}`}>
                                            <div className="attribute-main-selector">
                                                <label className="main-attribute-label">
                                                    <input
                                                        type="radio"
                                                        name="mainVariantAttribute"
                                                        checked={!!attr.isMain}
                                                        onChange={() => setMainVariantAttribute(index)}
                                                        aria-label={t('variantAttr.setMainAriaLabel')}
                                                    />
                                                    {t('variantAttr.main')}
                                                </label>
                                            </div>
                                            <div className="attribute-info">
                                                <div className="attribute-lang-pair">
                                                    <strong>EN</strong> {attr.name_en}: {attr.values.map(v => v.en).join(',')}
                                                </div>
                                                <div className="attribute-lang-pair">
                                                    <strong>FR</strong> {attr.name_fr}: {attr.values.map(v => v.fr).join(',')}
                                                </div>
                                            </div>
                                            <div className="attribute-action-buttons">
                                                <button
                                                    type="button"
                                                    onClick={() => editVariantAttribute(index)}
                                                    className="edit-attribute-btn"
                                                    disabled={editingVariantAttrIndex === index}
                                                >
                                                    {editingVariantAttrIndex === index ? t('common.editing') : t('products.edit')}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => removeVariantAttribute(index)}
                                                    className="remove-attribute-btn"
                                                >
                                                    {t('products.removeAttribute')}
                                                </button>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* Variant Features Section */}
                        <div className="item-attributes-section full-width">
                            <h4>{t('variantFeature.title')}</h4>
                            <p className="section-description">
                                {t('variantFeature.description')}
                            </p>
                            
                            <div className="attribute-input-container">
                                <div className="attribute-names">
                                    <div className="attribute-input-group">
                                        <label>{t('variantFeature.nameEn')}</label>
                                        <input
                                            type="text"
                                            value={newVariantFeature.name_en}
                                            onChange={(e) => setNewVariantFeature(prev => ({ ...prev, name_en: e.target.value }))}
                                            placeholder={t('variantFeature.namePlaceholderEn')}
                                            maxLength={255}
                                        />
                                    </div>
                                    <div className="attribute-input-group">
                                        <label>{t('variantFeature.nameFr')}</label>
                                        <input
                                            type="text"
                                            value={newVariantFeature.name_fr}
                                            onChange={(e) => setNewVariantFeature(prev => ({ ...prev, name_fr: e.target.value }))}
                                            placeholder={t('variantFeature.namePlaceholderFr')}
                                            maxLength={255}
                                        />
                                    </div>
                                </div>
                                
                                <div className="attribute-actions">
                                    <button
                                        type="button"
                                        onClick={addVariantFeature}
                                        className="add-attribute-btn"
                                        disabled={isAddVariantFeatureDisabled}
                                    >
                                        {editingFeatureIndex !== null ? t('variantFeature.updateButton') : t('variantFeature.addButton')}
                                    </button>
                                    {editingFeatureIndex !== null && (
                                        <button
                                            type="button"
                                            onClick={() => {
                                                setEditingFeatureIndex(null);
                                                setNewVariantFeature({
                                                    name_en: '',
                                                    name_fr: '',
                                                    values: []
                                                });
                                            }}
                                            className="cancel-edit-btn"
                                        >
                                            {t('common.cancel')}
                                        </button>
                                    )}
                                </div>
                            </div>

                            {formData.variantFeatures.length > 0 && (
                                <div className="added-item-attributes">
                                    <h5>{t('variantFeature.addedTitle')}</h5>
                                    {formData.variantFeatures.map((feat, index) => (
                                        <div key={index} className="item-attribute-display">
                                            <div className="attribute-display-content">
                                                <div className="attribute-lang-pair">
                                                    <strong>EN</strong> {feat.name_en}
                                                </div>
                                                <div className="attribute-lang-pair">
                                                    <strong>FR</strong> {feat.name_fr}
                                                </div>
                                            </div>
                                            <div className="attribute-action-buttons">
                                                <button
                                                    type="button"
                                                    onClick={() => editVariantFeature(index)}
                                                    className="edit-attribute-btn"
                                                    disabled={editingFeatureIndex === index}
                                                >
                                                    {editingFeatureIndex === index ? t('common.editing') : t('products.edit')}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => removeVariantFeature(index)}
                                                    className="remove-attribute-btn"
                                                >
                                                    {t('products.removeAttribute')}
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
                            {t('common.back')}
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

export default AddProductStep2;
