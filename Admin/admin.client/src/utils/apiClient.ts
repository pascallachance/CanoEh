// API client with automatic token refresh functionality
export class ApiClient {
    private static baseUrl = import.meta.env.VITE_API_ADMIN_BASE_URL;
    private static isRefreshing = false;
    private static refreshPromise: Promise<boolean> | null = null;

    private static is401(response: Response): boolean {
        return response.status === 401;
    }

    private static async refreshToken(): Promise<boolean> {
        try {
            const response = await fetch(`${this.baseUrl}/api/Login/refresh`, {
                method: 'POST',
                credentials: 'include',
            });

            if (response.ok) {
                return true;
            } else {
                return false;
            }
        } catch (error) {
            console.error('Token refresh error:', error);
            return false;
        }
    }

    private static async ensureValidToken(): Promise<boolean> {
        if (this.isRefreshing) {
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

    public static async request(url: string, options: RequestInit = {}): Promise<Response> {
        const requestOptions: RequestInit = {
            credentials: 'include',
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            },
        };

        let response = await fetch(url, requestOptions);

        if (this.is401(response)) {
            const refreshSuccess = await this.ensureValidToken();

            if (refreshSuccess) {
                response = await fetch(url, requestOptions);
            }
        }

        return response;
    }

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
