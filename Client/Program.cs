using Microsoft.Extensions.Configuration;

namespace Client
{
    internal static class Program
    {
        private static IConfigurationRoot config;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //// To customize application configuration such as set high DPI settings or default font,
            //// see https://aka.ms/applicationconfiguration.
            //ApplicationConfiguration.Initialize();
            //Application.Run(new frmStaffList());

            Initialize();

            //#region Test Code
            //var dateTimenow = DateTime.Now;
            //var user1 = new User
            //{
            //    uname = "testuser",
            //    firstname = "Test",
            //    lastname = "User",
            //    email = "testuser@email.com",
            //    phone = "4501234567",
            //    lastlogin = null,
            //    createdat = dateTimenow,
            //    lastupdatedat = null,
            //    password = "test",
            //    deleted = true,
            //};
            //addTestUser(user1);
            //getAllShouldReturnAllUsers();
            //var user1id = getUserByUname(user1.uname).id;
            //if (user1id == Guid.Empty)
            //{
            //    System.Diagnostics.Debug.WriteLine("User ID is empty. Cannot proceed with further operations.");
            //    return;
            //}
            //else
            //{
            //    user1.firstname = "UpdatedFirstName";
            //    updateUser(user1);
            //    getUserById(user1id);
            //    deleteUserById(user1id);
            //    userExists(user1id);
            //}
            //countUsers(u => u.deleted == true);

            //static void getAllShouldReturnAllUsers()
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();

            //    //Act
            //    var users = repository.GetAll();

            //    if (users == null || !users.Any())
            //    {
            //        System.Diagnostics.Debug.WriteLine("No users found.");
            //        return;
            //    }

            //    //Assert
            //    foreach (var user in users)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"User: {user.uname}, Email: {user.email}");
            //    }
            //}

            //static void addTestUser(User user)
            //{
            //    //Arrange                
            //    var repository = CreateUserRepository();

            //    //ACT
            //    if (findUser(u => u.uname == user.uname))
            //    {
            //        System.Diagnostics.Debug.WriteLine($"User with username {user.uname} already exists.");
            //    }
            //    else
            //    { 
            //        var addedUser = repository.Add(user);

            //        //Assert
            //        if (addedUser != null)
            //        {
            //            System.Diagnostics.Debug.WriteLine($"User added: {addedUser.uname}, Email: {addedUser.email}");
            //        }
            //        else
            //        {
            //            System.Diagnostics.Debug.WriteLine("Failed to add user.");
            //        }
            //    }

            //}

            //static void getUserById(Guid id)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();
            //    var userId = id;

            //    //Act
            //    var user = repository.GetById(userId);
                
            //    //Assert
            //    if (user != null)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"User found: {user.uname}, Email: {user.email}");
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("User not found.");
            //    }
            //}

            //static bool userExists(Guid id)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();
            //    var userId = id;

            //    //Act
            //    var exists = repository.Exists(userId);

            //    //Assert
            //    System.Diagnostics.Debug.WriteLine($"User exists: {exists}");
            //    return exists;
            //}
            
            //static void deleteUserById(Guid id)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();
            //    var userId = id;

            //    //Act
            //    var user = repository.GetById(userId);
            //    if (user != null)
            //    {
            //        repository.Delete(user);
            //        System.Diagnostics.Debug.WriteLine($"User deleted: {user.uname}, Email: {user.email}");
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("User not found for deletion.");
            //    }
            //}

            //static int countUsers(Func<User, bool> predicate)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();

            //    //Act
            //    var count = repository.Count(predicate);

            //    //Assert
            //    System.Diagnostics.Debug.WriteLine($"User count: {count}");
            //    return count;
            //}

            //static bool findUser(Func<User, bool> predicate)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();

            //    //Act
            //    var user = repository.Find(predicate).FirstOrDefault();

            //    //Assert
            //    if (user != null)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"User found: {user.uname}, Email: {user.email}");
            //        return true;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("User not found.");
            //        return false;
            //    }
            //}

            //static User getUserByUname(string uname)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();

            //    //Act
            //    var user = repository.Find(u => u.uname == uname).FirstOrDefault();

            //    //Assert
            //    if (user != null)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"User found: {user.uname}, Email: {user.email}");
            //        return user;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("User not found.");
            //        return new User();
            //    }
            //}

            //static User? updateUser(User user)
            //{
            //    //Arrange
            //    var repository = CreateUserRepository();

            //    //Act
            //    var updatedUser = repository.Update(user);

            //    //Assert
            //    if (updatedUser != null)
            //    {
            //        System.Diagnostics.Debug.WriteLine($"User updated: {updatedUser.uname}, Email: {updatedUser.email}");
            //        return updatedUser;
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debug.WriteLine("Failed to update user.");
            //        return null;
            //    }
            //}
            //#endregion
        }

        private static void Initialize()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config = builder.Build();
        }

        //public static GenericRepository<User> CreateUserRepository()
        //{
        //    var connectionString = config.GetConnectionString("DefaultConnection");
        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
        //    }
        //    return new UserRepository(connectionString);
        //}
    }
}