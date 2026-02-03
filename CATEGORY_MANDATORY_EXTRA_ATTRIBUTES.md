# CategoryMandatoryExtraAttributes Implementation

## Overview
This document describes the implementation of `categoryMandatoryExtraAttributes` feature for the CategoryNode endpoints in the CanoEh API.

## Purpose
The `categoryMandatoryExtraAttributes` field allows administrators to define mandatory extra attributes (such as SKU, Dimensions, etc.) that must be provided when creating or editing item variants in a specific category. This is separate from `categoryMandatoryAttributes` which is used for product-level attributes.

## Changes Made

### 1. Data Model
**File:** `Infrastructure/Data/CategoryMandatoryExtraAttribute.cs`
- Created new `CategoryMandatoryExtraAttribute` class with properties:
  - `Id`: Unique identifier
  - `CategoryNodeId`: Foreign key to CategoryNode
  - `Name_en`: English name
  - `Name_fr`: French name
  - `AttributeType`: Optional type specification (e.g., "string", "int")
  - `SortOrder`: Optional ordering

### 2. Repository Layer
**Files:**
- `Infrastructure/Repositories/Interfaces/ICategoryMandatoryExtraAttributeRepository.cs`
- `Infrastructure/Repositories/Implementations/CategoryMandatoryExtraAttributeRepository.cs`

Implemented standard CRUD operations for CategoryMandatoryExtraAttribute:
- `AddAsync`: Create a new extra attribute
- `GetAllAsync`: Retrieve all extra attributes
- `GetByIdAsync`: Get a specific extra attribute by ID
- `UpdateAsync`: Update an existing extra attribute
- `DeleteAsync`: Delete an extra attribute
- `GetAttributesByCategoryNodeIdAsync`: Get all extra attributes for a specific category node
- `DeleteAttributesByCategoryNodeIdAsync`: Delete all extra attributes for a category node

### 3. Request DTOs
**Files:**
- `Domain/Models/Requests/CreateCategoryNodeRequest.cs`
- `Domain/Models/Requests/BulkCreateCategoryNodesRequest.cs`

Added `CreateCategoryMandatoryExtraAttributeDto` class and integrated it into request models:
- Added `CategoryMandatoryExtraAttributes` list to `CreateCategoryNodeRequest`
- Added `CategoryMandatoryExtraAttributes` list to `CategoryNodeDto` (used in bulk operations)
- Added validation to ensure extra attributes are only provided for Category nodes
- Added validation for attribute name lengths and type constraints

### 4. Response DTOs
**Files:**
- `Domain/Models/Responses/CreateCategoryNodeResponse.cs`
- `Domain/Models/Responses/BulkCreateCategoryNodesResponse.cs`

Added `CategoryMandatoryExtraAttributeResponseDto` class and integrated it into response models:
- Added `CategoryMandatoryExtraAttributes` list to `CreateCategoryNodeResponse`
- Added `CategoryMandatoryExtraAttributes` list to `CategoryNodeResponseDto`

### 5. Service Layer
**File:** `Domain/Services/Implementations/CategoryNodeService.cs`

Updated service methods to handle extra attributes:
- `CreateCategoryNodeAsync`: Processes and persists extra attributes alongside regular attributes
- `ProcessCategoryNode`: Handles extra attributes in bulk creation operations
- Both methods now create extra attributes in the same database transaction as the node itself

### 6. Repository Updates
**Files:**
- `Infrastructure/Repositories/Interfaces/ICategoryNodeRepository.cs`
- `Infrastructure/Repositories/Implementations/CategoryNodeRepository.cs`

Updated repository methods to support extra attributes:
- `AddNodeWithAttributesAsync`: Now accepts and persists extra attributes
- `AddMultipleNodesWithAttributesAsync`: Batch creates nodes with both regular and extra attributes

### 7. Database Schema
**Files:**
- `Database/000_Create_Database_Schema.sql` (updated)
- `Database/Migrations/006_Add_CategoryMandatoryExtraAttribute_Table.sql` (new)

Created new `CategoryMandatoryExtraAttribute` table with:
- Primary key on `Id`
- Foreign key to `CategoryNode(Id)` with CASCADE delete
- Indexes on `CategoryNodeId` and `SortOrder` for performance

