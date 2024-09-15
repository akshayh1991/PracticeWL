using Microsoft.EntityFrameworkCore;
using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using Serilog;
using System.Net;
using static SecMan.Model.User;


namespace SecMan.BL
{
    public class UserBL : IUserBL
    {
        private readonly IUserDAL _userDAL;
        private readonly IEncryptionDecryption _encryptionDecryption;

        public UserBL(IUserDAL userDAL, IEncryptionDecryption encryptionDecryption)
        {
            _userDAL = userDAL;
            _encryptionDecryption = encryptionDecryption;
        }

        public async Task<ServiceResponse<UserDto>> AddUserAsync(AddUserDto model)
        {
            Log.Information("Starting to add user. User Name: {Username}", model.Username);

            var user = await _userDAL.GetUser()
                                     .Where(x => x.Username != null &&
                                                 model.Username != null &&
                                                 x.Username
                                                 .ToLower()
                                                 .Equals(model.Username
                                                             .ToLower()))
                                     .FirstOrDefaultAsync();
            if (user is not null)
            {
                Log.Information("User with username {Username} already present", model.Username);
                return new ServiceResponse<UserDto>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict);
            }

            Log.Information("Retriving all the that need to assigned to the user");
            var roles = await _userDAL.GetRoles()
                                      .Where(x => model.Roles
                                                       .Select(a => a.Id)
                                                       .Contains(x.Id))
                                      .ToListAsync();
            if (roles.Count != model.Roles.Count)
            {
                Log.Information("Some of the roles in {Roles} is not present in db", model.Roles);
                return new ServiceResponse<UserDto>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest);
            }

            Log.Information("Validating Boolean values and assigning its related dates");
            model = ValidateAddUserDto(model);

            model.Password = _encryptionDecryption.EncryptPassword(model.Password, model.IsLegacy);

            Log.Information("Adding new User to database. User Name: {Username}", model.Username);

            var userId = await _userDAL.AddUserAsync(model);

            Log.Information("User added successfully with ID: {userId}", userId);
            var res = new UserDto
            {
                Id = userId,
                Description = model.Description,
                Domain = model.Domain,
                Email = model.Email,
                FirstName = model.FirstName,
                Language = model.Language,
                LastName = model.LastName,
                Password = model.Password,
                PasswordExpiryDate = model.PasswordExpiryDate,
                RetiredDate = model.RetiredDate,
                IsActive = model.IsActive,
                UserAttributes = model.UserAttributes,
                InactiveDate = model.InactiveDate,
                IsLegacy = model.IsLegacy,
                IsLocked = model.IsLocked,
                IsPasswordExpiryEnabled = model.IsPasswordExpiryEnabled,
                IsRetired = model.IsRetired,
                LastLogin = model.LastLogin,
                LockedDate = model.LockedDate,
                LockedReason = model.LockedReason,
                ResetPassword = model.ResetPassword,
                Username = model.Username
            };

            Log.Information("User creation completed. User ID: {userId}", userId);

