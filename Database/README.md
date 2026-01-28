# CanoEh Database

This directory contains all database-related scripts for the CanoEh e-commerce application.

## Directory Structure

```
Database/
├── 000_Create_Database_Schema.sql    # Complete database creation script
├── Validate_Database_Schema.sql      # Validation script to verify schema
├── README.md                          # This file
├── QUICK_REFERENCE.md                 # Quick reference guide for tables and queries
└── Migrations/                        # Incremental migration scripts
    ├── 001_Add_Language_Column_To_User.sql
    ├── 002_Add_Email_Website_Columns_To_Company.sql
    ├── 003_Add_Failed_Login_Tracking_To_User.sql
    ├── 004_Add_CategoryNode_Table.sql
    ├── 005_Add_CategoryMandatoryAttribute_Table.sql
    └── README.md
```

## Database Overview

The CanoEh database is designed for a bilingual (English/French) e-commerce platform running on SQL Server.

### Key Features
- **Bilingual Support**: Most content tables include `_en` and `_fr` columns
- **User Management**: Authentication, email validation, password recovery, and session tracking
- **E-commerce**: Products (Items), categories, variants, orders, and payments
- **Security**: Failed login attempt tracking, token-based authentication, and soft deletes

### Database Tables

#### User & Authentication
- **User**: User accounts with authentication and profile information
- **Session**: User login sessions with expiry tracking

#### Company & Seller
- **Company**: Seller/company information with KYC details
- **Address**: User and company addresses (delivery, billing)
- **PaymentMethod**: Stored payment methods for users

#### Catalog
- **Category**: Hierarchical product categories (legacy)
- **CategoryNode**: Hierarchical product structure (Departement, Navigation, Category nodes)
- **CategoryMandatoryAttribute**: Mandatory attributes for category nodes
- **Item**: Products with bilingual names and descriptions
- **ItemVariant**: Product variants (SKU, price, stock, offers)
- **ItemAttribute**: Custom attributes for items
- **ItemVariantAttribute**: Custom attributes for variants

#### Orders
- **Order**: Customer orders with totals and status
- **OrderItem**: Individual items in an order
- **OrderAddress**: Shipping and billing addresses for orders
- **OrderPayment**: Payment information for orders
- **OrderStatus**: Lookup table for order statuses
- **OrderItemStatus**: Lookup table for order item statuses

#### Tax
- **TaxRate**: Tax rates by country and province/state

## Getting Started

### First Time Setup

1. **Create the database and all tables:**
   ```bash
   sqlcmd -S (localdb)\MSSQLLocalDB -i "Database/000_Create_Database_Schema.sql"
   ```

2. **Verify the database was created successfully:**
   ```bash
   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Validate_Database_Schema.sql"
   ```
   This validation script will check that all tables, indexes, constraints, and seed data were created correctly.

3. **Apply migration scripts** (if updating an existing database):
   ```bash
   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/001_Add_Language_Column_To_User.sql"
   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/002_Add_Email_Website_Columns_To_Company.sql"
   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/003_Add_Failed_Login_Tracking_To_User.sql"
   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/004_Add_CategoryNode_Table.sql"
   sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/005_Add_CategoryMandatoryAttribute_Table.sql"
   ```

### Using SQL Server Management Studio (SSMS) or Azure Data Studio

1. Connect to `(localdb)\MSSQLLocalDB`
2. Open `Database/000_Create_Database_Schema.sql`
3. Execute the script
4. Apply migration scripts in order from the `Migrations/` directory

## Database Maintenance

### When Making Schema Changes

When you modify the database schema during development, you must maintain **both**:

1. **Update the base schema script** (`000_Create_Database_Schema.sql`)
   - Update the relevant table definition
   - Ensure the script remains idempotent
   - Add appropriate indexes and constraints

2. **Create a new migration script** (`00X_Description.sql` in `Migrations/`)
   - Create a new numbered migration file
   - Write ALTER statements to modify existing databases
   - Make it idempotent (safe to run multiple times)
   - Update `Migrations/README.md` with the new migration entry

### Example: Adding a New Column

If you need to add a new column `PreferredLanguage` to the `User` table:

1. **Update** `000_Create_Database_Schema.sql`:
   ```sql
   -- In the CREATE TABLE dbo.[User] section, add:
   PreferredLanguage NVARCHAR(10) NULL,
   ```

2. **Create** `Database/Migrations/004_Add_PreferredLanguage_To_User.sql`:
   ```sql
   -- Migration: Add PreferredLanguage column to User table
   -- Date: <YYYY-MM-DD>  -- Replace with the actual migration date
   -- Description: Adds PreferredLanguage column for UI language preference
   
   IF NOT EXISTS (SELECT * FROM sys.columns 
                  WHERE object_id = OBJECT_ID(N'dbo.[User]') 
                  AND name = 'PreferredLanguage')
   BEGIN
       ALTER TABLE dbo.[User]
       ADD PreferredLanguage NVARCHAR(10) NULL;
       
       PRINT 'PreferredLanguage column added to User table';
   END
   ELSE
   BEGIN
       PRINT 'PreferredLanguage column already exists in User table';
   END
   GO
   ```

3. **Update** `Migrations/README.md` with the new migration entry

### Best Practices

- ✅ **Always** make scripts idempotent (check for existence before creating/altering)
- ✅ **Always** update both the base schema and create a migration
- ✅ **Always** test scripts on a development database first
- ✅ **Always** use transactions where appropriate
- ✅ **Always** document the purpose of each migration
- ❌ **Never** modify existing migration scripts that have been applied to production
- ❌ **Never** delete or drop tables without careful consideration
- ❌ **Never** remove columns that might contain important data

## Connection String

The default connection string for local development:
```
Server=(localdb)\MSSQLLocalDB;Database=CanoEh;Trusted_Connection=True;
```

This is configured in `API/appsettings.json` under `ConnectionStrings:DefaultConnection`.

## Troubleshooting

### Database doesn't exist
Run the base schema script: `Database/000_Create_Database_Schema.sql`

### Table already exists errors
The scripts are idempotent - these are just informational messages. The scripts check for existing objects before creating them.

### Migration applied to wrong database
Always specify the database with `-d CanoEh` when using sqlcmd, or ensure you've selected the correct database in SSMS/Azure Data Studio.

### Cannot connect to LocalDB
Ensure SQL Server LocalDB is installed:
```bash
SqlLocalDB info
SqlLocalDB start MSSQLLocalDB
```

## Additional Resources

- [SQL Server LocalDB Documentation](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Dapper Documentation](https://github.com/DapperLib/Dapper) (ORM used by CanoEh)
- [T-SQL Reference](https://docs.microsoft.com/en-us/sql/t-sql/language-reference)
