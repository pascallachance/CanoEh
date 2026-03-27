import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor, act } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import Categories from '../components/Categories';

// Mock fetch globally
global.fetch = vi.fn();

const API_BASE_URL = 'https://localhost:7182';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeCategoryNode(overrides: {
    id?: string;
    name_en?: string;
    name_fr?: string;
    nodeType?: string;
    parentId?: string | null;
    isActive?: boolean;
    sortOrder?: number | null;
    children?: object[];
} = {}) {
    return {
        id: overrides.id ?? 'node1',
        name_en: overrides.name_en ?? 'Electronics',
        name_fr: overrides.name_fr ?? 'Électronique',
        nodeType: overrides.nodeType ?? 'Department',
        parentId: overrides.parentId ?? null,
        isActive: overrides.isActive ?? true,
        sortOrder: overrides.sortOrder ?? null,
        children: overrides.children ?? [],
    };
}

function makeVariant(overrides: {
    id?: string;
    price?: number;
    stockQuantity?: number;
    imageUrls?: string;
    deleted?: boolean;
} = {}) {
    return {
        id: overrides.id ?? 'var1',
        price: overrides.price ?? 50,
        stockQuantity: overrides.stockQuantity ?? 10,
        sku: 'SKU-001',
        imageUrls: overrides.imageUrls ?? 'https://example.com/img_1.jpg',
        deleted: overrides.deleted ?? false,
    };
}

function makeItem(overrides: {
    id?: string;
    name_en?: string;
    name_fr?: string;
    categoryNodeID?: string;
} = {}) {
    return {
        id: overrides.id ?? 'item1',
        name_en: overrides.name_en ?? 'Test Item',
        name_fr: overrides.name_fr ?? 'Article Test',
        categoryNodeID: overrides.categoryNodeID ?? 'node1',
        categoryName_en: 'Electronics',
        categoryName_fr: 'Électronique',
        deleted: false,
        variants: [makeVariant()],
    };
}

function makeApiResult<T>(value: T) {
    return { isSuccess: true, value };
}

/**
 * Sets up fetch to return category nodes on the first call, then products on subsequent calls.
 */
function setupFetch(nodes: object[], products: object[] = []) {
    (global.fetch as ReturnType<typeof vi.fn>)
        .mockResolvedValueOnce({
            ok: true,
            json: async () => makeApiResult(nodes),
        })
        .mockResolvedValue({
            ok: true,
            json: async () => makeApiResult(products),
        });
}

/** Render Categories at a given URL path */
function renderCategories(path = '/categories') {
    return render(
        <MemoryRouter initialEntries={[path]}>
            <Routes>
                <Route path="/categories" element={<Categories />} />
                <Route path="/" element={<div>Home</div>} />
            </Routes>
        </MemoryRouter>
    );
}

/**
 * Creates N unique items with distinct names so they can be identified.
 */
function makeItems(count: number): object[] {
    return Array.from({ length: count }, (_, i) => makeItem({
        id: `item-${i + 1}`,
        name_en: `Product ${String(i + 1).padStart(2, '0')}`,
        name_fr: `Produit ${String(i + 1).padStart(2, '0')}`,
    }));
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('Categories page – breadcrumb (no Home item)', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('does not show "Home" as a breadcrumb item', async () => {
        setupFetch([makeCategoryNode()]);
        renderCategories();

        await waitFor(() => {
            const breadcrumb = document.querySelector('.categories-breadcrumb');
            expect(breadcrumb).toBeInTheDocument();
        });

        const homeBtn = screen.queryByRole('button', { name: /^Home$|^Accueil$/i });
        expect(homeBtn).not.toBeInTheDocument();
    });

    it('does not show "All Departments" in the breadcrumb', async () => {
        setupFetch([makeCategoryNode()]);
        renderCategories();

        await waitFor(() => {
            const breadcrumb = document.querySelector('.categories-breadcrumb');
            expect(breadcrumb).toBeInTheDocument();
        });

        const breadcrumb = document.querySelector('.categories-breadcrumb');
        expect(breadcrumb?.textContent).not.toMatch(/All Departments|Tous les rayons/i);
    });
});

