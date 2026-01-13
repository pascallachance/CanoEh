import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

describe('Home - Image Filtering', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        // Mock environment variable using vi.stubEnv for proper test isolation
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('should filter out products without images and only display products with valid images', async () => {
        // Mock API response with 6 products: 2 without images, 4 with images
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product 1 - No Image',
                    name_fr: 'Produit 1',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            // No imageUrls or thumbnailUrl
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '2',
                    sellerID: 'seller2',
                    name_en: 'Product 2 - With Image',
                    name_fr: 'Produit 2',
                    categoryID: 'cat1',
                    createdAt: '2024-01-02',
                    deleted: false,
                    variants: [
                        {
                            id: 'var2',
                            price: 20,
                            stockQuantity: 10,
                            sku: 'SKU2',
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '3',
                    sellerID: 'seller3',
                    name_en: 'Product 3 - No Image',
                    name_fr: 'Produit 3',
                    categoryID: 'cat1',
                    createdAt: '2024-01-03',
                    deleted: false,
                    variants: [
                        {
                            id: 'var3',
                            price: 15,
                            stockQuantity: 3,
                            sku: 'SKU3',
                            // No imageUrls or thumbnailUrl
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '4',
                    sellerID: 'seller4',
                    name_en: 'Product 4 - With Thumbnail',
                    name_fr: 'Produit 4',
                    categoryID: 'cat1',
                    createdAt: '2024-01-04',
                    deleted: false,
                    variants: [
                        {
                            id: 'var4',
                            price: 25,
                            stockQuantity: 8,
                            sku: 'SKU4',
                            thumbnailUrl: 'https://example.com/thumb2.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '5',
                    sellerID: 'seller5',
                    name_en: 'Product 5 - With Image',
                    name_fr: 'Produit 5',
                    categoryID: 'cat1',
                    createdAt: '2024-01-05',
                    deleted: false,
                    variants: [
                        {
                            id: 'var5',
                            price: 30,
                            stockQuantity: 12,
                            sku: 'SKU5',
                            imageUrls: 'https://example.com/image3.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '6',
                    sellerID: 'seller6',
                    name_en: 'Product 6 - With Image',
                    name_fr: 'Produit 6',
                    categoryID: 'cat1',
                    createdAt: '2024-01-06',
                    deleted: false,
                    variants: [
                        {
                            id: 'var6',
                            price: 35,
                            stockQuantity: 7,
                            sku: 'SKU6',
                            imageUrls: 'https://example.com/image4.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        // Wait for the fetch to complete
        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetRecentlyAddedProducts?count=20')
            );
        });

        // Wait a bit for state updates
        await waitFor(() => {
            // Check that the "Recently added items" section is rendered
            const recentlyAddedSection = screen.getByText(/Recently added items|Articles récemment ajoutés/);
            expect(recentlyAddedSection).toBeInTheDocument();
        });

        // Find all image elements in the recently added section
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        expect(recentlyAddedCard).toBeInTheDocument();

        // Count actual images (not placeholders)
        const images = recentlyAddedCard?.querySelectorAll('.item-image');
        
        // We should have exactly 4 images (the products with valid image URLs)
        // Products 2, 4, 5, 6 have images
        expect(images?.length).toBe(4);

        // Verify no empty placeholders are shown when there are images
        const placeholders = recentlyAddedCard?.querySelectorAll('.item-image-placeholder');
        // All 4 items should show images, not placeholders
        expect(placeholders?.length).toBe(0);
    });

    it('should show placeholders when no products have images', async () => {
        // Mock API response with products that have no images
        const mockResponse = {
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
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            // No images
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // When no products have images, should show the default 4 placeholders
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const placeholders = recentlyAddedCard?.querySelectorAll('.item-placeholder');
        
        // Should still show 4 placeholder slots
        expect(placeholders?.length).toBe(4);
    });

    it('should handle empty imageUrls string correctly', async () => {
        // Mock API response with product that has empty imageUrls
        const mockResponse = {
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
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: '',  // Empty string
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // Should show placeholders since empty string is not a valid image
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const placeholders = recentlyAddedCard?.querySelectorAll('.item-placeholder');
        
        expect(placeholders?.length).toBe(4);
    });

    it('should handle imageUrls with only whitespace or comma-separated whitespace', async () => {
        // Mock API response with product that has whitespace-only imageUrls
        const mockResponse = {
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
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: '   ,  ,  ',  // Only whitespace
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // Should show placeholders since whitespace-only strings are not valid images
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const placeholders = recentlyAddedCard?.querySelectorAll('.item-placeholder');
        
        expect(placeholders?.length).toBe(4);
    });

    it('should extract first URL from comma-separated imageUrls', async () => {
        // Mock API response with product that has multiple comma-separated URLs
        const mockResponse = {
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
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/first.jpg,https://example.com/second.jpg,https://example.com/third.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // Should display the first image from comma-separated list
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const images = recentlyAddedCard?.querySelectorAll('.item-image');
        
        expect(images?.length).toBe(1);
        expect(images?.[0].getAttribute('src')).toBe('https://example.com/first.jpg');
    });

    it('should display exactly 2 images when only 2 out of many products have images', async () => {
        // Mock API response with 6 products but only 2 have images
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product 1 - No Image',
                    name_fr: 'Produit 1',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            // No images
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '2',
                    sellerID: 'seller2',
                    name_en: 'Product 2 - With Image',
                    name_fr: 'Produit 2',
                    categoryID: 'cat1',
                    createdAt: '2024-01-02',
                    deleted: false,
                    variants: [
                        {
                            id: 'var2',
                            price: 20,
                            stockQuantity: 10,
                            sku: 'SKU2',
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '3',
                    sellerID: 'seller3',
                    name_en: 'Product 3 - No Image',
                    name_fr: 'Produit 3',
                    categoryID: 'cat1',
                    createdAt: '2024-01-03',
                    deleted: false,
                    variants: [
                        {
                            id: 'var3',
                            price: 15,
                            stockQuantity: 3,
                            sku: 'SKU3',
                            // No images
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '4',
                    sellerID: 'seller4',
                    name_en: 'Product 4 - With Image',
                    name_fr: 'Produit 4',
                    categoryID: 'cat1',
                    createdAt: '2024-01-04',
                    deleted: false,
                    variants: [
                        {
                            id: 'var4',
                            price: 25,
                            stockQuantity: 8,
                            sku: 'SKU4',
                            thumbnailUrl: 'https://example.com/thumb1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '5',
                    sellerID: 'seller5',
                    name_en: 'Product 5 - No Image',
                    name_fr: 'Produit 5',
                    categoryID: 'cat1',
                    createdAt: '2024-01-05',
                    deleted: false,
                    variants: [
                        {
                            id: 'var5',
                            price: 30,
                            stockQuantity: 12,
                            sku: 'SKU5',
                            // No images
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '6',
                    sellerID: 'seller6',
                    name_en: 'Product 6 - No Image',
                    name_fr: 'Produit 6',
                    categoryID: 'cat1',
                    createdAt: '2024-01-06',
                    deleted: false,
                    variants: [
                        {
                            id: 'var6',
                            price: 35,
                            stockQuantity: 7,
                            sku: 'SKU6',
                            // No images
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // Should display exactly 2 images, not 4 placeholders
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const images = recentlyAddedCard?.querySelectorAll('.item-image');
        const allItems = recentlyAddedCard?.querySelectorAll('.item-placeholder');
        
        // Should have 2 items total (matching the 2 products with images)
        expect(allItems?.length).toBe(2);
        // Both should be images, not placeholders
        expect(images?.length).toBe(2);
    });

    it('should prepend API base URL to relative image paths', async () => {
        // Mock API response with product that has relative image path
        const mockResponse = {
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
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: '/uploads/company-id/variant-id/image.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // Should prepend API base URL to relative path
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const images = recentlyAddedCard?.querySelectorAll('.item-image');
        
        expect(images?.length).toBe(1);
        expect(images?.[0].getAttribute('src')).toBe('https://localhost:7182/uploads/company-id/variant-id/image.jpg');
    });

    it('should not modify absolute URLs that already start with http', async () => {
        // Mock API response with product that has absolute URL
        const mockResponse = {
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
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: 'https://cdn.example.com/image.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
            ok: true,
            json: async () => mockResponse
        });

        render(
            <BrowserRouter>
                <Home />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalled();
        });

        // Should keep absolute URL as-is
        const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
        const images = recentlyAddedCard?.querySelectorAll('.item-image');
        
        expect(images?.length).toBe(1);
        expect(images?.[0].getAttribute('src')).toBe('https://cdn.example.com/image.jpg');
    });
});
