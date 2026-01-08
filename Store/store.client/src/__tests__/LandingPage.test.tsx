/**
 * Simple test to verify the Store landing page shows Home (shopping) page by default
 * and that cart navigation works correctly
 * 
 * This test verifies the requirement:
 * "When entering https://localhost:64941 the first page should be the main screen not the login"
 * 
 * Also tests cart functionality:
 * - Cart route renders Cart component
 * - Cart button navigates to /cart
 * - Cart count badge displays correct value
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { userEvent } from '@testing-library/user-event';
import App from '../App';

describe('Store Landing Page Routing', () => {
  it('should render Home component at root path /', () => {
    // This test verifies that navigating to "/" renders the Home component
    // The Home component is the shopping/browsing page, not the login page
    
    window.history.pushState({}, 'Test', '/');
    
    render(<App />);
    
    // Home component should have the CanoEh! branding
    const logoElements = screen.getAllByText(/CanoEh!/i);
    expect(logoElements.length).toBeGreaterThan(0);
    
    // Should have a Connect button (to navigate TO login, not already ON login)
    const connectButton = screen.getByRole('button', { name: /Connect|Connexion/i });
    expect(connectButton).toBeTruthy();
  });

  it('should allow browsing without authentication', () => {
    window.history.pushState({}, 'Test', '/');
    
    render(<App />);
    
    // Should have search functionality available without login
    const searchInputs = screen.getAllByPlaceholderText(/Search|Rechercher/i);
    expect(searchInputs.length).toBeGreaterThan(0);
    
    // Should show welcome banner
    const welcomeText = screen.getByText(/Welcome to CanoEh!|Bienvenue chez CanoEh!/i);
    expect(welcomeText).toBeTruthy();
  });

  it('should NOT show login form at root path', () => {
    window.history.pushState({}, 'Test', '/');
    
    render(<App />);
    
    // Login page would have both email and password inputs together
    // Home page should NOT have these (it has a Connect button instead)
    const passwordInputs = screen.queryAllByLabelText(/password/i);
    const emailInputs = screen.queryAllByLabelText(/email/i);
    
    // If we're on the home page (not login), these should not both be present
    const hasLoginForm = passwordInputs.length > 0 && emailInputs.length > 0;
    expect(hasLoginForm).toBe(false);
  });

  it('should show login page only at /login path', () => {
    window.history.pushState({}, 'Test', '/login');
    
    render(<App />);
    
    // Login page should have email and password inputs
    const passwordInputs = screen.queryAllByLabelText(/password/i);
    const emailInputs = screen.queryAllByLabelText(/email/i);
    
    expect(emailInputs.length).toBeGreaterThan(0);
    expect(passwordInputs.length).toBeGreaterThan(0);
  });
});

describe('Cart Navigation and Functionality', () => {
  it('should render Cart component at /cart path', () => {
    // Navigate to the cart page
    window.history.pushState({}, 'Test', '/cart');
    
    render(<App />);
    
    // Cart page should have "Shopping Cart" or "Panier d'achat" heading
    const cartHeading = screen.getByRole('heading', { name: /Shopping Cart|Panier d'achat/i });
    expect(cartHeading).toBeInTheDocument();
  });

  it('should show empty cart message when cart has no items', () => {
    window.history.pushState({}, 'Test', '/cart');
    
    render(<App />);
    
    // Should show empty cart message
    const emptyMessage = screen.getByRole('heading', { name: /Your cart is empty|Votre panier est vide/i });
    expect(emptyMessage).toBeInTheDocument();
    
    // Should have "Continue Shopping" button
    const continueButton = screen.getByRole('button', { name: /Continue Shopping|Continuer vos achats/i });
    expect(continueButton).toBeInTheDocument();
  });

  it('should navigate to cart page when cart button is clicked', async () => {
    window.history.pushState({}, 'Test', '/');
    
    const user = userEvent.setup();
    render(<App />);
    
    // Find and click the cart button
    const cartButton = screen.getByRole('button', { name: /Shopping cart|Panier d'achat/i });
    expect(cartButton).toBeInTheDocument();
    
    await user.click(cartButton);
    
    // After clicking, should navigate to cart page
    // Wait for cart heading to appear
    const cartHeading = await screen.findByRole('heading', { name: /Shopping Cart|Panier d'achat/i });
    expect(cartHeading).toBeInTheDocument();
  });

  it('should display cart button in navigation', () => {
    window.history.pushState({}, 'Test', '/');
    
    render(<App />);
    
    // Cart button should be present in navigation
    const cartButton = screen.getByRole('button', { name: /Shopping cart|Panier d'achat/i });
    expect(cartButton).toBeInTheDocument();
    
    // Cart button should contain "Cart" or "Panier" text
    expect(cartButton.textContent).toMatch(/Cart|Panier/i);
  });

  it('should not display cart count badge when count is 0', () => {
    window.history.pushState({}, 'Test', '/');
    
    render(<App />);
    
    // Cart button should be present
    const cartButton = screen.getByRole('button', { name: /Shopping cart|Panier d'achat/i });
    expect(cartButton).toBeInTheDocument();
    
    // Badge with class 'cart-count' should not be visible when count is 0
    const badge = cartButton.querySelector('.cart-count');
    expect(badge).not.toBeInTheDocument();
  });
});

