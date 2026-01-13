/**
 * Formats a date string to YYYY/MM/DD format
 * @param dateString - ISO date string or undefined
 * @returns Formatted date string in YYYY/MM/DD format, or '-' if dateString is undefined or invalid
 */
export function formatDate(dateString: string | undefined): string {
    if (!dateString) return '-';
    const date = new Date(dateString);
    
    // Check if date is invalid
    if (isNaN(date.getTime())) return '-';
    
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}/${month}/${day}`;
}
