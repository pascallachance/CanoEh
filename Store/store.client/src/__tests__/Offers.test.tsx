import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Offers from '../components/Offers';

// Mock fetch globally
global.fetch = vi.fn();

// Helper to build a minimal GetProductsWithOffers API response
function makeApiResponse(products: object[]) {
    return { isSuccess: true, value: products };
}

// Helper to build a product object with sensible defaults
function makeProduct(overrides: {
    id?: string;
    name_en?: string;
    name_fr?: string;
    variants?: object[];
} = {}) {
    return {
        id: overrides.id ?? '1',
        name_en: overrides.name_en ?? 'Product',
        name_fr: overrides.name_fr ?? 'Produit',
        deleted: false,
        variants: overrides.variants ?? [],
    };
}

// Helper to build a variant object with sensible defaults.
// Uses spread so passing imageUrls: undefined explicitly clears the default.
function makeVariant(overrides: {
    id?: string;
    price?: number;
    offer?: number | null;
    offerStart?: string | null;
    offerEnd?: string | null;
    imageUrls?: string;
    thumbnailUrl?: string;
} = {}) {
    return {
        id: 'var1',
        price: 100,
        stockQuantity: 10,
        sku: 'SKU1',
        deleted: false,
        offer: 20,
        offerStart: '2024-01-01T00:00:00Z',
        offerEnd: '2099-12-31T23:59:59Z',
        imageUrls: 'https://example.com/img_1.jpg' as string | undefined,
        thumbnailUrl: undefined as string | undefined,
        ...overrides,
    };
}

function setupFetchMock(products: object[]) {
    (global.fetch as ReturnType<typeof vi.fn>).mockResolvedValue({
        ok: true,
        json: async () => makeApiResponse(products),
    });
}

function renderOffers() {
    return render(
        <BrowserRouter>
            <Offers />
        </BrowserRouter>
    );
}

async function waitForProductsLoaded() {
    // The subtitle (.browse-subtitle) only appears when loading is complete.
    // We target it directly to avoid matching the empty-state "No offers found" paragraph.
    await waitFor(() => {
        const subtitle = document.querySelector('.browse-subtitle');
        expect(subtitle).toBeInTheDocument();
    });
}

describe('Offers page – highest-offer variant selection', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('selects the variant with the highest offer percentage when multiple variants have offers', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Multi-Variant Product',
                variants: [
                    makeVariant({ id: 'var-low', price: 100, offer: 10, imageUrls: 'https://example.com/low_1.jpg' }),
                    makeVariant({ id: 'var-high', price: 200, offer: 50, imageUrls: 'https://example.com/high_1.jpg' }),
                    makeVariant({ id: 'var-mid', price: 150, offer: 30, imageUrls: 'https://example.com/mid_1.jpg' }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        // Only one product card should be shown
        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);

        // The card should show the image from the highest-offer variant (50%)
        const img = cards[0].querySelector('img');
        expect(img?.getAttribute('src')).toContain('high_1.jpg');

        // The offer badge should show 50%
        const badge = cards[0].querySelector('.offer-badge');
        expect(badge?.textContent).toMatch(/50/);

        // The original price should be that of the best variant ($200)
        const originalPrice = cards[0].querySelector('.browse-original-price');
        expect(originalPrice?.textContent).toContain('200.00');
    });

    it('skips variants with expired offers', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Expired Offer Product',
                variants: [
                    // This variant's offer has expired
                    makeVariant({ id: 'expired', offer: 80, offerEnd: '2020-01-01T00:00:00Z', imageUrls: 'https://example.com/expired_1.jpg' }),
                    // This variant has a valid offer
                    makeVariant({ id: 'valid', offer: 25, imageUrls: 'https://example.com/valid_1.jpg' }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);

        // Should display the valid variant, not the expired one
        const img = cards[0].querySelector('img');
        expect(img?.getAttribute('src')).toContain('valid_1.jpg');

        const badge = cards[0].querySelector('.offer-badge');
        expect(badge?.textContent).toMatch(/25/);
    });

    it('skips variants with offers that have not yet started', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Future Offer Product',
                variants: [
                    // Future offer
                    makeVariant({ id: 'future', offer: 90, offerStart: '2099-01-01T00:00:00Z', imageUrls: 'https://example.com/future_1.jpg' }),
                    // Active offer
                    makeVariant({ id: 'active', offer: 15, imageUrls: 'https://example.com/active_1.jpg' }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);

        const img = cards[0].querySelector('img');
        expect(img?.getAttribute('src')).toContain('active_1.jpg');
    });

    it('excludes products where all variants have no active offer', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'No Active Offer Product',
                variants: [
                    makeVariant({ id: 'expired', offer: 50, offerEnd: '2020-01-01T00:00:00Z', imageUrls: 'https://example.com/img_1.jpg' }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        // No product cards should be shown
        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(0);

        // Empty state message should appear
        expect(screen.getByText(/No offers found|Aucune offre/i)).toBeInTheDocument();
    });

    it('computes discounted price correctly', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Discounted Product',
                variants: [
                    makeVariant({ price: 200, offer: 25, imageUrls: 'https://example.com/img_1.jpg' }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);

        // Original: $200.00, 25% off => discounted: $150.00
        const originalPrice = cards[0].querySelector('.browse-original-price');
        const discountedPrice = cards[0].querySelector('.browse-discounted-price');
        expect(originalPrice?.textContent).toContain('200.00');
        expect(discountedPrice?.textContent).toContain('150.00');
    });
});

