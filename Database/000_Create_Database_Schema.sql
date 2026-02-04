-- =============================================
-- CanoEh Database Creation Script
-- =============================================
-- This script creates the complete CanoEh database schema
-- including all tables, indexes, and constraints.
-- 
-- This script is idempotent and can be run multiple times safely.
-- It will only create objects that don't already exist.
--
-- Usage:
--   sqlcmd -S (localdb)\MSSQLLocalDB -i "Database/000_Create_Database_Schema.sql"
--
-- Or using SQL Server Management Studio / Azure Data Studio:
--   1. Connect to your SQL Server instance
--   2. Open this script
--   3. Execute against master database (for database creation)
--      or execute against CanoEh database (for table creation only)
-- =============================================

-- =============================================
-- Create Database
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CanoEh')
BEGIN
    CREATE DATABASE CanoEh;
    PRINT 'Database CanoEh created successfully.';
END
ELSE
BEGIN
    PRINT 'Database CanoEh already exists.';
END
GO

USE CanoEh;
GO

-- =============================================
-- Create User Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'User' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.[User] (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        email NVARCHAR(255) NOT NULL,
        firstname NVARCHAR(100) NOT NULL,
        lastname NVARCHAR(100) NOT NULL,
        phone NVARCHAR(20) NULL,
        language NVARCHAR(10) NOT NULL DEFAULT 'en',
        lastlogin DATETIME2 NULL,
        lastlogout DATETIME2 NULL,
        createdat DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        lastupdatedat DATETIME2 NULL,
        password NVARCHAR(255) NOT NULL,
        deleted BIT NOT NULL DEFAULT 0,
        validemail BIT NOT NULL DEFAULT 0,
        emailValidationToken NVARCHAR(255) NULL,
        passwordResetToken NVARCHAR(255) NULL,
        passwordResetTokenExpiry DATETIME2 NULL,
        restoreUserToken NVARCHAR(255) NULL,
        restoreUserTokenExpiry DATETIME2 NULL,
        refreshToken NVARCHAR(500) NULL,
        refreshTokenExpiry DATETIME2 NULL,
        failedLoginAttempts INT NOT NULL DEFAULT 0,
        lastFailedLoginAttempt DATETIME2 NULL,
        CONSTRAINT UQ_User_Email UNIQUE (email)
    );
    
    CREATE INDEX IX_User_Email ON dbo.[User](email);
    CREATE INDEX IX_User_EmailValidationToken ON dbo.[User](emailValidationToken) WHERE emailValidationToken IS NOT NULL;
    CREATE INDEX IX_User_PasswordResetToken ON dbo.[User](passwordResetToken) WHERE passwordResetToken IS NOT NULL;
    CREATE INDEX IX_User_RestoreUserToken ON dbo.[User](restoreUserToken) WHERE restoreUserToken IS NOT NULL;
    CREATE INDEX IX_User_RefreshToken ON dbo.[User](refreshToken) WHERE refreshToken IS NOT NULL;
    
    PRINT 'Table [User] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [User] already exists.';
END
GO

-- =============================================
-- Create Session Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Session' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Session (
        SessionId UNIQUEIDENTIFIER PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LoggedOutAt DATETIME2 NULL,
        ExpiresAt DATETIME2 NOT NULL,
        UserAgent NVARCHAR(500) NULL,
        IpAddress NVARCHAR(45) NULL,
        CONSTRAINT FK_Session_User FOREIGN KEY (UserId) REFERENCES dbo.[User](id)
    );
    
    CREATE INDEX IX_Session_UserId ON dbo.Session(UserId);
    CREATE INDEX IX_Session_ExpiresAt ON dbo.Session(ExpiresAt);
    
    PRINT 'Table Session created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Session already exists.';
END
GO

