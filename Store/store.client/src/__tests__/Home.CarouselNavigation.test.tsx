import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';
import userEvent from '@testing-library/user-event';

// Mock fetch globally
global.fetch = vi.fn();

describe('Home - Carousel Navigation', () => {
    let originalInnerWidth: number;

    beforeEach(() => {
        vi.clearAllMocks();
        // Capture the original innerWidth so afterEach can restore the exact value.
        originalInnerWidth = window.innerWidth;
        // Set window.innerWidth so that updateVisibleCardsCount computes exactly 3 visible cards.
        // availableWidth = 1110 - 2*10 = 1090; N = floor((1090+20)/370) = floor(3.0) = 3
        Object.defineProperty(window, 'innerWidth', { writable: true, value: 1110 });
        // Mock environment variable
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
        
        // Mock all API responses to return empty arrays
        (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
            return Promise.resolve({
                ok: true,
                json: () => Promise.resolve({
                    isSuccess: true,
                    value: []
                })
            });
        });
    });

    afterEach(() => {
        vi.unstubAllEnvs();
        Object.defineProperty(window, 'innerWidth', { writable: true, value: originalInnerWidth });
    });

    it('should navigate backwards correctly when scrollLeft is slightly past a page boundary', async () => {
        const user = userEvent.setup();
        
        // Render the Home component
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        // Wait for the component to render and fetch data
        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        // Get the carousel container and buttons
        const cardsContainer = document.querySelector('.cards-container') as HTMLElement;
        const prevButton = screen.getByLabelText(/Previous cards|Cartes précédentes/i);
        const nextButton = screen.getByLabelText(/Next cards|Cartes suivantes/i);

        expect(cardsContainer).toBeInTheDocument();
        expect(prevButton).toBeInTheDocument();
        expect(nextButton).toBeInTheDocument();

        // Mock the carousel dimensions
        // 3 visible cards with 350px card width and 20px gap.
        // Snap-aligned pageWidth = visibleCount * (cardWidth + gap) = 3 * 370 = 1110px
        const pageWidth = 1110;
        
        // Set scrollWidth to simulate multiple pages (e.g., 9 cards)
        Object.defineProperty(cardsContainer, 'scrollWidth', {
            writable: true,
            value: 3310 // 9 cards * 350px + 8 gaps * 20px
        });
        
        Object.defineProperty(cardsContainer, 'clientWidth', {
            writable: true,
            value: pageWidth
        });

        // Mock scrollTo method to capture calls
        const scrollToMock = vi.fn();
        cardsContainer.scrollTo = scrollToMock;

        // Simulate being at a position slightly past the first page boundary.
        // This represents the real-world scenario where the browser snaps to a position
        // that is microscopically past the exact page boundary.
        Object.defineProperty(cardsContainer, 'scrollLeft', {
            writable: true,
            value: pageWidth + 0.001 // 1110.001px
        });

        // Trigger the scroll event to update button states
        cardsContainer.dispatchEvent(new Event('scroll'));

        // Wait for button state to update
        await waitFor(() => {
            expect(prevButton).not.toBeDisabled();
        });

        // Click the Previous button
        await user.click(prevButton);

        // Verify that scrollTo was called with the correct position
        // Should scroll to page 0 (position 0), not stay at page 1 (position 1090)
        expect(scrollToMock).toHaveBeenCalledWith({
            left: 0,
            behavior: 'smooth'
        });
    });

    it('should navigate backwards correctly when scrollLeft is exactly at a page boundary', async () => {
        const user = userEvent.setup();
        
        // Render the Home component
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        // Wait for the component to render
        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        // Get the carousel container and buttons
        const cardsContainer = document.querySelector('.cards-container') as HTMLElement;
        const prevButton = screen.getByLabelText(/Previous cards|Cartes précédentes/i);

        expect(cardsContainer).toBeInTheDocument();
        expect(prevButton).toBeInTheDocument();

        // Mock the carousel dimensions
        // Snap-aligned pageWidth = 3 * (350 + 20) = 1110px
        const pageWidth = 1110;
        
        Object.defineProperty(cardsContainer, 'scrollWidth', {
            writable: true,
            value: 3310
        });
        
        Object.defineProperty(cardsContainer, 'clientWidth', {
            writable: true,
            value: pageWidth
        });

        // Mock scrollTo method
        const scrollToMock = vi.fn();
        cardsContainer.scrollTo = scrollToMock;

        // Simulate being exactly at the first page boundary
        Object.defineProperty(cardsContainer, 'scrollLeft', {
            writable: true,
            value: pageWidth // Exactly 1110px
        });

        // Trigger scroll event
        cardsContainer.dispatchEvent(new Event('scroll'));

        // Wait for button state to update
        await waitFor(() => {
            expect(prevButton).not.toBeDisabled();
        });

        // Click the Previous button
        await user.click(prevButton);

        // Verify that scrollTo was called to go back to page 0
        expect(scrollToMock).toHaveBeenCalledWith({
            left: 0,
            behavior: 'smooth'
        });
    });

    it('should not skip pages when navigating forward from a position between pages', async () => {
        const user = userEvent.setup();
        
        // Render the Home component
        render(
            <BrowserRouter>
                <Home isAuthenticated={false} />
            </BrowserRouter>
        );

        // Wait for the component to render
        await waitFor(() => {
            expect(screen.getByText(/Suggested items|Articles suggérés/i)).toBeInTheDocument();
        });

        // Get the carousel container and buttons
        const cardsContainer = document.querySelector('.cards-container') as HTMLElement;
        const nextButton = screen.getByLabelText(/Next cards|Cartes suivantes/i);

        expect(cardsContainer).toBeInTheDocument();
        expect(nextButton).toBeInTheDocument();

        // Mock the carousel dimensions
        // Snap-aligned pageWidth = 3 * (350 + 20) = 1110px
        const pageWidth = 1110;
        
        Object.defineProperty(cardsContainer, 'scrollWidth', {
            writable: true,
            value: 3310
        });
        
        Object.defineProperty(cardsContainer, 'clientWidth', {
            writable: true,
            value: pageWidth
        });

        // Mock scrollTo method
        const scrollToMock = vi.fn();
        cardsContainer.scrollTo = scrollToMock;

        // Simulate being at 80% of the way to page 1 (0.8 * pageWidth)
        Object.defineProperty(cardsContainer, 'scrollLeft', {
            writable: true,
            value: pageWidth * 0.8 // 888px
        });

        // Trigger scroll event
        cardsContainer.dispatchEvent(new Event('scroll'));

        // Wait for button state to update
        await waitFor(() => {
            expect(nextButton).not.toBeDisabled();
        });

        // Click the Next button
        await user.click(nextButton);

        // Verify that scrollTo was called to go to page 1 (not skip to page 2)
        expect(scrollToMock).toHaveBeenCalledWith({
            left: pageWidth, // Should go to 1110px (page 1), not 2220px (page 2)
            behavior: 'smooth'
        });
    });
});
