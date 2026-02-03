# CanoEh Database Quick Reference

## Database Schema Overview

### Tables Summary

| Table | Purpose | Key Columns |
|-------|---------|-------------|
| **User** | User accounts and authentication | id (PK), email (unique), password, validEmail, refreshToken |
| **Session** | User login sessions | SessionId (PK), UserId (FK), ExpiresAt, LoggedOutAt |
| **Company** | Seller/company information | Id (PK), OwnerID (FK→User), Name, Email, WebSite |
| **Address** | User/company addresses | Id (PK), UserId (FK→User), AddressType, IsDefault |
| **PaymentMethod** | Stored payment methods | ID (PK), UserID (FK→User), Type, CardLast4, IsDefault |
| **Category** | Product categories (hierarchical) | Id (PK), Name_en, Name_fr, ParentCategoryId (FK→Category) |
| **Item** | Products | Id (PK), SellerID (FK→User), CategoryID (FK→Category), Name_en/fr |
| **ItemVariant** | Product variants (SKU, price, stock) | Id (PK), ItemId (FK→Item), Price, StockQuantity, Sku, Offer |
| **ItemAttribute** | Custom item attributes | Id (PK), ItemID (FK→Item), AttributeName_en/fr, Attributes_en/fr |
| **ItemVariantAttribute** | Custom variant attributes | Id (PK), ItemVariantID (FK→ItemVariant), AttributeName_en/fr |
| **ItemVariantExtraAttribute** | Variant-specific attributes (not matrix) | Id (PK), ItemVariantId (FK→ItemVariant), Name_en/fr, Value_en/fr |
| **OrderStatus** | Lookup: order statuses | ID (PK), StatusCode (unique), Name_en, Name_fr |
| **OrderItemStatus** | Lookup: order item statuses | ID (PK), StatusCode (unique), Name_en, Name_fr |
| **Order** | Customer orders | ID (PK), UserID (FK→User), OrderNumber (auto), StatusID (FK) |
| **OrderItem** | Items in orders | ID (PK), OrderID (FK), ItemID (FK), ItemVariantID (FK) |
| **OrderAddress** | Order shipping/billing addresses | ID (PK), OrderID (FK→Order), Type, FullName, City, Country |
| **OrderPayment** | Order payments | ID (PK), OrderID (FK→Order), PaymentMethodID (FK), Amount, Provider |
| **TaxRate** | Tax rates by location | ID (PK), Country, ProvinceState, Rate, IsActive |

### Key Features

#### Bilingual Support
Most tables include `_en` and `_fr` columns for English and French content:
- Category: Name_en, Name_fr
- Item: Name_en, Name_fr, Description_en, Description_fr
- ItemVariant: ItemVariantName_en, ItemVariantName_fr
- ItemAttribute: AttributeName_en/fr, Attributes_en/fr
- OrderItem: Name_en/fr, VariantName_en/fr
- OrderStatus: Name_en, Name_fr
- TaxRate: Name_en, Name_fr

#### Security Features
- **Password Hashing**: Passwords stored hashed in User table
- **Email Validation**: emailValidationToken, validEmail fields
- **Password Recovery**: passwordResetToken, passwordResetTokenExpiry
- **User Restoration**: restoreUserToken, restoreUserTokenExpiry
- **Refresh Tokens**: refreshToken, refreshTokenExpiry
- **Failed Login Tracking**: failedLoginAttempts, lastFailedLoginAttempt
- **Session Management**: Session table with expiry tracking
- **Soft Deletes**: deleted flag in User and Item tables

#### E-commerce Features
- **Hierarchical Categories**: Categories can have parent categories
- **Product Variants**: Multiple variants per item with SKU, price, stock
- **Offers/Discounts**: ItemVariant.Offer (percentage), OfferStart, OfferEnd
- **Custom Attributes**: Flexible attribute system for items and variants
- **Order Tracking**: OrderStatus and OrderItemStatus lookup tables
- **Multiple Addresses**: Users can have multiple addresses (delivery, billing, company)
- **Payment Methods**: Store multiple payment methods per user
- **Tax Calculation**: TaxRate table with rates by country/province

### Indexes

The script creates indexes on:
- All foreign key columns for join performance
- Email lookups (User.email)
- Token lookups (emailValidationToken, passwordResetToken, restoreUserToken, refreshToken)
- Order date (Order.OrderDate)
- Active/status flags (PaymentMethod.IsActive, TaxRate.IsActive, Item.Deleted)
- Type columns (Address.AddressType, OrderAddress.Type)

### Auto-Generated Values

- **Primary Keys**: All tables use UNIQUEIDENTIFIER (GUID) with NEWID() default
- **Timestamps**: CreatedAt defaults to GETUTCDATE()
- **OrderNumber**: Uses SQL Server SEQUENCE (OrderNumberSequence) for auto-increment
- **Lookup IDs**: OrderStatus.ID and OrderItemStatus.ID use IDENTITY(1,1)

### Constraints

#### Unique Constraints
- User.email (UQ_User_Email)
- Order.OrderNumber (UQ_Order_OrderNumber)
- OrderStatus.StatusCode (UQ_OrderStatus_StatusCode)
- OrderItemStatus.StatusCode (UQ_OrderItemStatus_StatusCode)

#### Check Constraints
- ItemVariant.Offer: Must be between 0 and 100 (percentage)

#### Foreign Keys
18 foreign key relationships ensure referential integrity across tables.

### Default/Seed Data

The script populates lookup tables with initial values:

**OrderStatus:**
- PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED, REFUNDED

**OrderItemStatus:**
- PENDING, PROCESSING, SHIPPED, DELIVERED, ON_HOLD, CANCELLED, REFUNDED

## Common Queries

### Get all active user sessions
```sql
SELECT * FROM dbo.Session 
WHERE LoggedOutAt IS NULL 
  AND ExpiresAt > GETUTCDATE()
```

### Get items with offers
```sql
SELECT i.*, iv.* 
FROM dbo.Item i
INNER JOIN dbo.ItemVariant iv ON i.Id = iv.ItemId
WHERE iv.Offer IS NOT NULL
  AND iv.OfferStart <= GETUTCDATE()
  AND iv.OfferEnd >= GETUTCDATE()
  AND i.Deleted = 0
```

### Get order with all details
```sql
SELECT o.*, u.Email, os.Name_en as StatusName
FROM dbo.[Order] o
INNER JOIN dbo.[User] u ON o.UserID = u.id
INNER JOIN dbo.OrderStatus os ON o.StatusID = os.ID
WHERE o.ID = @OrderId
```

### Get user's default address
```sql
SELECT * FROM dbo.Address
WHERE UserId = @UserId AND IsDefault = 1
```

## Maintenance

### Add a column to a table
Always update BOTH the base schema script AND create a migration:

1. Update `000_Create_Database_Schema.sql`
2. Create new migration `00X_Description.sql`
3. Update `Migrations/README.md`

### Backup database
```sql
BACKUP DATABASE CanoEh 
TO DISK = 'C:\Backups\CanoEh_backup.bak'
WITH FORMAT, INIT, NAME = 'Full CanoEh Backup'
```

### Check database size
```sql
SELECT 
    DB_NAME() AS DatabaseName,
    SUM(size * 8 / 1024) AS SizeMB
FROM sys.master_files
WHERE database_id = DB_ID('CanoEh')
GROUP BY database_id
```
