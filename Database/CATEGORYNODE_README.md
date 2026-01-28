# CategoryNode Hierarchy Implementation

This document describes the CategoryNode hierarchy implementation for the CanoEh e-commerce application.

## Overview

The CategoryNode hierarchy is a flexible, multi-level categorization system for organizing product categories. It replaces the simpler Category table with a more robust structure that supports:

- **Departement Nodes**: Root-level organizational units (e.g., "Electronics", "Clothing")
- **Navigation Nodes**: Intermediate grouping levels for organizing categories (e.g., "Home Audio", "Portable Audio")
- **Category Nodes**: Leaf-level categories that can be assigned to products (e.g., "Speakers", "Headphones")

## Database Schema

### CategoryNode Table

```sql
CREATE TABLE dbo.CategoryNode (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name_en NVARCHAR(200) NOT NULL,
    Name_fr NVARCHAR(200) NOT NULL,
    NodeType NVARCHAR(32) NOT NULL, -- 'Departement', 'Navigation', 'Category'
    ParentId UNIQUEIDENTIFIER NULL, -- Self-reference to parent node
    IsActive BIT NOT NULL DEFAULT 1,
    SortOrder INT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT FK_CategoryNode_Parent FOREIGN KEY (ParentId) REFERENCES dbo.CategoryNode(Id),
    CONSTRAINT CK_CategoryNode_NodeType CHECK (NodeType IN ('Departement', 'Navigation', 'Category'))
);
```

### Migration Script

The database table can be created using the migration script:
```
Database/Migrations/004_Add_CategoryNode_Table.sql
```

Run with:
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/004_Add_CategoryNode_Table.sql"
```

## Hierarchy Rules

1. **Departement Nodes**:
   - Must have `ParentId = NULL` (they are root nodes)
   - Cannot have a parent
   
2. **Navigation Nodes**:
   - Must have a `ParentId` pointing to either a Departement or another Navigation node
   - Can contain other Navigation nodes or Category nodes
   
3. **Category Nodes**:
   - Must have a `ParentId` pointing to either a Departement or Navigation node
   - These are the only nodes that can be assigned to Products
   - Products reference Category nodes via the `CategoryID` field in the Item table

## Example Hierarchy

```
Electronics (Departement)
├── Home Audio (Navigation)
│   ├── Speakers (Category)
│   ├── Receivers (Category)
│   └── Turntables (Category)
├── Portable Audio (Navigation)
│   ├── Headphones (Category)
│   └── Bluetooth Speakers (Category)
└── Cameras (Category)

Clothing (Departement)
├── Men's Clothing (Navigation)
│   ├── Shirts (Category)
│   └── Pants (Category)
└── Women's Clothing (Navigation)
    ├── Dresses (Category)
    └── Tops (Category)
