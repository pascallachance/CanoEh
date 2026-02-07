# Manage Offers Inline Implementation

## Overview
This document describes the implementation of moving the Manage Offers functionality from a modal overlay to an inline display within the section-container in the Seller Products section.

## Problem Statement
Previously, the Manage Offers feature opened as a modal overlay on top of the product list. While the Add/Update Product workflows were moved to inline displays within the section-container (see INLINE_PRODUCT_WORKFLOW_IMPLEMENTATION.md), Manage Offers remained as a modal. This created an inconsistent user experience.

## Solution
Move Manage Offers to use the same inline pattern as Add/Update Product, displaying it directly within the section-container instead of as a modal overlay.

## Implementation Details

### State Management Changes

**Before:**
```typescript
const [showManageOffersModal, setShowManageOffersModal] = useState(false);
const manageOffersModalRef = useRef<HTMLDivElement>(null);
const previousActiveElementForOffers = useRef<HTMLElement | null>(null);
```

**After:**
```typescript
const [showManageOffers, setShowManageOffers] = useState(false);
// Modal-specific refs removed
```

### JSX Structure Changes

**Before (Modal Overlay):**
```jsx
{showManageOffersModal && (
    <div className="products-modal-overlay">
        <div className="products-modal-content products-modal-content--large"
             ref={manageOffersModalRef}
             tabIndex={-1}
             onKeyDown={handleManageOffersKeyDown}>
            <h3>{t('products.manageOffers')}</h3>
            {/* Offers table content */}
            <div className="products-modal-actions">
                <button className="products-modal-button">Cancel</button>
                <button className="products-modal-button">Save</button>
            </div>
        </div>
    </div>
)}
```

**After (Inline Section):**
```jsx
{showManageOffers && (
    <div className="products-manage-offers-section">
        <h3>{t('products.manageOffers')}</h3>
        {/* Offers table content */}
        <div className="products-form-actions">
            <button className="products-form-button--cancel">Cancel</button>
            <button className="products-form-button--save">Save</button>
        </div>
    </div>
)}
```

### Product List Visibility

**Before:**
```jsx
{inlineProductMode === 'none' && showListSection && (
    <div className="products-list-section">
        {/* Product list */}
    </div>
)}
```

**After:**
```jsx
{inlineProductMode === 'none' && !showManageOffers && showListSection && (
    <div className="products-list-section">
        {/* Product list */}
    </div>
)}
```

The additional `!showManageOffers` condition ensures the product list is hidden when Manage Offers is displayed.

### Removed Code

1. **Modal Focus Management** (~35 lines):
   - Focus trapping on modal open
   - Focus restoration on modal close
   - Body scroll prevention

2. **Keyboard Event Handlers** (~35 lines):
   - Escape key listener
   - Tab key trapping within modal

3. **handleManageOffersKeyDown Function** (~25 lines):
   - Tab focus trapping logic
   - Focusable elements query

**Total Code Removed:** ~95 lines of modal-specific code

### CSS Changes

**Added:**
```css
/* Inline Manage Offers Section */
.products-manage-offers-section {
    background: #f8f9fa;
    padding: 2rem;
    border-radius: 8px;
    margin-bottom: 2rem;
    border: 1px solid #e1e5e9;
}

.products-manage-offers-section h3 {
    margin: 0 0 1.5rem 0;
}

/* Form Button Styles */
.products-form-button {
    padding: 0.75rem 1.5rem;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 1rem;
    transition: background-color 0.2s;
}

.products-form-button--save {
    background: #28a745;
}

.products-form-button--cancel {
    background: #6c757d;
}
```

**Unchanged:**
- `.products-offers-container`
- `.products-offers-table-wrapper`
- `.products-offers-table`
- `.products-offer-input`
- `.products-clear-offer-button`

## Benefits

### 1. Consistent User Experience
- Matches the pattern established for Add/Update Product workflows
- Users stay within the seller dashboard context
- No jarring transition to a modal overlay

### 2. Simplified Code
- Removed 95+ lines of modal-specific code
- No focus management needed
- No keyboard trapping required
- Simpler event handling

### 3. Improved Accessibility
- Native flow without modal complications
- Standard keyboard navigation works naturally
- No need for aria-modal or role="dialog"
- Focus management handled by browser

### 4. Easier Maintenance
- Less complex code to maintain
- Follows established pattern (DRY principle)
- Fewer edge cases to handle

## Testing Recommendations

1. **Navigate to Seller Products Section**
   - Verify the "Manage Offers" button is visible when products exist
   - Verify the button is disabled when no products exist or while loading

2. **Open Manage Offers**
   - Click "Manage Offers" button
   - Verify the product list is hidden
   - Verify the Manage Offers section displays inline
   - Verify the table shows all products and variants

3. **Edit Offers**
   - Change offer percentage for a variant
   - Change offer start date
   - Change offer end date
   - Verify changes are tracked (Save button enables)

4. **Cancel Changes**
   - Make changes to offers
   - Click "Cancel" button
   - Verify returns to product list
   - Verify changes are discarded

5. **Save Changes**
   - Make changes to offers
   - Click "Save" button
   - Verify success notification
   - Verify returns to product list
   - Verify changes persist

6. **Keyboard Navigation**
   - Tab through all inputs and buttons
   - Verify natural tab order
   - Press Enter to submit from last input
   - Verify standard keyboard behavior works

## Files Modified

1. `Seller/seller.client/src/components/Seller/ProductsSection.tsx`
   - ~95 lines removed (modal infrastructure)
   - ~80 lines added (inline section)
   - Net: ~15 lines removed

2. `Seller/seller.client/src/components/Seller/ProductsSection.css`
   - ~50 lines added (inline section styles)

## Migration Notes

This change is backward compatible:
- No API changes
- No data structure changes
- No breaking changes to parent components
- Existing offer data and functionality preserved

## Related Documentation

- `INLINE_PRODUCT_WORKFLOW_IMPLEMENTATION.md` - Pattern this implementation follows
- `MANAGE_OFFERS_IMPLEMENTATION.md` - Original Manage Offers feature documentation
