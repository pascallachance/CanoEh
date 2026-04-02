export const validateEmailFormat = (value: string): string => {
    if (!value) return 'Email is required';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) return 'Invalid email format';
    if (value.length > 254) return 'Email is too long (max 254 characters)';
    return '';
};

export const validatePasswordLength = (value: string): string => {
    if (!value) return 'Password is required';
    if (value.length < 8) return 'Password must be at least 8 characters';
    return '';
};

export const validateName = (value: string, label: string): string => {
    if (!value.trim()) return `${label} is required`;
    if (value.length > 100) return `${label} is too long (max 100 characters)`;
    return '';
};

export const validatePhone = (value: string): string => {
    if (!value) return '';
    if (!/^\+?[\d\s\-().]{7,20}$/.test(value)) return 'Invalid phone number format';
    return '';
};
