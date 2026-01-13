import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { toAbsoluteUrl, toAbsoluteUrlArray } from '../utils/urlUtils';

describe('urlUtils', () => {
    beforeEach(() => {
        vi.clearAllMocks();
        vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182');
    });

    afterEach(() => {
        vi.unstubAllEnvs();
    });

    describe('toAbsoluteUrl', () => {
        it('should return empty string for falsy input', () => {
            expect(toAbsoluteUrl(undefined)).toBe('');
            expect(toAbsoluteUrl('')).toBe('');
        });

        it('should return absolute HTTP URLs unchanged', () => {
            const url = 'http://example.com/image.jpg';
            expect(toAbsoluteUrl(url)).toBe(url);
        });

        it('should return absolute HTTPS URLs unchanged', () => {
            const url = 'https://example.com/image.jpg';
            expect(toAbsoluteUrl(url)).toBe(url);
        });

        it('should return blob URLs unchanged', () => {
            const url = 'blob:http://localhost/abc-123';
            expect(toAbsoluteUrl(url)).toBe(url);
        });

        it('should return data URLs unchanged', () => {
            const url = 'data:image/png;base64,iVBORw0KGgo=';
            expect(toAbsoluteUrl(url)).toBe(url);
        });

        it('should prepend API base URL to relative paths with leading slash', () => {
            const url = '/uploads/company-id/image.jpg';
            expect(toAbsoluteUrl(url)).toBe('https://localhost:7182/uploads/company-id/image.jpg');
        });

        it('should prepend API base URL to relative paths without leading slash', () => {
            const url = 'uploads/company-id/image.jpg';
            expect(toAbsoluteUrl(url)).toBe('https://localhost:7182/uploads/company-id/image.jpg');
        });

        it('should handle API base URL with trailing slash', () => {
            vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182/');
            const url = '/uploads/image.jpg';
            expect(toAbsoluteUrl(url)).toBe('https://localhost:7182/uploads/image.jpg');
        });

        it('should handle API base URL with trailing slash and path without leading slash', () => {
            vi.stubEnv('VITE_API_STORE_BASE_URL', 'https://localhost:7182/');
            const url = 'uploads/image.jpg';
            expect(toAbsoluteUrl(url)).toBe('https://localhost:7182/uploads/image.jpg');
        });

        it('should return original URL when API base URL is not defined', () => {
            vi.stubEnv('VITE_API_STORE_BASE_URL', '');
            const url = '/uploads/image.jpg';
            const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
            
            expect(toAbsoluteUrl(url)).toBe(url);
            expect(consoleErrorSpy).toHaveBeenCalledWith(
                expect.stringContaining('VITE_API_STORE_BASE_URL environment variable is not defined')
            );
            
            consoleErrorSpy.mockRestore();
        });
    });

    describe('toAbsoluteUrlArray', () => {
        it('should return empty array for falsy input', () => {
            expect(toAbsoluteUrlArray(undefined)).toEqual([]);
            expect(toAbsoluteUrlArray('')).toEqual([]);
        });

        it('should convert comma-separated string to absolute URLs', () => {
            const urls = '/uploads/image1.jpg,/uploads/image2.jpg,/uploads/image3.jpg';
            const result = toAbsoluteUrlArray(urls);
            
            expect(result).toEqual([
                'https://localhost:7182/uploads/image1.jpg',
                'https://localhost:7182/uploads/image2.jpg',
                'https://localhost:7182/uploads/image3.jpg'
            ]);
        });

        it('should handle comma-separated URLs with whitespace', () => {
            const urls = ' /uploads/image1.jpg , /uploads/image2.jpg , /uploads/image3.jpg ';
            const result = toAbsoluteUrlArray(urls);
            
            expect(result).toEqual([
                'https://localhost:7182/uploads/image1.jpg',
                'https://localhost:7182/uploads/image2.jpg',
                'https://localhost:7182/uploads/image3.jpg'
            ]);
        });

        it('should filter out empty strings from comma-separated URLs', () => {
            const urls = '/uploads/image1.jpg,,/uploads/image2.jpg, ,';
            const result = toAbsoluteUrlArray(urls);
            
            expect(result).toEqual([
                'https://localhost:7182/uploads/image1.jpg',
                'https://localhost:7182/uploads/image2.jpg'
            ]);
        });

        it('should convert array of URLs to absolute URLs', () => {
            const urls = ['/uploads/image1.jpg', '/uploads/image2.jpg'];
            const result = toAbsoluteUrlArray(urls);
            
            expect(result).toEqual([
                'https://localhost:7182/uploads/image1.jpg',
                'https://localhost:7182/uploads/image2.jpg'
            ]);
        });

        it('should handle mixed absolute and relative URLs', () => {
            const urls = 'https://cdn.example.com/image1.jpg,/uploads/image2.jpg,uploads/image3.jpg';
            const result = toAbsoluteUrlArray(urls);
            
            expect(result).toEqual([
                'https://cdn.example.com/image1.jpg',
                'https://localhost:7182/uploads/image2.jpg',
                'https://localhost:7182/uploads/image3.jpg'
            ]);
        });
    });
});
