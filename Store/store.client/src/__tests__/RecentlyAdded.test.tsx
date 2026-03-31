import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import RecentlyAdded from '../components/RecentlyAdded';

// Mock fetch globally
global.fetch = vi.fn();

// ─── Helpers ────────────────────────────────────────────────────────────────

function makeApiResponse(products: object[]) {
    return { isSuccess: true, value: products };
}

function makeVariant(overrides: {
    id?: string;
    price?: number;
    imageUrls?: string;
    thumbnailUrl?: string;
    deleted?: boolean;
} = {}) {
    return {
        id: 'var1',
        price: 50,
        stockQuantity: 10,
        sku: 'SKU1',
        deleted: false,
        imageUrls: 'https://example.com/img_1.jpg' as string | undefined,
        thumbnailUrl: undefined as string | undefined,
        ...overrides,
    };
}

function makeProduct(overrides: {
    id?: string;
    name_en?: string;
    name_fr?: string;
    createdAt?: string;
    variants?: object[];
} = {}) {
    return {
        id: overrides.id ?? '1',
        name_en: overrides.name_en ?? 'Product',
        name_fr: overrides.name_fr ?? 'Produit',
        deleted: false,
        createdAt: overrides.createdAt ?? '2024-06-15T00:00:00Z',
        variants: overrides.variants ?? [makeVariant()],
    };
}

function setupFetchMock(products: object[]) {
    (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        json: async () => makeApiResponse(products),
    });
}

function renderRecentlyAdded() {
    return render(
        <BrowserRouter>
            <RecentlyAdded />
        </BrowserRouter>
    );
}

async function waitForLoaded() {
    await waitFor(() => {
        const subtitle = document.querySelector('.browse-subtitle');
        expect(subtitle).toBeInTheDocument();
    });
}

// ─── Tests ──────────────────────────────────────────────────────────────────

describe('RecentlyAdded page – API endpoint', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('calls GetRecentlyAddedProducts on mount', async () => {
        setupFetchMock([]);
        renderRecentlyAdded();

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetRecentlyAddedProducts')
            );
        });
    });

    it('shows the page title', async () => {
        setupFetchMock([]);
        renderRecentlyAdded();

        await waitForLoaded();

        expect(screen.getByText(/Recently Added Items|Articles récemment ajoutés/)).toBeInTheDocument();
    });
});