```

## API Endpoints

All endpoints are available at `/api/CategoryNode/`:

### Public Endpoints (No Authentication Required)

- `GET /api/CategoryNode/GetAllCategoryNodes` - Get all nodes
- `GET /api/CategoryNode/GetCategoryNodeById/{id}` - Get a specific node by ID with its children
- `GET /api/CategoryNode/GetRootNodes` - Get all root (Departement) nodes
- `GET /api/CategoryNode/GetChildren/{parentId}` - Get child nodes of a specific parent
- `GET /api/CategoryNode/GetNodesByType/{nodeType}` - Get nodes of a specific type (Departement, Navigation, or Category)
- `GET /api/CategoryNode/GetCategoryNodes` - Get all Category nodes

### Admin-Only Endpoints (Require Admin Role)

- `POST /api/CategoryNode/CreateCategoryNode` - Create a new node
- `PUT /api/CategoryNode/UpdateCategoryNode` - Update an existing node
- `DELETE /api/CategoryNode/DeleteCategoryNode/{id}` - Delete a node (only if it has no children or items)

## Code Structure

### Domain Models
- `Infrastructure/Data/BaseNode.cs` - Abstract base class
- `Infrastructure/Data/DepartementNode.cs` - Departement implementation
- `Infrastructure/Data/NavigationNode.cs` - Navigation implementation
- `Infrastructure/Data/CategoryNode.cs` - Category implementation

### Repository Layer
- `Infrastructure/Repositories/Interfaces/ICategoryNodeRepository.cs` - Repository interface
- `Infrastructure/Repositories/Implementations/CategoryNodeRepository.cs` - Dapper-based implementation

### Service Layer
- `Domain/Services/Interfaces/ICategoryNodeService.cs` - Service interface
- `Domain/Services/Implementations/CategoryNodeService.cs` - Business logic implementation

### API Layer
- `API/Controllers/CategoryNodeController.cs` - REST API controller
- `Domain/Models/Requests/CreateCategoryNodeRequest.cs` - Create request model
- `Domain/Models/Requests/UpdateCategoryNodeRequest.cs` - Update request model
- `Domain/Models/Responses/CreateCategoryNodeResponse.cs` - Create response model
- `Domain/Models/Responses/UpdateCategoryNodeResponse.cs` - Update response model
- `Domain/Models/Responses/GetCategoryNodeResponse.cs` - Get response model with children
- `Domain/Models/Responses/DeleteCategoryNodeResponse.cs` - Delete response model

### Tests
- `API.Tests/CategoryNodeControllerShould.cs` - 17 comprehensive unit tests covering all controller methods

## Usage Examples

### Creating a Departement Node

```json
POST /api/CategoryNode/CreateCategoryNode
{
  "name_en": "Electronics",
  "name_fr": "Électronique",
  "nodeType": "Departement",
  "parentId": null,
  "isActive": true,
  "sortOrder": 1
}
```

### Creating a Navigation Node

```json
POST /api/CategoryNode/CreateCategoryNode
{
  "name_en": "Home Audio",
  "name_fr": "Audio Maison",
  "nodeType": "Navigation",
  "parentId": "<electronics-departement-id>",
  "isActive": true,
  "sortOrder": 1
}
```

### Creating a Category Node

```json
POST /api/CategoryNode/CreateCategoryNode
{
  "name_en": "Speakers",
  "name_fr": "Haut-parleurs",
  "nodeType": "Category",
  "parentId": "<home-audio-navigation-id>",
  "isActive": true,
  "sortOrder": 1
}
```

### Creating a Category Node with Mandatory Attributes

When creating a Category node, you can optionally include CategoryMandatoryAttributes that will be created in the same operation:

```json
POST /api/CategoryNode/CreateCategoryNode
{
  "name_en": "Speakers",
  "name_fr": "Haut-parleurs",
  "nodeType": "Category",
  "parentId": "<home-audio-navigation-id>",
  "isActive": true,
  "sortOrder": 1,
  "categoryMandatoryAttributes": [
    {
      "name_en": "Wattage",
      "name_fr": "Puissance",
      "attributeType": "int",
      "sortOrder": 1
    },
    {
      "name_en": "Driver Size",
      "name_fr": "Taille du haut-parleur",
      "attributeType": "string",
      "sortOrder": 2
    }
  ]
}
```

**Response:**
```json
{
  "id": "category-node-id-guid",
  "name_en": "Speakers",
  "name_fr": "Haut-parleurs",
  "nodeType": "Category",
  "parentId": "<home-audio-navigation-id>",
  "isActive": true,
  "sortOrder": 1,
  "categoryMandatoryAttributes": [
    {
      "id": "attribute-1-guid",
      "name_en": "Wattage",
      "name_fr": "Puissance",
      "attributeType": "int",
      "sortOrder": 1
    },
    {
      "id": "attribute-2-guid",
      "name_en": "Driver Size",
      "name_fr": "Taille du haut-parleur",
      "attributeType": "string",
      "sortOrder": 2
    }
  ]
}
```

**Note:** CategoryMandatoryAttributes can only be provided when creating Category nodes. They are ignored for Departement and Navigation nodes.

## Features

### Bilingual Support
All nodes support both English (`Name_en`) and French (`Name_fr`) names.

### Circular Reference Prevention
The repository validates that parent-child relationships don't create circular references when updating nodes.

### Cascade Delete Protection
- Nodes with children cannot be deleted
- Category nodes with associated products cannot be deleted

### Active/Inactive Status
Nodes can be marked as inactive without deletion, allowing for soft deprecation.

### Custom Sort Order
The `SortOrder` field allows custom ordering of nodes within the same level.

## Testing

Run the tests with:
```bash
dotnet test --filter "FullyQualifiedName~CategoryNodeControllerShould"
```

All 17 tests cover:
- Creating all node types (Departement, Navigation, Category)
- Validation errors
- Getting all nodes
- Getting nodes by ID
- Getting root nodes
- Getting children
- Getting nodes by type
- Getting category nodes
- Updating nodes
- Deleting nodes
- Error handling

## Migration Path from Category Table

The existing `Category` table will continue to work alongside the new `CategoryNode` table. When ready to migrate:

1. Run the migration script to create the `CategoryNode` table
2. Create a data migration script to:
   - Convert existing root categories to Departement nodes
   - Convert subcategories to either Navigation or Category nodes based on business rules
   - Update Item.CategoryId to reference the new CategoryNode IDs
3. Update the frontend to use the new CategoryNode endpoints
4. Once migration is complete, the old `Category` table can be deprecated

## Future Enhancements

Possible future improvements:
- Add Description fields (Description_en, Description_fr)
- Add Slug fields for SEO-friendly URLs
- Add ImageUrl for node icons/images
- Add metadata fields for filtering and searching
- Implement full-text search across node names
- Add breadcrumb generation utilities
- Add tree navigation helpers
