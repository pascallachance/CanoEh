import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
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

    it('should not render previous/next carousel buttons', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        expect(screen.queryByLabelText(/Previous cards|Cartes précédentes/i)).not.toBeInTheDocument();
        expect(screen.queryByLabelText(/Next cards|Cartes suivantes/i)).not.toBeInTheDocument();
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

    it('should scroll items-grid to the right when wheel scrolls down (deltaY > 0)', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card');
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement | null;
        expect(itemsGrid).not.toBeNull();

        const scrollByMock = vi.fn();
        itemsGrid!.scrollBy = scrollByMock;

        // Dispatch wheel event with deltaY > 0 (scroll down → move items right)
        const wheelDown = new WheelEvent('wheel', { deltaY: 100, bubbles: true, cancelable: true });
        itemsGrid!.dispatchEvent(wheelDown);

        expect(scrollByMock).toHaveBeenCalledOnce();
        const arg = scrollByMock.mock.calls[0][0] as ScrollToOptions;
        expect(typeof arg.left).toBe('number');
        expect(arg.left).toBeGreaterThan(0);
    });

    it('should scroll items-grid to the left when wheel scrolls up (deltaY < 0)', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card');
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement | null;
        expect(itemsGrid).not.toBeNull();

        const scrollByMock = vi.fn();
        itemsGrid!.scrollBy = scrollByMock;

        // Dispatch wheel event with deltaY < 0 (scroll up → move items left)
        const wheelUp = new WheelEvent('wheel', { deltaY: -100, bubbles: true, cancelable: true });
        itemsGrid!.dispatchEvent(wheelUp);

        expect(scrollByMock).toHaveBeenCalledOnce();
        const arg = scrollByMock.mock.calls[0][0] as ScrollToOptions;
        expect(typeof arg.left).toBe('number');
        expect(arg.left).toBeLessThan(0);
    });

    it('should not scroll when deltaY is 0', async () => {
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        const cardTitle = screen.getByText(/Suggested items|Articles suggérés/i);
        const card = cardTitle.closest('.item-preview-card');
        const itemsGrid = card?.querySelector('.items-grid') as HTMLElement | null;
        expect(itemsGrid).not.toBeNull();

        const scrollByMock = vi.fn();
        itemsGrid!.scrollBy = scrollByMock;

        // Dispatch wheel event with deltaY = 0 (no vertical scroll — should be ignored)
        const wheelHorizontal = new WheelEvent('wheel', { deltaY: 0, bubbles: true, cancelable: true });
        itemsGrid!.dispatchEvent(wheelHorizontal);

        expect(scrollByMock).not.toHaveBeenCalled();
    });
});
