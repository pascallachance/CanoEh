using System.Data;
using Dapper;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class UserRepository(string connectionString) : GenericRepository<User>(connectionString)
    {
        public override User Add(User entity)
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
    validemail) 
VALUES (
    @uname, 
    @firstname, 
    @lastname, 
    @email, 
    @phone, 
    @lastlogin, 
    @createdat, 
    @lastupdatedat, 
    @password, 
    @deleted,
    @validemail)
";

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
            dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override int Count(Func<User, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return dbConnection.Query<User>("SELECT * FROM dbo.Users").Count(predicate);
        }

        public override void Delete(User entity)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            entity.Deleted = true;
            Update(entity);
        }

        public override bool Exists(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return dbConnection.ExecuteScalar<bool>("SELECT COUNT(1) FROM dbo.Users WHERE id = @id", new { id });
        }

        public override IEnumerable<User> Find(Func<User, bool> predicate)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return dbConnection.Query<User>("SELECT * FROM dbo.Users").Where(predicate);
        }

        public override IEnumerable<User> GetAll()
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            return dbConnection.Query<User>("SELECT * FROM dbo.Users");
        }

        public override User GetById(Guid id)
        {
            if (dbConnection.State != ConnectionState.Open)
            {
                dbConnection.Open();
            }
            var query = @"
SELECT TOP(1) * 
FROM dbo.Users 
WHERE id = @id";
            return dbConnection.QueryFirst<User>(query, new { id });
        }

        public override User Update(User entity)
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
                DateTime.UtcNow, //Lastupdatedat is set to current time
                entity.Password,
                entity.Deleted,
                entity.ValidEmail,
            };
            dbConnection.Execute(query, parameters);
            return entity;
        }
    }
}
