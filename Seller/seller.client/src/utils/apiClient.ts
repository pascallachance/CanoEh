// API client with automatic token refresh functionality
export class ApiClient {
    private static baseUrl = import.meta.env.VITE_API_SELLER_BASE_URL;
    private static isRefreshing = false;
    private static refreshPromise: Promise<boolean> | null = null;

    // Helper to check if response is 401 Unauthorized
    private static is401(response: Response): boolean {
        return response.status === 401;
    }

    // Refresh access token using refresh token
    private static async refreshToken(): Promise<boolean> {
        try {
            const response = await fetch(`${this.baseUrl}/api/Login/refresh`, {
                method: 'POST',
                credentials: 'include', // Include cookies
            });

            if (response.ok) {
                console.log('Token refreshed successfully');
                return true;
            } else {
                console.log('Token refresh failed');
                return false;
            }
        } catch (error) {
            console.error('Token refresh error:', error);
            return false;
        }
    }

    // Ensure only one refresh request is active at a time
    private static async ensureValidToken(): Promise<boolean> {
        if (this.isRefreshing) {
            // Wait for existing refresh to complete
            if (this.refreshPromise) {
                return await this.refreshPromise;
            }
            return false;
        }

        this.isRefreshing = true;
        this.refreshPromise = this.refreshToken();
        
        try {
            const result = await this.refreshPromise;
            return result;
        } finally {
            this.isRefreshing = false;
            this.refreshPromise = null;
        }
    }

    // Main API request method with automatic token refresh
    public static async request(url: string, options: RequestInit = {}): Promise<Response> {
        // Default options
        const requestOptions: RequestInit = {
            credentials: 'include', // Always include cookies
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            },
        };

        // Make initial request
        let response = await fetch(url, requestOptions);

        // If unauthorized, try to refresh token and retry once
        if (this.is401(response)) {
            console.log('Received 401, attempting token refresh...');
            
            const refreshSuccess = await this.ensureValidToken();
            
            if (refreshSuccess) {
                console.log('Token refreshed, retrying original request...');
                // Retry the original request with refreshed token
                response = await fetch(url, requestOptions);
            } else {
                console.log('Token refresh failed, user needs to log in again');
                // Could redirect to login or trigger logout here
                // For now, just return the 401 response
            }
        }

        return response;
    }

    // Convenience methods for common HTTP verbs
    public static async get(url: string, options: RequestInit = {}): Promise<Response> {
        return this.request(url, { ...options, method: 'GET' });
    }

    public static async post(url: string, body?: unknown, options: RequestInit = {}): Promise<Response> {
        return this.request(url, {
            ...options,
            method: 'POST',
            body: body ? JSON.stringify(body) : undefined,
        });
    }

    public static async put(url: string, body?: unknown, options: RequestInit = {}): Promise<Response> {
        return this.request(url, {
            ...options,
            method: 'PUT',
            body: body ? JSON.stringify(body) : undefined,
        });
    }

    public static async delete(url: string, options: RequestInit = {}): Promise<Response> {
        return this.request(url, { ...options, method: 'DELETE' });
    }
}