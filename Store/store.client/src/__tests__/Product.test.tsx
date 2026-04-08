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
    productIdentifierType?: string;
    productIdentifierValue?: string;
    imageUrls?: string;
    thumbnailUrl?: string;
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
    itemVariantAttributes?: object[];
    itemVariantFeatures?: object[];
    deleted?: boolean;
} = {}) {
    return {
        id: overrides.id ?? 'var1',
        price: overrides.price ?? 50,
        stockQuantity: overrides.stockQuantity ?? 10,
        sku: overrides.sku ?? 'SKU-001',
        productIdentifierType: overrides.productIdentifierType,
        productIdentifierValue: overrides.productIdentifierValue,
        imageUrls: overrides.imageUrls ?? 'https://example.com/img1.jpg',
        thumbnailUrl: overrides.thumbnailUrl,
        offer: overrides.offer ?? null,
        offerStart: overrides.offerStart ?? null,
        offerEnd: overrides.offerEnd ?? null,
        itemVariantAttributes: overrides.itemVariantAttributes ?? [],
        itemVariantFeatures: overrides.itemVariantFeatures ?? [],
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
    itemVariantFeatures?: object[];
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
        itemVariantFeatures: overrides.itemVariantFeatures ?? [],
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

function makeCategoryNode(overrides: {
    id?: string;
    name_en?: string;
    name_fr?: string;
    parentId?: string | null;
} = {}) {
    return {
        id: overrides.id ?? 'cat1',
        name_en: overrides.name_en ?? 'Electronics',
        name_fr: overrides.name_fr ?? 'Électronique',
        nodeType: 'category',
        parentId: overrides.parentId ?? null,
        isActive: true,
        sortOrder: null,
        children: [],
    };
}

function setupFetchWithCategories(product: object, categoryNodes: object[] = []) {
    (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
        if (url.includes('/api/CategoryNode/GetAllCategoryNodes')) {
            return Promise.resolve({
                ok: true,
                json: async () => ({ isSuccess: true, value: categoryNodes }),
            });
        }
        return Promise.resolve({
            ok: true,
            json: async () => makeApiResult(product),
        });
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

describe('Product page – product attributes section', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('renders the product-attributes section with a title when SKU is present', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ sku: 'ABC-123' })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const section = document.querySelector('.product-attributes');
        expect(section).toBeInTheDocument();
        const title = document.querySelector('.product-attributes-title');
        expect(title).toBeInTheDocument();
        expect(title?.textContent).toMatch(/product details/i);
    });

    it('renders the SKU row inside product-attributes', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ sku: 'MY-SKU-999' })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const rows = document.querySelectorAll('.product-attributes-row');
        expect(rows.length).toBeGreaterThanOrEqual(1);
        const skuRow = Array.from(rows).find(r => r.textContent?.includes('MY-SKU-999'));
        expect(skuRow).toBeInTheDocument();
    });

    it('renders the Product ID Type and Value row when both are present', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ productIdentifierType: 'GTIN', productIdentifierValue: '01234567890123' })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const rows = document.querySelectorAll('.product-attributes-row');
        const idRow = Array.from(rows).find(r => r.textContent?.includes('GTIN') && r.textContent?.includes('01234567890123'));
        expect(idRow).toBeInTheDocument();
    });

    it('does not render the Product ID row when only type is present (no value)', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ productIdentifierType: 'UPC', productIdentifierValue: undefined })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const rows = document.querySelectorAll('.product-attributes-row');
        const idRow = Array.from(rows).find(r => r.textContent?.includes('UPC'));
        expect(idRow).toBeUndefined();
    });

    it('does not render product-attributes section when no active variant is selected', async () => {
        setupFetchSuccess(makeProduct({
            variants: [makeVariant({ deleted: true })],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(document.querySelector('.product-attributes')).toBeNull();
    });
});

describe('Product page – variant features section', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('renders the variant features section when itemVariantFeatures are present', async () => {
        setupFetchSuccess(makeProduct({
            itemVariantFeatures: [
                { id: 'f1', attributeName_en: 'Material', attributeName_fr: 'Matériau', attributes_en: 'Cotton', attributes_fr: 'Coton' },
            ],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(document.querySelector('.product-variant-features')).toBeInTheDocument();
    });

    it('renders the Features heading in English by default', async () => {
        setupFetchSuccess(makeProduct({
            itemVariantFeatures: [
                { id: 'f1', attributeName_en: 'Material', attributes_en: 'Cotton' },
            ],
        }));
        renderProduct();
        await waitForProductLoaded();
        const title = document.querySelector('.product-variant-features-title');
        expect(title).toBeInTheDocument();
        expect(title?.textContent).toBe('Features');
    });

    it('renders each feature name and value in English', async () => {
        setupFetchSuccess(makeProduct({
            itemVariantFeatures: [
                { id: 'f1', attributeName_en: 'Material', attributes_en: 'Cotton' },
                { id: 'f2', attributeName_en: 'Weight', attributes_en: '200g' },
            ],
        }));
        renderProduct();
        await waitForProductLoaded();
        expect(screen.getByText('Material')).toBeInTheDocument();
        expect(screen.getByText('Cotton')).toBeInTheDocument();
        expect(screen.getByText('Weight')).toBeInTheDocument();
        expect(screen.getByText('200g')).toBeInTheDocument();
    });

    it('does not render the variant features section when itemVariantFeatures is empty', async () => {
        setupFetchSuccess(makeProduct({ itemVariantFeatures: [] }));
        renderProduct();
        await waitForProductLoaded();
        expect(document.querySelector('.product-variant-features')).toBeNull();
    });

    it('does not render the variant features section when itemVariantFeatures is absent', async () => {
        // Build a product object that genuinely lacks the itemVariantFeatures property
        const productWithoutFeatures = makeProduct();
        const { itemVariantFeatures: _omit, ...productMissingField } = productWithoutFeatures;
        setupFetchSuccess(productMissingField);
        renderProduct();
        await waitForProductLoaded();
        expect(document.querySelector('.product-variant-features')).toBeNull();
    });

    it('renders French feature names and values when French translation is available', async () => {
        const user = userEvent.setup();
        setupFetchSuccess(makeProduct({
            itemVariantFeatures: [
                { id: 'f1', attributeName_en: 'Material', attributeName_fr: 'Matériau', attributes_en: 'Cotton', attributes_fr: 'Coton' },
            ],
        }));
        renderProduct();
        await waitForProductLoaded();

        const langSelect = screen.getByRole('combobox', { name: /language|langue/i });
        await user.selectOptions(langSelect, 'fr');
        expect(screen.getByText('Matériau')).toBeInTheDocument();
        expect(screen.getByText('Coton')).toBeInTheDocument();
    });

    it('falls back to English feature name/value when French translation is absent', async () => {
        const user = userEvent.setup();
        setupFetchSuccess(makeProduct({
            itemVariantFeatures: [
                { id: 'f1', attributeName_en: 'Material', attributeName_fr: null, attributes_en: 'Cotton', attributes_fr: null },
            ],
        }));
        renderProduct();
        await waitForProductLoaded();

        const langSelect = screen.getByRole('combobox', { name: /language|langue/i });
        await user.selectOptions(langSelect, 'fr');
        expect(screen.getByText('Material')).toBeInTheDocument();
        expect(screen.getByText('Cotton')).toBeInTheDocument();
    });

    it('renders the variant features section between description and product-attributes', async () => {
        setupFetchSuccess(makeProduct({
            description_en: 'A great product.',
            itemVariantFeatures: [
                { id: 'f1', attributeName_en: 'Material', attributes_en: 'Cotton' },
            ],
            variants: [makeVariant({ sku: 'SKU-001' })],
        }));
        renderProduct();
        await waitForProductLoaded();

        const productInfo = document.querySelector('.product-info');
        expect(productInfo).toBeInTheDocument();
        const infoChildren = Array.from(productInfo!.children);
        const featIndex = infoChildren.findIndex(el => el.classList.contains('product-variant-features'));
        const attrIndex = infoChildren.findIndex(el => el.classList.contains('product-attributes'));
        expect(featIndex).toBeGreaterThanOrEqual(0);
        expect(attrIndex).toBeGreaterThan(featIndex);

        const productGallery = document.querySelector('.product-gallery');
        expect(productGallery).toBeInTheDocument();
        const galleryChildren = Array.from(productGallery!.children);
        const descIndex = galleryChildren.findIndex(el => el.classList.contains('product-description'));
        expect(descIndex).toBeGreaterThanOrEqual(0);
    });
});

describe('Product page – product-breadcrumb', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('does not show "Home" or "Accueil" in the product breadcrumb', async () => {
        const node = makeCategoryNode({ id: 'cat1', name_en: 'Electronics' });
        setupFetchWithCategories(makeProduct(), [node]);
        renderProduct();
        await waitForProductLoaded();
        const breadcrumb = document.querySelector('.product-category');
        expect(breadcrumb).toBeInTheDocument();
        expect(screen.queryByRole('button', { name: /^Home$|^Accueil$/i })).not.toBeInTheDocument();
    });

    it('renders the category node name in the breadcrumb', async () => {
        const node = makeCategoryNode({ id: 'cat1', name_en: 'Electronics' });
        setupFetchWithCategories(makeProduct(), [node]);
        renderProduct();
        await waitForProductLoaded();
        const breadcrumb = document.querySelector('.product-category');
        expect(breadcrumb?.textContent).toMatch(/Electronics/);
    });

    it('renders no leading separator when there is only one category node', async () => {
        const node = makeCategoryNode({ id: 'cat1', name_en: 'Electronics' });
        setupFetchWithCategories(makeProduct(), [node]);
        renderProduct();
        await waitForProductLoaded();
        const breadcrumb = document.querySelector('.product-category');
        const seps = breadcrumb?.querySelectorAll('.breadcrumb-sep');
        expect(seps?.length).toBe(0);
    });

    it('renders a separator only between nodes when there are multiple category nodes', async () => {
        const parent = makeCategoryNode({ id: 'parent', name_en: 'Electronics', parentId: null });
        const child = makeCategoryNode({ id: 'cat1', name_en: 'Phones', parentId: 'parent' });
        setupFetchWithCategories(makeProduct(), [parent, child]);
        renderProduct();
        await waitForProductLoaded();
        const breadcrumb = document.querySelector('.product-category');
        const seps = breadcrumb?.querySelectorAll('.breadcrumb-sep');
        expect(seps?.length).toBe(1);
        expect(breadcrumb?.textContent).toMatch(/Electronics/);
        expect(breadcrumb?.textContent).toMatch(/Phones/);
    });
});

describe('Product page – out-of-stock variant options', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', API_BASE_URL);
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('greys out a main option when no combination with that option has stock', async () => {
        // Color (main): Red has stock, Blue has none
        const product = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    stockQuantity: 5,
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Red', isMain: true },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    stockQuantity: 0,
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'Blue', isMain: true },
                    ],
                }),
            ],
        });
        setupFetchWithCategories(product);
        renderProduct();
        await waitForProductLoaded();

        const blueBtn = screen.getByRole('button', { name: 'Blue' });
        expect(blueBtn).toBeDisabled();
        expect(blueBtn.className).toContain('out-of-stock');

        const redBtn = screen.getByRole('button', { name: 'Red' });
        expect(redBtn).not.toBeDisabled();
        expect(redBtn.className).not.toContain('out-of-stock');
    });

    it('does not grey out a main option that has at least one in-stock combination', async () => {
        // Color (main): Blue has Small in stock and Large out of stock → Blue itself is NOT greyed
        const product = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    stockQuantity: 5,
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Blue', isMain: true },
                        { id: 'a3', attributeName_en: 'Size', attributes_en: 'Small' },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    stockQuantity: 0,
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'Blue', isMain: true },
                        { id: 'a4', attributeName_en: 'Size', attributes_en: 'Large' },
                    ],
                }),
            ],
        });
        setupFetchWithCategories(product);
        renderProduct();
        await waitForProductLoaded();

        const blueBtn = screen.getByRole('button', { name: 'Blue' });
        expect(blueBtn).not.toBeDisabled();
        expect(blueBtn.className).not.toContain('out-of-stock');
    });

    it('greys out a secondary option with no stock for the currently selected main option', async () => {
        // Color (main): Red — Size: Small in stock, Large out of stock with Red
        const product = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    stockQuantity: 5,
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Red', isMain: true },
                        { id: 'a3', attributeName_en: 'Size', attributes_en: 'Small' },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    stockQuantity: 0,
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'Red', isMain: true },
                        { id: 'a4', attributeName_en: 'Size', attributes_en: 'Large' },
                    ],
                }),
            ],
        });
        setupFetchWithCategories(product);
        renderProduct();
        await waitForProductLoaded();

        const largeBtn = screen.getByRole('button', { name: 'Large' });
        expect(largeBtn).toBeDisabled();
        expect(largeBtn.className).toContain('out-of-stock');

        const smallBtn = screen.getByRole('button', { name: 'Small' });
        expect(smallBtn).not.toBeDisabled();
        expect(smallBtn.className).not.toContain('out-of-stock');
    });

    it('updates secondary out-of-stock state when the main selection changes', async () => {
        const user = userEvent.setup();
        // Color (main): Red → Small in stock, Large out of stock
        //               Blue → Large in stock, Small out of stock
        const product = makeProduct({
            variants: [
                makeVariant({
                    id: 'v1',
                    stockQuantity: 5,
                    itemVariantAttributes: [
                        { id: 'a1', attributeName_en: 'Color', attributes_en: 'Red', isMain: true },
                        { id: 'a3', attributeName_en: 'Size', attributes_en: 'Small' },
                    ],
                }),
                makeVariant({
                    id: 'v2',
                    stockQuantity: 0,
                    itemVariantAttributes: [
                        { id: 'a2', attributeName_en: 'Color', attributes_en: 'Red', isMain: true },
                        { id: 'a4', attributeName_en: 'Size', attributes_en: 'Large' },
                    ],
                }),
                makeVariant({
                    id: 'v3',
                    stockQuantity: 5,
                    itemVariantAttributes: [
                        { id: 'a5', attributeName_en: 'Color', attributes_en: 'Blue', isMain: true },
                        { id: 'a6', attributeName_en: 'Size', attributes_en: 'Large' },
                    ],
                }),
                makeVariant({
                    id: 'v4',
                    stockQuantity: 0,
                    itemVariantAttributes: [
                        { id: 'a7', attributeName_en: 'Color', attributes_en: 'Blue', isMain: true },
                        { id: 'a8', attributeName_en: 'Size', attributes_en: 'Small' },
                    ],
                }),
            ],
        });
        setupFetchWithCategories(product);
        renderProduct();
        await waitForProductLoaded();

        // Initially Red is selected: Large should be disabled, Small enabled
        expect(screen.getByRole('button', { name: 'Large' })).toBeDisabled();
        expect(screen.getByRole('button', { name: 'Small' })).not.toBeDisabled();

        // Switch to Blue
        await user.click(screen.getByRole('button', { name: 'Blue' }));

        // Now Small should be disabled, Large enabled
        await waitFor(() => {
            expect(screen.getByRole('button', { name: 'Small' })).toBeDisabled();
            expect(screen.getByRole('button', { name: 'Large' })).not.toBeDisabled();
        });
    });
});