describe('Offers page – price range filter', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('filters by minimum price (on discounted price)', async () => {
        // Products: $100 at 50% off => $50; $200 at 10% off => $180
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Cheap Product', variants: [makeVariant({ price: 100, offer: 50, imageUrls: 'https://example.com/cheap_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Expensive Product', variants: [makeVariant({ id: 'var2', price: 200, offer: 10, imageUrls: 'https://example.com/expensive_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        // Both products shown initially
        expect(document.querySelectorAll('.browse-product-card').length).toBe(2);

        // Set min price to 100 (discounted price >= $100) → only Expensive Product should show
        const minPriceInput = screen.getByLabelText(/Minimum price|Prix minimum/i);
        await user.clear(minPriceInput);
        await user.type(minPriceInput, '100');

        await waitFor(() => {
            expect(document.querySelectorAll('.browse-product-card').length).toBe(1);
        });

        expect(screen.getByText('Expensive Product')).toBeInTheDocument();
        expect(screen.queryByText('Cheap Product')).not.toBeInTheDocument();
    });

    it('filters by maximum price (on discounted price)', async () => {
        // Products: $100 at 50% off => $50; $200 at 10% off => $180
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Cheap Product', variants: [makeVariant({ price: 100, offer: 50, imageUrls: 'https://example.com/cheap_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Expensive Product', variants: [makeVariant({ id: 'var2', price: 200, offer: 10, imageUrls: 'https://example.com/expensive_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        // Set max price to 100 → only Cheap Product ($50 discounted) should show
        const maxPriceInput = screen.getByLabelText(/Maximum price|Prix maximum/i);
        await user.clear(maxPriceInput);
        await user.type(maxPriceInput, '100');

        await waitFor(() => {
            expect(document.querySelectorAll('.browse-product-card').length).toBe(1);
        });

        expect(screen.getByText('Cheap Product')).toBeInTheDocument();
        expect(screen.queryByText('Expensive Product')).not.toBeInTheDocument();
    });

    it('shows empty state when no products match the price range', async () => {
        // Product: $100 at 10% off => $90 discounted
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Product A', variants: [makeVariant({ price: 100, offer: 10, imageUrls: 'https://example.com/a_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        // Set min price above the discounted price
        const minPriceInput = screen.getByLabelText(/Minimum price|Prix minimum/i);
        await user.clear(minPriceInput);
        await user.type(minPriceInput, '500');

        await waitFor(() => {
            expect(screen.getByText(/No offers found|Aucune offre/i)).toBeInTheDocument();
        });
    });
});