describe('Categories page – category-nodes-label removed', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('does not render the category-nodes-label element', async () => {
        setupFetch([makeCategoryNode()]);
        renderCategories();

        await waitFor(() => {
            expect(document.querySelector('.category-nodes-list')).toBeInTheDocument();
        });

        expect(document.querySelector('.category-nodes-label')).toBeNull();
    });
});

describe('Categories page – ?nodeId= URL param pre-selection', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('pre-selects the node from the URL param and renders it in the breadcrumb', async () => {
        const node = makeCategoryNode({ id: 'electronics', name_en: 'Electronics', name_fr: 'Électronique' });
        setupFetch([node], []);

        renderCategories('/categories?nodeId=electronics');

        await waitFor(() => {
            // The breadcrumb should show "Electronics" as the current node
            const breadcrumb = document.querySelector('.categories-breadcrumb');
            expect(breadcrumb).toBeInTheDocument();
            expect(breadcrumb?.textContent).toMatch(/Electronics/);
        });
    });

    it('fetches products for the pre-selected nodeId from the URL param', async () => {
        const node = makeCategoryNode({ id: 'electronics', name_en: 'Electronics' });
        setupFetch([node], [makeItem({ categoryNodeID: 'electronics' })]);

        renderCategories('/categories?nodeId=electronics');

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetItemsByCategoryNode/electronics')
            );
        });
    });

    it('builds the ancestor breadcrumb path when navigating to a nested node via URL param', async () => {
        const parent = makeCategoryNode({ id: 'parent-node', name_en: 'Tools', name_fr: 'Outils', parentId: null });
        const child = makeCategoryNode({ id: 'child-node', name_en: 'Power Tools', name_fr: 'Outils électriques', parentId: 'parent-node' });
        // Simulate the API returning flat list; buildCategoryTree will wire up the hierarchy
        setupFetch([parent, child], []);

        renderCategories('/categories?nodeId=child-node');

        await waitFor(() => {
            const breadcrumb = document.querySelector('.categories-breadcrumb');
            expect(breadcrumb?.textContent).toMatch(/Tools/);
            expect(breadcrumb?.textContent).toMatch(/Power Tools/);
        });
    });

    it('URL-encodes the nodeId when fetching products for the pre-selected node', async () => {
        // Use a nodeId with a space to verify encoding: "node 1" → "node%201" in the URL path
        const node = makeCategoryNode({ id: 'node 1', name_en: 'Electronics' });
        setupFetch([node], []);

        // searchParams.get() decodes the query string value; the component should re-encode for the path
        renderCategories('/categories?nodeId=node%201');

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetItemsByCategoryNode/node%201')
            );
        });
    });

    it('shows an "All Products" root breadcrumb button when a node is selected', async () => {
        const node = makeCategoryNode({ id: 'electronics', name_en: 'Electronics' });
        setupFetch([node], []);

        renderCategories('/categories?nodeId=electronics');

        await waitFor(() => {
            const breadcrumb = document.querySelector('.categories-breadcrumb');
            expect(breadcrumb?.textContent).toMatch(/All Products/);
        });
    });

    it('clicking the "All Products" breadcrumb root re-fetches all products and clears navPath', async () => {
        const node = makeCategoryNode({ id: 'electronics', name_en: 'Electronics' });
        setupFetch([node], []);

        renderCategories('/categories?nodeId=electronics');

        // Wait until the node breadcrumb is visible
        await waitFor(() => {
            expect(document.querySelector('.categories-breadcrumb')?.textContent).toMatch(/All Products/);
        });

        const allProductsBtn = screen.getByRole('button', { name: /All Products/i });
        await act(async () => { allProductsBtn.click(); });

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetAllItems')
            );
        });

        // The node name should no longer appear in the breadcrumb
        expect(document.querySelector('.categories-breadcrumb')?.textContent).not.toMatch(/Electronics/);
    });
});

