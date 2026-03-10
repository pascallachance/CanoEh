import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import Product from '../components/Product';

// Mock fetch globally
global.fetch = vi.fn();

const API_BASE_URL = 'https://localhost:7182';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function makeVariant(overrides: {
    id?: string;
    price?: number;
    stockQuantity?: number;
    sku?: string;
    imageUrls?: string;
    thumbnailUrl?: string;
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
    itemVariantAttributes?: object[];
    deleted?: boolean;
} = {}) {
    return {
        id: overrides.id ?? 'var1',
        price: overrides.price ?? 50,
        stockQuantity: overrides.stockQuantity ?? 10,
        sku: overrides.sku ?? 'SKU-001',
        imageUrls: overrides.imageUrls ?? 'https://example.com/img1.jpg',
        thumbnailUrl: overrides.thumbnailUrl,
        offer: overrides.offer ?? null,
        offerStart: overrides.offerStart ?? null,
        offerEnd: overrides.offerEnd ?? null,
        itemVariantAttributes: overrides.itemVariantAttributes ?? [],
        deleted: overrides.deleted ?? false,
    };
}

function makeProduct(overrides: {
    id?: string;
    name_en?: string;
    name_fr?: string;
    description_en?: string;
    description_fr?: string;
    categoryName_en?: string;
    categoryName_fr?: string;
    variants?: object[];
} = {}) {
    return {
        id: overrides.id ?? 'prod1',
        sellerID: 'seller1',
        name_en: overrides.name_en ?? 'Test Product',
        name_fr: overrides.name_fr ?? 'Produit Test',
        description_en: overrides.description_en,
        description_fr: overrides.description_fr,
        categoryNodeID: 'cat1',
        categoryName_en: overrides.categoryName_en ?? 'Electronics',
        categoryName_fr: overrides.categoryName_fr ?? 'Électronique',
        createdAt: '2024-01-01',
        deleted: false,
        itemAttributes: [],
        variants: overrides.variants ?? [makeVariant()],
    };
}

function makeApiResult(product: object) {
    return { isSuccess: true, value: product };
}

function setupFetchSuccess(product: object) {
    (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        json: async () => makeApiResult(product),
    });
}

function setupFetchNotFound() {
    (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: false,
        status: 404,
        json: async () => ({ isSuccess: false, error: 'Not Found' }),
    });
}

function setupFetchNetworkError() {
    (global.fetch as ReturnType<typeof vi.fn>).mockRejectedValue(new Error('Network Error'));
}

/** Render the Product page with a given product ID in the URL */
function renderProduct(productId = 'prod1') {
    return render(
        <MemoryRouter initialEntries={[`/product/${productId}`]}>
            <Routes>
                <Route path="/product/:id" element={<Product />} />
                <Route path="/" element={<div>Home</div>} />
            </Routes>
        </MemoryRouter>
    );
}

async function waitForProductLoaded(name = 'Test Product') {
    await waitFor(() => {
        const heading = document.querySelector('.product-name');
        expect(heading).toBeInTheDocument();
        expect(heading?.textContent).toBe(name);
    });
}

// ---------------------------------------------------------------------------
// Test suites
// ---------------------------------------------------------------------------

