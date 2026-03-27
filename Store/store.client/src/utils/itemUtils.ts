/**
 * Shared helpers for selecting the best variant and primary image from a product.
 */

/** Pattern that identifies a primary product image (filename ends with _1 before the extension). */
export const PRIMARY_IMAGE_PATTERN = /_1\.(jpg|jpeg|png|gif|webp)$/i;

export interface MinimalVariant {
    price: number;
    imageUrls?: string;
    thumbnailUrl?: string;
    deleted: boolean;
}

/**
 * From a list of non-deleted variants, returns the one with the lowest price.
 * Returns null when the list is empty.
 */
export function cheapestActiveVariant<T extends MinimalVariant>(variants: T[]): T | null {
    const active = variants.filter(v => !v.deleted);
    if (active.length === 0) return null;
    return active.reduce((prev, curr) => (curr.price < prev.price ? curr : prev));
}

/**
 * Picks the best display image from an ordered list of variants.
 * Prefers an image whose filename ends with _1 (PRIMARY_IMAGE_PATTERN);
 * falls back to the first image in imageUrls, then thumbnailUrl.
 * Returns null when no image is found.
 */
export function pickPrimaryImage(variants: MinimalVariant[]): string | null {
    for (const variant of variants) {
        if (variant.imageUrls) {
            const urls = variant.imageUrls
                .split(',')
                .map(u => u.trim())
                .filter(u => u.length > 0);
            const primary = urls.find(u => PRIMARY_IMAGE_PATTERN.test(u));
            const chosen = primary ?? urls[0] ?? null;
            if (chosen) return chosen;
        }
        if (variant.thumbnailUrl) return variant.thumbnailUrl;
    }
    return null;
}
