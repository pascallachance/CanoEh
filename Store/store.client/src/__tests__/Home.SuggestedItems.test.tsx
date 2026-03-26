import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

// Helper to build a minimal product with given variants
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

// Empty response used to satisfy fetch calls that aren't under test
const emptyApiResponse = { isSuccess: true, value: [] };

// Mock all 4 fetch calls made by Home: recently added, suggested, offers, categories
// The order is: fetchRecentlyAddedProducts → fetchSuggestedProducts → fetchProductsWithOffers → fetchSuggestedCategoriesProducts
function mockFetchCalls(suggestedResponse: object) {
    (global.fetch as ReturnType<typeof vi.fn>)
        .mockResolvedValueOnce({ ok: true, json: async () => emptyApiResponse })  // recently added
        .mockResolvedValueOnce({ ok: true, json: async () => suggestedResponse }) // suggested
        .mockResolvedValueOnce({ ok: true, json: async () => emptyApiResponse })  // offers
        .mockResolvedValueOnce({ ok: true, json: async () => emptyApiResponse }); // categories
}

describe('Home - Suggested Items image filtering', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('should find an image on a later variant when the first variant has no image', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [
                    { /* no imageUrls, no thumbnailUrl */ },
                    { imageUrls: 'https://example.com/variant2.jpg' },
                ]),
                makeProduct('2', [{ imageUrls: 'https://example.com/p2.jpg' }]),
                makeProduct('3', [{ imageUrls: 'https://example.com/p3.jpg' }]),
                makeProduct('4', [{ imageUrls: 'https://example.com/p4.jpg' }]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');
        expect(card).toBeInTheDocument();

        // All 4 products have at least one variant with an image, so 4 images expected
        const images = card?.querySelectorAll('.item-image');
        expect(images?.length).toBe(4);
    });

    it('should still show 4 images when extra products without images are returned', async () => {
        // Simulate over-fetch: API returns 6 products, first 2 have no images
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('noimg1', [{ /* no imageUrls, no thumbnailUrl */ }]),
                makeProduct('noimg2', [{ /* no imageUrls, no thumbnailUrl */ }]),
                makeProduct('3', [{ imageUrls: 'https://example.com/p3.jpg' }]),
                makeProduct('4', [{ imageUrls: 'https://example.com/p4.jpg' }]),
                makeProduct('5', [{ imageUrls: 'https://example.com/p5.jpg' }]),
                makeProduct('6', [{ imageUrls: 'https://example.com/p6.jpg' }]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');
        expect(card).toBeInTheDocument();

        // Despite 2 products lacking images, the remaining 4 fill in the display
        const images = card?.querySelectorAll('.item-image');
        expect(images?.length).toBe(4);
    });

    it('should use the first variant that has an image when multiple variants have images', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [
                    { imageUrls: 'https://example.com/first.jpg' },
                    { imageUrls: 'https://example.com/second.jpg' },
                ]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');
        const images = card?.querySelectorAll('.item-image');

        expect(images?.length).toBe(1);
        expect(images?.[0].getAttribute('src')).toBe('https://example.com/first.jpg');
    });

    it('should fall back to thumbnailUrl when no variant has imageUrls', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [
                    { thumbnailUrl: 'https://example.com/thumb.jpg' },
                ]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');
        const images = card?.querySelectorAll('.item-image');

        expect(images?.length).toBe(1);
        expect(images?.[0].getAttribute('src')).toBe('https://example.com/thumb.jpg');
    });

    it('should prefer imageUrls over thumbnailUrl on the same variant', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [
                    {
                        imageUrls: 'https://example.com/full.jpg',
                        thumbnailUrl: 'https://example.com/thumb.jpg',
                    },
                ]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');
        const images = card?.querySelectorAll('.item-image');

        expect(images?.length).toBe(1);
        // imageUrls should be chosen over thumbnailUrl
        expect(images?.[0].getAttribute('src')).toBe('https://example.com/full.jpg');
    });

    it('should not show offer badge when variant offer is expired', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [{
                    imageUrls: 'https://example.com/p1.jpg',
                    offer: 25,
                    offerStart: '2024-01-01T00:00:00Z',
                    offerEnd: '2024-01-02T00:00:00Z', // expired in the past
                }]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');

        // Image should still appear
        const images = card?.querySelectorAll('.item-image');
        expect(images?.length).toBe(1);

        // But offer badge should NOT be shown for an expired offer
        const offerBadges = card?.querySelectorAll('.offer-badge');
        expect(offerBadges?.length).toBe(0);
    });

    it('should not show offer badge when variant offerStart is in the future', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [{
                    imageUrls: 'https://example.com/p1.jpg',
                    offer: 15,
                    offerStart: '2099-01-01T00:00:00Z', // not started yet
                    offerEnd: '2099-12-31T23:59:59Z',
                }]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');

        // Image should still appear
        const images = card?.querySelectorAll('.item-image');
        expect(images?.length).toBe(1);

        // Offer badge should NOT be shown for an offer that hasn't started yet
        const offerBadges = card?.querySelectorAll('.offer-badge');
        expect(offerBadges?.length).toBe(0);
    });

    it('should show offer badge when variant offer is active', async () => {
        const suggestedResponse = {
            isSuccess: true,
            value: [
                makeProduct('1', [{
                    imageUrls: 'https://example.com/p1.jpg',
                    offer: 20,
                    offerStart: '2024-01-01T00:00:00Z',
                    offerEnd: '2099-12-31T23:59:59Z', // active offer
                }]),
            ],
        };

        mockFetchCalls(suggestedResponse);
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetSuggestedProducts?count=24')
            );
        });

        const suggestedSection = await screen.findByText(/Suggested items|Articles suggérés/);
        const card = suggestedSection.closest('.item-preview-card');

        const offerBadges = card?.querySelectorAll('.offer-badge');
        expect(offerBadges?.length).toBe(1);
        const badgeTexts = Array.from(offerBadges || []).map(b => b.textContent || '');
        expect(badgeTexts.some(t => t === '20% OFF' || t === 'Rabais 20%')).toBe(true);
    });
});
