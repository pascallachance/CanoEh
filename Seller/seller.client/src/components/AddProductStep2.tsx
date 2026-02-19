import { useState, useEffect } from 'react';
import './AddProductStep2.css';
import { ApiClient } from '../utils/apiClient';
import type { AddProductStep1Data } from './AddProductStep1';
import StepIndicator from './StepIndicator';
import BilingualTagInput, { type BilingualValue } from './BilingualTagInput';

export interface ItemAttribute {
    name_en: string;
    name_fr: string;
    values: BilingualValue[];
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
            newErrors.categoryId = 'Category is required';
        }

        if (formData.variantAttributes.length === 0 && !editMode) {
            newErrors.variantAttributes = 'Please add at least one variant attribute to continue.';
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

    // Build path string for a given node id (e.g. "Dept EN / Dept FR > Category EN / Category FR")
    const getCategoryPath = (nodeId: string): string => {
        const nodeMap = buildNodeMap(allCategoryNodes);
        const parts: string[] = [];
        let current = nodeMap.get(nodeId);
        while (current) {
            parts.unshift(`${current.name_en} / ${current.name_fr}`);
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
            // Update existing attribute
            setFormData(prev => ({
                ...prev,
                variantAttributes: prev.variantAttributes.map((attr, i) =>
                    i === editingVariantAttrIndex ? {
                        name_en: newVariantAttribute.name_en,
                        name_fr: newVariantAttribute.name_fr,
                        values: newVariantAttribute.values
                    } : attr
                )
            }));
            setEditingVariantAttrIndex(null);
        } else {
            // Add new attribute
            setFormData(prev => ({
                ...prev,
                variantAttributes: [...prev.variantAttributes, {
                    name_en: newVariantAttribute.name_en,
                    name_fr: newVariantAttribute.name_fr,
                    values: newVariantAttribute.values
                }]
            }));
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
        
        setFormData(prev => ({
            ...prev,
            variantAttributes: prev.variantAttributes.filter((_, i) => i !== index)
        }));
    };
    
    const editVariantAttribute = (index: number) => {
        if (editingVariantAttrIndex !== null && editingVariantAttrIndex !== index) {
            const confirmSwitch = window.confirm(
                'You are currently editing another attribute. Switching will discard any unsaved changes to that attribute. Do you want to continue?'
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
                'You are currently editing another feature. Switching will discard any unsaved changes to that feature. Do you want to continue?'
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
    
    const isAddVariantAttributeDisabled = !newVariantAttribute.name_en || !newVariantAttribute.name_fr || 
                                           newVariantAttribute.values.length === 0;
    
    const isAddVariantFeatureDisabled = !newVariantFeature.name_en || !newVariantFeature.name_fr;

    return (
        <div className="add-product-step2-container">
            <div className="add-product-step2-content">
                <header className="step-header">
                    <h1>{editMode ? 'Edit Product' : 'Add New Product'}</h1>
                    <StepIndicator 
                        currentStep={2}
                        totalSteps={3}
                        onStepClick={onStepNavigate}
                        completedSteps={completedSteps || [1]}
                    />
                    <h2>Step 2: Category, Variant Attributes and Features</h2>
                    <p>Select a category, define variant attributes (required), and optionally add item attributes and variant features.</p>
                </header>

                <form className="product-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        {/* Category Selection */}
                        <div className="form-group full-width">
                            <label>Category *</label>
                            {formData.categoryId && (
                                <div className="category-selected-path">
                                    <strong>Selected:</strong> {getCategoryPath(formData.categoryId)}
                                    <button
                                        type="button"
                                        className="category-clear-btn"
                                        onClick={() => { setFormData(prev => ({ ...prev, categoryId: '' })); setNavigationPath([]); }}
                                    >
                                        Change
                                    </button>
                                </div>
                            )}
                            {!formData.categoryId && (
                                <div className="category-navigator">
                                    <div className="category-breadcrumb">
                                        <span
                                            className="category-breadcrumb-item"
                                            role="button"
                                            tabIndex={0}
                                            onClick={() => handleBreadcrumbClick(0)}
                                            onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleBreadcrumbClick(0); } }}
                                            style={{ cursor: 'pointer' }}
                                        >
                                            All
                                        </span>
                                        {navigationPath.map((node, index) => (
                                            <span key={node.id}>
                                                <span className="category-breadcrumb-sep"> &gt; </span>
                                                <span
                                                    className="category-breadcrumb-item"
                                                    role="button"
                                                    tabIndex={0}
                                                    onClick={() => handleBreadcrumbClick(index + 1)}
                                                    onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleBreadcrumbClick(index + 1); } }}
                                                    style={{ cursor: 'pointer' }}
                                                >
                                                    {node.name_en}
                                                </span>
                                            </span>
                                        ))}
                                    </div>
                                    <div className="category-node-list">
                                        {currentLevelNodes.length === 0 && (
                                            <p className="category-empty">No categories available.</p>
                                        )}
                                        {currentLevelNodes.map(node => (
                                            <div
                                                key={node.id}
                                                className={`category-node-item category-node-type-${node.nodeType.toLowerCase()}`}
                                                role="button"
                                                className={`category-node-item category-node-type-${node.nodeType.toLowerCase()}`}
                                                role="button"
                                                tabIndex={0}
                                                aria-label={`${node.nodeType === 'Category' ? 'Select category' : 'Navigate to subcategory'}: ${node.name_en} / ${node.name_fr}`}
                                                onClick={() => handleNodeClick(node)}
                                                onKeyDown={(event) => {
                                                    if (event.key === 'Enter' || event.key === ' ') {
                                                        event.preventDefault();
                                                        handleNodeClick(node);
                                                    }
                                                }}
                                                onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); handleNodeClick(node); } }}
                                            >
                                                <span className="category-node-name">{node.name_en} / {node.name_fr}</span>
                                                {node.nodeType !== 'Category' && (
                                                    <span className="category-node-arrow">›</span>
                                                )}
                                                {node.nodeType === 'Category' && (
                                                    <span className="category-node-select">Select</span>
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
                            <h4>Variant Attributes *</h4>
                            <p className="section-description">
                                <strong>At least one variant attribute is required.</strong> Add attributes that create different variants of your product (e.g., Size, Color). Each combination of values will generate a unique variant in the next step.
                            </p>
                            {errors.variantAttributes && (
                                <div className="error-message" role="alert">
                                    {errors.variantAttributes}
                                </div>
                            )}
                            
                            <div className="attribute-input-container">
                                <div className="attribute-names">
                                    <div className="attribute-input-group">
                                        <label>Attribute Name (English)</label>
                                        <input
                                            type="text"
                                            value={newVariantAttribute.name_en}
                                            onChange={(e) => setNewVariantAttribute(prev => ({ ...prev, name_en: e.target.value }))}
                                            placeholder="e.g., Size"
                                        />
                                    </div>
                                    <div className="attribute-input-group">
                                        <label>Attribute Name (French)</label>
                                        <input
                                            type="text"
                                            value={newVariantAttribute.name_fr}
                                            onChange={(e) => setNewVariantAttribute(prev => ({ ...prev, name_fr: e.target.value }))}
                                            placeholder="e.g., Taille"
                                        />
                                    </div>
                                </div>
                                
                                <div className="attribute-values">
                                    <BilingualTagInput
                                        values={newVariantAttribute.values}
                                        onValuesChange={(values) => setNewVariantAttribute(prev => ({ ...prev, values }))}
                                        placeholderEn="e.g., Small, Medium, Large"
                                        placeholderFr="e.g., Petit, Moyen, Grand"
                                        labelEn="Values (English)"
                                        labelFr="Values (French)"
                                        id="variant_attribute_values"
                                    />
                                </div>
                                
                                <div className="attribute-actions">
                                    <button
                                        type="button"
                                        onClick={addVariantAttribute}
                                        className="add-attribute-btn"
                                        disabled={isAddVariantAttributeDisabled}
                                    >
                                        {editingVariantAttrIndex !== null ? 'Update Attribute' : 'Add Attribute'}
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
                                            Cancel
                                        </button>
                                    )}
                                </div>
                            </div>

                            {formData.variantAttributes.length > 0 && (
                                <div className="added-attributes">
                                    <h5>Added Variant Attributes</h5>
                                    {formData.variantAttributes.map((attr, index) => (
                                        <div key={index} className="attribute-display">
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
                                                    {editingVariantAttrIndex === index ? 'Editing...' : 'Edit'}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => removeVariantAttribute(index)}
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

                        {/* Variant Features Section */}
                        <div className="item-attributes-section full-width">
                            <h4>Variant Features (Optional)</h4>
                            <p className="section-description">
                                Add features that can vary by variant but don't create new variants (e.g., Weight, Dimensions). You can specify different values for each variant in the next step.
                            </p>
                            
                            <div className="attribute-input-container">
                                <div className="attribute-names">
                                    <div className="attribute-input-group">
                                        <label>Feature Name (English)</label>
                                        <input
                                            type="text"
                                            value={newVariantFeature.name_en}
                                            onChange={(e) => setNewVariantFeature(prev => ({ ...prev, name_en: e.target.value }))}
                                            placeholder="e.g., Weight"
                                        />
                                    </div>
                                    <div className="attribute-input-group">
                                        <label>Feature Name (French)</label>
                                        <input
                                            type="text"
                                            value={newVariantFeature.name_fr}
                                            onChange={(e) => setNewVariantFeature(prev => ({ ...prev, name_fr: e.target.value }))}
                                            placeholder="e.g., Poids"
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
                                        {editingFeatureIndex !== null ? 'Update Feature' : 'Add Feature'}
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
                                            Cancel
                                        </button>
                                    )}
                                </div>
                            </div>

                            {formData.variantFeatures.length > 0 && (
                                <div className="added-item-attributes">
                                    <h5>Added Variant Features</h5>
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
                                                    {editingFeatureIndex === index ? 'Editing...' : 'Edit'}
                                                </button>
                                                <button
                                                    type="button"
                                                    onClick={() => removeVariantFeature(index)}
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