-- =============================================
-- Create Company Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Company' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Company (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OwnerID UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Logo NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CountryOfCitizenship NVARCHAR(100) NULL,
        FullBirthName NVARCHAR(255) NULL,
        CountryOfBirth NVARCHAR(100) NULL,
        BirthDate DATETIME2 NULL,
        IdentityDocumentType NVARCHAR(50) NULL,
        IdentityDocument NVARCHAR(500) NULL,
        BankDocument NVARCHAR(500) NULL,
        FacturationDocument NVARCHAR(500) NULL,
        CompanyPhone NVARCHAR(20) NULL,
        CompanyType NVARCHAR(100) NULL,
        Email NVARCHAR(255) NULL,
        WebSite NVARCHAR(500) NULL,
        Address1 NVARCHAR(255) NULL,
        Address2 NVARCHAR(255) NULL,
        Address3 NVARCHAR(255) NULL,
        City NVARCHAR(100) NULL,
        ProvinceState NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        CONSTRAINT FK_Company_User FOREIGN KEY (OwnerID) REFERENCES dbo.[User](id)
    );
    
    CREATE INDEX IX_Company_OwnerID ON dbo.Company(OwnerID);
    
    PRINT 'Table Company created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Company already exists.';
END
GO

-- =============================================
-- Create Address Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Address' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Address (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        FullName NVARCHAR(255) NOT NULL,
        AddressLine1 NVARCHAR(255) NOT NULL,
        AddressLine2 NVARCHAR(255) NULL,
        AddressLine3 NVARCHAR(255) NULL,
        City NVARCHAR(100) NOT NULL,
        ProvinceState NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NOT NULL,
        Country NVARCHAR(100) NOT NULL,
        AddressType NVARCHAR(50) NOT NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Address_User FOREIGN KEY (UserId) REFERENCES dbo.[User](id)
    );
    
    CREATE INDEX IX_Address_UserId ON dbo.Address(UserId);
    CREATE INDEX IX_Address_AddressType ON dbo.Address(AddressType);
    
    PRINT 'Table Address created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Address already exists.';
END
GO

-- =============================================
-- Create PaymentMethod Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentMethod' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.PaymentMethod (
        ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserID UNIQUEIDENTIFIER NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        CardHolderName NVARCHAR(255) NULL,
        CardLast4 NVARCHAR(4) NULL,
        CardBrand NVARCHAR(50) NULL,
        ExpirationMonth INT NULL,
        ExpirationYear INT NULL,
        BillingAddress NVARCHAR(MAX) NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_PaymentMethod_User FOREIGN KEY (UserID) REFERENCES dbo.[User](id)
    );
    
    CREATE INDEX IX_PaymentMethod_UserID ON dbo.PaymentMethod(UserID);
    CREATE INDEX IX_PaymentMethod_IsActive ON dbo.PaymentMethod(IsActive);
    
    PRINT 'Table PaymentMethod created successfully.';
END
ELSE
BEGIN
    PRINT 'Table PaymentMethod already exists.';
END
GO

-- =============================================
-- Create Category Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Category' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Category (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name_en NVARCHAR(255) NOT NULL,
        Name_fr NVARCHAR(255) NOT NULL,
        ParentCategoryId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Category_ParentCategory FOREIGN KEY (ParentCategoryId) REFERENCES dbo.Category(Id)
    );
    
    CREATE INDEX IX_Category_ParentCategoryId ON dbo.Category(ParentCategoryId);
    
    PRINT 'Table Category created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Category already exists.';
END
GO

-- =============================================
-- Create Item Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Item' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Item (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        SellerID UNIQUEIDENTIFIER NOT NULL,
        Name_en NVARCHAR(255) NOT NULL,
        Name_fr NVARCHAR(255) NOT NULL,
        Description_en NVARCHAR(MAX) NULL,
        Description_fr NVARCHAR(MAX) NULL,
        CategoryID UNIQUEIDENTIFIER NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        Deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Item_Seller FOREIGN KEY (SellerID) REFERENCES dbo.[User](id),
        CONSTRAINT FK_Item_Category FOREIGN KEY (CategoryID) REFERENCES dbo.Category(Id)
    );
    
    CREATE INDEX IX_Item_SellerID ON dbo.Item(SellerID);
    CREATE INDEX IX_Item_CategoryID ON dbo.Item(CategoryID);
    CREATE INDEX IX_Item_Deleted ON dbo.Item(Deleted);
    
    PRINT 'Table Item created successfully.';
