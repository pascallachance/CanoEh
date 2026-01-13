/**
 * Test script to validate date formatting utilities
 * 
 * This script validates:
 * 1. formatDate() correctly formats valid dates to YYYY/MM/DD
 * 2. formatDate() handles undefined input by returning '-'
 * 3. formatDate() handles invalid date strings by returning '-'
 * 4. formatDate() correctly zero-pads single-digit months and days
 * 5. formatShortDate() correctly formats valid dates to MM/DD
 * 6. formatShortDate() handles edge cases properly
 */

import { formatDate, formatShortDate } from '../src/utils/dateUtils';

console.log('🧪 Testing Date Formatting Utilities\n');

// Test 1: formatDate() with valid dates
console.log('Test 1: formatDate() with valid dates');
const validDates = [
    { input: '2024-01-15T10:30:00Z', expected: '2024/01/15' },
    { input: '2023-12-25T00:00:00Z', expected: '2023/12/25' },
    { input: '2024-06-01T14:22:33Z', expected: '2024/06/01' },
    { input: '2024-03-05T08:15:00Z', expected: '2024/03/05' } // Single-digit month and day
];

let allPassed = true;
validDates.forEach(({ input, expected }) => {
    const result = formatDate(input);
    const passed = result === expected;
    allPassed = allPassed && passed;
    console.log(`  ${passed ? '✅' : '❌'} formatDate("${input}") = "${result}" ${passed ? '' : `(expected "${expected}")`}`);
});
console.log(`Result: ${allPassed ? '✅ PASSED' : '❌ FAILED'}\n`);

// Test 2: formatDate() with undefined input
console.log('Test 2: formatDate() with undefined input');
const undefinedResult = formatDate(undefined);
const undefinedPassed = undefinedResult === '-';
console.log(`  ${undefinedPassed ? '✅' : '❌'} formatDate(undefined) = "${undefinedResult}" ${undefinedPassed ? '' : '(expected "-")'}`);
console.log(`Result: ${undefinedPassed ? '✅ PASSED' : '❌ FAILED'}\n`);

// Test 3: formatDate() with invalid date strings
console.log('Test 3: formatDate() with invalid date strings');
const invalidDates = [
    'invalid-date-string',
    '',
    'not a date',
    '2024-13-45T00:00:00Z' // Invalid month/day
];

let invalidPassed = true;
invalidDates.forEach((input) => {
    const result = formatDate(input);
    const passed = result === '-';
    invalidPassed = invalidPassed && passed;
    console.log(`  ${passed ? '✅' : '❌'} formatDate("${input}") = "${result}" ${passed ? '' : '(expected "-")'}`);
});
console.log(`Result: ${invalidPassed ? '✅ PASSED' : '❌ FAILED'}\n`);

// Test 4: formatDate() zero-padding for single-digit months and days
console.log('Test 4: formatDate() zero-padding for single-digit months and days');
const zeroPadTests = [
    { input: '2024-01-05T00:00:00Z', expected: '2024/01/05' },
    { input: '2024-09-09T00:00:00Z', expected: '2024/09/09' },
    { input: '2024-03-01T00:00:00Z', expected: '2024/03/01' }
];

let zeroPadPassed = true;
zeroPadTests.forEach(({ input, expected }) => {
    const result = formatDate(input);
    const passed = result === expected;
    zeroPadPassed = zeroPadPassed && passed;
    console.log(`  ${passed ? '✅' : '❌'} formatDate("${input}") = "${result}" ${passed ? '' : `(expected "${expected}")`}`);
});
console.log(`Result: ${zeroPadPassed ? '✅ PASSED' : '❌ FAILED'}\n`);

// Test 5: formatShortDate() with valid dates
console.log('Test 5: formatShortDate() with valid dates');
const shortDateTests = [
    { input: '2024-01-15T10:30:00Z', expected: '01/15' },
    { input: '2023-12-25T00:00:00Z', expected: '12/25' },
    { input: '2024-06-01T14:22:33Z', expected: '06/01' },
    { input: '2024-03-05T08:15:00Z', expected: '03/05' }
];

let shortDatePassed = true;
shortDateTests.forEach(({ input, expected }) => {
    const result = formatShortDate(input);
    const passed = result === expected;
    shortDatePassed = shortDatePassed && passed;
    console.log(`  ${passed ? '✅' : '❌'} formatShortDate("${input}") = "${result}" ${passed ? '' : `(expected "${expected}")`}`);
});
console.log(`Result: ${shortDatePassed ? '✅ PASSED' : '❌ FAILED'}\n`);

// Test 6: formatShortDate() with edge cases
console.log('Test 6: formatShortDate() with edge cases');
const shortEdgeCases = [
    { input: undefined, expected: '-', label: 'undefined' },
    { input: 'invalid-date', expected: '-', label: 'invalid date' },
    { input: '', expected: '-', label: 'empty string' }
];

let shortEdgePassed = true;
shortEdgeCases.forEach(({ input, expected, label }) => {
    const result = formatShortDate(input);
    const passed = result === expected;
    shortEdgePassed = shortEdgePassed && passed;
    console.log(`  ${passed ? '✅' : '❌'} formatShortDate(${label}) = "${result}" ${passed ? '' : `(expected "${expected}")`}`);
});
console.log(`Result: ${shortEdgePassed ? '✅ PASSED' : '❌ FAILED'}\n`);

// Summary
console.log('═══════════════════════════════════════');
const allTestsPassed = allPassed && undefinedPassed && invalidPassed && zeroPadPassed && shortDatePassed && shortEdgePassed;
console.log(`Overall: ${allTestsPassed ? '✅ ALL TESTS PASSED' : '❌ SOME TESTS FAILED'}`);
console.log('═══════════════════════════════════════');

if (!allTestsPassed) {
    process.exit(1);
}
