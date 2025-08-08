# PaymentMethod Security Implementation

This document describes the security measures implemented to ensure that PaymentMethods can only be edited or deleted by the User who created them.

## Security Model

### Authentication & Authorization
- All PaymentMethod API endpoints require authentication via the `[Authorize]` attribute
- JWT tokens are used to identify the authenticated user
- User identity is extracted from the `ClaimTypes.NameIdentifier` claim in the JWT token

### User Ownership Validation

#### Controller Level
The `PaymentMethodController` implements user ownership validation by:
1. Extracting the authenticated user's email from JWT claims
2. Looking up the complete user entity via `IUserService.GetUserEntityAsync()`
3. Passing the user's ID to all service method calls
4. Returning 401 Unauthorized if user is not authenticated
5. Returning 404 Not Found if user entity cannot be found

#### Service Level
The `PaymentMethodService` enforces ownership by:
- Always requiring a `userId` parameter for operations
- Using user-scoped repository methods like `FindByUserIdAndIdAsync()`
- Validating that payment methods belong to the requesting user before any operations

#### Repository Level
The `PaymentMethodRepository` provides secure methods:
- `FindByUserIdAndIdAsync(userId, id)` - Only returns payment methods owned by the specified user
- `FindByUserIdAsync(userId)` - Returns all payment methods for a specific user
- `FindActiveByUserIdAsync(userId)` - Returns active payment methods for a specific user
- `FindDefaultByUserIdAsync(userId)` - Returns default payment method for a specific user
- `SetDefaultPaymentMethodAsync(userId, paymentMethodId)` - Only sets default for user-owned payment methods
- `ClearDefaultPaymentMethodsAsync(userId)` - Only clears defaults for the specified user
- `DeactivatePaymentMethodAsync(userId, id)` - Only deactivates payment methods owned by the user

### Security Safeguards

#### Prevented Attack Vectors
1. **Cross-User Access**: Users cannot access payment methods belonging to other users
2. **Unauthorized Modification**: Users cannot update payment methods they don't own
3. **Unauthorized Deletion**: Users cannot delete payment methods they don't own
4. **Privilege Escalation**: Users cannot set other users' payment methods as their default

#### Implementation Details
- The generic repository methods (`GetByIdAsync`, `UpdateAsync`, `DeleteAsync`) from `IRepository<T>` are not used in the service layer
- All operations go through user-scoped methods that include `userId` validation
- SQL queries include `WHERE UserID = @userId` clauses to ensure database-level security
- Repository methods return null/empty results for non-owned resources instead of throwing exceptions

## API Endpoints Security

All PaymentMethod endpoints implement the same security pattern:

### Create PaymentMethod
- `POST /api/PaymentMethod/CreatePaymentMethod`
- Validates user authentication
- Associates new payment method with authenticated user's ID

### Get PaymentMethod
- `GET /api/PaymentMethod/GetPaymentMethod/{paymentMethodId}`
- Returns 404 if payment method doesn't belong to authenticated user
- Uses `FindByUserIdAndIdAsync()` for ownership validation

### Update PaymentMethod
- `PUT /api/PaymentMethod/UpdatePaymentMethod`
- Returns 404 if payment method doesn't belong to authenticated user
- Uses `FindByUserIdAndIdAsync()` for ownership validation before update

### Delete PaymentMethod
- `DELETE /api/PaymentMethod/DeletePaymentMethod/{paymentMethodId}`
- Returns 404 if payment method doesn't belong to authenticated user
- Uses `FindByUserIdAndIdAsync()` for ownership validation before deletion

### Set Default PaymentMethod
- `POST /api/PaymentMethod/SetDefaultPaymentMethod/{paymentMethodId}`
- Returns 404 if payment method doesn't belong to authenticated user
- Uses `FindByUserIdAndIdAsync()` for ownership validation

### Get User PaymentMethods
- `GET /api/PaymentMethod/GetUserPaymentMethods`
- `GET /api/PaymentMethod/GetActiveUserPaymentMethods`
- `GET /api/PaymentMethod/GetDefaultPaymentMethod`
- Only returns payment methods belonging to the authenticated user

## Testing

Comprehensive security tests are implemented in `PaymentMethodControllerShould.cs`:

- `GetPaymentMethod_CannotAccessOtherUsersPaymentMethod()` - Verifies users cannot access other users' payment methods
- `UpdatePaymentMethod_CannotUpdateOtherUsersPaymentMethod()` - Verifies users cannot update other users' payment methods
- `DeletePaymentMethod_CannotDeleteOtherUsersPaymentMethod()` - Verifies users cannot delete other users' payment methods
- `SetDefaultPaymentMethod_CannotSetOtherUsersPaymentMethodAsDefault()` - Verifies users cannot set other users' payment methods as default
- `CreatePaymentMethod_ReturnUnauthorized_WhenUserNotAuthenticated()` - Verifies unauthenticated access is blocked

## Security Validation

The implementation has been validated through:
1. Unit tests covering all security scenarios
2. Integration tests confirming authentication requirements
3. Manual testing of API endpoints
4. Code review of all data access patterns

## Conclusion

The PaymentMethod system implements defense-in-depth security:
- **Authentication**: JWT token validation
- **Authorization**: User ownership validation at multiple layers
- **Data Access**: User-scoped repository methods
- **Database**: SQL-level user filtering
- **Testing**: Comprehensive security test coverage

This ensures that PaymentMethods can only be edited or deleted by the User who created them, as required.