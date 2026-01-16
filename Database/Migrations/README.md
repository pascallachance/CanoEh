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

## Notes

- Each migration script is idempotent (safe to run multiple times)
- Scripts check for existing schema changes before applying them
- The base schema script (000) should be run first on a new database
- Migrations (001, 002, 003, etc.) are incremental changes to the base schema
- Always maintain the base schema script (000) when making structural changes to the database
