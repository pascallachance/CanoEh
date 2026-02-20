/**
 * Utility functions for URL handling
 */

/**
 * Converts a relative URL to an absolute URL by prepending the API base URL.
 * If the URL is already absolute (starts with http:// or https://), returns it unchanged.
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
    
    // If URL is relative (starts with /), prepend API base URL
    if (url.startsWith('/')) {
        const baseUrl = import.meta.env.VITE_API_SELLER_BASE_URL;
        
        // Guard against missing or empty environment variable
        if (!baseUrl) {
            console.error('[toAbsoluteUrl] VITE_API_SELLER_BASE_URL environment variable is not defined');
            return url; // Return original URL as fallback
        }
        
        const absoluteUrl = `${baseUrl}${url}`;
        return absoluteUrl;
    }
    
    // For other cases, return as-is
    return url;
}

/**
 * Converts an absolute URL back to a relative URL by stripping the API base URL prefix.
 * If the URL is already relative or a blob/data URL, returns it unchanged.
 *
 * @param url - The URL to convert (can be absolute or relative)
 * @returns The relative URL, or the original URL if it cannot be converted
 */
export function toRelativeUrl(url: string | undefined): string {
    if (!url) {
        return '';
    }

    // Blob or data URLs cannot be stored as server paths - return as-is
    if (url.startsWith('blob:') || url.startsWith('data:')) {
        return url;
    }

    // If URL is absolute, strip the base URL prefix to get the relative path
    if (url.startsWith('http://') || url.startsWith('https://')) {
        const baseUrl = import.meta.env.VITE_API_SELLER_BASE_URL;
        if (baseUrl && url.startsWith(baseUrl)) {
            return url.substring(baseUrl.length);
        }
        // Cannot convert to relative – return as-is
        return url;
    }

    // Already relative
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
