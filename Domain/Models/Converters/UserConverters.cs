using Domain.Models.Responses;
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
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted,
                ValidEmail = user.ValidEmail
            };
        }

        public static GetUserResponse ConvertToGetUserResponse(this User user)
        {
            return new GetUserResponse
            {
                ID = user.ID,
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted,
                ValidEmail = user.ValidEmail,
            };
        }

        public static UpdateUserResponse ConvertToUpdateUserResponse(this User user)
        {
            return new UpdateUserResponse
            {
                ID = user.ID,
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted,
                ValidEmail = user.ValidEmail,
            };
        }

        public static DeleteUserResponse ConvertToDeleteUserResponse(this User user)
        {
            return new DeleteUserResponse
            {
                ID = user.ID,
                Email = user.Email,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Phone = user.Phone,
                Lastlogin = user.Lastlogin,
                CreatedAt = user.Createdat,
                LastupdatedAt = user.Lastupdatedat,
                Deleted = user.Deleted,
                ValidEmail = user.ValidEmail,
            };
        }

        public static RestoreUserResponse ConvertToRestoreUserResponse(this User user)
        {
            return new RestoreUserResponse
            {
                Email = user.Email.Split('@')[0], // Extract username part of email
                Message = "Your account has been successfully restored."
            };
        }
    }
}