describe('Product page – successful fetch & initial state', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows loading indicator while fetching', async () => {
        // Keep fetch pending so loading stays true
        (global.fetch as ReturnType<typeof vi.fn>).mockReturnValue(new Promise(() => {}));
        renderProduct();
        expect(screen.getByRole('status')).toBeInTheDocument();
    });

    it('renders product name and category after successful fetch', async () => {
        setupFetchSuccess(makeProduct({ name_en: 'Blue Shirt', categoryName_en: 'Clothing' }));
        renderProduct();
        await waitForProductLoaded('Blue Shirt');
        expect(document.querySelector('.product-name')?.textContent).toBe('Blue Shirt');
        expect(document.querySelector('.product-category')?.textContent).toMatch(/Clothing/);
    });

    it('calls the correct API endpoint with the product ID from the URL', async () => {
        setupFetchSuccess(makeProduct({ id: 'abc-123' }));
        renderProduct('abc-123');
        await waitForProductLoaded();
        expect(global.fetch).toHaveBeenCalledWith(
            expect.stringContaining('/api/Item/GetItemById/abc-123')
        );
    });

    it('displays the first variant image as the main image', async () => {
        setupFetchSuccess(makeProduct({
            name_en: 'Camera',
            variants: [makeVariant({ imageUrls: 'https://example.com/camera_1.jpg' })],
        }));
        renderProduct();
        await waitForProductLoaded('Camera');

        const mainImg = document.querySelector('.product-main-image') as HTMLImageElement | null;
        expect(mainImg).not.toBeNull();
        expect(mainImg?.src).toContain('camera_1.jpg');
    });

    it('displays the first variant price', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ price: 79.99 })],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(screen.getByText('$79.99')).toBeInTheDocument();
    });

    it('shows stock quantity when variant has stock', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ stockQuantity: 5 })],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(screen.getByText(/5 in stock/i)).toBeInTheDocument();
    });

    it('shows out of stock message when stockQuantity is 0', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ stockQuantity: 0 })],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(screen.getByText(/out of stock/i)).toBeInTheDocument();
    });

    it('renders product description when present', async () => {
        setupFetchSuccess(makeProduct({ description_en: 'A great product for everyone.' }));
        renderProduct();
        await waitForProductLoaded();
        expect(screen.getByText('A great product for everyone.')).toBeInTheDocument();
    });

    it('does not render description section when absent', async () => {
        setupFetchSuccess(makeProduct({ description_en: undefined }));
        renderProduct();
        await waitForProductLoaded();
        expect(screen.queryByText('Description')).not.toBeInTheDocument();
    });
});

describe('Product page – offer display', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows discounted price and offer badge when variant has active offer', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({
                price: 100,
                offer: 20,
                offerStart: '2024-01-01T00:00:00Z',
                offerEnd: '2099-12-31T23:59:59Z',
            })],
        }));
        renderProduct();
        await waitForProductLoaded();

        expect(screen.getByText('$100.00')).toBeInTheDocument(); // original
        expect(screen.getByText('$80.00')).toBeInTheDocument();  // discounted
        expect(screen.getByText(/20% OFF/i)).toBeInTheDocument();
    });

    it('shows regular price without badge when offer is not active', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({
                price: 50,
                offer: 10,
                offerStart: '2099-01-01T00:00:00Z', // starts in the far future
                offerEnd: '2099-12-31T23:59:59Z',
            })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const priceEl = document.querySelector('.product-price');
        expect(priceEl?.textContent).toContain('50.00');
        expect(document.querySelector('.product-offer-badge')).toBeNull();
    });
});

