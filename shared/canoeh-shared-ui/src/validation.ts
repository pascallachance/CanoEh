export const validateEmailFormat = (value: string): string => {
    const trimmedValue = value.trim();
    if (!trimmedValue) return 'Email is required';
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedValue)) return 'Invalid email format';
    if (trimmedValue.length > 254) return 'Email is too long (max 254 characters)';
    return '';
};

export const validatePasswordLength = (value: string): string => {
    if (!value) return 'Password is required';
    if (value.length < 8) return 'Password must be at least 8 characters';
    return '';
};

export const validateName = (value: string, label: string): string => {
    const trimmedValue = value.trim();
    if (!trimmedValue) return `${label} is required`;
    if (trimmedValue.length > 100) return `${label} is too long (max 100 characters)`;
    return '';
};

export const validatePhone = (value: string): string => {
    const trimmedValue = value.trim();
    if (!trimmedValue) return '';
    if (!/^\+?[\d\s\-().]{7,20}$/.test(trimmedValue)) return 'Invalid phone number format';
    return '';
};

