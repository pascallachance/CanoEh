using System.Data;
using Dapper;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.Repositories.Implementations
{
    public class UserRepository(string connectionString) : GenericRepository<User>(connectionString), IUserRepository
    {
        public override async Task<User> AddAsync(User entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
INSERT INTO dbo.[User] (
    email,
    firstname, 
    lastname, 
    phone,
    language, 
    lastlogin, 
    createdat, 
    lastupdatedat, 
    password, 
    deleted, 
    validEmail,
    emailValidationToken,
    failedLoginAttempts,
    lastFailedLoginAttempt)
OUTPUT INSERTED.ID
VALUES (
    @Email,
    @Firstname, 
    @Lastname, 
    @Phone,
    @Language, 
    @Lastlogin, 
    @Createdat, 
    @Lastupdatedat, 
    @Password, 
    @Deleted, 
    @ValidEmail,
    @EmailValidationToken,
    @FailedLoginAttempts,
    @LastFailedLoginAttempt)";

            var parameters = new
            {
                entity.Email,
                entity.Firstname,
                entity.Lastname,
                entity.Phone,
                entity.Language,
                entity.Lastlogin,
                entity.Createdat,
                entity.Lastupdatedat,
                entity.Password,
                entity.Deleted,
                entity.ValidEmail,
                entity.EmailValidationToken,
                entity.FailedLoginAttempts,
                entity.LastFailedLoginAttempt,
            };
            Guid newUserId = await dbConnection.ExecuteScalarAsync<Guid>(query, parameters);
            entity.ID = newUserId; 
            return entity;
        }

        public override async Task<int> CountAsync(Func<User, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var users = await dbConnection.QueryAsync<User>("SELECT * FROM dbo.[User]");
            return users.Count(predicate);
        }

        public override async Task DeleteAsync(User entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            entity.Deleted = true;
            await UpdateAsync(entity);
        }

        public override async Task<bool> ExistsAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.[User] WHERE id = @id", new { id });
        }

        public override async Task<IEnumerable<User>> FindAsync(Func<User, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var users = await dbConnection.QueryAsync<User>("SELECT * FROM dbo.[User]");
            return users.Where(predicate);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.QueryAsync<User>("SELECT * FROM dbo.[User]");
        }

        public override async Task<User> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE id = @id";
            return await dbConnection.QueryFirstAsync<User>(query, new { id });
        }

        public override async Task<User> UpdateAsync(User entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User]
SET
     dbo.[User].firstname = @firstname,
    dbo.[User].lastname = @lastname,
    dbo.[User].email = @email,
    dbo.[User].phone = @phone,
    dbo.[User].language = @language,
    dbo.[User].lastlogin = @lastlogin,
    dbo.[User].lastupdatedat = @lastupdatedat,
    dbo.[User].deleted = @deleted,
    dbo.[User].validemail = @validemail,
    dbo.[User].emailValidationToken = @emailValidationToken,
    dbo.[User].passwordResetToken = @passwordResetToken,
    dbo.[User].passwordResetTokenExpiry = @passwordResetTokenExpiry,
    dbo.[User].restoreUserToken = @restoreUserToken,
    dbo.[User].restoreUserTokenExpiry = @restoreUserTokenExpiry,
    dbo.[User].failedLoginAttempts = @failedLoginAttempts,
    dbo.[User].lastFailedLoginAttempt = @lastFailedLoginAttempt
WHERE dbo.[User].id = @id";

            var parameters = new
            {
                entity.ID,
                entity.Email,
                entity.Firstname,
                entity.Lastname,
                entity.Phone,
                entity.Language,
                entity.Lastlogin,
                entity.Lastupdatedat, //Use entity's Lastupdatedat value
                entity.Password,
                entity.Deleted,
                entity.ValidEmail,
                entity.EmailValidationToken,
                entity.PasswordResetToken,
                entity.PasswordResetTokenExpiry,
                entity.RestoreUserToken,
                entity.RestoreUserTokenExpiry,
                entity.FailedLoginAttempts,
                entity.LastFailedLoginAttempt,
            };
            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        // IUserRepository specific methods
        public async Task<User?> FindByEmailAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE email = @email AND deleted = 0";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { email });
        }

        public async Task<IEnumerable<User>> FindByDeletedStatusAsync(bool deleted)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT * 
FROM dbo.[User] 
WHERE deleted = @deleted";
            return await dbConnection.QueryAsync<User>(query, new { deleted });
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.[User] WHERE email = @email AND deleted = 0", new { email });
        }

        public async Task<User?> FindByEmailValidationTokenAsync(string token)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE emailValidationToken = @token AND deleted = 0";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { token });
        }

        public async Task<User?> FindByPasswordResetTokenAsync(string token)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE passwordResetToken = @token AND deleted = 0 AND passwordResetTokenExpiry > @now";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { token, now = DateTime.UtcNow });
        }

        public async Task<bool> UpdatePasswordResetTokenAsync(string email, string token, DateTime expiry)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User] 
SET passwordResetToken = @token, passwordResetTokenExpiry = @expiry, lastupdatedat = @now
WHERE email = @email AND deleted = 0";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { email, token, expiry, now = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<bool> ClearPasswordResetTokenAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User] 
SET passwordResetToken = NULL, passwordResetTokenExpiry = NULL, lastupdatedat = @now
WHERE email = @email AND deleted = 0";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { email, now = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<User?> FindDeletedByEmailAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE email = @email AND deleted = 1";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { email });
        }

        public async Task<User?> FindByRestoreUserTokenAsync(string token)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE restoreUserToken = @token AND deleted = 1 AND restoreUserTokenExpiry > @now";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { token, now = DateTime.UtcNow });
        }

        public async Task<bool> UpdateRestoreUserTokenAsync(string email, string token, DateTime expiry)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User] 
SET restoreUserToken = @token, restoreUserTokenExpiry = @expiry, lastupdatedat = @now
WHERE email = @email AND deleted = 1";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { email, token, expiry, now = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<bool> RestoreUserByTokenAsync(string token)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User] 
SET deleted = 0, restoreUserToken = NULL, restoreUserTokenExpiry = NULL, lastupdatedat = @now
WHERE restoreUserToken = @token AND deleted = 1 AND restoreUserTokenExpiry > @now";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { token, now = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<User?> FindByRefreshTokenAsync(string refreshToken)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.[User] 
WHERE refreshToken = @refreshToken AND deleted = 0 AND refreshTokenExpiry > @now";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { refreshToken, now = DateTime.UtcNow });
        }

        public async Task<bool> UpdateRefreshTokenAsync(string email, string refreshToken, DateTime expiry)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User] 
SET refreshToken = @refreshToken, refreshTokenExpiry = @expiry, lastupdatedat = @now
WHERE email = @email AND deleted = 0";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { email, refreshToken, expiry, now = DateTime.UtcNow });
            return rowsAffected > 0;
        }

        public async Task<bool> ClearRefreshTokenAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
UPDATE dbo.[User] 
SET refreshToken = NULL, refreshTokenExpiry = NULL, lastupdatedat = @now
WHERE email = @email AND deleted = 0";
            var rowsAffected = await dbConnection.ExecuteAsync(query, new { email, now = DateTime.UtcNow });
            return rowsAffected > 0;
        }
    }
}