            return new ServiceResponse<UserDto>(ResponseConstants.Success, HttpStatusCode.OK, res);
        }


        public async Task<ServiceResponse<UsersWithCountDto>> GetUsersAsync(UsersFilterDto model)
        {
            Log.Information("Starting to retrieve all users. based on filters {filters}", model);

            Log.Information("Retriving all users from db as queryable");
            var usersQuery = _userDAL.GetUser();


            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                Log.Information("Attaching Username filter to above query");
                usersQuery = usersQuery.Where(x => x.Username != null &&
                                           x.Username
                                          .ToLower()
                                          .Replace(" ", string.Empty)
                                          .Contains(model.Username
                                                         .ToLower()
                                                         .Replace(" ", string.Empty)));
            }
            if (model.Role?.Count > 0)
            {
                Log.Information("Attaching role filter to above query");
                usersQuery = usersQuery.Where(x => x.Roles.Any(x => x.Name != null && model.Role.Contains(x.Name)));
            }
            if (model.Status?.Count > 0)
            {
                Log.Information("Attaching Status filter to above query");
                usersQuery = usersQuery.Where(x =>
                                             (model.Status.Contains("active") && x.IsActive) ||
                                             (model.Status.Contains("inactive") && !x.IsActive) ||
                                             (model.Status.Contains("retired") && x.IsRetired) ||
                                             (model.Status.Contains("locked") && x.IsLocked)
                                             );
            }

            Log.Information("Getting count of all the users after filter");
            var userCount = await usersQuery.CountAsync();

            Log.Information("Getting list of users by adding the pagination as Offset: {Offset} and Limit {Limit}", model.Offset, model.Limit);
            var users = await usersQuery.Skip(model.Offset).Take(model.Limit).ToListAsync();

            Log.Information("Completed retriving all the users based on the filters {filters}", model);
            return new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = userCount, Users = users });
        }


        public async Task<ServiceResponse<UserDto>> GetUserByIdAsync(ulong userId)
        {
            Log.Information("Starting to retrieve user by id, User Id {userId}", userId);

            Log.Information("Retriving the user from db by user id : {userId}", userId);
            var user = await _userDAL.GetUser()
                                     .Where(x => x.Id == userId)
                                     .FirstOrDefaultAsync();

            Log.Information("Validating for invalid user id");
            if (user is null)
                return new ServiceResponse<UserDto>(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest);

            Log.Information("Completed retriving user by id, {userId}", userId);

            return new ServiceResponse<UserDto>(ResponseConstants.Success, HttpStatusCode.OK, user);
        }


        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(AddUserDto model, ulong userId)
        {
            Log.Information("Starting to update user with id : {userId}", userId);

            Log.Information("Retriving the user from db by user id : {userId}", userId);
            var user = await _userDAL.GetUser()
                                     .Where(x => x.Id == userId)
                                     .FirstOrDefaultAsync();

            Log.Information("Validating for invalid user id");
            if (user is null)
                return new ServiceResponse<UserDto>(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest);

            Log.Information("Retriving all the that need to assigned to the user");
            var roles = await _userDAL.GetRoles()
                                      .Where(x => model.Roles
                                                       .Select(x => x.Id)
                                                       .Contains(x.Id))
                                      .AsNoTracking()
                                      .ToListAsync();
            if (roles.Count != model.Roles.Count)
            {
                Log.Information("Some of the roles in {Roles} is not present in db", model.Roles);
                return new ServiceResponse<UserDto>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest);
            }

            Log.Information("Validating Boolean values and assigning its related dates");
            model = ValidateAddUserDto(model);

            Log.Information("Updating the User to database. User Id: {userId}", userId);
            await _userDAL.UpdateUserAsync(model, userId);
            Log.Information("User updated successfully with ID: {userId}", userId);

            var res = new UserDto
            {
                Id = userId,
                Description = model.Description,
                Domain = model.Domain,
                Email = model.Email,
                FirstName = model.FirstName,
                Language = model.Language,
                LastName = model.LastName,
                PasswordExpiryDate = model.PasswordExpiryDate,
                RetiredDate = model.RetiredDate,
                IsActive = model.IsActive,
                UserAttributes = model.UserAttributes,
                InactiveDate = model.InactiveDate,
                IsLegacy = model.IsLegacy,
                IsLocked = model.IsLocked,
                IsPasswordExpiryEnabled = model.IsPasswordExpiryEnabled,
                IsRetired = model.IsRetired,
                LastLogin = model.LastLogin,
                LockedDate = model.LockedDate,
                LockedReason = model.LockedReason,
                ResetPassword = model.ResetPassword,
                Username = model.Username
            };
            Log.Information("User Updation completed. User ID: {userId}", userId);

            return new ServiceResponse<UserDto>(ResponseConstants.Success, HttpStatusCode.OK, res);
        }


        private static AddUserDto ValidateAddUserDto(AddUserDto model)
        {
            // need to update password enpiry based on config in future
            if (model.IsPasswordExpiryEnabled)
                model.PasswordExpiryDate = DateTime.Now.AddDays(30);

            if (!model.IsActive)
                model.InactiveDate = DateTime.Now;

            if (model.IsLocked)
                model.LockedDate = DateTime.Now;

            if (model.IsRetired)
                model.RetiredDate = DateTime.Now;

            return model;
        }


        public async Task<ApiResponse> DeleteUserAsync(ulong userId)
        {
            Log.Information("Starting to Delete user with id : {userId}", userId);
            var user = await _userDAL.GetUser()
                                     .Where(x => x.Id == userId)
                                     .FirstOrDefaultAsync();
            Log.Information("Validating for invalid user id");
            if (user is null)
                return new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest);
            Log.Information("Deleting user from db with user id {userId}", userId);
            await _userDAL.DeleteUserAsync(userId);

            Log.Information("Deleted user from db with user id {userId}", userId);
            return new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK);
        }
    }
}
