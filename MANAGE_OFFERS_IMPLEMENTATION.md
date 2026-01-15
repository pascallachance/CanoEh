# Manage Offers Implementation

## Overview
This document describes the implementation of the "Manage Offers" functionality added to the Seller ProductsSection component in the CanoEh e-commerce application.

## Feature Description
The Manage Offers feature allows sellers to:
- View all their products and variants in a single interface
- Set promotional offer percentages (0-100%) for individual product variants
- Define offer start and end dates
- Clear/remove offers from variants
- Batch update multiple variants at once

## Technical Implementation

### Frontend Changes

#### 1. ProductsSection.tsx (`/Seller/seller.client/src/components/Seller/ProductsSection.tsx`)

**Interface Updates:**
- Updated `ApiItemVariant` interface to include offer fields:
  ```typescript
  interface ApiItemVariant {
    // ... existing fields
    offer?: number;
    offerStart?: string;
    offerEnd?: string;
  }
  ```

**State Management:**
- `showManageOffersModal`: Boolean to control modal visibility
- `offerChanges`: Map to track pending changes before saving
- `isSavingOffers`: Boolean to indicate save operation in progress
- `manageOffersModalRef`: Ref for accessibility and focus management
- `previousActiveElementForOffers`: Ref to restore focus when modal closes

**Key Functions:**

1. `handleOpenManageOffers()`: Opens the modal and initializes state
2. `handleCloseManageOffers()`: Closes the modal and clears pending changes
3. `handleOfferChange(variantId, field, value)`: Updates offer fields in local state with validation
4. `getCurrentOffer(variant, field)`: Gets current value from changes or original data
5. `toISODateOrUndefined(dateString)`: Helper to convert and validate dates
6. `handleSaveOffers()`: Batch saves all changes to the API
7. `handleClearOffer(variantId)`: Clears all offer fields for a variant

**UI Components:**

- **Manage Offers Button**: Located at the top of the products list
  - Disabled when no products exist or data is loading
  - Styled in teal/cyan color to differentiate from other actions

- **Manage Offers Modal**: Large modal (1200px max width) with scrollable content
  - Table layout showing all products and variants
  - Item name column with rowspan for products with multiple variants
  - Editable inputs for:
    - Offer percentage (number input, 0-100, 0.01 step)
    - Offer start date (date picker)
    - Offer end date (date picker)
  - Clear button to remove offers
  - Save/Cancel buttons at bottom

**Validation:**
- Offer percentage: 0-100 range, prevents NaN values
- Dates: Validated before conversion to ISO format
- Batch validation before API submission

**Accessibility Features:**
- ARIA labels and roles for modal
- Focus management (trap focus within modal)
- Keyboard navigation support (Tab, Shift+Tab, Escape)
- Focus restoration when modal closes
- Proper semantic HTML structure

#### 2. ProductsSection.css (`/Seller/seller.client/src/components/Seller/ProductsSection.css`)

**New CSS Classes:**
- `.products-list-header`: Container for the Manage Offers button
- `.products-manage-offers-button`: Styled button (teal background)
- `.products-modal-content--large`: Large modal variant (90% max width)
- `.products-offers-container`: Scrollable container for offers table
- `.products-offers-table`: Table styling with sticky header
- `.products-offer-input`: Input field styling for offers
- `.products-clear-offer-button`: Red button for clearing offers

**Responsive Design:**
- Mobile-friendly table layout
- Adjusts modal width for smaller screens (95% on mobile)
- Reduced font size and padding on mobile

#### 3. translations.ts (`/Seller/seller.client/src/resources/translations.ts`)

