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
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
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

        // Mock response for GetProductsWithOffers (called third) - products with offers and images ending with _1
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
                            imageUrls: 'https://example.com/product1_1.jpg,https://example.com/product1_2.jpg',
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
                            imageUrls: 'https://example.com/product2_1.png',
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
        
        // We should have 2 images (one from each product with offer)
        expect(images?.length).toBe(2);

        // Verify image sources are correct - should prioritize images ending with _1
        const imageSrcs = Array.from(images || []).map(img => img.getAttribute('src') || '');
        const hasProduct1 = imageSrcs.some(src => src.includes('product1_1.jpg'));
        const hasProduct2 = imageSrcs.some(src => src.includes('product2_1.png'));
        expect(hasProduct1).toBe(true);
        expect(hasProduct2).toBe(true);

        // Check that offer badges are displayed
        const offerBadges = offersCard?.querySelectorAll('.offer-badge');
        expect(offerBadges?.length).toBe(2);

        // Verify offer percentages are correct (check for both English and French formats)
        const badgeTexts = Array.from(offerBadges || []).map(badge => badge.textContent || '');
        expect(badgeTexts.some(text => text === '25% OFF' || text === 'Rabais 25%')).toBe(true);
        expect(badgeTexts.some(text => text === '50% OFF' || text === 'Rabais 50%')).toBe(true);

        // Check that product names are displayed
        const itemNames = offersCard?.querySelectorAll('.item-name');
        expect(itemNames?.length).toBe(2);
        const nameTexts = Array.from(itemNames || []).map(name => name.textContent || '');
        expect(nameTexts).toContain('Product 1 with Offer');
        expect(nameTexts).toContain('Product 2 with Offer');
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

    it('should display first variant with offer when product has multiple variants with offers', async () => {
        const mockRecentProducts = { isSuccess: true, value: [] };
        const mockSuggestedProducts = { isSuccess: true, value: [] };

        // Product with 2 variants, both with offers - should only show the first variant with offer
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
                            imageUrls: 'https://example.com/variant1_1.jpg',
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
                            imageUrls: 'https://example.com/variant2_1.jpg',
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
        
        // Should display only one variant from the product (the first one with an offer)
        expect(images?.length).toBe(1);
        
        // Verify it's the first variant's image, not the second variant
        const imageSrcs = Array.from(images || []).map(img => img.getAttribute('src') || '');
        expect(imageSrcs[0]).toContain('variant1_1.jpg');
        expect(imageSrcs[0]).not.toContain('variant2_1.jpg');
    });

    it('should prioritize images ending with _1 when selecting from multiple images', async () => {
        const mockRecentProducts = { isSuccess: true, value: [] };
        const mockSuggestedProducts = { isSuccess: true, value: [] };

        // Product with variant that has multiple images - should select the one ending with _1
        const mockOffersResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product with Multiple Images',
                    name_fr: 'Produit avec plusieurs images',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 100,
                            stockQuantity: 10,
                            sku: 'SKU1',
                            // _1 image is not first, but should still be selected
                            imageUrls: 'https://example.com/product_3.jpg,https://example.com/product_1.jpg,https://example.com/product_2.jpg',
                            offer: 20,
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
        expect(images?.length).toBe(1);
        
        // Verify that the image ending with _1 was selected (not the first image in the list)
        const imageSrcs = Array.from(images || []).map(img => img.getAttribute('src') || '');
        expect(imageSrcs[0]).toContain('product_1.jpg');
        expect(imageSrcs[0]).not.toContain('product_3.jpg');
    });

    it('should fallback to first image when no _1 image is found', async () => {
        const mockRecentProducts = { isSuccess: true, value: [] };
        const mockSuggestedProducts = { isSuccess: true, value: [] };

        // Product with variant that has images but none ending with _1
        const mockOffersResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product without _1 Image',
                    name_fr: 'Produit sans image _1',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 100,
                            stockQuantity: 10,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/product_first.jpg,https://example.com/product_second.jpg',
                            offer: 15,
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
        expect(images?.length).toBe(1);
        
        // Should fallback to the first image when no _1 image is found
        const imageSrcs = Array.from(images || []).map(img => img.getAttribute('src') || '');
        expect(imageSrcs[0]).toContain('product_first.jpg');
    });

    it('should support all file extensions in the _1 pattern (jpg, jpeg, png, gif, webp)', async () => {
        const mockRecentProducts = { isSuccess: true, value: [] };
        const mockSuggestedProducts = { isSuccess: true, value: [] };

        // Test products with different _1 image extensions
        const mockOffersResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product with PNG',
                    name_fr: 'Produit avec PNG',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 100,
                            stockQuantity: 10,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/product_2.png,https://example.com/product_1.png',
                            offer: 10,
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
                    name_en: 'Product with GIF',
                    name_fr: 'Produit avec GIF',
                    categoryID: 'cat1',
                    createdAt: '2024-01-02',
                    deleted: false,
                    variants: [
                        {
                            id: 'var2',
                            price: 200,
                            stockQuantity: 5,
                            sku: 'SKU2',
                            imageUrls: 'https://example.com/product_3.gif,https://example.com/product_1.gif',
                            offer: 15,
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '3',
                    sellerID: 'seller3',
                    name_en: 'Product with WEBP',
                    name_fr: 'Produit avec WEBP',
                    categoryID: 'cat1',
                    createdAt: '2024-01-03',
                    deleted: false,
                    variants: [
                        {
                            id: 'var3',
                            price: 300,
                            stockQuantity: 8,
                            sku: 'SKU3',
                            imageUrls: 'https://example.com/product_2.webp,https://example.com/product_1.webp',
                            offer: 20,
                            offerStart: '2024-01-01T00:00:00Z',
                            offerEnd: '2024-12-31T23:59:59Z',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '4',
                    sellerID: 'seller4',
                    name_en: 'Product with JPEG',
                    name_fr: 'Produit avec JPEG',
                    categoryID: 'cat1',
                    createdAt: '2024-01-04',
                    deleted: false,
                    variants: [
                        {
                            id: 'var4',
                            price: 400,
                            stockQuantity: 3,
                            sku: 'SKU4',
                            imageUrls: 'https://example.com/product_3.jpeg,https://example.com/product_1.jpeg',
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
        expect(images?.length).toBe(4);
        
        // Verify that all supported extensions are matched correctly
        const imageSrcs = Array.from(images || []).map(img => img.getAttribute('src') || '');
        expect(imageSrcs.some(src => src.includes('product_1.png'))).toBe(true);
        expect(imageSrcs.some(src => src.includes('product_1.gif'))).toBe(true);
        expect(imageSrcs.some(src => src.includes('product_1.webp'))).toBe(true);
        expect(imageSrcs.some(src => src.includes('product_1.jpeg'))).toBe(true);
    });
});