END
ELSE
BEGIN
    PRINT 'Table Item already exists.';
END
GO

-- =============================================
-- Create ItemVariant Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemVariant' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ItemVariant (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ItemId UNIQUEIDENTIFIER NOT NULL,
        Price DECIMAL(18, 2) NOT NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        Sku NVARCHAR(100) NOT NULL,
        ProductIdentifierType NVARCHAR(50) NULL,
        ProductIdentifierValue NVARCHAR(100) NULL,
        ImageUrls NVARCHAR(MAX) NULL,
        ThumbnailUrl NVARCHAR(500) NULL,
        ItemVariantName_en NVARCHAR(255) NULL,
        ItemVariantName_fr NVARCHAR(255) NULL,
        Deleted BIT NOT NULL DEFAULT 0,
        Offer DECIMAL(5, 2) NULL,
        OfferStart DATETIME2 NULL,
        OfferEnd DATETIME2 NULL,
        CONSTRAINT FK_ItemVariant_Item FOREIGN KEY (ItemId) REFERENCES dbo.Item(Id),
        CONSTRAINT CK_ItemVariant_Offer CHECK (Offer IS NULL OR (Offer >= 0 AND Offer <= 100))
    );
    
    CREATE INDEX IX_ItemVariant_ItemId ON dbo.ItemVariant(ItemId);
    CREATE INDEX IX_ItemVariant_Sku ON dbo.ItemVariant(Sku);
    CREATE INDEX IX_ItemVariant_Deleted ON dbo.ItemVariant(Deleted);
    
    PRINT 'Table ItemVariant created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ItemVariant already exists.';
END
GO

-- =============================================
-- Create ItemVariantFeatures Table (renamed from ItemAttribute)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemVariantFeatures' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ItemVariantFeatures (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ItemVariantID UNIQUEIDENTIFIER NOT NULL,
        AttributeName_en NVARCHAR(255) NOT NULL,
        AttributeName_fr NVARCHAR(255) NULL,
        Attributes_en NVARCHAR(MAX) NOT NULL,
        Attributes_fr NVARCHAR(MAX) NULL,
        CONSTRAINT FK_ItemVariantFeatures_ItemVariant FOREIGN KEY (ItemVariantID) REFERENCES dbo.ItemVariant(Id)
    );
    
    CREATE INDEX IX_ItemVariantFeatures_ItemVariantID ON dbo.ItemVariantFeatures(ItemVariantID);
    
    PRINT 'Table ItemVariantFeatures created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ItemVariantFeatures already exists.';
END
GO

-- =============================================
-- Create ItemVariantAttribute Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemVariantAttribute' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.ItemVariantAttribute (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ItemVariantID UNIQUEIDENTIFIER NOT NULL,
        AttributeName_en NVARCHAR(255) NOT NULL,
        AttributeName_fr NVARCHAR(255) NULL,
        Attributes_en NVARCHAR(MAX) NOT NULL,
        Attributes_fr NVARCHAR(MAX) NULL,
        CONSTRAINT FK_ItemVariantAttribute_ItemVariant FOREIGN KEY (ItemVariantID) REFERENCES dbo.ItemVariant(Id)
    );
    
    CREATE INDEX IX_ItemVariantAttribute_ItemVariantID ON dbo.ItemVariantAttribute(ItemVariantID);
    
    PRINT 'Table ItemVariantAttribute created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ItemVariantAttribute already exists.';
END
GO

