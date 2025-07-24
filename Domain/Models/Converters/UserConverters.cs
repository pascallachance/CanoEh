using Infrastructure.Data;

namespace Domain.Models.Converters
{
    public static class UserConverters
    {
        public static CreateUserResponse ConvertToCreateUserResponse(this User user)
        {
            return new CreateUserResponse
            {
                ID = user.ID,
                Uname = user.Uname,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted
            };
        }

        public static GetUserResponse ConvertToGetUserResponse(this User user)
        {
            return new GetUserResponse
            {
                ID = user.ID,
                Uname = user.Uname,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted
            };
        }

        public static UpdateUserResponse ConvertToUpdateUserResponse(this User user)
        {
            return new UpdateUserResponse
            {
                ID = user.ID,
                Uname = user.Uname,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted
            };
        }
    }
}