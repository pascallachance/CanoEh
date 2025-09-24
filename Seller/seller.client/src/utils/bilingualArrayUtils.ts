/**
 * Utility functions for synchronizing bilingual arrays (English and French)
 * This module provides reusable functions to maintain synchronized arrays
 * and common operations on bilingual data structures.
 */

import { translations } from '../resources/translations';

export interface BilingualArrays {
    values_en: string[];
    values_fr: string[];
    length: number;
}

export interface BilingualAttribute {
    name_en: string;
    name_fr: string;
    values_en: string[];
    values_fr: string[];
}

/**
 * Synchronizes bilingual arrays by padding the shorter array with empty strings
 * to ensure both arrays have the same length
 */
export const synchronizeBilingualArrays = (arrayEn: string[], arrayFr: string[]): BilingualArrays => {
    const maxLength = Math.max(arrayEn.length, arrayFr.length);
    const syncedArrayEn = [...arrayEn];
    const syncedArrayFr = [...arrayFr];
    
    // Pad shorter array with empty strings to maintain synchronization
    const paddedArrayEn = [...syncedArrayEn, ...Array(maxLength - syncedArrayEn.length).fill('')];
    const paddedArrayFr = [...syncedArrayFr, ...Array(maxLength - syncedArrayFr.length).fill('')];
    
    return { 
        values_en: paddedArrayEn, 
        values_fr: paddedArrayFr, 
        length: maxLength 
    };
};

/**
 * Validation result for bilingual array synchronization
 */
export interface BilingualArrayValidationResult {
    isValid: boolean;
    errorMessage?: string;
    values_en?: string[];
    values_fr?: string[];
}

/**
 * Validates and optionally synchronizes bilingual arrays
 * This function consolidates synchronization validation logic to reduce code duplication
 */
export const validateBilingualArraySync = (
    arrayEn: string[],
    arrayFr: string[],
    options?: {
        filterEmpty?: boolean;
        attributeName?: string;
        errorType?: 'console' | 'user' | 'none';
        customUserErrorMessage?: string;
        allowEmpty?: boolean;
    }
): BilingualArrayValidationResult => {
    const { 
        filterEmpty = false, 
        attributeName, 
        errorType = 'none', 
        customUserErrorMessage = translations['error.bilingualValuesMismatch'].en,
        allowEmpty = true
    } = options || {};
    
    // Apply filtering if requested
    const workingArrayEn = filterEmpty ? arrayEn.filter(v => v.trim()) : arrayEn;
    const workingArrayFr = filterEmpty ? arrayFr.filter(v => v.trim()) : arrayFr;
    
    // Check if lengths match
    if (workingArrayEn.length !== workingArrayFr.length) {
        let errorMessage = '';
        
        if (errorType === 'console') {
            errorMessage = attributeName 
                ? `Attribute "${attributeName}" has mismatched array lengths: EN(${workingArrayEn.length}) vs FR(${workingArrayFr.length})`
                : `Mismatched array lengths: EN(${workingArrayEn.length}) vs FR(${workingArrayFr.length})`;
            console.error(errorMessage);
        } else if (errorType === 'user') {
            errorMessage = customUserErrorMessage;
        }
        
        return {
            isValid: false,
            errorMessage,
            values_en: workingArrayEn,
            values_fr: workingArrayFr
        };
    }
    
    // Check for empty arrays if not allowed
    if (!allowEmpty && (workingArrayEn.length === 0 || workingArrayFr.length === 0)) {
        let errorMessage = '';
        
        if (errorType === 'console') {
            errorMessage = attributeName 
                ? `Attribute "${attributeName}" has empty arrays: EN(${workingArrayEn.length}) FR(${workingArrayFr.length})`
                : `Empty arrays detected: EN(${workingArrayEn.length}) FR(${workingArrayFr.length})`;
            console.error(errorMessage);
        } else if (errorType === 'user') {
            errorMessage = customUserErrorMessage;
        }
        
        return {
            isValid: false,
            errorMessage,
            values_en: workingArrayEn,
            values_fr: workingArrayFr
        };
    }
    
    return {
        isValid: true,
        values_en: workingArrayEn,
        values_fr: workingArrayFr
    };
};

/**
 * Creates a state update function that synchronizes bilingual arrays before applying changes
 * This is a higher-order function that reduces code duplication in state updates
 */

/**
 * Updates a specific index in synchronized bilingual arrays
 */
export const updateBilingualArrayValue = (
    arrayEn: string[],
    arrayFr: string[], 
    index: number, 
    value: string, 
    language: 'en' | 'fr'
): BilingualArrays => {
    const syncedData = synchronizeBilingualArrays(arrayEn, arrayFr);
    
    // Update the specific value if index is valid
    if (index >= 0 && index < syncedData.length) {
        if (language === 'en') {
            syncedData.values_en[index] = value;
        } else {
            syncedData.values_fr[index] = value;
        }
    }
    
    return syncedData;
};

/**
 * Removes an item at the specified index from synchronized bilingual arrays
 */
export const removeBilingualArrayValue = (
    arrayEn: string[], 
    arrayFr: string[], 
    index: number
): BilingualArrays => {
    const syncedData = synchronizeBilingualArrays(arrayEn, arrayFr);
    
    // Only remove if the index is valid for both arrays
    if (index >= 0 && index < syncedData.length) {
        return {
            values_en: syncedData.values_en.filter((_, i) => i !== index),
            values_fr: syncedData.values_fr.filter((_, i) => i !== index),
            length: syncedData.length - 1
        };
    }
    
    // Return synchronized arrays without removing if index is invalid
    return syncedData;
};