describe('Product page – variant attribute selection', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('renders variant attribute buttons when variants have attributes', async () => {
        const productWithVariants = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1', price: 30,
                    imageUrls: 'https://example.com/black_1.jpg',
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Black' },
                    ],
                }),
                makeVariant({
                    id: 'v2', price: 30,
                    imageUrls: 'https://example.com/white_1.jpg',
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'White' },
                    ],
                }),
            ],
        });
        setupFetchSuccess(productWithVariants);
        renderProduct();
        await waitForProductLoaded();

        expect(screen.getByRole('button', { name: 'Black' })).toBeInTheDocument();
        expect(screen.getByRole('button', { name: 'White' })).toBeInTheDocument();
    });

    it('selecting a variant attribute updates the displayed price', async () => {
        const user = userEvent.setup();
        const productWithVariants = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1', price: 50,
                    imageUrls: 'https://example.com/black_1.jpg',
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Black' },
                    ],
                }),
                makeVariant({
                    id: 'v2', price: 100,
                    imageUrls: 'https://example.com/white_1.jpg',
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'White' },
                    ],
                }),
            ],
        });
        setupFetchSuccess(productWithVariants);
        renderProduct();
        await waitForProductLoaded();

        // Initially shows Black variant ($50)
        expect(screen.getByText('$50.00')).toBeInTheDocument();

        // Click White
        await user.click(screen.getByRole('button', { name: 'White' }));

        // Price should update to $100
        await waitFor(() => {
            expect(screen.getByText('$100.00')).toBeInTheDocument();
        });
    });

    it('selecting a variant attribute updates the main image', async () => {
        const user = userEvent.setup();
        const productWithVariants = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    imageUrls: 'https://example.com/black.jpg',
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Black' },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    imageUrls: 'https://example.com/white.jpg',
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'White' },
                    ],
                }),
            ],
        });
        setupFetchSuccess(productWithVariants);
        renderProduct();
        await waitForProductLoaded();

        const mainImg = () => document.querySelector('.product-main-image') as HTMLImageElement | null;
        expect(mainImg()?.src).toContain('black.jpg');

        await user.click(screen.getByRole('button', { name: 'White' }));

        await waitFor(() => {
            expect(mainImg()?.src).toContain('white.jpg');
        });
    });

    it('marks the selected variant attribute button as pressed', async () => {
        const user = userEvent.setup();
        const productWithVariants = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    imageUrls: 'https://example.com/s.jpg',
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Size', attributes_en: 'Small' },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    imageUrls: 'https://example.com/l.jpg',
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Size', attributes_en: 'Large' },
                    ],
                }),
            ],
        });
        setupFetchSuccess(productWithVariants);
        renderProduct();
        await waitForProductLoaded();

        const smallBtn = screen.getByRole('button', { name: 'Small' });
        const largeBtn = screen.getByRole('button', { name: 'Large' });
        expect(smallBtn).toHaveAttribute('aria-pressed', 'true');
        expect(largeBtn).toHaveAttribute('aria-pressed', 'false');

        await user.click(largeBtn);

        await waitFor(() => {
            expect(largeBtn).toHaveAttribute('aria-pressed', 'true');
            expect(smallBtn).toHaveAttribute('aria-pressed', 'false');
        });
    });

    it('shows "combination not available" when no variant matches selected attributes', async () => {
        const user = userEvent.setup();
        // Two variants with different Color+Size combos: Black+Small and White+Large
        // Selecting White (while Small is still selected) gives no match
        const productWithVariants = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    imageUrls: 'https://example.com/bs.jpg',
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Black' },
                        { id: 'a2', attributeName_en: 'Size', attributes_en: 'Small' },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    imageUrls: 'https://example.com/wl.jpg',
                    itemVariantAttributes: [
                        { id: 'a3', attributeName_en: 'Color', attributes_en: 'White' },
                        { id: 'a4', attributeName_en: 'Size', attributes_en: 'Large' },
                    ],
                }),
            ],
        });
        setupFetchSuccess(productWithVariants);
        renderProduct();
        await waitForProductLoaded();

        // Initially Black+Small is selected → valid variant
        // Click White: now Color=White but Size=Small is still selected → no White+Small variant
        await user.click(screen.getByRole('button', { name: 'White' }));

        await waitFor(() => {
            expect(screen.getByText(/combination.*not available|not available/i)).toBeInTheDocument();
        });
    });

    it('preserves variant selection when language is switched', async () => {
        const user = userEvent.setup();
        const productWithVariants = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1', price: 25,
                    imageUrls: 'https://example.com/noir.jpg',
                    itemVariantAttributes: [
                        {
                            id: 'a1',
                            attributeName_en: 'Color',
                            attributeName_fr: 'Couleur',
                            attributes_en: 'Black',
                            attributes_fr: 'Noir',
                        },
                    ],
                }),
                makeVariant({
                    id: 'v2', price: 35,
                    imageUrls: 'https://example.com/blanc.jpg',
                    itemVariantAttributes: [
                        {
                            id: 'a2',
                            attributeName_en: 'Color',
                            attributeName_fr: 'Couleur',
                            attributes_en: 'White',
                            attributes_fr: 'Blanc',
                        },
                    ],
                }),
            ],
        });
        setupFetchSuccess(productWithVariants);
        renderProduct();
        await waitForProductLoaded();

        // Select the White variant in English
        await user.click(screen.getByRole('button', { name: 'White' }));
        await waitFor(() => expect(screen.getByText('$35.00')).toBeInTheDocument());

        // Switch to French
        const langSelect = screen.getByRole('combobox', { name: /language|langue/i });
        await user.selectOptions(langSelect, 'fr');

        // The White variant should still be selected (shown as "Blanc") and price unchanged
        await waitFor(() => {
            expect(screen.getByRole('button', { name: 'Blanc' })).toHaveAttribute('aria-pressed', 'true');
            expect(screen.getByText('$35.00')).toBeInTheDocument();
        });
    });
});

