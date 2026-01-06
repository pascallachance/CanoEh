# Failed Login Attempts Feature - Manual Testing Guide

## Overview
This feature prevents brute force attacks by locking accounts after 3 failed login attempts for 10 minutes.

## Test Scenarios

### Scenario 1: Normal Failed Login (1-2 attempts)
1. Start the API server
2. Create a test user with valid email validation
3. Try to login with incorrect password
4. **Expected**: Login fails with "Invalid email or password" (401)
5. Try again with incorrect password
6. **Expected**: Login still fails with same message
7. Login with correct password
8. **Expected**: Login succeeds, failed attempts reset

### Scenario 2: Account Lockout (3 failed attempts)
1. Use the same test user
2. Try to login with incorrect password 3 times
3. **Expected**: First 3 attempts fail with "Invalid email or password" (401)
4. Try to login again (even with correct password)
5. **Expected**: Login blocked with "Account is locked due to too many failed login attempts. Please try again in X minute(s)." (429)

### Scenario 3: Lockout Expiry
1. Wait 10 minutes after the 3rd failed attempt
2. Try to login with correct password
3. **Expected**: Login succeeds, failed attempts reset

### Scenario 4: Security - Non-existent User
1. Try to login with a non-existent email
2. **Expected**: Same "Invalid email or password" error, no tracking

## Database Verification

Check the User table to verify the fields:
```sql
SELECT Email, FailedLoginAttempts, LastFailedLoginAttempt 
FROM dbo.[User] 
WHERE Email = 'test@example.com';
```

Expected values:
- After failed login: FailedLoginAttempts increments, LastFailedLoginAttempt updated
- After successful login: FailedLoginAttempts = 0, LastFailedLoginAttempt = NULL
- After 3 failed attempts: FailedLoginAttempts = 3, LastFailedLoginAttempt recent
- After lockout expiry + successful login: FailedLoginAttempts = 0, LastFailedLoginAttempt = NULL

## API Endpoints

### Create User (if needed)
```
POST /api/user
{
  "email": "test@example.com",
  "firstname": "Test",
  "lastname": "User",
  "password": "Test123!",
  "language": "en"
}
```

### Login
```
POST /api/login/login
{
  "email": "test@example.com",
  "password": "Test123!"
}
```

## Expected HTTP Status Codes
- 200: Successful login
- 401: Invalid credentials (failed attempts < 3 or account deleted/unvalidated)
- 403: Email not validated
- 429: Too many requests (account locked)

## Notes
- The lockout period is exactly 10 minutes from the last failed attempt
- The lockout counter resets on successful login
- The lockout counter also resets automatically when 10 minutes have passed and user tries to login
- Non-existent users don't trigger the tracking mechanism (security feature)