-- =============================================
-- Create OrderStatus Table (Lookup Table)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderStatus' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrderStatus (
        ID INT PRIMARY KEY IDENTITY(1,1),
        StatusCode NVARCHAR(50) NOT NULL,
        Name_en NVARCHAR(100) NOT NULL,
        Name_fr NVARCHAR(100) NOT NULL,
        CONSTRAINT UQ_OrderStatus_StatusCode UNIQUE (StatusCode)
    );
    
    -- Insert default order statuses
    INSERT INTO dbo.OrderStatus (StatusCode, Name_en, Name_fr) VALUES
        ('PENDING', 'Pending', 'En attente'),
        ('PROCESSING', 'Processing', 'En traitement'),
        ('SHIPPED', 'Shipped', 'Expédié'),
        ('DELIVERED', 'Delivered', 'Livré'),
        ('CANCELLED', 'Cancelled', 'Annulé'),
        ('REFUNDED', 'Refunded', 'Remboursé');
    
    PRINT 'Table OrderStatus created and populated successfully.';
END
ELSE
BEGIN
    PRINT 'Table OrderStatus already exists.';
END
GO

-- =============================================
-- Create OrderItemStatus Table (Lookup Table)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItemStatus' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrderItemStatus (
        ID INT PRIMARY KEY IDENTITY(1,1),
        StatusCode NVARCHAR(50) NOT NULL,
        Name_en NVARCHAR(100) NOT NULL,
        Name_fr NVARCHAR(100) NOT NULL,
        CONSTRAINT UQ_OrderItemStatus_StatusCode UNIQUE (StatusCode)
    );
    
    -- Insert default order item statuses
    INSERT INTO dbo.OrderItemStatus (StatusCode, Name_en, Name_fr) VALUES
        ('PENDING', 'Pending', 'En attente'),
        ('PROCESSING', 'Processing', 'En traitement'),
        ('SHIPPED', 'Shipped', 'Expédié'),
        ('DELIVERED', 'Delivered', 'Livré'),
        ('ON_HOLD', 'On Hold', 'En suspens'),
        ('CANCELLED', 'Cancelled', 'Annulé'),
        ('REFUNDED', 'Refunded', 'Remboursé');
    
    PRINT 'Table OrderItemStatus created and populated successfully.';
END
ELSE
BEGIN
    PRINT 'Table OrderItemStatus already exists.';
END
GO

-- =============================================
-- Create Order Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Order' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.[Order] (
        ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserID UNIQUEIDENTIFIER NOT NULL,
        OrderNumber INT NOT NULL,
        OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        StatusID INT NOT NULL,
        Subtotal DECIMAL(18, 2) NOT NULL,
        TaxTotal DECIMAL(18, 2) NOT NULL,
        ShippingTotal DECIMAL(18, 2) NOT NULL,
        GrandTotal DECIMAL(18, 2) NOT NULL,
        Notes NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_Order_User FOREIGN KEY (UserID) REFERENCES dbo.[User](id),
        CONSTRAINT FK_Order_OrderStatus FOREIGN KEY (StatusID) REFERENCES dbo.OrderStatus(ID),
        CONSTRAINT UQ_Order_OrderNumber UNIQUE (OrderNumber)
    );
    
    CREATE INDEX IX_Order_UserID ON dbo.[Order](UserID);
    CREATE INDEX IX_Order_StatusID ON dbo.[Order](StatusID);
    CREATE INDEX IX_Order_OrderDate ON dbo.[Order](OrderDate);
    
    -- Create a sequence for OrderNumber auto-increment
    IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'OrderNumberSequence')
    BEGIN
        CREATE SEQUENCE dbo.OrderNumberSequence
            START WITH 1
            INCREMENT BY 1;
        
        -- Set default constraint to use the sequence
        ALTER TABLE dbo.[Order]
            ADD CONSTRAINT DF_Order_OrderNumber DEFAULT (NEXT VALUE FOR dbo.OrderNumberSequence) FOR OrderNumber;
        
        PRINT 'Sequence OrderNumberSequence created and applied to Order table.';
    END
    
    PRINT 'Table [Order] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [Order] already exists.';
