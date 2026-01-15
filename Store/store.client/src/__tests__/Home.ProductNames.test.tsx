import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

describe('Home - Product Name Extraction', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('should extract product names and display them for recently added products', async () => {
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Test Product Name',
                    name_fr: 'Nom de Produit Test',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
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

        // Wait for the component to render with the product name
        await waitFor(() => {
            const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
            const itemName = recentlyAddedCard?.querySelector('.item-name');
            expect(itemName).toBeInTheDocument();
            // Should display the English name by default (navigator.language defaults to en-US in test environment)
            expect(itemName?.textContent).toBe('Test Product Name');
        });
    });

    it('should ensure names array matches images array in length', async () => {
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
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '2',
                    sellerID: 'seller2',
                    name_en: 'Product 2',
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
                            imageUrls: 'https://example.com/image2.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '3',
                    sellerID: 'seller3',
                    name_en: 'Product 3',
                    name_fr: 'Produit 3',
                    categoryID: 'cat1',
                    createdAt: '2024-01-03',
                    deleted: false,
                    variants: [
                        {
                            id: 'var3',
                            price: 30,
                            stockQuantity: 15,
                            sku: 'SKU3',
                            imageUrls: 'https://example.com/image3.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
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

        // Verify that the number of item-name elements matches the number of images
        await waitFor(() => {
            const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
            const images = recentlyAddedCard?.querySelectorAll('.item-image');
            const itemNames = recentlyAddedCard?.querySelectorAll('.item-name');
            
            expect(images?.length).toBe(3);
            expect(itemNames?.length).toBe(3);
        });
    });

    it('should render item-name elements with correct text content', async () => {
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'First Product',
                    name_fr: 'Premier Produit',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                },
                {
                    id: '2',
                    sellerID: 'seller2',
                    name_en: 'Second Product',
                    name_fr: 'Deuxième Produit',
                    categoryID: 'cat1',
                    createdAt: '2024-01-02',
                    deleted: false,
                    variants: [
                        {
                            id: 'var2',
                            price: 20,
                            stockQuantity: 10,
                            sku: 'SKU2',
                            imageUrls: 'https://example.com/image2.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
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

        // Verify that each item-name element has the correct text content
        await waitFor(() => {
            const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
            const itemNames = recentlyAddedCard?.querySelectorAll('.item-name');
            
            expect(itemNames?.[0]?.textContent).toBe('First Product');
            expect(itemNames?.[1]?.textContent).toBe('Second Product');
        });
    });

    it('should extract product names for suggested products', async () => {
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Suggested Product',
                    name_fr: 'Produit Suggéré',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
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

        // Verify that suggested items also have product names
        await waitFor(() => {
            const suggestedCard = screen.getByText(/Suggested items|Articles suggérés/).closest('.item-preview-card');
            const itemName = suggestedCard?.querySelector('.item-name');
            expect(itemName).toBeInTheDocument();
            expect(itemName?.textContent).toBe('Suggested Product');
        });
    });

    it('should use product name in alt text when available', async () => {
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product With Alt Text',
                    name_fr: 'Produit Avec Texte Alt',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            imageUrls: 'https://example.com/image1.jpg',
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
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

        // Verify that the alt text uses the product name
        await waitFor(() => {
            const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
            const image = recentlyAddedCard?.querySelector('.item-image') as HTMLImageElement;
            expect(image?.alt).toBe('Product With Alt Text');
        });
    });

    it('should not display product names for items without images', async () => {
        const mockResponse = {
            isSuccess: true,
            value: [
                {
                    id: '1',
                    sellerID: 'seller1',
                    name_en: 'Product Without Image',
                    name_fr: 'Produit Sans Image',
                    categoryID: 'cat1',
                    createdAt: '2024-01-01',
                    deleted: false,
                    variants: [
                        {
                            id: 'var1',
                            price: 10,
                            stockQuantity: 5,
                            sku: 'SKU1',
                            // No imageUrls
                            itemVariantAttributes: [],
                            deleted: false
                        }
                    ],
                    itemAttributes: []
                }
            ]
        };

        (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
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

        // Verify that products without images don't show item names
        await waitFor(() => {
            const recentlyAddedCard = screen.getByText(/Recently added items|Articles récemment ajoutés/).closest('.item-preview-card');
            const itemNames = recentlyAddedCard?.querySelectorAll('.item-name');
            // No product names should be displayed since there are no images
            expect(itemNames?.length).toBe(0);
        });
    });
});
