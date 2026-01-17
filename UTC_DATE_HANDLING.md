# UTC Date Handling Implementation

## Overview
All dates in the CanoEh application are stored in UTC and converted to the viewer's local timezone in the user interface.

## Backend (.NET 8.0)

### Database Storage
- All `DATETIME2` columns in SQL Server use `GETUTCDATE()` as the default value
- Dates are stored in UTC timezone

### Application Code
- All date assignments use `DateTime.UtcNow` (not `DateTime.Now`)
- DateTime objects have `Kind` set to `DateTimeKind.Utc`

### JSON Serialization
- .NET's `System.Text.Json` automatically serializes `DateTime` values in ISO 8601 format
- UTC dates are serialized with 'Z' suffix (e.g., `"2024-01-15T10:30:00.000Z"`)
- This happens by default when `DateTime.Kind` is `Utc`

### Examples
```csharp
// Correct: Using UTC
var user = new User {
    Createdat = DateTime.UtcNow,
    Lastupdatedat = DateTime.UtcNow
};

// API Response (JSON)
{
    "createdAt": "2024-01-15T10:30:00.123Z",
    "updatedAt": "2024-01-16T14:22:00.456Z"
}
```

## Frontend (React/TypeScript)

### Date Display
The `dateUtils.ts` utility provides functions to format UTC dates for display in the user's local timezone:

```typescript
import { formatDate, formatShortDate } from './utils/dateUtils';

// Format full date: "2024/01/15" (in user's local timezone)
const displayDate = formatDate(item.createdAt);

// Format short date: "01/15" (for charts/compact displays)
const shortDate = formatShortDate(item.createdAt);
```

### Date Input
When collecting date input from users (e.g., offer dates, birth dates), use the `toUTCISOString` function to convert to UTC:

```typescript
import { toUTCISOString } from './utils/dateUtils';

// User selects "2024-01-15" in date picker
const userSelectedDate = "2024-01-15";

// Convert to UTC ISO string for API
const utcDate = toUTCISOString(userSelectedDate);
// Result: "2024-01-15T00:00:00.000Z"

// Send to API
const apiData = {
    birthDate: toUTCISOString(formData.birthDate),
    offerStart: toUTCISOString(offerStartDate)
};
```

### How It Works
1. **Display**: When the API returns `"2024-01-15T10:30:00Z"`, JavaScript's `new Date()` parses it as UTC
2. **Local Conversion**: Methods like `getFullYear()`, `getMonth()`, `getDate()` automatically convert to the browser's local timezone
3. **Input**: Date picker values (YYYY-MM-DD) are explicitly converted to UTC midnight for storage

### Browser Timezone Handling
- The browser automatically detects the user's timezone
- All date display functions use the browser's local timezone
- No timezone selection needed - it's automatic

## Testing

### Frontend Tests
Run the date utilities test suite:
```bash
cd Seller/seller.client
npx tsx tests/dateUtils-test.ts
```

### Verification
To verify UTC handling:
1. API responses contain dates with 'Z' suffix
2. Database stores dates in UTC (verify with `SELECT GETUTCDATE()`)
3. Frontend displays dates in local timezone (check browser's timezone)

## Migration Notes
All existing code already uses UTC:
- Database defaults to `GETUTCDATE()`
- Application code uses `DateTime.UtcNow`
- Frontend utilities handle UTC to local conversion

No data migration required - dates are already stored in UTC.
