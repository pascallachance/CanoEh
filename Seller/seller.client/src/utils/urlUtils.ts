/**
 * Utility functions for URL handling
 */

/**
 * Converts a relative URL to an absolute URL by prepending the API base URL.
 * If the URL is already absolute (starts with http:// or https://), returns it unchanged.
 * 
 * @param url - The URL to convert (can be relative or absolute)
 * @param verbose - Whether to log detailed conversion steps (default: true)
 * @returns The absolute URL, or empty string if input is falsy
 */
export function toAbsoluteUrl(url: string | undefined, verbose: boolean = true): string {
    if (import.meta.env.DEV && verbose) {
        console.log('[toAbsoluteUrl] Input URL:', url);
    }
    
    if (!url) {
        if (import.meta.env.DEV && verbose) {
            console.log('[toAbsoluteUrl] URL is falsy, returning empty string');
        }
        return '';
    }
    
    // If URL is already absolute or special scheme (blob:, data:), return as-is
    if (url.startsWith('http://') || 
        url.startsWith('https://') || 
        url.startsWith('blob:') || 
        url.startsWith('data:')) {
        if (import.meta.env.DEV && verbose) {
            console.log('[toAbsoluteUrl] URL is already absolute or special scheme, returning as-is:', url);
        }
        return url;
    }
    
    // If URL is relative (starts with /), prepend API base URL
    if (url.startsWith('/')) {
        const baseUrl = import.meta.env.VITE_API_SELLER_BASE_URL;
        
        if (import.meta.env.DEV && verbose) {
            console.log('[toAbsoluteUrl] Relative URL detected, base URL:', baseUrl);
        }
        
        // Guard against missing or empty environment variable
        if (!baseUrl) {
            console.error('[toAbsoluteUrl] VITE_API_SELLER_BASE_URL environment variable is not defined');
            return url; // Return original URL as fallback
        }
        
        const absoluteUrl = `${baseUrl}${url}`;
        if (import.meta.env.DEV && verbose) {
            console.log('[toAbsoluteUrl] Converted to absolute URL:', absoluteUrl);
        }
        return absoluteUrl;
    }
    
    // For other cases, return as-is
    if (import.meta.env.DEV && verbose) {
        console.log('[toAbsoluteUrl] No conversion needed, returning as-is:', url);
    }
    return url;
}

/**
 * Converts an array of URLs (which might be a comma-separated string or an array)
 * to an array of absolute URLs.
 * 
 * @param urls - The URLs to convert (can be a comma-separated string or an array)
 * @returns An array of absolute URLs (empty strings are filtered out)
 */
export function toAbsoluteUrlArray(urls: string | string[] | undefined): string[] {
    if (import.meta.env.DEV) {
        console.log('[toAbsoluteUrlArray] Processing', typeof urls === 'string' ? 'comma-separated string' : `array of ${Array.isArray(urls) ? urls.length : 0} URLs`);
    }
    
    if (!urls) {
        if (import.meta.env.DEV) {
            console.log('[toAbsoluteUrlArray] URLs is falsy, returning empty array');
        }
        return [];
    }
    
    // Convert string to array if needed
    const urlArray = typeof urls === 'string' ? urls.split(',') : urls;
    
    // Trim whitespace, filter out empty strings, then convert each URL to absolute
    // Use verbose=false to suppress per-URL logging and reduce console noise
    const result = urlArray
        .map(url => url.trim())
        .filter(url => url.length > 0)
        .map(url => toAbsoluteUrl(url, false));
    
    if (import.meta.env.DEV) {
        console.log('[toAbsoluteUrlArray] Converted', result.length, 'URLs');
    }
    return result;
}
