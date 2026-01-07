/**
 * Simple test to verify the Store landing page shows Home (shopping) page by default
 * 
 * This test verifies the requirement:
 * "When entering https://localhost:64941 the first page should be the main screen not the login"
 * 
 * Run with: npm test (when testing framework is set up)
 */

import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import App from '../App';

describe('Store Landing Page Routing', () => {
  it('should render Home component at root path /', () => {
    // This test verifies that navigating to "/" renders the Home component
    // The Home component is the shopping/browsing page, not the login page
    
    window.history.pushState({}, 'Test', '/');
    
    render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );
    
    // Home component should have the CanoEh! branding
    const logoElements = screen.getAllByText(/CanoEh!/i);
    expect(logoElements.length).toBeGreaterThan(0);
    
    // Should have a Connect button (to navigate TO login, not already ON login)
    const connectButton = screen.getByRole('button', { name: /Connect|Connexion/i });
    expect(connectButton).toBeTruthy();
  });

  it('should allow browsing without authentication', () => {
    window.history.pushState({}, 'Test', '/');
    
    render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );
    
    // Should have search functionality available without login
    const searchInputs = screen.getAllByPlaceholderText(/Search|Rechercher/i);
    expect(searchInputs.length).toBeGreaterThan(0);
    
    // Should show welcome banner
    const welcomeText = screen.getByText(/Welcome to CanoEh!|Bienvenue chez CanoEh!/i);
    expect(welcomeText).toBeTruthy();
  });

  it('should NOT show login form at root path', () => {
    window.history.pushState({}, 'Test', '/');
    
    render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );
    
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
    
    render(
      <BrowserRouter>
        <App />
      </BrowserRouter>
    );
    
    // Login page should have email and password inputs
    const passwordInputs = screen.queryAllByLabelText(/password/i);
    const emailInputs = screen.queryAllByLabelText(/email/i);
    
    expect(emailInputs.length).toBeGreaterThan(0);
    expect(passwordInputs.length).toBeGreaterThan(0);
  });
});
