/**
 * Validates a date string and returns a Date object or null
 * @param dateString - ISO date string (UTC) or undefined
 * @returns Valid Date object or null
 * @note Accepts UTC ISO strings from the API and creates a Date object that will display in local timezone
 */
function validateDate(dateString: string | undefined): Date | null {
    if (!dateString) return null;
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return null;
    return date;
}

/**
 * Formats a UTC date string to YYYY/MM/DD format in the viewer's local timezone
 * @param dateString - ISO date string (UTC) or undefined
 * @returns Formatted date string in YYYY/MM/DD format in local timezone, or '-' if dateString is undefined or invalid
 */
export function formatDate(dateString: string | undefined): string {
    const date = validateDate(dateString);
    if (!date) return '-';
    
    // getFullYear, getMonth, and getDate automatically convert UTC to local timezone
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}/${month}/${day}`;
}

/**
 * Formats a UTC date string to a short MM/DD format in the viewer's local timezone
 * Suitable for charts and compact displays
 * @param dateString - ISO date string (UTC) or undefined
 * @returns Formatted date string in MM/DD format in local timezone, or '-' if dateString is undefined or invalid
 */
export function formatShortDate(dateString: string | undefined): string {
    const date = validateDate(dateString);
    if (!date) return '-';
    
    // getMonth and getDate automatically convert UTC to local timezone
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${month}/${day}`;
}

/**
 * Converts a date input (YYYY-MM-DD) to UTC ISO string for API submission
 * The date is interpreted as midnight UTC on the specified date.
 * @param dateString - Date string in YYYY-MM-DD format from HTML date input
 * @returns UTC ISO string suitable for API, or undefined if invalid
 */
export function toUTCISOString(dateString: string | undefined): string | undefined {
    if (!dateString) return undefined;
    
    // Explicitly parse as UTC by appending time and timezone
    // This ensures "2024-01-15" becomes "2024-01-15T00:00:00.000Z"
    const date = new Date(dateString + 'T00:00:00.000Z');
    if (isNaN(date.getTime())) return undefined;
    
    return date.toISOString();
}
