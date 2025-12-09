# Database Migrations

This directory contains SQL migration scripts for the CanoEh database.

## How to Apply Migrations

Migrations should be applied in order by their numeric prefix (001, 002, etc.).

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
| 001 | 001_Add_Language_Column_To_User.sql | Adds Language column to User table for multilingual email support | 2025-12-09 |

## Notes

- Each migration script is idempotent (safe to run multiple times)
- Scripts check for existing schema changes before applying them
- Default language is set to 'en' (English) for existing users