describe('Offers page – minimum discount filter', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('filters out products below the minimum discount threshold', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Small Discount', variants: [makeVariant({ offer: 5, imageUrls: 'https://example.com/small_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Big Discount', variants: [makeVariant({ id: 'var2', offer: 40, imageUrls: 'https://example.com/big_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        expect(document.querySelectorAll('.browse-product-card').length).toBe(2);

        // Set minimum discount to 20% → only "Big Discount" (40%) should remain
        const minDiscountInput = screen.getByLabelText(/Minimum discount percentage|Pourcentage de rabais minimum/i);
        await user.clear(minDiscountInput);
        await user.type(minDiscountInput, '20');

        await waitFor(() => {
            expect(document.querySelectorAll('.browse-product-card').length).toBe(1);
        });

        expect(screen.getByText('Big Discount')).toBeInTheDocument();
        expect(screen.queryByText('Small Discount')).not.toBeInTheDocument();
    });

    it('shows all products when minimum discount is 0', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Product A', variants: [makeVariant({ offer: 5, imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Product B', variants: [makeVariant({ id: 'var2', offer: 50, imageUrls: 'https://example.com/b_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        const minDiscountInput = screen.getByLabelText(/Minimum discount percentage|Pourcentage de rabais minimum/i);
        await user.clear(minDiscountInput);
        await user.type(minDiscountInput, '0');

        await waitFor(() => {
            expect(document.querySelectorAll('.browse-product-card').length).toBe(2);
        });
    });
});