**New Translation Keys:**
```typescript
'products.manageOffers': { en: 'Manage Offers', fr: 'Gérer les offres' }
'products.offers.offer': { en: 'Offer', fr: 'Offre' }
'products.offers.offerStart': { en: 'Offer Start', fr: 'Début de l\'offre' }
'products.offers.offerEnd': { en: 'Offer End', fr: 'Fin de l\'offre' }
'products.offers.save': { en: 'Save Offers', fr: 'Enregistrer les offres' }
'products.offers.clear': { en: 'Clear', fr: 'Effacer' }
'products.offers.clearOffer': { en: 'Clear offer for this variant', fr: 'Effacer l\'offre pour cette variante' }
'products.offers.noChanges': { en: 'No changes to save', fr: 'Aucune modification à enregistrer' }
'products.offers.saveSuccess': { en: 'Offers updated successfully', fr: 'Offres mises à jour avec succès' }
'products.offers.saveError': { en: 'Failed to update offers', fr: 'Échec de la mise à jour des offres' }
```

### Backend Integration

**API Endpoint Used:**
- `PUT /api/Item/UpdateItemVariantOffer`

**Request Format:**
```typescript
{
  variantId: string;      // GUID
  offer?: number;         // 0-100
  offerStart?: string;    // ISO 8601 date
  offerEnd?: string;      // ISO 8601 date
}
```

**Authentication:**
- Uses existing ApiClient with automatic token refresh
- Credentials included in all requests
- Controller validates user ownership of variants

## User Workflow

1. Seller navigates to Products section
2. Clicks "Manage Offers" button
3. Modal opens showing all products and variants
4. Seller edits offer fields for desired variants:
   - Enter percentage (e.g., 25 for 25% off)
   - Select start date (optional)
   - Select end date (optional)
5. Seller can clear offers using "Clear" button
6. Clicks "Save Offers" to submit changes
7. System saves all changes via batch API calls
8. Success notification appears
9. Modal closes and product list refreshes

## Code Quality

### Code Review Results
- ✅ No security vulnerabilities (CodeQL scan)
- ✅ Input validation implemented
- ✅ Date validation with helper function
- ✅ NaN prevention for numeric inputs
- ✅ Consistent with existing code patterns
- ✅ Accessibility features included
- ✅ Translation support for i18n

### Best Practices Applied
- Minimal, surgical changes to existing code
- Reused existing patterns (modal structure, API client, notifications)
- Proper error handling and user feedback
- Separation of concerns (UI, state, API)
- Clean, readable code with comments
- Responsive design
- Type safety with TypeScript

## Testing Recommendations

### Manual Testing
1. **Basic Functionality:**
   - Open Manage Offers modal
   - Set offer on a variant
   - Save and verify change persists
   
2. **Validation Testing:**
   - Try invalid offer percentage (negative, > 100, text)
   - Enter invalid dates
   - Verify error messages

3. **Batch Operations:**
   - Update multiple variants at once
   - Verify all changes save correctly
   
4. **Clear Functionality:**
   - Set an offer
   - Clear it
   - Save and verify offer is removed
   
5. **UI/UX Testing:**
   - Test on mobile device
   - Verify accessibility (keyboard navigation)
   - Test with screen reader
   - Verify translations (English/French)

6. **Error Scenarios:**
   - Network failure during save
   - Verify error message appears
   - Changes are not lost

### Integration Testing
- Verify API integration with backend
- Test with real product data
- Verify offer dates are stored correctly in database
- Test token refresh during long sessions

## Future Enhancements

Potential improvements for future iterations:

1. **Bulk Actions:**
   - Apply same offer to multiple variants
   - Copy offer from one variant to others

2. **Filtering:**
   - Filter variants with/without offers
   - Search by product name in modal

3. **Validation Enhancements:**
   - Warn if end date is before start date
   - Suggest end date based on start date

4. **Preview:**
   - Show calculated discount price
   - Preview offer appearance in store

5. **History:**
   - Track offer changes
   - Show offer performance analytics

## Conclusion

The Manage Offers implementation provides sellers with an intuitive, efficient way to manage promotional offers across all their product variants. The implementation follows best practices, maintains consistency with existing code patterns, and includes proper validation, accessibility features, and internationalization support.
