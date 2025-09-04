# JWT Cookie Authentication Fix

## Problem
The frontend login was successful and received a JWT token, but subsequent API calls (like `GetMyCompanies`) were failing with:
- CORS error: "No 'Access-Control-Allow-Origin' header is present on the requested resource"
- 401 Unauthorized error

## Root Cause
The JWT token was being stored in a cookie by the `LoginController`, but the JWT Bearer authentication middleware was only configured to read tokens from the `Authorization` header, not from cookies.

## Solution
Modified the JWT Bearer authentication configuration in `API/Program.cs` to read tokens from both:
1. **Authorization header** (standard approach): `Authorization: Bearer <token>`
2. **Cookies** (for this application): `Cookie: AuthToken=<token>`

### Code Changes
```csharp
.AddJwtBearer(jwtOptions =>
{
    // ... existing configuration ...
    
    // Configure JWT Bearer to read tokens from cookies
    jwtOptions.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // First try to get token from Authorization header (standard approach)
            var token = context.Request.Headers.Authorization
                .FirstOrDefault()?.Split(" ").Last();
            
            // If no token in header, try to get it from cookie
            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["AuthToken"];
            }
            
            context.Token = token;
            return Task.CompletedTask;
        }
    };
});
```

## Verification
- ✅ API correctly processes JWT tokens from cookies
- ✅ API still works with Authorization headers
- ✅ CORS is properly configured
- ✅ All existing tests pass
- ✅ Added focused unit tests for JWT cookie authentication

## Impact
This fix allows the frontend to successfully authenticate using the JWT token stored in the `AuthToken` cookie, resolving the 401 Unauthorized errors after login.