describe('Product page – thumbnail gallery', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('does not show thumbnails when variant has only one image', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ imageUrls: 'https://example.com/img1.jpg' })],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(document.querySelector('.product-thumbnails')).toBeNull();
    });

    it('shows thumbnails when variant has multiple images', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({
                imageUrls: 'https://example.com/img1.jpg,https://example.com/img2.jpg,https://example.com/img3.jpg',
            })],
        }));
        renderProduct();
        await waitForProductLoaded();
        const thumbnails = document.querySelectorAll('.product-thumbnail-btn');
        expect(thumbnails.length).toBe(3);
    });

    it('clicking a thumbnail updates the main image', async () => {
        const user = userEvent.setup();
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({
                imageUrls: 'https://example.com/img1.jpg,https://example.com/img2.jpg',
            })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const mainImg = () => document.querySelector('.product-main-image') as HTMLImageElement | null;
        expect(mainImg()?.src).toContain('img1.jpg');

        // Click the second thumbnail
        const thumbBtns = screen.getAllByRole('button', { name: /View image/i });
        await user.click(thumbBtns[1]);

        await waitFor(() => {
            expect(mainImg()?.src).toContain('img2.jpg');
        });
    });

    it('marks the active thumbnail with aria-pressed=true', async () => {
        const user = userEvent.setup();
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({
                imageUrls: 'https://example.com/img1.jpg,https://example.com/img2.jpg',
            })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const thumbBtns = () => screen.getAllByRole('button', { name: /View image/i });
        expect(thumbBtns()[0]).toHaveAttribute('aria-pressed', 'true');
        expect(thumbBtns()[1]).toHaveAttribute('aria-pressed', 'false');

        await user.click(thumbBtns()[1]);
        await waitFor(() => {
            expect(thumbBtns()[1]).toHaveAttribute('aria-pressed', 'true');
            expect(thumbBtns()[0]).toHaveAttribute('aria-pressed', 'false');
        });
    });
});

describe('Product page – error and empty states', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows error message when fetch returns 404', async () => {
        setupFetchNotFound();
        renderProduct();
        await waitFor(() => {
            expect(screen.getByText(/product not found/i)).toBeInTheDocument();
        });
    });

    it('shows error message on network error', async () => {
        setupFetchNetworkError();
        renderProduct();
        await waitFor(() => {
            expect(screen.getByText(/failed to load product/i)).toBeInTheDocument();
        });
    });

    it('error state renders a Go Back button', async () => {
        setupFetchNotFound();
        renderProduct();
        await waitFor(() => {
            expect(screen.getByRole('button', { name: /go back|retour/i })).toBeInTheDocument();
        });
    });

    it('shows no price/variant section when product has no active variants', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ deleted: true })],
        }));
        renderProduct();
        await waitForProductLoaded();
        // No price shown for a product with all deleted variants
        expect(document.querySelector('.product-price')).toBeNull();
        expect(document.querySelector('.product-discounted-price')).toBeNull();
    });

    it('shows full top nav and footer even in error state', async () => {
        setupFetchNotFound();
        renderProduct();
        await waitFor(() => {
            expect(screen.getByText(/product not found/i)).toBeInTheDocument();
        });
        expect(screen.getByRole('navigation')).toBeInTheDocument();
        expect(screen.getByText('CanoEh!')).toBeInTheDocument();
        expect(document.querySelector('.store-footer')).toBeInTheDocument();
    });
});
