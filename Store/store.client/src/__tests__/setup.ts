import '@testing-library/jest-dom';

// ResizeObserver is not available in some JSDOM environments — provide a no-op stub if needed
if (globalThis.ResizeObserver === undefined) {
    class ResizeObserver {
        observe() {}
        unobserve() {}
        disconnect() {}
    }

    (globalThis as unknown as { ResizeObserver: typeof ResizeObserver }).ResizeObserver = ResizeObserver;
}
