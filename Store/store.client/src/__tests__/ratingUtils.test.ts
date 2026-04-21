import { describe, expect, it } from 'vitest';
import { formatMapleRating, mapleLeavesFromRating } from '../utils/ratingUtils';

describe('ratingUtils', () => {
    it('uses a maple leaf when rounded rating is zero', () => {
        expect(mapleLeavesFromRating(0)).toBe('🍁');
        expect(mapleLeavesFromRating(0.4)).toBe('🍁');
    });

    it('omits trailing .0 in formatted rating values', () => {
        expect(formatMapleRating(1, 3, 'en')).toBe('🍁 1/5 • 3 reviews');
        expect(formatMapleRating(5, 2, 'fr')).toBe('🍁🍁🍁🍁🍁 5/5 • 2 avis');
    });

    it('keeps one decimal place when needed', () => {
        expect(formatMapleRating(1.4, 7, 'en')).toBe('🍁 1.4/5 • 7 reviews');
    });
});
