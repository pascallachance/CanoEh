import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

const emptyApiResponse = { isSuccess: true, value: [] };

/**
 * Sets up fetch mock responses keyed by URL segment.
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

function makeCategoryProduct(id: string, categoryName_en: string, categoryName_fr: string) {
    return {
        id,
        sellerID: 'seller1',
        name_en: `Product ${id}`,
        name_fr: `Produit ${id}`,
        categoryNodeID: `cat-${id}`,
        categoryName_en,
        categoryName_fr,
        createdAt: '2024-01-01',
        deleted: false,
        variants: [
            {
                id: `var${id}`,
                price: 10,
                stockQuantity: 5,
                sku: `SKU${id}`,
                imageUrls: `https://example.com/${id}.jpg`,
                itemVariantAttributes: [],
                deleted: false,
            },
        ],
        itemAttributes: [],
    };
}

describe('Home - Explore Categories language', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
        vi.restoreAllMocks();
    });

    it('displays English category names when language is English', async () => {
        vi.spyOn(navigator, 'language', 'get').mockReturnValue('en-US');

        mockFetchByUrl({
            categories: {
                isSuccess: true,
                value: [makeCategoryProduct('1', 'Electronics', 'Électronique')],
            },
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            const title = screen.getByText(/Explore Categories|Explorer les catégories/);
            const container = title.closest('.item-preview-card');
            const itemName = container?.querySelector('.item-name');
            expect(itemName?.textContent).toBe('Electronics');
        });
    });

    it('displays French category names when browser language is French', async () => {
        vi.spyOn(navigator, 'language', 'get').mockReturnValue('fr-CA');

        mockFetchByUrl({
            categories: {
                isSuccess: true,
                value: [makeCategoryProduct('1', 'Electronics', 'Électronique')],
            },
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            const title = screen.getByText(/Explore Categories|Explorer les catégories/);
            const container = title.closest('.item-preview-card');
            const itemName = container?.querySelector('.item-name');
            expect(itemName?.textContent).toBe('Électronique');
        });
    });

    it('displays French category names after user switches language to French', async () => {
        vi.spyOn(navigator, 'language', 'get').mockReturnValue('en-US');

        mockFetchByUrl({
            categories: {
                isSuccess: true,
                value: [makeCategoryProduct('1', 'Electronics', 'Électronique')],
            },
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        // Wait for English name to appear first
        await waitFor(() => {
            const title = screen.getByText('Explore Categories');
            const container = title.closest('.item-preview-card');
            const itemName = container?.querySelector('.item-name');
            expect(itemName?.textContent).toBe('Electronics');
        });

        // Switch language to French
        const languageSelect = screen.getByRole('combobox', {
            name: /Select language|Sélectionner la langue/,
        });
        const user = userEvent.setup();
        await user.selectOptions(languageSelect, 'fr');

        // Category name should now be in French
        await waitFor(() => {
            const title = screen.getByText('Explorer les catégories');
            const container = title.closest('.item-preview-card');
            const itemName = container?.querySelector('.item-name');
            expect(itemName?.textContent).toBe('Électronique');
        });
    });
});
