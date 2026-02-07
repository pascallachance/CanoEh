# Inline Product Workflow - Manual Testing Guide

## Overview
This document provides step-by-step instructions for manually testing the new inline product workflow feature where Add Product, Edit Product, and Manage Offers operations are displayed within the section-container instead of opening in separate screens.

## Prerequisites
1. API server running on https://localhost:7182
2. Seller client running on https://localhost:62209
3. Valid seller account with at least one company
4. At least one existing product in the system (for testing edit functionality)

## Test Scenarios

### 1. Add Product Workflow - Inline Display

**Goal**: Verify that clicking "Add Product" displays the product creation form inline within the seller page, without navigating to a separate route.

**Steps**:
1. Login to the Seller application at https://localhost:62209
2. Navigate to the "Products" section by clicking the "Products" tab
3. Click the "Add Product" button in the action bar
4. **Expected Result**: 
   - The product list should be replaced by the "Add Product Step 1" form
   - The URL should remain `/seller` (not change to `/add-product`)
   - The seller navigation bar should remain visible at the top
   - The form should display "Add New Product" as the title
   - Step indicator should show "Step 1 of 3"

5. Fill in the Step 1 form:
   - Item Name (English): "Test Product"
   - Item Name (French): "Produit Test"
   - Description (English): "This is a test product"
   - Description (French): "Ceci est un produit test"
6. Click "Next" button
7. **Expected Result**:
   - Step 2 form should appear inline (replacing Step 1)
   - URL should still be `/seller`
   - Step indicator should show "Step 2 of 3"

8. Fill in the Step 2 form:
   - Select a category
   - Add variant attributes if desired
9. Click "Next" button
10. **Expected Result**:
    - Step 3 form should appear inline (replacing Step 2)
    - URL should still be `/seller`
    - Step indicator should show "Step 3 of 3"

11. Fill in the Step 3 form with variant details
12. Click "Submit" button
13. **Expected Result**:
    - Should return to the product list view
    - New product should appear in the list
    - URL should still be `/seller`
    - Should see a success notification

14. Click "Cancel" button (test from any step)
15. **Expected Result**:
    - Should return to the product list view
    - No product should be created
    - URL should still be `/seller`

### 2. Edit Product Workflow - Inline Display

**Goal**: Verify that clicking "Edit" on a product displays the edit form inline within the seller page.

**Steps**:
1. From the Products section, locate an existing product in the list
2. Click the "Edit" button on that product
3. **Expected Result**:
   - The product list should be replaced by the "Edit Product Step 1" form
   - The URL should remain `/seller` (not change to `/edit-product`)
   - The form should display "Edit Product" as the title
   - Form fields should be pre-populated with the existing product data
   - Step indicator should show "Step 1 of 3"

4. Modify some fields in Step 1
5. Click "Next" button
6. **Expected Result**:
   - Step 2 form should appear with existing data
   - URL should still be `/seller`
   - Category and variant attributes should be pre-populated

7. Navigate through Step 2 and Step 3
8. **Expected Result**:
   - Each step should display inline
   - URL should remain `/seller` throughout
   - Data should be preserved between steps

9. Click "Submit" button on Step 3
10. **Expected Result**:
    - Should return to the product list view
    - Updated product should appear in the list with changes
    - URL should still be `/seller`
    - Should see a success notification

11. Test navigation between steps using step indicator (if in edit mode)
12. **Expected Result**:
    - Should be able to click on completed steps
    - Should navigate between steps without losing data
    - URL should remain `/seller`

### 3. Manage Offers Workflow - Verify Unchanged

**Goal**: Verify that Manage Offers continues to work as before (it was already inline).

**Steps**:
1. From the Products section with at least one product
2. Click the "Manage Offers" button
3. **Expected Result**:
   - A modal dialog should appear overlaying the product list
   - The modal should display "Manage Offers" as the title
   - URL should still be `/seller`
   - Should see a table of products with offer fields

4. Modify offer percentage, start date, or end date for a product
5. Click "Save" button
6. **Expected Result**:
   - Modal should close
   - Should return to product list view
   - Changes should be saved
   - URL should still be `/seller`

7. Click "Cancel" button (or click outside modal)
8. **Expected Result**:
   - Modal should close without saving
   - Should return to product list view

### 4. Navigation Consistency

**Goal**: Verify that all product operations keep users within the CanoEh/seller context.

**Steps**:
1. Perform a complete Add Product workflow
2. Check browser history - **Expected**: No entries for `/add-product/*` routes
3. Perform a complete Edit Product workflow  
4. Check browser history - **Expected**: No entries for `/edit-product/*` routes
5. Open and close Manage Offers
6. Check browser history - **Expected**: No navigation changes

7. Switch to different seller sections (Analytics, Orders, Company) while in the middle of Add/Edit Product
8. **Expected Result**:
   - Should be able to navigate away
   - Product workflow data should be lost (this is expected behavior)
   - Each section should display correctly

### 5. User Experience Validation

**Goal**: Verify that the changes improve the user experience by maintaining context.

**Checklist**:
- [ ] Seller navigation bar is always visible during Add/Edit Product workflows
- [ ] Users can see they are still in the "Products" section (active tab)
- [ ] No feeling of leaving the CanoEh/seller website
- [ ] Consistent UI styling between list view and form views
- [ ] Smooth transitions between steps
- [ ] Clear visual indicators of current step
- [ ] Cancel/Back buttons work as expected
- [ ] No unexpected page reloads or navigations

## Common Issues to Watch For

1. **Form Data Loss**: Ensure data is preserved when navigating between steps
2. **URL Changes**: URL should never change from `/seller` during product operations
3. **Broken Navigation**: Verify navigation bar remains functional
4. **Missing Success Messages**: Should see notifications after saving
5. **Step Indicator Issues**: Should accurately reflect current step and completed steps
6. **Edit Mode**: Pre-filled data should be correct and complete

## Comparison with Old Behavior

### Before Changes:
- Clicking "Add Product" navigated to `/add-product` route (separate page)
- Clicking "Edit" navigated to `/edit-product` route (separate page)
- Users felt like they were leaving the seller dashboard
- Navigation bar disappeared during product operations
- Browser back button would take you through each step

### After Changes:
- Clicking "Add Product" displays form inline within `/seller` route
- Clicking "Edit" displays form inline within `/seller` route
- Users remain in the seller dashboard context
- Navigation bar remains visible
- Browser back button behavior unchanged (still on `/seller`)
- Consistent with "Manage Offers" pattern (modal/inline display)

## Success Criteria

✅ All product operations (Add, Edit, Manage Offers) display content inline within section-container
✅ URL remains `/seller` throughout all operations
✅ Navigation bar remains visible and functional
✅ User feels they are staying within CanoEh/seller website
✅ All existing functionality works correctly
✅ Data is saved and loaded properly
✅ No regressions in Manage Offers functionality

## Reporting Issues

When reporting issues, please include:
1. Which test scenario was being performed
2. Specific step where the issue occurred
3. Expected vs actual behavior
4. Browser console errors (if any)
5. Network tab errors (if any)
6. Screenshots or screen recordings

## Notes

- The old `/add-product/*` and `/edit-product/*` routes still exist in the codebase for backward compatibility
- These routes are no longer used by the Seller component
- Direct navigation to these routes will still work if entered manually in the URL bar