END
GO

-- =============================================
-- Create OrderItem Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItem' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrderItem (
        ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OrderID UNIQUEIDENTIFIER NOT NULL,
        ItemID UNIQUEIDENTIFIER NOT NULL,
        ItemVariantID UNIQUEIDENTIFIER NOT NULL,
        Name_en NVARCHAR(255) NOT NULL,
        Name_fr NVARCHAR(255) NOT NULL,
        VariantName_en NVARCHAR(255) NULL,
        VariantName_fr NVARCHAR(255) NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18, 2) NOT NULL,
        TotalPrice DECIMAL(18, 2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        DeliveredAt DATETIME2 NULL,
        OnHoldReason NVARCHAR(MAX) NULL,
        CONSTRAINT FK_OrderItem_Order FOREIGN KEY (OrderID) REFERENCES dbo.[Order](ID),
        CONSTRAINT FK_OrderItem_Item FOREIGN KEY (ItemID) REFERENCES dbo.Item(Id),
        CONSTRAINT FK_OrderItem_ItemVariant FOREIGN KEY (ItemVariantID) REFERENCES dbo.ItemVariant(Id)
    );
    
    CREATE INDEX IX_OrderItem_OrderID ON dbo.OrderItem(OrderID);
    CREATE INDEX IX_OrderItem_ItemID ON dbo.OrderItem(ItemID);
    CREATE INDEX IX_OrderItem_ItemVariantID ON dbo.OrderItem(ItemVariantID);
    
    PRINT 'Table OrderItem created successfully.';
END
ELSE
BEGIN
    PRINT 'Table OrderItem already exists.';
END
GO

-- =============================================
-- Create OrderAddress Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderAddress' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrderAddress (
        ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OrderID UNIQUEIDENTIFIER NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        FullName NVARCHAR(255) NOT NULL,
        AddressLine1 NVARCHAR(255) NOT NULL,
        AddressLine2 NVARCHAR(255) NULL,
        AddressLine3 NVARCHAR(255) NULL,
        City NVARCHAR(100) NOT NULL,
        ProvinceState NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NOT NULL,
        Country NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_OrderAddress_Order FOREIGN KEY (OrderID) REFERENCES dbo.[Order](ID)
    );
    
    CREATE INDEX IX_OrderAddress_OrderID ON dbo.OrderAddress(OrderID);
    CREATE INDEX IX_OrderAddress_Type ON dbo.OrderAddress(Type);
    
    PRINT 'Table OrderAddress created successfully.';
END
ELSE
BEGIN
    PRINT 'Table OrderAddress already exists.';
END
GO

-- =============================================
-- Create OrderPayment Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderPayment' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrderPayment (
        ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OrderID UNIQUEIDENTIFIER NOT NULL,
        PaymentMethodID UNIQUEIDENTIFIER NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Provider NVARCHAR(100) NOT NULL,
        ProviderReference NVARCHAR(255) NULL,
        PaidAt DATETIME2 NULL,
        CONSTRAINT FK_OrderPayment_Order FOREIGN KEY (OrderID) REFERENCES dbo.[Order](ID),
        CONSTRAINT FK_OrderPayment_PaymentMethod FOREIGN KEY (PaymentMethodID) REFERENCES dbo.PaymentMethod(ID)
    );
    
    CREATE INDEX IX_OrderPayment_OrderID ON dbo.OrderPayment(OrderID);
    CREATE INDEX IX_OrderPayment_PaymentMethodID ON dbo.OrderPayment(PaymentMethodID);
    
    PRINT 'Table OrderPayment created successfully.';
END
ELSE
BEGIN
    PRINT 'Table OrderPayment already exists.';
END
GO