describe('RecentlyAdded page – product rendering', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('renders a product card for each product with an image', async () => {
        setupFetchMock([
            makeProduct({ id: '1', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', variants: [makeVariant({ imageUrls: 'https://example.com/b_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(2);
    });

    it('skips products that have no usable image', async () => {
        setupFetchMock([
            makeProduct({ id: '1', variants: [makeVariant({ imageUrls: undefined, thumbnailUrl: undefined })] }),
            makeProduct({ id: '2', variants: [makeVariant({ imageUrls: 'https://example.com/ok_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);
    });

    it('displays the price of the cheapest variant', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [
                    makeVariant({ id: 'var-expensive', price: 200, imageUrls: 'https://example.com/a_1.jpg' }),
                    makeVariant({ id: 'var-cheap', price: 25, imageUrls: 'https://example.com/b_1.jpg' }),
                ],
            }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        expect(screen.getByText('$25.00')).toBeInTheDocument();
        expect(screen.queryByText('$200.00')).not.toBeInTheDocument();
    });

    it('shows the createdAt date on each card', async () => {
        setupFetchMock([
            makeProduct({ id: '1', createdAt: '2024-03-15T00:00:00Z', variants: [makeVariant()] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        // The date should appear somewhere in the card area
        const dateEl = document.querySelector('.browse-product-date') as HTMLElement;
        expect(dateEl).toBeInTheDocument();
        // Should contain some representation of 2024 and March
        expect(dateEl.textContent).toMatch(/2024/);
    });
});

describe('RecentlyAdded page – image selection', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('prefers image ending with _1 over others in imageUrls', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [makeVariant({ imageUrls: 'https://example.com/img_2.jpg,https://example.com/img_1.jpg' })],
            }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const img = document.querySelector('.browse-product-image') as HTMLImageElement;
        expect(img?.src).toContain('img_1.jpg');
    });

    it('falls back to first imageUrl when no _1 image is present', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [makeVariant({ imageUrls: 'https://example.com/img_3.jpg,https://example.com/img_2.jpg' })],
            }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const img = document.querySelector('.browse-product-image') as HTMLImageElement;
        expect(img?.src).toContain('img_3.jpg');
    });

    it('falls back to thumbnailUrl when imageUrls is absent', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [makeVariant({ imageUrls: undefined, thumbnailUrl: 'https://example.com/thumb.jpg' })],
            }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const img = document.querySelector('.browse-product-image') as HTMLImageElement;
        expect(img?.src).toContain('thumb.jpg');
    });
});

describe('RecentlyAdded page – default sort (newest first)', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('renders items sorted by createdAt descending by default', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Older', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/o_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Newer', createdAt: '2024-06-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/n_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'Newest', createdAt: '2024-12-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/ne_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Newest', 'Newer', 'Older']);
    });

    it('sorts oldest first when date-asc is selected', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Older', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/o_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Newest', createdAt: '2024-12-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/n_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const sortSelect = screen.getByRole('combobox', { name: /sort by|trier par/i });
        await user.selectOptions(sortSelect, 'date-asc');

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Older', 'Newest']);
    });
});

describe('RecentlyAdded page – sort & filter', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('sorts by Price: Low to High when selected', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Expensive', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ price: 99, imageUrls: 'https://example.com/e_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Cheap', createdAt: '2024-02-01T00:00:00Z', variants: [makeVariant({ price: 10, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const sortSelect = screen.getByRole('combobox', { name: /sort by|trier par/i });
        await user.selectOptions(sortSelect, 'price-asc');

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Cheap', 'Expensive']);
    });

    it('sorts by Name: A to Z when selected', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Zebra', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/z_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Apple', createdAt: '2024-02-01T00:00:00Z', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const sortSelect = screen.getByRole('combobox', { name: /sort by|trier par/i });
        await user.selectOptions(sortSelect, 'name-asc');

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Apple', 'Zebra']);
    });

    it('filters out products below minimum price', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Budget', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Premium', createdAt: '2024-02-01T00:00:00Z', variants: [makeVariant({ price: 50, imageUrls: 'https://example.com/p_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const minInput = screen.getByRole('spinbutton', { name: /minimum price|prix minimum/i });
        await user.clear(minInput);
        await user.type(minInput, '20');

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);
        expect(screen.getByText('Premium')).toBeInTheDocument();
    });

    it('filters out products above maximum price', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Budget', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Premium', createdAt: '2024-02-01T00:00:00Z', variants: [makeVariant({ price: 50, imageUrls: 'https://example.com/p_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const maxInput = screen.getByRole('spinbutton', { name: /maximum price|prix maximum/i });
        await user.clear(maxInput);
        await user.type(maxInput, '30');

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);
        expect(screen.getByText('Budget')).toBeInTheDocument();
    });

    it('clear filters resets sort and price range', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Budget', createdAt: '2024-01-01T00:00:00Z', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Premium', createdAt: '2024-02-01T00:00:00Z', variants: [makeVariant({ price: 50, imageUrls: 'https://example.com/p_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();

        const maxInput = screen.getByRole('spinbutton', { name: /maximum price|prix maximum/i });
        await user.clear(maxInput);
        await user.type(maxInput, '10');
        expect(document.querySelectorAll('.browse-product-card').length).toBe(1);

        await user.click(screen.getAllByRole('button', { name: /clear filters|effacer les filtres/i })[0]);
        expect(document.querySelectorAll('.browse-product-card').length).toBe(2);
    });
});

describe('RecentlyAdded page – language', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows French name when language is switched to French', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'English Name', name_fr: 'Nom français', variants: [makeVariant()] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const langSelect = screen.getByRole('combobox', { name: /select language|sélectionner la langue/i });
        await user.selectOptions(langSelect, 'fr');

        expect(await screen.findByText('Nom français')).toBeInTheDocument();
        expect(screen.queryByText('English Name')).not.toBeInTheDocument();
    });

    it('renders date in French locale format', async () => {
        setupFetchMock([
            makeProduct({ id: '1', createdAt: '2024-03-15T00:00:00Z', variants: [makeVariant()] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const langSelect = screen.getByRole('combobox', { name: /select language|sélectionner la langue/i });
        await user.selectOptions(langSelect, 'fr');

        const dateEl = document.querySelector('.browse-product-date') as HTMLElement;
        expect(dateEl).toBeInTheDocument();
        // French locale should show "Ajouté le" label and year
        expect(dateEl.textContent).toMatch(/Ajouté le/);
        expect(dateEl.textContent).toMatch(/2024/);
    });
});

describe('RecentlyAdded page – loading state', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows loading indicator while fetching', async () => {
        (global.fetch as ReturnType<typeof vi.fn>).mockReturnValue(new Promise(() => {}));

        const { container } = renderRecentlyAdded();
        expect(within(container).getByRole('status')).toBeInTheDocument();
    });

    it('hides loading indicator after fetch completes', async () => {
        setupFetchMock([]);
        renderRecentlyAdded();
        await waitForLoaded();

        expect(document.querySelector('[role="status"]')).not.toBeInTheDocument();
    });
});

describe('RecentlyAdded page – empty state', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows empty state message when no products match filters', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Cheap', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        renderRecentlyAdded();
        await waitForLoaded();

        const user = userEvent.setup();
        const minInput = screen.getByRole('spinbutton', { name: /minimum price|prix minimum/i });
        await user.clear(minInput);
        await user.type(minInput, '9999');

        expect(screen.getByText(/No items found|Aucun article trouvé/)).toBeInTheDocument();
    });
});
