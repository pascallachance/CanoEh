import { useState, useEffect } from 'react';
import './AddProductStep2.css';
import { ApiClient } from '../utils/apiClient';
import type { AddProductStep1Data } from './AddProductStep1';
import StepIndicator from './StepIndicator';
import BilingualTagInput, { type BilingualValue } from './BilingualTagInput';
import { useLanguage } from '../contexts/LanguageContext';

export interface ItemAttribute {
    clientId: string;
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
    const [errors, setErrors] = useState<{ categoryId?: string; variantAttributes?: string; variantFeatures?: string }>({});

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
        const newErrors: { categoryId?: string; variantAttributes?: string; variantFeatures?: string } = {};

        if (!formData.categoryId) {
            newErrors.categoryId = t('error.categoryRequired');
        }

        if (formData.variantAttributes.length === 0 && !editMode) {
            newErrors.variantAttributes = t('error.variantAttributesRequired');
        } else {
            const seenEnglishNames = new Set<string>();
            const seenFrenchNames = new Set<string>();

            const hasInvalidVariantAttributes = formData.variantAttributes.some(attr => {
                const trimmedNameEn = attr.name_en.trim();
                const trimmedNameFr = attr.name_fr.trim();

                if (!trimmedNameEn || !trimmedNameFr || attr.values.length === 0) {
                    return true;
                }

                const normalizedNameEn = trimmedNameEn.toLowerCase();
                const normalizedNameFr = trimmedNameFr.toLowerCase();

                if (seenEnglishNames.has(normalizedNameEn) || seenFrenchNames.has(normalizedNameFr)) {
                    return true;
                }

                seenEnglishNames.add(normalizedNameEn);
                seenFrenchNames.add(normalizedNameFr);

                return attr.values.some(value => !value.en.trim() || !value.fr.trim());
            });

            if (hasInvalidVariantAttributes) {
                newErrors.variantAttributes = t('error.variantAttributesIncomplete');
            }
        }

