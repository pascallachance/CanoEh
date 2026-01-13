/**
 * Validates a date string and returns a Date object or null
 * @param dateString - ISO date string or undefined
 * @returns Valid Date object or null
 */
function validateDate(dateString: string | undefined): Date | null {
    if (!dateString) return null;
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return null;
    return date;
}

/**
 * Formats a date string to YYYY/MM/DD format
 * @param dateString - ISO date string or undefined
 * @returns Formatted date string in YYYY/MM/DD format, or '-' if dateString is undefined or invalid
 */
export function formatDate(dateString: string | undefined): string {
    const date = validateDate(dateString);
    if (!date) return '-';
    
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}/${month}/${day}`;
}

/**
 * Formats a date string to a short MM/DD format suitable for charts and compact displays
 * @param dateString - ISO date string or undefined
 * @returns Formatted date string in MM/DD format, or '-' if dateString is undefined or invalid
 */
export function formatShortDate(dateString: string | undefined): string {
    const date = validateDate(dateString);
    if (!date) return '-';
    
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${month}/${day}`;
}
