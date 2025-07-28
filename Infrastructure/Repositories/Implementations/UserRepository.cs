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
INSERT INTO dbo.Users (
    uname, 
    firstname, 
    lastname, 
    email, 
    phone, 
    lastlogin, 
    createdat, 
    lastupdatedat, 
    password, 
    deleted, 
    validEmail)
OUTPUT INSERTED.ID
VALUES (
    @Uname, 
    @Firstname, 
    @Lastname, 
    @Email, 
    @Phone, 
    @Lastlogin, 
    @Createdat, 
    @Lastupdatedat, 
    @Password, 
    @Deleted, 
    @ValidEmail)";

            var parameters = new
            {
                entity.Uname,
                entity.Firstname,
                entity.Lastname,
                entity.Email,
                entity.Phone,
                entity.Lastlogin,
                entity.Createdat,
                entity.Lastupdatedat,
                entity.Password,
                entity.Deleted,
                entity.ValidEmail,
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
            var users = await dbConnection.QueryAsync<User>("SELECT * FROM dbo.Users");
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
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Users WHERE id = @id", new { id });
        }

        public override async Task<IEnumerable<User>> FindAsync(Func<User, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var users = await dbConnection.QueryAsync<User>("SELECT * FROM dbo.Users");
            return users.Where(predicate);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.QueryAsync<User>("SELECT * FROM dbo.Users");
        }

        public override async Task<User> GetByIdAsync(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Users 
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
UPDATE dbo.Users
SET
     dbo.Users.firstname = @firstname,
    dbo.Users.lastname = @lastname,
    dbo.Users.email = @email,
    dbo.Users.phone = @phone,
    dbo.Users.lastlogin = @lastlogin,
    dbo.Users.lastupdatedat = @lastupdatedat,
    dbo.Users.deleted = @deleted,
    dbo.Users.validemail = @validemail
WHERE dbo.Users.id = @id";

            var parameters = new
            {
                entity.ID,
                entity.Uname,
                entity.Firstname,
                entity.Lastname,
                entity.Email,
                entity.Phone,
                entity.Lastlogin,
                entity.Lastupdatedat, //Use entity's Lastupdatedat value
                entity.Password,
                entity.Deleted,
                entity.ValidEmail,
            };
            await dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        // IUserRepository specific methods
        public async Task<User?> FindByUsernameAsync(string username)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Users 
WHERE uname = @username AND deleted = 0";
            return await dbConnection.QueryFirstOrDefaultAsync<User>(query, new { username });
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Users 
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
FROM dbo.Users 
WHERE deleted = @deleted";
            return await dbConnection.QueryAsync<User>(query, new { deleted });
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Users WHERE uname = @username AND deleted = 0", new { username });
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return await dbConnection.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM dbo.Users WHERE email = @email AND deleted = 0", new { email });
        }
    }
}