-- =============================================
-- Create TaxRate Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaxRate' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.TaxRate (
        ID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name_en NVARCHAR(255) NOT NULL,
        Name_fr NVARCHAR(255) NOT NULL,
        Country NVARCHAR(100) NOT NULL,
        ProvinceState NVARCHAR(100) NULL,
        Rate DECIMAL(5, 4) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );
    
    CREATE INDEX IX_TaxRate_Country ON dbo.TaxRate(Country);
    CREATE INDEX IX_TaxRate_ProvinceState ON dbo.TaxRate(ProvinceState);
    CREATE INDEX IX_TaxRate_IsActive ON dbo.TaxRate(IsActive);
    
    PRINT 'Table TaxRate created successfully.';
END
ELSE
BEGIN
    PRINT 'Table TaxRate already exists.';
END
GO

-- =============================================
-- Create CategoryNode Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryNode' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CategoryNode (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name_en NVARCHAR(200) NOT NULL,
        Name_fr NVARCHAR(200) NOT NULL,
        NodeType NVARCHAR(32) NOT NULL, -- 'Departement', 'Navigation', 'Category'
        ParentId UNIQUEIDENTIFIER NULL, -- Self-reference to parent node
        -- Example: for CategoryNode, ParentId points to a NavigationNode or DepartementNode
        --          for NavigationNode, ParentId points to a DepartementNode or another NavigationNode
        --          for DepartementNode, ParentId is NULL (root)
        IsActive BIT NOT NULL DEFAULT 1,
        SortOrder INT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        CONSTRAINT FK_CategoryNode_Parent FOREIGN KEY (ParentId) REFERENCES dbo.CategoryNode(Id),
        CONSTRAINT CK_CategoryNode_NodeType CHECK (NodeType IN ('Departement', 'Navigation', 'Category'))
    );
    
    -- Indexes for performance
    CREATE INDEX IX_CategoryNode_ParentId ON dbo.CategoryNode(ParentId);
    CREATE INDEX IX_CategoryNode_NodeType ON dbo.CategoryNode(NodeType);
    CREATE INDEX IX_CategoryNode_IsActive ON dbo.CategoryNode(IsActive);
    CREATE INDEX IX_CategoryNode_SortOrder ON dbo.CategoryNode(SortOrder);
    
    PRINT 'Table CategoryNode created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CategoryNode already exists.';
END
GO

-- =============================================
-- Create CategoryMandatoryFeature Table (renamed from CategoryMandatoryAttribute)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CategoryMandatoryFeature' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CategoryMandatoryFeature (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CategoryNodeId UNIQUEIDENTIFIER NOT NULL, -- FK to CategoryNode(Id), must be a CategoryNode
        Name_en NVARCHAR(100) NOT NULL,
        Name_fr NVARCHAR(100) NOT NULL,
        AttributeType NVARCHAR(50) NULL, -- e.g., 'string', 'int', 'enum', etc. (optional)
        SortOrder INT NULL,
        CONSTRAINT FK_CategoryMandatoryFeature_CategoryNode
            FOREIGN KEY (CategoryNodeId) REFERENCES dbo.CategoryNode(Id) ON DELETE CASCADE
    );
    
    -- Indexes for performance
    CREATE INDEX IX_CategoryMandatoryFeature_CategoryNodeId ON dbo.CategoryMandatoryFeature(CategoryNodeId);
    CREATE INDEX IX_CategoryMandatoryFeature_SortOrder ON dbo.CategoryMandatoryFeature(SortOrder);
    
    PRINT 'Table CategoryMandatoryFeature created successfully.';
END
ELSE
BEGIN
    PRINT 'Table CategoryMandatoryFeature already exists.';
END
GO

-- =============================================
-- Final Message
-- =============================================
PRINT '';
PRINT '=============================================';
PRINT 'CanoEh Database Schema Creation Complete';
PRINT '=============================================';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Review the created tables and indexes';
PRINT '2. Apply any migration scripts in order (001, 002, 003, etc.)';
PRINT '3. Note: OrderStatus and OrderItemStatus tables are already populated with initial data';
PRINT '';
GO
