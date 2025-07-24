using Dapper;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class UserRepository(string connectionString) : GenericRepository<User>(connectionString)
    {
        public override User Add(User entity)
        {            
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
    deleted) 
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
    @deleted)
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
                entity.Deleted
            };
            dbConnection.ExecuteAsync(query, parameters);
            return entity;
        }

        public override int Count(Func<User, bool> predicate)
        {
            return dbConnection.Query<User>("SELECT * FROM dbo.Users").Count(predicate);
        }

        public override void Delete(User entity)
        {
            entity.Deleted = true;
            Update(entity);
        }

        public override bool Exists(Guid id)
        {
            return dbConnection.ExecuteScalar<bool>("SELECT COUNT(1) FROM dbo.Users WHERE id = @id", new { id });
        }

        public override IEnumerable<User> Find(Func<User, bool> predicate)
        {
            return dbConnection.Query<User>("SELECT * FROM dbo.Users").Where(predicate);
        }

        public override IEnumerable<User> GetAll()
        {
            return dbConnection.Query<User>("SELECT * FROM dbo.Users");
        }

        public override User GetById(Guid id)
        {
            var query = @"
SELECT TOP(1) * 
FROM dbo.Users 
WHERE id = @id";
            return dbConnection.QueryFirst<User>(query, new { id });
        }

        public override User Update(User entity)
        {
            var query = @"
UPDATE dbo.Users
SET
     dbo.Users.firstname = @firstname,
    dbo.Users.lastname = @lastname,
    dbo.Users.email = @email,
    dbo.Users.phone = @phone,
    dbo.Users.lastupdatedat = @lastupdatedat,
    dbo.Users.deleted = @deleted
WHERE dbo.Users.id = @id";

            var parameters = new
            {
                entity.ID,
                entity.Uname,
                entity.Firstname,
                entity.Lastname,
                entity.Email,
                entity.Phone,
                entity.Lastupdatedat,
                entity.Password,
                entity.Deleted
            };
            dbConnection.Execute(query, parameters);
            return entity;
        }
    }
}