describe('Offers page – sort modes', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    // Helper: return product names in displayed order from offer cards
    function getCardNames() {
        return Array.from(document.querySelectorAll('.browse-product-name')).map(el => el.textContent ?? '');
    }

    it('sorts by discount descending (default)', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Low Discount', variants: [makeVariant({ offer: 10, imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'High Discount', variants: [makeVariant({ id: 'var2', offer: 60, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'Mid Discount', variants: [makeVariant({ id: 'var3', offer: 30, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const names = getCardNames();
        expect(names[0]).toBe('High Discount');
        expect(names[1]).toBe('Mid Discount');
        expect(names[2]).toBe('Low Discount');
    });

    it('sorts by discount ascending', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Low Discount', variants: [makeVariant({ offer: 10, imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'High Discount', variants: [makeVariant({ id: 'var2', offer: 60, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'Mid Discount', variants: [makeVariant({ id: 'var3', offer: 30, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        await user.selectOptions(screen.getByRole('combobox', { name: /Sort by|Trier par/i }), 'discount-asc');

        await waitFor(() => {
            const names = getCardNames();
            expect(names[0]).toBe('Low Discount');
            expect(names[1]).toBe('Mid Discount');
            expect(names[2]).toBe('High Discount');
        });
    });

    it('sorts by price ascending (discounted price)', async () => {
        // Discounted prices: $100*0.8=$80, $50*0.5=$25, $200*0.9=$180
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Medium Price', variants: [makeVariant({ price: 100, offer: 20, imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Low Price', variants: [makeVariant({ id: 'var2', price: 50, offer: 50, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'High Price', variants: [makeVariant({ id: 'var3', price: 200, offer: 10, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        await user.selectOptions(screen.getByRole('combobox', { name: /Sort by|Trier par/i }), 'price-asc');

        await waitFor(() => {
            const names = getCardNames();
            expect(names[0]).toBe('Low Price');   // $25
            expect(names[1]).toBe('Medium Price'); // $80
            expect(names[2]).toBe('High Price');   // $180
        });
    });

    it('sorts by price descending (discounted price)', async () => {
        // Discounted prices: $100*0.8=$80, $50*0.5=$25, $200*0.9=$180
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Medium Price', variants: [makeVariant({ price: 100, offer: 20, imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Low Price', variants: [makeVariant({ id: 'var2', price: 50, offer: 50, imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'High Price', variants: [makeVariant({ id: 'var3', price: 200, offer: 10, imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        await user.selectOptions(screen.getByRole('combobox', { name: /Sort by|Trier par/i }), 'price-desc');

        await waitFor(() => {
            const names = getCardNames();
            expect(names[0]).toBe('High Price');   // $180
            expect(names[1]).toBe('Medium Price'); // $80
            expect(names[2]).toBe('Low Price');    // $25
        });
    });

    it('sorts by name A to Z', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Zebra Product', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Apple Product', variants: [makeVariant({ id: 'var2', imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'Mango Product', variants: [makeVariant({ id: 'var3', imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        await user.selectOptions(screen.getByRole('combobox', { name: /Sort by|Trier par/i }), 'name-asc');

        await waitFor(() => {
            const names = getCardNames();
            expect(names[0]).toBe('Apple Product');
            expect(names[1]).toBe('Mango Product');
            expect(names[2]).toBe('Zebra Product');
        });
    });

    it('sorts by name Z to A', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Zebra Product', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Apple Product', variants: [makeVariant({ id: 'var2', imageUrls: 'https://example.com/b_1.jpg' })] }),
            makeProduct({ id: '3', name_en: 'Mango Product', variants: [makeVariant({ id: 'var3', imageUrls: 'https://example.com/c_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        await user.selectOptions(screen.getByRole('combobox', { name: /Sort by|Trier par/i }), 'name-desc');

        await waitFor(() => {
            const names = getCardNames();
            expect(names[0]).toBe('Zebra Product');
            expect(names[1]).toBe('Mango Product');
            expect(names[2]).toBe('Apple Product');
        });
    });
});

describe('Offers page – clear filters', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('restores all products after clearing filters', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Product A', variants: [makeVariant({ offer: 5, imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Product B', variants: [makeVariant({ id: 'var2', offer: 50, imageUrls: 'https://example.com/b_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        // Apply a filter that hides Product A
        const minDiscountInput = screen.getByLabelText(/Minimum discount percentage|Pourcentage de rabais minimum/i);
        await user.clear(minDiscountInput);
        await user.type(minDiscountInput, '20');

        await waitFor(() => {
            expect(document.querySelectorAll('.browse-product-card').length).toBe(1);
        });

        // Click "Clear Filters" - find the first clear filters button (in the sidebar)
        const clearButtons = screen.getAllByRole('button', { name: /Clear Filters|Effacer les filtres/i });
        await user.click(clearButtons[0]);

        await waitFor(() => {
            expect(document.querySelectorAll('.browse-product-card').length).toBe(2);
        });
    });
});

describe('Offers page – UI structure', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('renders the page title', async () => {
        setupFetchMock([]);
        renderOffers();

        expect(screen.getByRole('heading', { name: /Products with Offers|Produits en promotion/i })).toBeInTheDocument();
    });

    it('renders the filter sidebar', async () => {
        setupFetchMock([]);
        renderOffers();

        expect(screen.getByRole('complementary', { name: /Filters|Filtres/i })).toBeInTheDocument();
        expect(screen.getByLabelText(/Sort by|Trier par/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Minimum price|Prix minimum/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Maximum price|Prix maximum/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/Minimum discount percentage|Pourcentage de rabais minimum/i)).toBeInTheDocument();
    });

    it('shows loading state initially', () => {
        // Mock a fetch that never resolves to keep the loading state visible
        (global.fetch as ReturnType<typeof vi.fn>).mockReturnValue(new Promise(() => {}));
        renderOffers();

        expect(screen.getByRole('status')).toBeInTheDocument();
        expect(screen.getByText(/Loading offers|Chargement des offres/i)).toBeInTheDocument();
    });

    it('displays offer count label after loading', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Product A', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Product B', variants: [makeVariant({ id: 'var2', imageUrls: 'https://example.com/b_1.jpg' })] }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const subtitle = document.querySelector('.browse-subtitle');
        expect(subtitle?.textContent).toMatch(/2 offers found|2 offres trouvées/i);
    });

    it('renders product images with offer badges', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Offer Product',
                variants: [makeVariant({ offer: 35, imageUrls: 'https://example.com/img_1.jpg' })],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);

        const img = cards[0].querySelector('img');
        expect(img).toBeInTheDocument();
        expect(img?.getAttribute('src')).toContain('img_1.jpg');

        const badge = cards[0].querySelector('.offer-badge');
        expect(badge?.textContent).toMatch(/35% OFF|Rabais 35%/);
    });

    it('fetches from the correct API endpoint', async () => {
        setupFetchMock([]);
        renderOffers();

        await waitFor(() => {
            expect(global.fetch).toHaveBeenCalledWith(
                expect.stringContaining('/api/Item/GetProductsWithOffers?count=100')
            );
        });
    });

    it('renders within browse-layout with sidebar and product area', async () => {
        setupFetchMock([]);
        renderOffers();

        const layout = document.querySelector('.browse-layout');
        expect(layout).toBeInTheDocument();

        const sidebar = layout?.querySelector('.browse-filters');
        expect(sidebar).toBeInTheDocument();

        const productArea = layout?.querySelector('.browse-products');
        expect(productArea).toBeInTheDocument();
    });

    it('sidebar takes up 20% width by having the browse-filters class', async () => {
        setupFetchMock([]);
        renderOffers();

        // Verify the sidebar has the class that controls its width
        const sidebar = document.querySelector('.browse-filters');
        expect(sidebar).toBeInTheDocument();
        expect(sidebar?.tagName.toLowerCase()).toBe('aside');
    });

    it('renders browse-product-card as a clickable button to navigate to the product page', async () => {
        setupFetchMock([
            makeProduct({ id: '1', variants: [makeVariant({ imageUrls: 'https://example.com/img_1.jpg' })] }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const card = document.querySelector('.browse-product-card');
        expect(card).toBeInTheDocument();
        // Cards are now clickable to navigate to the product detail page
        expect(card?.getAttribute('role')).toBe('button');
        expect(card?.getAttribute('tabIndex')).toBe('0');
    });
});

describe('Offers page – language support', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('shows French product names and offer badge text when language is set to French', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'English Name',
                name_fr: 'Nom Français',
                variants: [makeVariant({ offer: 20, imageUrls: 'https://example.com/img_1.jpg' })],
            }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        // Switch language to French
        const languageSelect = screen.getByLabelText(/Select language|Sélectionner la langue/i);
        await user.selectOptions(languageSelect, 'fr');

        await waitFor(() => {
            expect(screen.getByText('Nom Français')).toBeInTheDocument();
        });

        const badge = document.querySelector('.offer-badge');
        expect(badge?.textContent).toMatch(/Rabais 20%/);
    });

    it('shows French product name in name sort', async () => {
        setupFetchMock([
            makeProduct({ id: '1', name_en: 'Zebra EN', name_fr: 'Zèbre FR', variants: [makeVariant({ imageUrls: 'https://example.com/a_1.jpg' })] }),
            makeProduct({ id: '2', name_en: 'Apple EN', name_fr: 'Pomme FR', variants: [makeVariant({ id: 'var2', imageUrls: 'https://example.com/b_1.jpg' })] }),
        ]);

        const user = userEvent.setup();
        renderOffers();
        await waitForProductsLoaded();

        // Switch to French
        const languageSelect = screen.getByLabelText(/Select language|Sélectionner la langue/i);
        await user.selectOptions(languageSelect, 'fr');

        // Sort by name A to Z in French: "Pomme" before "Zèbre"
        await user.selectOptions(screen.getByRole('combobox', { name: /Trier par/i }), 'name-asc');

        await waitFor(() => {
            const names = Array.from(document.querySelectorAll('.browse-product-name')).map(el => el.textContent ?? '');
            expect(names[0]).toBe('Pomme FR');
            expect(names[1]).toBe('Zèbre FR');
        });
    });
});

describe('Offers page – product image selection', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('uses first image in imageUrls as the main image', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Product',
                variants: [
                    makeVariant({
                        imageUrls: 'https://example.com/product_3.jpg,https://example.com/product_1.jpg,https://example.com/product_2.jpg',
                    }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const img = document.querySelector('.browse-product-card img');
        expect(img?.getAttribute('src')).toContain('product_3.jpg');
    });

    it('falls back to first image when no _1 image is present', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Product',
                variants: [
                    makeVariant({
                        imageUrls: 'https://example.com/first.jpg,https://example.com/second.jpg',
                    }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const img = document.querySelector('.browse-product-card img');
        expect(img?.getAttribute('src')).toContain('first.jpg');
    });

    it('falls back to thumbnailUrl when imageUrls is empty', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'Product',
                variants: [
                    makeVariant({
                        imageUrls: undefined,
                        thumbnailUrl: 'https://example.com/thumbnail.jpg',
                    }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const img = document.querySelector('.browse-product-card img');
        expect(img?.getAttribute('src')).toContain('thumbnail.jpg');
    });

    it('excludes products with no image at all', async () => {
        setupFetchMock([
            makeProduct({
                id: '1',
                name_en: 'No Image Product',
                variants: [
                    makeVariant({ imageUrls: undefined, thumbnailUrl: undefined }),
                ],
            }),
            makeProduct({
                id: '2',
                name_en: 'Has Image Product',
                variants: [
                    makeVariant({ id: 'var2', imageUrls: 'https://example.com/img_1.jpg' }),
                ],
            }),
        ]);

        renderOffers();
        await waitForProductsLoaded();

        const cards = document.querySelectorAll('.browse-product-card');
        expect(cards.length).toBe(1);
        expect(screen.getByText('Has Image Product')).toBeInTheDocument();
        expect(screen.queryByText('No Image Product')).not.toBeInTheDocument();
    });
});

// Suppress react-router warning about wrapping with within() for the sidebar test
// The within() import is present to keep it available if needed later.
const _within = within;
void _within;
