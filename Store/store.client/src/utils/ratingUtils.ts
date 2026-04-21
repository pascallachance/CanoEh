const MIN_RATING = 0;
const MAX_RATING = 5;

export function clampRating(rating: number): number {
    if (!Number.isFinite(rating)) return 0;
    return Math.min(MAX_RATING, Math.max(MIN_RATING, rating));
}

export function mapleLeavesFromRating(rating: number): string {
    const rounded = Math.round(clampRating(rating));
    return rounded > 0 ? '🍁'.repeat(rounded) : '';
}

export function formatMapleRating(rating: number, ratingCount: number, language: string): string {
    const normalizedRating = clampRating(rating);
    const leaves = mapleLeavesFromRating(normalizedRating) || '🍁';
    const formattedRating = normalizedRating.toFixed(1).replace(/\.0$/, '');
    const countLabel = language === 'fr'
        ? `${ratingCount} avis`
        : `${ratingCount} reviews`;

    return `${leaves} ${formattedRating}/5 • ${countLabel}`;
}
