import { describe, expect, it } from 'vitest';
import { formatMapleRating, formatRatingValue, mapleLeavesFromRating } from '../utils/ratingUtils';

describe('ratingUtils', () => {
    it('returns no leaves when rounded rating is zero', () => {
        expect(mapleLeavesFromRating(0)).toBe('');
        expect(mapleLeavesFromRating(0.4)).toBe('');
    });

    it('omits trailing .0 in formatted rating values', () => {
        expect(formatMapleRating(1, 3, 'en')).toBe('🍁 1/5 • 3 reviews');
        expect(formatMapleRating(5, 2, 'fr')).toBe('🍁🍁🍁🍁🍁 5/5 • 2 avis');
    });

    it('keeps one decimal place when needed', () => {
        expect(formatMapleRating(1.4, 7, 'en')).toBe('🍁 1.4/5 • 7 reviews');
    });

    it('uses a maple leaf placeholder in formatted product rating when rounded leaves are zero', () => {
        expect(formatMapleRating(0, 1, 'en')).toBe('🍁 0/5 • 1 reviews');
        expect(formatMapleRating(0.4, 4, 'fr')).toBe('🍁 0.4/5 • 4 avis');
    });

    it('formats badge rating values without trailing .0 and clamps to range', () => {
        expect(formatRatingValue(4)).toBe('4');
        expect(formatRatingValue(4.2)).toBe('4.2');
        expect(formatRatingValue(8)).toBe('5');
        expect(formatRatingValue(-2)).toBe('0');
    });
});
