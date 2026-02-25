import { useState, useEffect, useCallback, forwardRef, useImperativeHandle } from 'react';
import './CategoryNodesSection.css';
import { ApiClient } from '../../utils/apiClient';
import { useLanguage } from '../../contexts/LanguageContext';
import { useNotifications } from '../../contexts/useNotifications';

interface CategoryNode {
    id: string;
    name_en: string;
    name_fr: string;
    nodeType: string;
    parentId: string | null;
    isActive: boolean;
    sortOrder: number | null;
    children: CategoryNode[];
}

const NODE_TYPES = ['Departement', 'Navigation', 'Category'] as const;
type NodeType = typeof NODE_TYPES[number];

interface CreateNodeForm {
    name_en: string;
    name_fr: string;
    nodeType: NodeType;
    parentId: string;
    isActive: boolean;
    sortOrder: string;
}

interface MoveNodeForm {
    newParentId: string;
}

export interface CategoryNodesSectionRef {
    openCreateModal: () => void;
}

// Sort nodes alphabetically by display name, recursively
function sortNodes(nodes: CategoryNode[], language: string): CategoryNode[] {
    return [...nodes]
        .sort((a, b) => {
            const nameA = language === 'fr' ? a.name_fr : a.name_en;
            const nameB = language === 'fr' ? b.name_fr : b.name_en;
            return nameA.localeCompare(nameB);
        })
        .map(node => ({
            ...node,
            children: sortNodes(node.children, language),
        }));
}

// Flatten tree into a list for parent selection dropdowns
function flattenNodes(nodes: CategoryNode[], result: CategoryNode[] = []): CategoryNode[] {
    for (const node of nodes) {
        result.push(node);
        if (node.children?.length) {
            flattenNodes(node.children, result);
        }
    }
    return result;
}

// Collect IDs of a node and all its descendants (to prevent moving into own subtree)
function collectSubtreeIds(node: CategoryNode): Set<string> {
    const ids = new Set<string>();
    const queue = [node];
    while (queue.length > 0) {
        const current = queue.shift()!;
        ids.add(current.id);
        for (const child of current.children) {
            queue.push(child);
        }
    }
    return ids;
}

// Recursive tree node component
interface TreeNodeProps {
    node: CategoryNode;
    depth: number;
    language: string;
    onDelete: (node: CategoryNode) => void;
    onMove: (node: CategoryNode) => void;
    t: (key: string) => string;
}

