const MIN_RATING = 0;
const MAX_RATING = 5;

export function clampRating(rating: number): number {
    if (!Number.isFinite(rating)) return 0;
    return Math.min(MAX_RATING, Math.max(MIN_RATING, rating));
}

export function mapleLeavesFromRating(rating: number): string {
    const integerPoints = Math.floor(clampRating(rating));
    return integerPoints > 0 ? '🍁'.repeat(integerPoints) : '';
}

export interface MapleLeafDisplayParts {
    fullLeaves: string;
    decimalLeafSize: number | null;
}

export function mapleLeafDisplayPartsFromRating(rating: number): MapleLeafDisplayParts {
    const normalized = clampRating(rating);
    const roundedToTenth = Math.round(normalized * 10) / 10;
    const integerPoints = Math.floor(roundedToTenth);
    const decimalTenths = Math.round((roundedToTenth - integerPoints) * 10);
    // decimalTenths is 1..9, so +4 maps it to the requested 5..13px range (.1=>5px ... .9=>13px).
    const decimalLeafSize = decimalTenths > 0 ? decimalTenths + 4 : null;

    return {
        fullLeaves: integerPoints > 0 ? '🍁'.repeat(integerPoints) : '',
        decimalLeafSize,
    };
}

export function formatRatingValue(rating: number): string {
    return clampRating(rating).toFixed(1).replace(/\.0$/, '');
}

export function formatMapleRating(rating: number, ratingCount: number, language: string): string {
    const normalizedRating = clampRating(rating);
    const leaves = mapleLeavesFromRating(normalizedRating) || '🍁';
    const formattedRating = formatRatingValue(normalizedRating);
    const countLabel = language === 'fr'
        ? `${ratingCount} avis`
        : `${ratingCount} reviews`;

    return `${leaves} ${formattedRating}/5 • ${countLabel}`;
}
