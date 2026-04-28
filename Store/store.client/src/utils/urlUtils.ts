/**
 * Utility functions for URL handling
 */

/**
 * Converts a relative URL to an absolute URL by prepending the API base URL.
 * If the URL is already absolute (starts with http:// or https://), returns it unchanged.
 * Properly handles edge cases like missing leading slashes and trailing slashes in base URL.
 *
 * When VITE_API_STORE_BASE_URL is empty or not set, relative URLs are returned as-is so
 * that the Vite dev-server proxy can forward them to the backend. This is the recommended
 * setup for local development.
 *
 * @param url - The URL to convert (can be relative or absolute)
 * @returns The absolute URL, or empty string if input is falsy
 */
export function toAbsoluteUrl(url: string | undefined): string {
    if (!url) {
        return '';
    }
    
    // If URL is already absolute or special scheme (blob:, data:), return as-is
    if (url.startsWith('http://') || 
        url.startsWith('https://') || 
        url.startsWith('blob:') || 
        url.startsWith('data:')) {
        return url;
    }
    
    // Get the API base URL
    const baseUrl = import.meta.env.VITE_API_STORE_BASE_URL;
    
    // When base URL is not configured or intentionally empty, return the relative URL
    // as-is. The Vite dev proxy (or a production reverse-proxy) will forward it to the
    // correct backend origin, keeping the resource same-origin from the browser's
    // perspective and avoiding all cross-origin / CORS issues.
    if (!baseUrl) {
        return url;
    }
    
    // Normalize the base URL and relative path to prevent malformed URLs
    // Remove trailing slash from base URL if present
    const normalizedBaseUrl = baseUrl.replace(/\/$/, '');
    
    // Ensure relative path starts with a slash
    const normalizedPath = url.startsWith('/') ? url : `/${url}`;
    
    const absoluteUrl = `${normalizedBaseUrl}${normalizedPath}`;
    return absoluteUrl;
}

/**
 * Converts an array of URLs (which might be a comma-separated string or an array)
 * to an array of absolute URLs.
 * 
 * @param urls - The URLs to convert (can be a comma-separated string or an array)
 * @returns An array of absolute URLs (empty strings are filtered out)
 */
export function toAbsoluteUrlArray(urls: string | string[] | undefined): string[] {
    if (!urls) {
        return [];
    }
    
    // Convert string to array if needed
    const urlArray = typeof urls === 'string' ? urls.split(',') : urls;
    
    // Trim whitespace, filter out empty strings, then convert each URL to absolute
    const result = urlArray
        .map(url => url.trim())
        .filter(url => url.length > 0)
        .map(url => toAbsoluteUrl(url));
    
    return result;
}