describe('Categories page – unknown nodeId fallback', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('fetches all products when the nodeId in the URL is not found in the category tree', async () => {
        // The category tree does NOT contain 'unknown-category'
        const node = makeCategoryNode({ id: 'electronics', name_en: 'Electronics' });
        const items = [makeItem({ name_en: 'Fallback Item' })];
        setupFetch([node], items);

        renderCategories('/categories?nodeId=unknown-category');

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetAllItems')
            );
        });
    });

    it('does not call GetItemsByCategoryNode when the nodeId is not found in the category tree', async () => {
        const node = makeCategoryNode({ id: 'electronics', name_en: 'Electronics' });
        setupFetch([node], []);

        renderCategories('/categories?nodeId=unknown-category');

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetAllItems')
            );
        });

        expect(global.fetch).not.toHaveBeenCalledWith(
            expect.stringContaining('/api/Item/GetItemsByCategoryNode')
        );
    });
});

describe('Categories page – list all products when no node is selected', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('fetches all products from GetAllItems when no nodeId is in the URL', async () => {
        setupFetch([makeCategoryNode()]);
        renderCategories('/categories');

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetAllItems')
            );
        });
    });

    it('displays products returned by GetAllItems when no node is selected', async () => {
        const node = makeCategoryNode();
        const item = makeItem({ name_en: 'All Products Item' });
        setupFetch([node], [item]);

        renderCategories('/categories');

        await waitFor(() => {
            expect(screen.getByText('All Products Item')).toBeInTheDocument();
        });
    });

    it('does not show the "Select a department" prompt', async () => {
        setupFetch([makeCategoryNode()]);
        renderCategories('/categories');

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetAllItems')
            );
        });

        expect(screen.queryByText(/Select a department/i)).not.toBeInTheDocument();
        expect(screen.queryByText(/Sélectionnez un rayon/i)).not.toBeInTheDocument();
    });
});

describe('Categories page – pagination', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows only 12 products on the first page when more than 12 products are returned', async () => {
        const node = makeCategoryNode({ id: 'node1' });
        const items = makeItems(15);
        setupFetch([node], items);

        renderCategories('/categories?nodeId=node1');

        await waitFor(() => {
            // Product 01 through 12 should be visible
            expect(screen.getByText('Product 01')).toBeInTheDocument();
            expect(screen.getByText('Product 12')).toBeInTheDocument();
        });

        // Product 13 should NOT be on the first page
        expect(screen.queryByText('Product 13')).not.toBeInTheDocument();
    });

    it('does not render pagination controls when 12 or fewer products are returned', async () => {
        const node = makeCategoryNode({ id: 'node1' });
        const items = makeItems(8);
        setupFetch([node], items);

        renderCategories('/categories?nodeId=node1');

        await waitFor(() => {
            expect(screen.getByText('Product 01')).toBeInTheDocument();
        });

        expect(document.querySelector('.categories-pagination')).toBeNull();
    });

    it('renders pagination controls when more than 12 products are returned', async () => {
        const node = makeCategoryNode({ id: 'node1' });
        const items = makeItems(13);
        setupFetch([node], items);

        renderCategories('/categories?nodeId=node1');

        await waitFor(() => {
            expect(document.querySelector('.categories-pagination')).toBeInTheDocument();
        });
    });

    it('shows products on the second page when the next-page button is clicked', async () => {
        const node = makeCategoryNode({ id: 'node1' });
        const items = makeItems(15);
        setupFetch([node], items);

        renderCategories('/categories?nodeId=node1');

        // Wait for first page to load
        await waitFor(() => {
            expect(screen.getByText('Product 01')).toBeInTheDocument();
            expect(document.querySelector('.categories-pagination')).toBeInTheDocument();
        });

        const nextBtn = screen.getByLabelText(/Next page/i);
        await act(async () => { nextBtn.click(); });

        await waitFor(() => {
            expect(screen.getByText('Product 13')).toBeInTheDocument();
        });

        expect(screen.queryByText('Product 01')).not.toBeInTheDocument();
    });
});
