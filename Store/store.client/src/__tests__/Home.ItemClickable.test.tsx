import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock useNavigate to capture navigation calls without a real router
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => {
    const actual = await importOriginal<typeof import('react-router-dom')>();
    return {
        ...actual,
        useNavigate: () => mockNavigate,
    };
});

// Mock fetch globally
global.fetch = vi.fn();

// Helper to build a minimal API product with given variants
function makeProduct(id: string, variants: object[]) {
    return {
        id,
        sellerID: 'seller1',
        name_en: `Product ${id}`,
        name_fr: `Produit ${id}`,
        categoryNodeID: 'cat1',
        createdAt: '2024-01-01',
        deleted: false,
        variants: variants.map((v, i) => ({
            id: `var${id}-${i}`,
            price: 10,
            stockQuantity: 5,
            sku: `SKU${id}-${i}`,
            itemVariantAttributes: [],
            deleted: false,
            ...v,
        })),
        itemAttributes: [],
    };
}

const emptyApiResponse = { isSuccess: true, value: [] };

/**
 * Sets up fetch mock responses keyed by URL segment.
 * Unspecified endpoints return an empty success response.
 */
function mockFetchByUrl({
    recently = emptyApiResponse,
    suggested = emptyApiResponse,
    offers = emptyApiResponse,
    categories = emptyApiResponse,
}: {
    recently?: object;
    suggested?: object;
    offers?: object;
    categories?: object;
} = {}) {
    (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
        if (url.includes('GetRecentlyAddedProducts')) {
            return Promise.resolve({ ok: true, json: async () => recently });
        }
        if (url.includes('GetSuggestedProducts')) {
            return Promise.resolve({ ok: true, json: async () => suggested });
        }
        if (url.includes('GetProductsWithOffers')) {
            return Promise.resolve({ ok: true, json: async () => offers });
        }
        if (url.includes('GetSuggestedCategoriesProducts')) {
            return Promise.resolve({ ok: true, json: async () => categories });
        }
        return Promise.resolve({ ok: false, json: async () => ({}) });
    });
}

describe('Home - Clickable item placeholders', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    // ─── Suggested items card ────────────────────────────────────────────────

    it('should render product items in Suggested card as clickable buttons', async () => {
        mockFetchByUrl({
            suggested: {
                isSuccess: true,
                value: [makeProduct('s1', [{ imageUrls: 'https://example.com/s1.jpg' }])],
            },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts')
            );
        });

        const cardTitle = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = cardTitle.closest('.item-preview-card');
        expect(card).toBeInTheDocument();

        const clickableItems = card?.querySelectorAll('button.item-placeholder-clickable');
        expect(clickableItems?.length).toBeGreaterThan(0);
    });

    it('should navigate to /product/:id when a Suggested item is clicked', async () => {
        mockFetchByUrl({
            suggested: {
                isSuccess: true,
                value: [makeProduct('s42', [{ imageUrls: 'https://example.com/s42.jpg' }])],
            },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        // Wait for the clickable button to appear (products need to load first)
        // Re-query the card on each attempt since React re-renders on data load
        let itemButton: HTMLElement | null = null;
        await waitFor(() => {
            const title = screen.getByText(/Suggested items|Articles suggérés/);
            const card = title.closest('.item-preview-card');
            const btn = card?.querySelector('button.item-placeholder-clickable');
            expect(btn).not.toBeNull();
            itemButton = btn as HTMLElement;
        });

        const user = userEvent.setup();
        await user.click(itemButton as HTMLElement);

        expect(mockNavigate).toHaveBeenCalledWith('/product/s42');
    });

    // ─── Recently Added items card ───────────────────────────────────────────

    it('should render product items in Recently Added card as clickable buttons', async () => {
        mockFetchByUrl({
            recently: {
                isSuccess: true,
                value: [makeProduct('r1', [{ imageUrls: 'https://example.com/r1.jpg' }])],
            },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetRecentlyAddedProducts')
            );
        });

        const cardTitle = await screen.findByText(/Recently added items|Articles récemment ajoutés/);
        const card = cardTitle.closest('.item-preview-card');
        expect(card).toBeInTheDocument();

        const clickableItems = card?.querySelectorAll('button.item-placeholder-clickable');
        expect(clickableItems?.length).toBeGreaterThan(0);
    });

    it('should navigate to /product/:id when a Recently Added item is clicked', async () => {
        mockFetchByUrl({
            recently: {
                isSuccess: true,
                value: [makeProduct('r99', [{ imageUrls: 'https://example.com/r99.jpg' }])],
            },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        // Wait for the clickable button to appear (products need to load first)
        // Re-query the card on each attempt since React re-renders on data load
        let itemButton: HTMLElement | null = null;
        await waitFor(() => {
            const title = screen.getByText(/Recently added items|Articles récemment ajoutés/);
            const card = title.closest('.item-preview-card');
            const btn = card?.querySelector('button.item-placeholder-clickable');
            expect(btn).not.toBeNull();
            itemButton = btn as HTMLElement;
        });

        const user = userEvent.setup();
        await user.click(itemButton as HTMLElement);

        expect(mockNavigate).toHaveBeenCalledWith('/product/r99');
    });

    // ─── Static placeholder cards (no product data) ──────────────────────────

    it('should render Best Sellers items as non-clickable divs', async () => {
        mockFetchByUrl();

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        const cardTitle = screen.getByText(/Best Sellers|Meilleures ventes/);
        const card = cardTitle.closest('.item-preview-card');
        expect(card).toBeInTheDocument();

        // Items should be plain divs, not buttons
        const placeholderDivs = card?.querySelectorAll('div.item-placeholder');
        expect(placeholderDivs?.length).toBeGreaterThan(0);

        const clickableButtons = card?.querySelectorAll('button.item-placeholder-clickable');
        expect(clickableButtons?.length).toBe(0);
    });

    it('should render Best Rated items as non-clickable divs', async () => {
        mockFetchByUrl();

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        const cardTitle = screen.getByText(/Best Rated|Mieux notés/);
        const card = cardTitle.closest('.item-preview-card');
        expect(card).toBeInTheDocument();

        // Items should be plain divs, not buttons
        const placeholderDivs = card?.querySelectorAll('div.item-placeholder');
        expect(placeholderDivs?.length).toBeGreaterThan(0);

        const clickableButtons = card?.querySelectorAll('button.item-placeholder-clickable');
        expect(clickableButtons?.length).toBe(0);
    });

    // ─── Card title button (see-all) still works when items are clickable ─────

    it('should render card title as a button when product items are clickable in Suggested card', async () => {
        mockFetchByUrl({
            suggested: {
                isSuccess: true,
                value: [makeProduct('s1', [{ imageUrls: 'https://example.com/s1.jpg' }])],
            },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        // Wait for products to load — the title becomes a <button> once items are clickable
        let cardTitle: HTMLElement | null = null;
        await waitFor(() => {
            const el = screen.getByText(/Suggested items|Articles suggérés/);
            expect(el.tagName).toBe('BUTTON');
            cardTitle = el;
        });

        expect(cardTitle).not.toBeNull();
        expect(cardTitle).toHaveClass('card-title-btn');
    });

    it('should render card title as heading when no product items are clickable (Best Sellers)', async () => {
        mockFetchByUrl();

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        const cardTitle = screen.getByText(/Best Sellers|Meilleures ventes/);
        // Title should be a plain h3, not a button
        expect(cardTitle.tagName).toBe('H3');
    });
});
