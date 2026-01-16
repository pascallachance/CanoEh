import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

describe('Home - Offers Card', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        // Mock environment variable
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'http://localhost:5269');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('should display images in Offers card when products with offers have images', async () => {
        // Mock response for GetRecentlyAddedProducts (called first)
        const mockRecentProducts = {
            isSuccess: true,
            value: []
        };

        // Mock response for GetSuggestedProducts (called second)
        const mockSuggestedProducts = {
            isSuccess: true,
            value: []
        };

        // Mock response for GetProductsWithOffers (called third) - products with offers and images
        const mockOffersResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product 1 with Offer',
                    name_fr: 'Produit 1 avec offre',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 100,
                            stockQuantity: 10,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/product1.jpg',
                            offer: 25, // 25% off
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '2',
                    sellerID: 'seller2',
                    name_en: 'Product 2 with Offer',
                    name_fr: 'Produit 2 avec offre',
                    categoryID: 'cat1',
                    createdAt: '2024-01-02',
                    deleted: false,
                    variants: [
                        {
                            id: 'var2',
                            price: 200,
                            stockQuantity: 5,
                            sku: 'SKU2',
                            thumbnailUrl: 'https://example.com/product2-thumb.jpg',
                            offer: 50, // 50% off
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        // Setup fetch mock to return different responses based on URL
        (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
            if (url.includes('GetRecentlyAddedProducts')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockRecentProducts
                });
            } else if (url.includes('GetSuggestedProducts')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockSuggestedProducts
                });
            } else if (url.includes('GetProductsWithOffers')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockOffersResponse
                });
            }
            return Promise.resolve({
                ok: false,
                json: async () => ({})
            });
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        // Wait for all three fetch calls
        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetProductsWithOffers?count=4')
            );
        });

        // Wait for Offers section to be rendered
        await waitFor(() => {
            const allOffersElements = screen.queryAllByText(/^Offers$|^Offres$/);
            const offersCardTitle = allOffersElements.find(el => el.classList.contains('card-title'));
            expect(offersCardTitle).toBeInTheDocument();
        });

        // Find the Offers card by getting the card title specifically
        const allOffersElements = screen.getAllByText(/^Offers$|^Offres$/);
        const offersCardTitle = allOffersElements.find(el => el.classList.contains('card-title'));
        const offersCard = offersCardTitle?.closest('.item-preview-card');
        expect(offersCard).toBeInTheDocument();

        // Check that images are present in the Offers card
        const images = offersCard?.querySelectorAll('.item-image');
        
        // We should have 2 images (one from each product variant with offer)
        expect(images?.length).toBe(2);

        // Verify image sources are correct (order is random due to shuffle)
        const imageSrcs = Array.from(images || []).map(img => img.getAttribute('src') || '');
        const hasProduct1 = imageSrcs.some(src => src.includes('product1.jpg'));
        const hasProduct2 = imageSrcs.some(src => src.includes('product2-thumb.jpg'));
        expect(hasProduct1).toBe(true);
        expect(hasProduct2).toBe(true);

        // Check that offer badges are displayed
        const offerBadges = offersCard?.querySelectorAll('.offer-badge');
        expect(offerBadges?.length).toBe(2);

        // Verify offer percentages are correct (order is random)
        const badgeTexts = Array.from(offerBadges || []).map(badge => badge.textContent || '');
        const has25Off = badgeTexts.some(text => text.includes('25% OFF'));
        const has50Off = badgeTexts.some(text => text.includes('50% OFF'));
        expect(has25Off).toBe(true);
        expect(has50Off).toBe(true);

        // Check that product names are displayed
        const itemNames = offersCard?.querySelectorAll('.item-name');
        expect(itemNames?.length).toBe(2);
        const nameTexts = Array.from(itemNames || []).map(name => name.textContent || '');
        const hasProduct1Name = nameTexts.some(text => text === 'Product 1 with Offer');
        const hasProduct2Name = nameTexts.some(text => text === 'Product 2 with Offer');
        expect(hasProduct1Name).toBe(true);
        expect(hasProduct2Name).toBe(true);
    });

    it('should handle offers without images gracefully', async () => {
        // Mock response for GetRecentlyAddedProducts
        const mockRecentProducts = {
            isSuccess: true,
            value: []
        };

        // Mock response for GetSuggestedProducts
        const mockSuggestedProducts = {
            isSuccess: true,
            value: []
        };

        // Mock response for GetProductsWithOffers - products with offers but NO images
        const mockOffersResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product 1 with Offer',
                    name_fr: 'Produit 1 avec offre',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 100,
                            stockQuantity: 10,
                            sku: 'SKU1',
                            // No imageUrls or thumbnailUrl
                            offer: 25,
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
            if (url.includes('GetRecentlyAddedProducts')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockRecentProducts
                });
            } else if (url.includes('GetSuggestedProducts')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockSuggestedProducts
                });
            } else if (url.includes('GetProductsWithOffers')) {
                return Promise.resolve({
                    ok: true,
                    json: async () => mockOffersResponse
                });
            }
            return Promise.resolve({
                ok: false,
                json: async () => ({})
            });
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetProductsWithOffers?count=4')
            );
        });

        await waitFor(() => {
            const allOffersElements = screen.queryAllByText(/^Offers$|^Offres$/);
            const offersCardTitle = allOffersElements.find(el => el.classList.contains('card-title'));
            expect(offersCardTitle).toBeInTheDocument();
        });

        // Find the Offers card
        const allOffersElements = screen.getAllByText(/^Offers$|^Offres$/);
        const offersCardTitle = allOffersElements.find(el => el.classList.contains('card-title'));
        const offersCard = offersCardTitle?.closest('.item-preview-card');
        expect(offersCard).toBeInTheDocument();

        // When offers have no images, should show placeholders
        const images = offersCard?.querySelectorAll('.item-image');
        const placeholders = offersCard?.querySelectorAll('.item-image-placeholder');
        
        // Should have 0 images and 4 placeholders
        expect(images?.length).toBe(0);
        expect(placeholders?.length).toBe(4);
    });

    it('should display multiple variants with offers from the same product', async () => {
        const mockRecentProducts = { isSuccess: true, value: [] };
        const mockSuggestedProducts = { isSuccess: true, value: [] };

        // Product with 2 variants, both with offers
        const mockOffersResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product 1',
                    name_fr: 'Produit 1',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 100,
                            stockQuantity: 10,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/variant1.jpg',
                            offer: 25,
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        },
                        {
                            id: 'var2',
                            price: 150,
                            stockQuantity: 5,
                            sku: 'SKU2',
                            imageUrls: 'https://example.com/variant2.jpg',
                            offer: 30,
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
            if (url.includes('GetRecentlyAddedProducts')) {
                return Promise.resolve({ ok: true, json: async () => mockRecentProducts });
            } else if (url.includes('GetSuggestedProducts')) {
                return Promise.resolve({ ok: true, json: async () => mockSuggestedProducts });
            } else if (url.includes('GetProductsWithOffers')) {
                return Promise.resolve({ ok: true, json: async () => mockOffersResponse });
            }
            return Promise.resolve({ ok: false, json: async () => ({}) });
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetProductsWithOffers?count=4')
            );
        });

        await waitFor(() => {
            const allOffersElements = screen.queryAllByText(/^Offers$|^Offres$/);
            const offersCardTitle = allOffersElements.find(el => el.classList.contains('card-title'));
            expect(offersCardTitle).toBeInTheDocument();
        });

        const allOffersElements = screen.getAllByText(/^Offers$|^Offres$/);
        const offersCardTitle = allOffersElements.find(el => el.classList.contains('card-title'));
        const offersCard = offersCardTitle?.closest('.item-preview-card');
        expect(offersCard).toBeInTheDocument();

        const images = offersCard?.querySelectorAll('.item-image');
        
        // Should display both variants (randomly selected, but we have only 2 so both should show)
        expect(images?.length).toBeGreaterThanOrEqual(1);
        expect(images?.length).toBeLessThanOrEqual(2);
    });
});