        // Catch partially-filled feature rows (one name filled but not the other)
        const hasPartialFeature = formData.variantFeatures.some(feat => {
            const hasEn = feat.name_en.trim().length > 0;
            const hasFr = feat.name_fr.trim().length > 0;
            return hasEn !== hasFr;
        });
        if (hasPartialFeature) {
            newErrors.variantFeatures = t('error.variantFeaturesIncomplete');
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
    const updateVariantAttribute = (index: number, field: 'name_en' | 'name_fr', value: string) => {
        setFormData(prev => ({
            ...prev,
            variantAttributes: prev.variantAttributes.map((attr, i) =>
                i === index ? { ...attr, [field]: value } : attr
            )
        }));
    };

    const updateVariantAttributeValues = (index: number, values: BilingualValue[]) => {
        setFormData(prev => ({
            ...prev,
            variantAttributes: prev.variantAttributes.map((attr, i) =>
                i === index ? { ...attr, values } : attr
            )
        }));
    };

    const addNewVariantAttribute = () => {
        setFormData(prev => ({
            ...prev,
            variantAttributes: [...prev.variantAttributes, {
                clientId: crypto.randomUUID(),
                name_en: '',
                name_fr: '',
                values: [],
                isMain: prev.variantAttributes.length === 0
            }]
        }));
        if (errors.variantAttributes) {
            setErrors(prev => ({ ...prev, variantAttributes: undefined }));
        }
    };

    const removeVariantAttribute = (index: number) => {
        setFormData(prev => {
            const removedAttr = prev.variantAttributes[index];
            const filtered = prev.variantAttributes.filter((_, i) => i !== index);
            if (removedAttr?.isMain && filtered.length > 0) {
                filtered[0] = { ...filtered[0], isMain: true };
            }
            return { ...prev, variantAttributes: filtered };
        });
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
    const updateVariantFeature = (index: number, field: 'name_en' | 'name_fr', value: string) => {
        setFormData(prev => ({
            ...prev,
            variantFeatures: prev.variantFeatures.map((feat, i) =>
                i === index ? { ...feat, [field]: value } : feat
            )
        }));
    };

    const addNewVariantFeature = () => {
        setFormData(prev => ({
            ...prev,
            variantFeatures: [...prev.variantFeatures, {
                clientId: crypto.randomUUID(),
                name_en: '',
                name_fr: '',
                values: []
            }]
        }));
    };

    const removeVariantFeature = (index: number) => {
        setFormData(prev => ({
            ...prev,
            variantFeatures: prev.variantFeatures.filter((_, i) => i !== index)
        }));
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (validateForm()) {
            // Filter out completely blank feature rows before proceeding
            const cleanedData: AddProductStep2Data = {
                ...formData,
                variantFeatures: formData.variantFeatures.filter(
                    feat => feat.name_en.trim() || feat.name_fr.trim()
                )
            };
            onNext(cleanedData);
        }
    };
    
    const MAX_VARIANT_ATTRIBUTES = 3;
    const variantAttributeLimitReached = formData.variantAttributes.length >= MAX_VARIANT_ATTRIBUTES;
    const lastVariantAttr = formData.variantAttributes[formData.variantAttributes.length - 1];
    const isAddVariantAttributeDisabled = variantAttributeLimitReached ||
        (formData.variantAttributes.length > 0 && lastVariantAttr != null && (!lastVariantAttr.name_en || !lastVariantAttr.name_fr || lastVariantAttr.values.length === 0));

    const lastVariantFeature = formData.variantFeatures[formData.variantFeatures.length - 1];
    const isAddVariantFeatureDisabled = formData.variantFeatures.length > 0 && lastVariantFeature != null &&
        (!lastVariantFeature.name_en || !lastVariantFeature.name_fr);

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
                                <div
                                    className="category-selected-path"
                                    role="button"
                                    tabIndex={0}
                                    title={t('category.changeHint')}
                                    onDoubleClick={() => { setFormData(prev => ({ ...prev, categoryId: '' })); setNavigationPath([]); }}
                                    onKeyDown={(e) => {
                                        if (e.key === 'Enter' || e.key === ' ') {
                                            e.preventDefault();
                                            setFormData(prev => ({ ...prev, categoryId: '' }));
                                            setNavigationPath([]);
                                        }
                                    }}
                                    aria-label={`${t('category.selected')} ${getCategoryPath(formData.categoryId)}. ${t('category.changeHint')}`}
                                >
                                    <strong>{t('category.selected')}</strong> {getCategoryPath(formData.categoryId)}
                                    <span className="category-change-hint">{t('category.changeHint')}</span>
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
                            {errors.variantAttributes && (
                                <div className="error-message" role="alert">
                                    {errors.variantAttributes}
                                </div>
                            )}

                            {formData.variantAttributes.map((attr, index) => (
                                <div key={attr.clientId} className={`attribute-input-container${attr.isMain ? ' attribute-input-container-main' : ''}`}>
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
                                    <div className="attribute-names">
                                        <div className="attribute-input-group">
                                            <label>{t('products.attributeNameEn')}</label>
                                            <input
                                                type="text"
                                                value={attr.name_en}
                                                onChange={(e) => updateVariantAttribute(index, 'name_en', e.target.value)}
                                                placeholder={t('variantAttr.namePlaceholderEn')}
                                                maxLength={255}
                                            />
                                        </div>
                                        <div className="attribute-input-group">
                                            <label>{t('products.attributeNameFr')}</label>
                                            <input
                                                type="text"
                                                value={attr.name_fr}
                                                onChange={(e) => updateVariantAttribute(index, 'name_fr', e.target.value)}
                                                placeholder={t('variantAttr.namePlaceholderFr')}
                                                maxLength={255}
                                            />
                                        </div>
                                    </div>
                                    <div className="attribute-values">
                                        <BilingualTagInput
                                            values={attr.values}
                                            onValuesChange={(values) => updateVariantAttributeValues(index, values)}
                                            placeholderEn="e.g., Small, Medium, Large"
                                            placeholderFr="e.g., Petit, Moyen, Grand"
                                            labelEn={t('variantAttr.valuesEn')}
                                            labelFr={t('variantAttr.valuesFr')}
                                            id={`variant_attribute_values_${index}`}
                                        />
                                    </div>
                                    <div className="attribute-actions">
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

                            <div className="attribute-actions">
                                {variantAttributeLimitReached && (
                                    <span className="sr-only" role="status" aria-live="polite">
                                        {t('variantAttr.maxReached')}
                                    </span>
                                )}
                                <button
                                    type="button"
                                    onClick={addNewVariantAttribute}
                                    className="add-attribute-btn"
                                    disabled={isAddVariantAttributeDisabled}
                                    style={variantAttributeLimitReached ? { visibility: 'hidden' } : undefined}
                                >
                                    {t('products.addAttribute')}
                                </button>
                            </div>
                        </div>

                        {/* Variant Features Section */}
                        <h4 style={{ textAlign: 'left' }}>{t('variantFeature.title')}</h4>
                        <div className="item-attributes-section full-width">
                            {errors.variantFeatures && (
                                <div className="error-message" role="alert">
                                    {errors.variantFeatures}
                                </div>
                            )}

                            {formData.variantFeatures.map((feat, index) => (
                                <div key={feat.clientId} className="attribute-input-container">
                                    <div className="attribute-names">
                                        <div className="attribute-input-group">
                                            <label>{t('variantFeature.nameEn')}</label>
                                            <input
                                                type="text"
                                                value={feat.name_en}
                                                onChange={(e) => updateVariantFeature(index, 'name_en', e.target.value)}
                                                placeholder={t('variantFeature.namePlaceholderEn')}
                                                maxLength={255}
                                            />
                                        </div>
                                        <div className="attribute-input-group">
                                            <label>{t('variantFeature.nameFr')}</label>
                                            <input
                                                type="text"
                                                value={feat.name_fr}
                                                onChange={(e) => updateVariantFeature(index, 'name_fr', e.target.value)}
                                                placeholder={t('variantFeature.namePlaceholderFr')}
                                                maxLength={255}
                                            />
                                        </div>
                                    </div>
                                    <div className="attribute-actions">
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

                            <div className="attribute-actions">
                                <button
                                    type="button"
                                    onClick={addNewVariantFeature}
                                    className="add-attribute-btn"
                                    disabled={isAddVariantFeatureDisabled}
                                >
                                    {t('variantFeature.addButton')}
                                </button>
                            </div>
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