### 8. Tests
**File:** `API.Tests/CategoryNodeControllerShould.cs`

Added comprehensive tests:
- `CreateCategoryNode_ReturnOk_WhenCategoryNodeWithExtraAttributesCreatedSuccessfully`: Tests single category node creation with extra attributes
- `BulkCreateCategoryNodes_ReturnOk_WhenNodesWithExtraAttributesCreatedSuccessfully`: Tests bulk category node creation with extra attributes

## API Usage Examples

### Creating a Single Category Node with Extra Attributes

```http
POST /api/CategoryNode/CreateCategoryNode
Content-Type: application/json
Authorization: Bearer {admin-token}

{
  "name_en": "Speakers",
  "name_fr": "Haut-parleurs",
  "nodeType": "Category",
  "parentId": "{parent-navigation-node-id}",
  "isActive": true,
  "sortOrder": 1,
  "categoryMandatoryAttributes": [
    {
      "name_en": "Wattage",
      "name_fr": "Puissance",
      "attributeType": "int",
      "sortOrder": 1
    }
  ],
  "categoryMandatoryExtraAttributes": [
    {
      "name_en": "SKU",
      "name_fr": "SKU",
      "attributeType": "string",
      "sortOrder": 1
    },
    {
      "name_en": "Dimensions",
      "name_fr": "Dimensions",
      "attributeType": "string",
      "sortOrder": 2
    }
  ]
}
```

### Bulk Creating Category Nodes with Extra Attributes

```http
POST /api/CategoryNode/BulkCreateCategoryNodes
Content-Type: application/json
Authorization: Bearer {admin-token}

{
  "departements": [
    {
      "name_en": "Electronics",
      "name_fr": "Électronique",
      "isActive": true,
      "sortOrder": 1,
      "navigationNodes": [
        {
          "name_en": "Audio",
          "name_fr": "Audio",
          "isActive": true,
          "sortOrder": 1,
          "categoryNodes": [
            {
              "name_en": "Headphones",
              "name_fr": "Écouteurs",
              "isActive": true,
              "sortOrder": 1,
              "categoryMandatoryAttributes": [
                {
                  "name_en": "Impedance",
                  "name_fr": "Impédance",
                  "attributeType": "int",
                  "sortOrder": 1
                }
              ],
              "categoryMandatoryExtraAttributes": [
                {
                  "name_en": "SKU",
                  "name_fr": "SKU",
                  "attributeType": "string",
                  "sortOrder": 1
                },
                {
                  "name_en": "Weight",
                  "name_fr": "Poids",
                  "attributeType": "string",
                  "sortOrder": 2
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}
```

## Validation Rules

1. **Extra attributes can only be provided for Category nodes** - Attempting to add extra attributes to Departement or Navigation nodes will result in a 400 Bad Request error.

2. **English and French names are required** - Both `name_en` and `name_fr` must be provided for each extra attribute.

3. **Name length validation** - Attribute names cannot exceed 100 characters.

4. **AttributeType length validation** - If provided, `attributeType` cannot exceed 50 characters.

## Database Migration

To apply the database migration, run:

```bash
sqlcmd -S (localdb)\MSSQLLocalDB -d CanoEh -i "Database/Migrations/006_Add_CategoryMandatoryExtraAttribute_Table.sql"
```

Or recreate the entire database from scratch using:

```bash
sqlcmd -S (localdb)\MSSQLLocalDB -i "Database/000_Create_Database_Schema.sql"
```

## Testing

Run the new tests with:

```bash
dotnet test --filter "FullyQualifiedName~CategoryNodeControllerShould&FullyQualifiedName~ExtraAttributes"
```

## Future Considerations

1. The extra attributes are stored but not yet enforced when creating/editing item variants. This enforcement will need to be implemented in the item management workflow.

2. Consider adding validation endpoints to check which mandatory extra attributes are required for a given category.

3. May need to add update/delete operations specifically for extra attributes if they need to be modified independently of the category node.

## Notes

- All changes maintain backward compatibility - existing category nodes without extra attributes continue to work.
- The feature uses the same transactional approach as regular mandatory attributes to ensure data consistency.
- Both single and bulk creation operations support extra attributes.
