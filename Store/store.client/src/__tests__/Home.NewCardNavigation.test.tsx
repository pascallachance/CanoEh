import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import Home from '../components/Home';

// Capture navigate calls
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async (importOriginal) => {
    const actual = await importOriginal<typeof import('react-router-dom')>();
    return {
        ...actual,
        useNavigate: () => mockNavigate,
    };
});

global.fetch = vi.fn();

const emptyApiResponse = { isSuccess: true, value: [] };

function mockFetchByUrl() {
    (global.fetch as ReturnType<typeof vi.fn>).mockImplementation((url: string) => {
        if (url.includes('GetRecentlyAddedProducts')) {
            return Promise.resolve({ ok: true, json: async () => emptyApiResponse });
        }
        if (url.includes('GetSuggestedProducts')) {
            return Promise.resolve({ ok: true, json: async () => emptyApiResponse });
        }
        if (url.includes('GetProductsWithOffers')) {
            return Promise.resolve({ ok: true, json: async () => emptyApiResponse });
        }
        if (url.includes('GetSuggestedCategoriesProducts')) {
            return Promise.resolve({ ok: true, json: async () => emptyApiResponse });
        }
        return Promise.resolve({ ok: false, json: async () => ({}) });
    });
}

describe('Home – card navigation to new browse pages', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    it('navigates to /suggested-items when the Suggested items card title is clicked', async () => {
        mockFetchByUrl();
        render(<BrowserRouter><Home /></BrowserRouter>);

        // Wait for fetch to complete so the card title renders
        await waitFor(() => expect(global.fetch).toHaveBeenCalled());

        const user = userEvent.setup();

        // The card title is rendered as a button (even with empty products the card title is clickable)
        const cardTitle = await screen.findByText(/Suggested items|Articles suggérés/);
        await user.click(cardTitle);

        expect(mockNavigate).toHaveBeenCalledWith('/suggested-items');
    });

    it('navigates to /recently-added when the Recently added items card title is clicked', async () => {
        mockFetchByUrl();
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => expect(global.fetch).toHaveBeenCalled());

        const user = userEvent.setup();

        const cardTitle = await screen.findByText(/Recently added items|Articles récemment ajoutés/);
        await user.click(cardTitle);

        expect(mockNavigate).toHaveBeenCalledWith('/recently-added');
    });

    it('still navigates to /offers when the Offers card title is clicked', async () => {
        mockFetchByUrl();
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => expect(global.fetch).toHaveBeenCalled());

        const user = userEvent.setup();

        const cardTitle = await screen.findByText(/^Offers$|^Offres$/);
        await user.click(cardTitle);

        expect(mockNavigate).toHaveBeenCalledWith('/offers');
    });

    it('still navigates to /categories when the Explore Categories card title is clicked', async () => {
        mockFetchByUrl();
        render(<BrowserRouter><Home /></BrowserRouter>);

        await waitFor(() => expect(global.fetch).toHaveBeenCalled());

        const user = userEvent.setup();

        const cardTitle = await screen.findByText(/Explore Categories|Explorer les catégories/);
        await user.click(cardTitle);

        expect(mockNavigate).toHaveBeenCalledWith('/categories');
    });
});
