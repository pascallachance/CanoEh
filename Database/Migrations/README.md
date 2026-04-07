# Database Migrations

This directory contains SQL migration scripts for the CanoEh database.

## Initial Database Setup

Before applying any migrations, you must first create the complete database schema using the base script:

### Create the Database Schema (First Time Setup)
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -i "Database/000_Create_Database_Schema.sql"
```

This script (`000_Create_Database_Schema.sql`) is located in the parent `Database/` directory and creates:
- The CanoEh database
- All tables with proper relationships and constraints
- All indexes for optimal performance
- Lookup tables with initial data (OrderStatus, OrderItemStatus)

**Important:** This script is idempotent and can be run multiple times safely. It will only create objects that don't already exist.

## How to Apply Migrations

After the initial database schema is created, migrations should be applied in order by their numeric prefix (001, 002, 003, etc.).

### Using SQL Server Management Studio (SSMS) or Azure Data Studio:
1. Connect to your SQL Server instance
2. Open the migration script file
3. Execute the script against your CanoEh database

### Using sqlcmd command line:
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/001_Add_Language_Column_To_User.sql"
```

## Migration History

| ID | File | Description | Date |
|----|------|-------------|------|
| 000 | 000_Create_Database_Schema.sql | Creates complete CanoEh database schema with all tables, relationships, and indexes | 2026-01-15 |
| 001 | 001_Add_Language_Column_To_User.sql | Adds Language column to User table for multilingual email support | 2025-12-09 |
| 002 | 002_Add_Email_Website_Columns_To_Company.sql | Adds Email (required) and WebSite (optional) columns to Company table | 2025-12-19 |
| 003 | 003_Add_Failed_Login_Tracking_To_User.sql | Adds FailedLoginAttempts and LastFailedLoginAttempt columns to prevent brute force attacks | 2026-01-06 |
| 004 | 004_Add_CategoryNode_Table.sql | Adds CategoryNode table for hierarchical category structure (Departement, Navigation, Category nodes) | 2026-01-28 |
| 005 | 005_Add_CategoryMandatoryAttribute_Table.sql | Adds CategoryMandatoryAttribute table for category-specific mandatory product attributes | 2026-01-28 |
| 006 | 006_Add_CategoryMandatoryExtraAttribute_Table.sql | Adds CategoryMandatoryExtraAttribute table for mandatory extra attributes (SKU, Dimensions, etc.) on item variants per category | 2026-01-28 |
| 007 | 007_Refactor_ItemAttribute_And_CategoryMandatoryAttribute.sql | Moves ItemAttribute FK from Item to ItemVariant, renames ItemAttribute→ItemVariantFeatures and CategoryMandatoryAttribute→CategoryMandatoryFeature, drops ItemVariantExtraAttribute | 2026-02-04 |
| 008 | 008_Rename_CategoryID_To_CategoryNodeID.sql | Renames CategoryID to CategoryNodeID on the Item table and migrates the FK from Category to CategoryNode | 2026-02-04 |
| 009 | 009_Drop_Category_Table.sql | Drops the old Category table (now replaced by the CategoryNode hierarchy) | 2026-02-04 |
| 010 | 010_Add_ItemID_To_ItemVariantFeatures.sql | Adds nullable ItemID column to ItemVariantFeatures to allow item-level feature queries without a join | 2026-02-20 |
| 011 | 011_Add_Refresh_Token_To_User.sql | Adds refreshToken and refreshTokenExpiry columns to User table for JWT refresh token support | 2026-02-23 |
| 012 | 012_Add_Offer_Columns_To_ItemVariant.sql | Adds Offer, OfferStart, and OfferEnd columns to ItemVariant for the Manage Offers feature | 2026-03-19 |
| 013 | 013_Add_ItemVariantAttribute_Table.sql | Creates ItemVariantAttribute table on databases provisioned before it was added to the base schema | 2026-03-19 |
| 014 | 014_Expand_Item_Name_Columns.sql | Expands Item.Name_en, Item.Name_fr, OrderItem.Name_en, and OrderItem.Name_fr to NVARCHAR(MAX) to support long product names (fixes 500 error on UpdateItem) | 2026-03-31 |
| 015 | 015_Expand_Item_Text_Columns_To_2000.sql | Alters Name_en, Name_fr, Description_en, and Description_fr on dbo.Item to NVARCHAR(2000) for sellers who need up to 2000 characters | 2026-03-31 |
| 016 | 016_Narrow_Item_Name_Columns_To_300.sql | Narrows Item.Name_en and Item.Name_fr back to NVARCHAR(300); aborts if any existing rows would be truncated | 2026-04-02 |
| 017 | 017_Add_IsMain_To_ItemVariantAttribute.sql | Adds IsMain BIT NOT NULL DEFAULT 0 to dbo.ItemVariantAttribute and backfills existing rows so every ItemVariantID has exactly one IsMain=1 attribute | 2026-04-07 |

## Notes

- Each migration script is idempotent (safe to run multiple times)
- Scripts check for existing schema changes before applying them
- The base schema script (000) should be run first on a new database
- Migrations (001, 002, 003, etc.) are incremental changes to the base schema
- Always maintain the base schema script (000) when making structural changes to the database
