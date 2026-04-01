import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

const emptyApiResponse = { isSuccess: true, value: [] };

function makeProduct(id: string, nameEn: string, nameFr: string) {
    return {
        id,
        sellerID: 'seller1',
        name_en: nameEn,
        name_fr: nameFr,
        categoryNodeID: `cat-${id}`,
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

function makeOfferProduct(id: string, nameEn: string, nameFr: string) {
    return {
        ...makeProduct(id, nameEn, nameFr),
        variants: [
            {
                id: `var${id}`,
                price: 100,
                stockQuantity: 5,
                sku: `SKU${id}`,
                imageUrls: `https://example.com/${id}.jpg`,
                offer: 20,
                offerStart: '2024-01-01T00:00:00Z',
                offerEnd: '2099-12-31T23:59:59Z',
                itemVariantAttributes: [],
                deleted: false,
            },
        ],
    };
}

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

describe('Home - Language switching updates all card names', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
        vi.spyOn(navigator, 'language', 'get').mockReturnValue('en-US');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
        vi.restoreAllMocks();
    });

    it('displays French names in Recently Added card after switching to French', async () => {
        mockFetchByUrl({
            recently: { isSuccess: true, value: [makeProduct('1', 'Winter Jacket', 'Veste d\'hiver')] },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            const card = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
            expect(card?.querySelector('.item-name')?.textContent).toBe('Winter Jacket');
        });

        const languageSelect = screen.getByRole('combobox', { name: /Select language|Sélectionner la langue/ });
        const user = userEvent.setup();
        await user.selectOptions(languageSelect, 'fr');

        await waitFor(() => {
            const card = screen.getByText(/Articles récemment ajoutés/).closest('.item-preview-card');
            expect(card?.querySelector('.item-name')?.textContent).toBe('Veste d\'hiver');
        });
    });

    it('displays French names in Suggested Items card after switching to French', async () => {
        mockFetchByUrl({
            suggested: { isSuccess: true, value: [makeProduct('2', 'Running Shoes', 'Chaussures de course')] },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            const card = screen.getByText(/Suggested items|Articles suggérés/).closest('.item-preview-card');
            expect(card?.querySelector('.item-name')?.textContent).toBe('Running Shoes');
        });

        const languageSelect = screen.getByRole('combobox', { name: /Select language|Sélectionner la langue/ });
        const user = userEvent.setup();
        await user.selectOptions(languageSelect, 'fr');

        await waitFor(() => {
            const card = screen.getByText(/Articles suggérés/).closest('.item-preview-card');
            expect(card?.querySelector('.item-name')?.textContent).toBe('Chaussures de course');
        });
    });

    it('displays French names in Offers card after switching to French', async () => {
        mockFetchByUrl({
            offers: { isSuccess: true, value: [makeOfferProduct('3', 'Coffee Maker', 'Cafetière')] },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            const allOffers = screen.getAllByText(/^Offers$|^Offres$/);
            const offersTitle = allOffers.find(el => el.classList.contains('card-title'));
            const card = offersTitle?.closest('.item-preview-card');
            expect(card?.querySelector('.item-name')?.textContent).toBe('Coffee Maker');
        });

        const languageSelect = screen.getByRole('combobox', { name: /Select language|Sélectionner la langue/ });
        const user = userEvent.setup();
        await user.selectOptions(languageSelect, 'fr');

        await waitFor(() => {
            const allOffers = screen.getAllByText(/^Offers$|^Offres$/);
            const offersTitle = allOffers.find(el => el.classList.contains('card-title'));
            const card = offersTitle?.closest('.item-preview-card');
            expect(card?.querySelector('.item-name')?.textContent).toBe('Cafetière');
        });
    });

    it('displays French names in all cards when browser language is French', async () => {
        vi.spyOn(navigator, 'language', 'get').mockReturnValue('fr-CA');

        mockFetchByUrl({
            recently: { isSuccess: true, value: [makeProduct('1', 'Winter Jacket', 'Veste d\'hiver')] },
            suggested: { isSuccess: true, value: [makeProduct('2', 'Running Shoes', 'Chaussures de course')] },
            offers: { isSuccess: true, value: [makeOfferProduct('3', 'Coffee Maker', 'Cafetière')] },
        });

        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            const recentCard = screen.getByText(/Articles récemment ajoutés/).closest('.item-preview-card');
            expect(recentCard?.querySelector('.item-name')?.textContent).toBe('Veste d\'hiver');
        });

        await waitFor(() => {
            const suggestedCard = screen.getByText(/Articles suggérés/).closest('.item-preview-card');
            expect(suggestedCard?.querySelector('.item-name')?.textContent).toBe('Chaussures de course');
        });

        await waitFor(() => {
            const allOffers = screen.getAllByText(/^Offers$|^Offres$/);
            const offersTitle = allOffers.find(el => el.classList.contains('card-title'));
            const offersCard = offersTitle?.closest('.item-preview-card');
            expect(offersCard?.querySelector('.item-name')?.textContent).toBe('Cafetière');
        });
    });
});
