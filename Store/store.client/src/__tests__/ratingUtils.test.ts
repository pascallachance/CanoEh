import { describe, expect, it } from 'vitest';
import {
    formatMapleRating,
    formatRatingValue,
    mapleLeafDisplayPartsFromRating,
    mapleLeavesFromRating,
} from '../utils/ratingUtils';

describe('ratingUtils', () => {
    it('returns no full leaves when rating is below one point', () => {
        expect(mapleLeavesFromRating(0)).toBe('');
        expect(mapleLeavesFromRating(0.9)).toBe('');
    });

    it('returns one leaf per full rating point', () => {
        expect(mapleLeavesFromRating(1.9)).toBe('🍁');
        expect(mapleLeavesFromRating(4.8)).toBe('🍁🍁🍁🍁');
    });

    it('returns decimal leaf size mapped from tenths (.1=>5 ... .9=>13)', () => {
        expect(mapleLeafDisplayPartsFromRating(3.1)).toEqual({ fullLeaves: '🍁🍁🍁', decimalLeafSize: 5 });
        expect(mapleLeafDisplayPartsFromRating(3.7)).toEqual({ fullLeaves: '🍁🍁🍁', decimalLeafSize: 11 });
        expect(mapleLeafDisplayPartsFromRating(3.9)).toEqual({ fullLeaves: '🍁🍁🍁', decimalLeafSize: 13 });
        expect(mapleLeafDisplayPartsFromRating(4)).toEqual({ fullLeaves: '🍁🍁🍁🍁', decimalLeafSize: null });
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