function TreeNodeRow({ node, depth, language, onDelete, onMove, t }: TreeNodeProps) {
    const [expanded, setExpanded] = useState(true);
    const hasChildren = node.children?.length > 0;
    const displayName = language === 'fr' ? node.name_fr : node.name_en;
    const secondaryName = language === 'fr' ? node.name_en : node.name_fr;

    const badgeClass = node.nodeType === 'Departement'
        ? 'departement'
        : node.nodeType === 'Navigation'
            ? 'navigation'
            : 'category';

    const typeLabel = node.nodeType === 'Departement'
        ? t('categories.departement')
        : node.nodeType === 'Navigation'
            ? t('categories.navigation')
            : t('categories.category');

    return (
        <div className="tree-node">
            <div className="tree-node-row">
                <div className="tree-node-indent" style={{ width: `${depth * 24}px` }} />
                {hasChildren ? (
                    <button
                        className="tree-toggle"
                        onClick={() => setExpanded(prev => !prev)}
                        aria-label={expanded ? 'Collapse' : 'Expand'}
                    >
                        {expanded ? '▾' : '▸'}
                    </button>
                ) : (
                    <div className="tree-toggle-placeholder" />
                )}
                <span className={`node-badge ${badgeClass}`}>{typeLabel}</span>
                <span className="node-name">
                    {displayName}
                    {secondaryName && secondaryName !== displayName && (
                        <span className="node-name-secondary">({secondaryName})</span>
                    )}
                </span>
                <div className="node-actions">
                    {node.nodeType !== 'Departement' && (
                        <button
                            className="btn-move"
                            onClick={() => onMove(node)}
                            title={t('categories.move')}
                        >
                            {t('categories.move')}
                        </button>
                    )}
                    <button
                        className="btn-danger"
                        onClick={() => onDelete(node)}
                        title={t('categories.delete')}
                    >
                        {t('categories.delete')}
                    </button>
                </div>
            </div>
            {hasChildren && expanded && (
                <div className="tree-children">
                    {node.children.map(child => (
                        <TreeNodeRow
                            key={child.id}
                            node={child}
                            depth={depth + 1}
                            language={language}
                            onDelete={onDelete}
                            onMove={onMove}
                            t={t}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}

const CategoryNodesSection = forwardRef<CategoryNodesSectionRef, object>(function CategoryNodesSection(_props, ref) {
    const { language, t } = useLanguage();
    const { showSuccess, showError } = useNotifications();
    const baseUrl = import.meta.env.VITE_API_ADMIN_BASE_URL;

    const [nodes, setNodes] = useState<CategoryNode[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Create modal state
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [createForm, setCreateForm] = useState<CreateNodeForm>({
        name_en: '',
        name_fr: '',
        nodeType: 'Departement',
        parentId: '',
        isActive: true,
        sortOrder: '',
    });
    const [creating, setCreating] = useState(false);

    // Move modal state
    const [showMoveModal, setShowMoveModal] = useState(false);
    const [movingNode, setMovingNode] = useState<CategoryNode | null>(null);
    const [moveForm, setMoveForm] = useState<MoveNodeForm>({ newParentId: '' });
    const [moving, setMoving] = useState(false);

    const loadNodes = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const response = await ApiClient.get(`${baseUrl}/api/CategoryNode/GetAllCategoryNodes`);
            if (response.ok) {
                const result = await response.json();
                const data: CategoryNode[] = result.value ?? [];
                setNodes(data);
            } else {
                const text = await response.text();
                setError(text || t('categories.error'));
            }
        } catch {
            setError(t('categories.error'));
        } finally {
            setLoading(false);
        }
    }, [baseUrl, t]);

    useEffect(() => {
        loadNodes();
    }, [loadNodes]);

    // --- Create node ---
    const openCreateModal = () => {
        setCreateForm({
            name_en: '',
            name_fr: '',
            nodeType: 'Departement',
            parentId: '',
            isActive: true,
            sortOrder: '',
        });
        setShowCreateModal(true);
    };

    useImperativeHandle(ref, () => ({
        openCreateModal,
    }));

    const handleCreateSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setCreating(true);
        try {
            const body: Record<string, unknown> = {
                name_en: createForm.name_en,
                name_fr: createForm.name_fr,
                nodeType: createForm.nodeType,
                isActive: createForm.isActive,
            };
            if (createForm.parentId) {
                body.parentId = createForm.parentId;
            }
            if (createForm.sortOrder !== '') {
                body.sortOrder = parseInt(createForm.sortOrder, 10);
            }

            const response = await ApiClient.post(`${baseUrl}/api/CategoryNode/CreateCategoryNode`, body);
            if (response.ok) {
                showSuccess(t('categories.createSuccess'));
                setShowCreateModal(false);
                await loadNodes();
            } else {
                const text = await response.text();
                showError(text || t('categories.createError'));
            }
        } catch {
            showError(t('categories.createError'));
        } finally {
            setCreating(false);
        }
    };

    // --- Delete node ---
    const handleDelete = async (node: CategoryNode) => {
        if (!window.confirm(t('categories.deleteConfirm'))) return;
        try {
            const response = await ApiClient.delete(`${baseUrl}/api/CategoryNode/DeleteCategoryNode/${node.id}`);
            if (response.ok) {
                showSuccess(t('categories.deleteSuccess'));
                await loadNodes();
            } else {
                const text = await response.text();
                showError(text || t('categories.deleteError'));
            }
        } catch {
            showError(t('categories.deleteError'));
        }
    };

    // --- Move node ---
    const openMoveModal = (node: CategoryNode) => {
        setMovingNode(node);
        setMoveForm({ newParentId: node.parentId ?? '' });
        setShowMoveModal(true);
    };

    const handleMoveSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!movingNode) return;
        setMoving(true);
        try {
            const body = {
                id: movingNode.id,
                name_en: movingNode.name_en,
                name_fr: movingNode.name_fr,
                parentId: moveForm.newParentId || null,
                isActive: movingNode.isActive,
                sortOrder: movingNode.sortOrder,
            };
            const response = await ApiClient.put(`${baseUrl}/api/CategoryNode/UpdateCategoryNode`, body);
            if (response.ok) {
                showSuccess(t('categories.moveSuccess'));
                setShowMoveModal(false);
                setMovingNode(null);
                await loadNodes();
            } else {
                const text = await response.text();
                showError(text || t('categories.moveError'));
            }
        } catch {
            showError(t('categories.moveError'));
        } finally {
            setMoving(false);
        }
    };

    // Compute eligible parent nodes for move dialog
    // A node cannot be moved into itself or its descendants
    const eligibleParentsForMove = (() => {
        if (!movingNode) return [];
        // Departement nodes cannot have a parent
        if (movingNode.nodeType === 'Departement') {
            return [];
        }
        const subtreeIds = collectSubtreeIds(movingNode);
        const all = flattenNodes(nodes);
        if (movingNode.nodeType === 'Navigation') {
            return all.filter(
                n =>
                    !subtreeIds.has(n.id) &&
                    (n.nodeType === 'Departement' || n.nodeType === 'Navigation')
            );
        }
        // Category node being moved: parent must be Navigation or Departement
        return all.filter(
            n =>
                !subtreeIds.has(n.id) &&
                (n.nodeType === 'Navigation' || n.nodeType === 'Departement')
        );
    })();

    // Compute eligible parent nodes for create dialog
    const allFlatNodes = flattenNodes(nodes);
    const eligibleParentsForCreate = (() => {
        if (createForm.nodeType === 'Departement') return [];
        if (createForm.nodeType === 'Navigation') {
            return allFlatNodes.filter(n => n.nodeType === 'Departement' || n.nodeType === 'Navigation');
        }
        // Category: parent must be Navigation or Departement
        return allFlatNodes.filter(n => n.nodeType === 'Navigation' || n.nodeType === 'Departement');
    })();

    const getNodeLabel = (node: CategoryNode) => {
        const name = language === 'fr' ? node.name_fr : node.name_en;
        const typeLabel = node.nodeType === 'Departement'
            ? t('categories.departement')
            : node.nodeType === 'Navigation'
                ? t('categories.navigation')
                : t('categories.category');
        return `[${typeLabel}] ${name}`;
    };

    return (
        <div className="category-nodes-section">
            <div className="section-header">
                <h2 className="section-title">{t('categories.title')}</h2>
            </div>

            {loading && (
                <div className="state-message">{t('categories.loading')}</div>
            )}
            {!loading && error && (
                <div className="state-message error">{error}</div>
            )}
            {!loading && !error && nodes.length === 0 && (
                <div className="state-message">{t('categories.empty')}</div>
            )}
            {!loading && !error && nodes.length > 0 && (
                <div className="tree-container">
                    {sortNodes(nodes, language).map(node => (
                        <TreeNodeRow
                            key={node.id}
                            node={node}
                            depth={0}
                            language={language}
                            onDelete={handleDelete}
                            onMove={openMoveModal}
                            t={t}
                        />
                    ))}
                </div>
            )}

            {/* Create Modal */}
            {showCreateModal && (
                <div className="modal-overlay" onClick={() => setShowCreateModal(false)}>
                    <div className="modal-box" onClick={e => e.stopPropagation()}>
                        <h3 className="modal-title">{t('categories.addNode')}</h3>
                        <form onSubmit={handleCreateSubmit}>
                            <div className="form-group">
                                <label htmlFor="nodeType">{t('categories.nodeType')}</label>
                                <select
                                    id="nodeType"
                                    className="form-control"
                                    value={createForm.nodeType}
                                    onChange={e => setCreateForm(prev => ({
                                        ...prev,
                                        nodeType: e.target.value as NodeType,
                                        parentId: '',
                                    }))}
                                >
                                    {NODE_TYPES.map(type => (
                                        <option key={type} value={type}>
                                            {type === 'Departement'
                                                ? t('categories.departement')
                                                : type === 'Navigation'
                                                    ? t('categories.navigation')
                                                    : t('categories.category')}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div className="form-group">
                                <label htmlFor="name_en">{t('categories.nodeName_en')}</label>
                                <input
                                    id="name_en"
                                    type="text"
                                    className="form-control"
                                    value={createForm.name_en}
                                    onChange={e => setCreateForm(prev => ({ ...prev, name_en: e.target.value }))}
                                    required
                                    maxLength={200}
                                />
                            </div>

                            <div className="form-group">
                                <label htmlFor="name_fr">{t('categories.nodeName_fr')}</label>
                                <input
                                    id="name_fr"
                                    type="text"
                                    className="form-control"
                                    value={createForm.name_fr}
                                    onChange={e => setCreateForm(prev => ({ ...prev, name_fr: e.target.value }))}
                                    required
                                    maxLength={200}
                                />
                            </div>

                            {createForm.nodeType !== 'Departement' && (
                                <div className="form-group">
                                    <label htmlFor="parentId">{t('categories.parent')}</label>
                                    <select
                                        id="parentId"
                                        className="form-control"
                                        value={createForm.parentId}
                                        onChange={e => setCreateForm(prev => ({ ...prev, parentId: e.target.value }))}
                                        required
                                    >
                                        <option value="">{t('categories.selectParent')}</option>
                                        {eligibleParentsForCreate.map(n => (
                                            <option key={n.id} value={n.id}>
                                                {getNodeLabel(n)}
                                            </option>
                                        ))}
                                    </select>
                                </div>
                            )}

                            <div className="form-group">
                                <label htmlFor="sortOrder">{t('categories.sortOrder')}</label>
                                <input
                                    id="sortOrder"
                                    type="number"
                                    className="form-control"
                                    value={createForm.sortOrder}
                                    onChange={e => setCreateForm(prev => ({ ...prev, sortOrder: e.target.value }))}
                                    placeholder={t('common.optional')}
                                />
                            </div>

                            <div className="form-group">
                                <div className="form-check">
                                    <input
                                        id="isActive"
                                        type="checkbox"
                                        checked={createForm.isActive}
                                        onChange={e => setCreateForm(prev => ({ ...prev, isActive: e.target.checked }))}
                                    />
                                    <label htmlFor="isActive">{t('categories.isActive')}</label>
                                </div>
                            </div>

                            <div className="modal-actions">
                                <button
                                    type="button"
                                    className="btn-cancel"
                                    onClick={() => setShowCreateModal(false)}
                                >
                                    {t('common.cancel')}
                                </button>
                                <button
                                    type="submit"
                                    className="btn-submit"
                                    disabled={creating}
                                >
                                    {creating ? t('categories.creating') : t('categories.create')}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}

            {/* Move Modal */}
            {showMoveModal && movingNode && (
                <div className="modal-overlay" onClick={() => setShowMoveModal(false)}>
                    <div className="modal-box" onClick={e => e.stopPropagation()}>
                        <h3 className="modal-title">{t('categories.moveTitle')}</h3>
                        <p style={{ color: '#495057', marginBottom: '1rem', fontSize: '14px' }}>
                            <strong>{language === 'fr' ? movingNode.name_fr : movingNode.name_en}</strong>
                        </p>
                        <form onSubmit={handleMoveSubmit}>
                            <div className="form-group">
                                <label htmlFor="newParentId">{t('categories.newParent')}</label>
                                <select
                                    id="newParentId"
                                    className="form-control"
                                    value={moveForm.newParentId}
                                    onChange={e => setMoveForm({ newParentId: e.target.value })}
                                    required
                                >
                                    {eligibleParentsForMove.map(n => (
                                        <option key={n.id} value={n.id}>
                                            {getNodeLabel(n)}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <div className="modal-actions">
                                <button
                                    type="button"
                                    className="btn-cancel"
                                    onClick={() => setShowMoveModal(false)}
                                >
                                    {t('common.cancel')}
                                </button>
                                <button
                                    type="submit"
                                    className="btn-submit"
                                    disabled={moving}
                                >
                                    {moving ? '...' : t('categories.confirmMove')}
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
});

export default CategoryNodesSection;
