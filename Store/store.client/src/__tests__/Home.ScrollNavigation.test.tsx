import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Mock fetch globally
global.fetch = vi.fn();

describe('Home - Scroll Navigation', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');

        // All API responses return empty arrays
        (global.fetch as ReturnType<typeof vi.fn>).mockImplementation(() =>
            Promise.resolve({
                ok: true,
                json: () => Promise.resolve({ isSuccess: true, value: [] }),
            })
        );
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    /** Helper: make itemsGrid look like it overflows with room to scroll in both directions */
    function mockOverflowingGrid(itemsGrid: HTMLElement) {
        Object.defineProperty(itemsGrid, 'scrollWidth', { writable: true, value: 1200 });
        Object.defineProperty(itemsGrid, 'clientWidth', { writable: true, value: 600 });
        Object.defineProperty(itemsGrid, 'scrollLeft', { writable: true, value: 300 }); // mid-scroll
    }

    /** Helper: make the card appear to have a measurable height (so chevronHeight > 0) */
    function mockCardHeight(card: HTMLElement, height = 280) {
        Object.defineProperty(card, 'offsetHeight', { writable: true, value: height });
    }

    it('should not render carousel chevrons when mouse is not over the card', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).not.toBeInTheDocument();
        expect(screen.queryByLabelText(/Next items|Articles suivants/i)).not.toBeInTheDocument();
    });

    it('should display sections stacked vertically inside cards-section', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const section = document.querySelector('.cards-section');
        expect(section).toBeInTheDocument();

        const cards = section?.querySelectorAll('.item-preview-card');
        expect(cards?.length).toBeGreaterThan(1);

        // Each card should be a direct child of cards-section (vertical stacking)
        cards?.forEach((card) => {
            expect(card.parentElement).toBe(section);
        });
    });

    it('should show right chevron when hovered and grid overflows to the right', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;
        expect(itemsGrid).not.toBeNull();

        // Set up overflow at the left edge (scrollLeft=0) so only right chevron should appear
        Object.defineProperty(itemsGrid, 'scrollWidth', { writable: true, value: 1200 });
        Object.defineProperty(itemsGrid, 'clientWidth', { writable: true, value: 600 });
        Object.defineProperty(itemsGrid, 'scrollLeft', { writable: true, value: 0 });
        mockCardHeight(card);

        fireEvent.mouseEnter(card);

        // Trigger scroll event to update state
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Next items|Articles suivants/i)).toBeInTheDocument();
        });
        expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).not.toBeInTheDocument();
    });

    it('should show left chevron when hovered and grid can scroll left', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;
        expect(itemsGrid).not.toBeNull();

        // Set up overflow at the right edge so only left chevron should appear
        Object.defineProperty(itemsGrid, 'scrollWidth', { writable: true, value: 1200 });
        Object.defineProperty(itemsGrid, 'clientWidth', { writable: true, value: 600 });
        Object.defineProperty(itemsGrid, 'scrollLeft', { writable: true, value: 600 }); // at right edge
        mockCardHeight(card);

        fireEvent.mouseEnter(card);
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).toBeInTheDocument();
        });
        expect(screen.queryByLabelText(/Next items|Articles suivants/i)).not.toBeInTheDocument();
    });

    it('should show both chevrons when hovered and grid is mid-scroll', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;
        expect(itemsGrid).not.toBeNull();

        mockOverflowingGrid(itemsGrid);
        mockCardHeight(card);

        fireEvent.mouseEnter(card);
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).toBeInTheDocument();
        });
        expect(screen.queryByLabelText(/Next items|Articles suivants/i)).toBeInTheDocument();
    });

    it('should hide chevrons when mouse leaves the card', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;

        mockOverflowingGrid(itemsGrid);
        mockCardHeight(card);

        fireEvent.mouseEnter(card);
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).toBeInTheDocument();
        });

        fireEvent.mouseLeave(card);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).not.toBeInTheDocument();
            expect(screen.queryByLabelText(/Next items|Articles suivants/i)).not.toBeInTheDocument();
        });
    });

    it('should call scrollBy with positive left when right chevron is clicked', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;

        Object.defineProperty(itemsGrid, 'scrollWidth', { writable: true, value: 1200 });
        Object.defineProperty(itemsGrid, 'clientWidth', { writable: true, value: 600 });
        Object.defineProperty(itemsGrid, 'scrollLeft', { writable: true, value: 0 });
        mockCardHeight(card);

        const scrollByMock = vi.fn();
        itemsGrid.scrollBy = scrollByMock;

        fireEvent.mouseEnter(card);
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Next items|Articles suivants/i)).toBeInTheDocument();
        });

        const nextBtn = screen.getByLabelText(/Next items|Articles suivants/i);
        fireEvent.click(nextBtn);

        expect(scrollByMock).toHaveBeenCalledOnce();
        const arg = scrollByMock.mock.calls[0][0] as ScrollToOptions;
        expect(typeof arg.left).toBe('number');
        expect(arg.left).toBeGreaterThan(0);
    });

    it('should call scrollBy with negative left when left chevron is clicked', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;

        Object.defineProperty(itemsGrid, 'scrollWidth', { writable: true, value: 1200 });
        Object.defineProperty(itemsGrid, 'clientWidth', { writable: true, value: 600 });
        Object.defineProperty(itemsGrid, 'scrollLeft', { writable: true, value: 600 }); // at right edge
        mockCardHeight(card);

        const scrollByMock = vi.fn();
        itemsGrid.scrollBy = scrollByMock;

        fireEvent.mouseEnter(card);
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).toBeInTheDocument();
        });

        const prevBtn = screen.getByLabelText(/Previous items|Articles précédents/i);
        fireEvent.click(prevBtn);

        expect(scrollByMock).toHaveBeenCalledOnce();
        const arg = scrollByMock.mock.calls[0][0] as ScrollToOptions;
        expect(typeof arg.left).toBe('number');
        expect(arg.left).toBeLessThan(0);
    });

    it('should not show chevrons when grid does not overflow', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card') as HTMLElement;
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement;

        // No overflow — scrollWidth === clientWidth
        Object.defineProperty(itemsGrid, 'scrollWidth', { writable: true, value: 500 });
        Object.defineProperty(itemsGrid, 'clientWidth', { writable: true, value: 500 });
        Object.defineProperty(itemsGrid, 'scrollLeft', { writable: true, value: 0 });
        mockCardHeight(card);

        fireEvent.mouseEnter(card);
        fireEvent.scroll(itemsGrid);

        await waitFor(() => {
            expect(screen.queryByLabelText(/Previous items|Articles précédents/i)).not.toBeInTheDocument();
            expect(screen.queryByLabelText(/Next items|Articles suivants/i)).not.toBeInTheDocument();
        });
    });
});
