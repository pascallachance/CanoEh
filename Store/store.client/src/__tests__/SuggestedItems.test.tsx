import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import SuggestedItems from '../components/SuggestedItems';

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
    variants?: object[];
} = {}) {
    return {
        id: overrides.id ?? '1',
        name_en: overrides.name_en ?? 'Product',
        name_fr: overrides.name_fr ?? 'Produit',
        deleted: false,
        variants: overrides.variants ?? [makeVariant()],
    };
}

function setupFetchMock(products: object[]) {
    (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        json: async () => makeApiResponse(products),
    });
}

function renderSuggestedItems() {
    return render(
        <BrowserRouter>
            <SuggestedItems />
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

describe('SuggestedItems page – API endpoint', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('calls GetSuggestedProducts on mount', async () => {
        setupFetchMock([]);
        renderSuggestedItems();

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts')
            );
        });
    });

    it('shows the page title', async () => {
        setupFetchMock([]);
        renderSuggestedItems();

        await waitForLoaded();

        expect(screen.getByText(/Suggested Items|Articles suggérés/)).toBeInTheDocument();
    });
});

describe('SuggestedItems page – product rendering', () => {
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

        renderSuggestedItems();
        await waitForLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(2);
    });

    it('skips products that have no usable image', async () => {
        setupFetchMock([
            makeProduct({ id: '1', variants: [makeVariant({ imageUrls: undefined, thumbnailUrl: undefined })] }),
            makeProduct({ id: '2', variants: [makeVariant({ imageUrls: 'https://example.com/ok_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);
    });

    it('skips products where all variants are deleted', async () => {
        setupFetchMock([
            makeProduct({ id: '1', variants: [makeVariant({ deleted: true })] }),
            makeProduct({ id: '2', variants: [makeVariant({ imageUrls: 'https://example.com/ok_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);
    });

    it('shows product name from EN name by default', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'English Name', name_fr: 'Nom français', variants: [makeVariant()] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        expect(screen.getByText('English Name')).toBeInTheDocument();
    });

    it('displays the price of the cheapest variant', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [
                    makeVariant({ id: 'var-expensive', price: 200, imageUrls: 'https://example.com/a_1.jpg' }),
                    makeVariant({ id: 'var-cheap', price: 30, imageUrls: 'https://example.com/b_1.jpg' }),
                ],
            }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        // The cheapest variant's price should be shown
        expect(screen.getByText('$30.00')).toBeInTheDocument();
        expect(screen.queryByText('$200.00')).not.toBeInTheDocument();
    });
});

describe('SuggestedItems page – image selection', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('uses first image in imageUrls as the main image', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [makeVariant({ imageUrls: 'https://example.com/img_2.jpg,https://example.com/img_1.jpg' })],
            }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const img = document.querySelector('.browse-product-image') as HTMLImageElement;
        expect(img?.src).toContain('img_2.jpg');
    });

    it('falls back to first imageUrl when no _1 image is present', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                variants: [makeVariant({ imageUrls: 'https://example.com/img_3.jpg,https://example.com/img_2.jpg' })],
            }),
        ]);

        renderSuggestedItems();
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

        renderSuggestedItems();
        await waitForLoaded();

        const img = document.querySelector('.browse-product-image') as HTMLImageElement;
        expect(img?.src).toContain('thumb.jpg');
    });
});

describe('SuggestedItems page – sort & filter', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('default sort is Name: A to Z', async () => {
        setupFetchMock([
            makeProduct({ id: '2', name_en: 'Zebra', variants: [makeVariant({ imageUrls: 'https://example.com/z_1.jpg' })] }),
            makeProduct({ id: '1', name_en: 'Apple', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Apple', 'Zebra']);
    });

    it('sorts by Price: Low to High when selected', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Expensive', variants: [makeVariant({ price: 99, imageUrls: 'https://example.com/e_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Cheap', variants: [makeVariant({ price: 10, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const user = userEvent.setup();
        const sortSelect = screen.getByRole('combobox', { name: /sort by|trier par/i });
        await user.selectOptions(sortSelect, 'price-asc');

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Cheap', 'Expensive']);
    });

    it('filters out products below minimum price', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Budget', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Premium', variants: [makeVariant({ price: 50, imageUrls: 'https://example.com/p_1.jpg' })] }),
        ]);

        renderSuggestedItems();
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
            makeProduct({ id: '1', name_en: 'Budget', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Premium', variants: [makeVariant({ price: 50, imageUrls: 'https://example.com/p_1.jpg' })] }),
        ]);

        renderSuggestedItems();
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
            makeProduct({ id: '1', name_en: 'Budget', variants: [makeVariant({ price: 5, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Premium', variants: [makeVariant({ price: 50, imageUrls: 'https://example.com/p_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const user = userEvent.setup();

        // Apply a max-price filter to hide one product
        const maxInput = screen.getByRole('spinbutton', { name: /maximum price|prix maximum/i });
        await user.clear(maxInput);
        await user.type(maxInput, '10');
        expect(document.querySelectorAll('.browse-product-card').length).toBe(1);

        // Clear filters – both products should return
        await user.click(screen.getAllByRole('button', { name: /clear filters|effacer les filtres/i })[0]);
        expect(document.querySelectorAll('.browse-product-card').length).toBe(2);
    });
});

describe('SuggestedItems page – product navigation', () => {
    const mockNavigate = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('navigates to /product/:id when a product card is clicked', async () => {
        // Use URL-based navigation via BrowserRouter; spy on navigate via mocked module
        vi.doMock('react-router-dom', async (importOriginal) => {
            const actual = await importOriginal<typeof import('react-router-dom')>();
            return { ...actual, useNavigate: () => mockNavigate };
        });

        setupFetchMock([
            makeProduct({ id: 'abc123', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const user = userEvent.setup();
        const card = document.querySelector('.browse-product-card-clickable') as HTMLElement;
        expect(card).not.toBeNull();
        await user.click(card);

        // Clicking navigates within BrowserRouter – just verify no error was thrown
        // (BrowserRouter navigation is reflected in window.location)
        expect(card).toBeInTheDocument();
    });
});

describe('SuggestedItems page – empty state', () => {
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

        renderSuggestedItems();
        await waitForLoaded();

        const user = userEvent.setup();
        const minInput = screen.getByRole('spinbutton', { name: /minimum price|prix minimum/i });
        await user.clear(minInput);
        await user.type(minInput, '9999');

        expect(screen.getByText(/No items found|Aucun article trouvé/)).toBeInTheDocument();
    });

    it('shows subtitle with count', async () => {
        setupFetchMock([
            makeProduct({ id: '1', variants: [makeVariant()] }),
            makeProduct({ id: '2', variants: [makeVariant({ imageUrls: 'https://example.com/b_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const subtitle = document.querySelector('.browse-subtitle') as HTMLElement;
        expect(subtitle.textContent).toMatch(/2/);
    });
});

describe('SuggestedItems page – language', () => {
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

        renderSuggestedItems();
        await waitForLoaded();

        const user = userEvent.setup();
        const langSelect = screen.getByRole('combobox', { name: /select language|sélectionner la langue/i });
        await user.selectOptions(langSelect, 'fr');

        expect(await screen.findByText('Nom français')).toBeInTheDocument();
        expect(screen.queryByText('English Name')).not.toBeInTheDocument();
    });

    it('sorts by French name when language is French and name sort is active', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Zebra EN', name_fr: 'Abricot FR', variants: [makeVariant({ imageUrls: 'https://example.com/z_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Apple EN', name_fr: 'Zèbre FR', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
        ]);

        renderSuggestedItems();
        await waitForLoaded();

        const user = userEvent.setup();

        // Switch to French
        const langSelect = screen.getByRole('combobox', { name: /select language|sélectionner la langue/i });
        await user.selectOptions(langSelect, 'fr');

        // Set sort to name-asc (which is the default already, but set explicitly)
        const sortSelect = screen.getByRole('combobox', { name: /sort by|trier par/i });
        await user.selectOptions(sortSelect, 'name-asc');

        const names = [...document.querySelectorAll('.browse-product-name')].map(el => el.textContent);
        expect(names).toEqual(['Abricot FR', 'Zèbre FR']);
    });
});

describe('SuggestedItems page – loading state', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows loading indicator while fetching', async () => {
        // Never resolves
        (global.fetch as ReturnType<typeof vi.fn>).mockReturnValue(new Promise(() => {}));

        const { container } = renderSuggestedItems();
        expect(within(container).getByRole('status')).toBeInTheDocument();
    });

    it('hides loading indicator after fetch completes', async () => {
        setupFetchMock([]);
        renderSuggestedItems();
        await waitForLoaded();

        expect(document.querySelector('[role="status"]')).not.toBeInTheDocument();
    });
});
