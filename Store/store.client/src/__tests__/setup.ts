import '@testing-library/jest-dom';

// ResizeObserver is not available in JSDOM — provide a no-op stub
global.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
};
