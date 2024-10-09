using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SecMan.Data.Repository;
using SecMan.Interfaces.BL;
using SecMan.Model;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using User = SecMan.Model.User;


namespace SecMan.BL
{
    public class UserBL : IUserBL, IAuthBL
    {
        private readonly IEncryptionDecryption _encryptionDecryption;
        private readonly JwtTokenOptions _jwt;

        private readonly IUnitOfWork _unitOfWork;

        public UserBL(IEncryptionDecryption encryptionDecryption,
                      IOptionsSnapshot<JwtTokenOptions> tokenOptions,
                      IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _encryptionDecryption = encryptionDecryption;
            _jwt = tokenOptions.Value ?? new JwtTokenOptions();
        }        
            
        public async Task<ServiceResponse<UsersWithCountDto>> GetUsersAsync(UsersFilterDto model)
        {
            Log.Information("Starting to retrieve all users. based on filters {@Filters}", model);

            var usersQuery = await _unitOfWork.IUserRepository.GetAll(r => r.Roles);            
          

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                Log.Information("Attaching Username filter to above query");
                usersQuery = usersQuery.Where(x => x.UserName != null &&
                                           x.UserName
                                          .ToLower()
                                          .Replace(" ", string.Empty)
                                          .Contains(model.Username
                                                         .ToLower()
                                                         .Replace(" ", string.Empty)));
            }
            if (model.Role?.Count > 0)
            {
                Log.Information("Attaching role filter to above query");
                usersQuery = usersQuery.Where(x => x.Roles.Exists(x => x.Name != null && model.Role.Contains(x.Name.ToLower().Replace(" ", string.Empty))));
            }
            if (model.Status?.Count > 0)
            {
                Log.Information("Attaching Status filter to above query");
                usersQuery = usersQuery.Where(x =>
                                             (model.Status.Contains("active") && x.IsActive) ||
                                             (model.Status.Contains("inactive") && !x.IsActive) ||
                                             (model.Status.Contains("retired") && x.Retired) ||
                                             (model.Status.Contains("locked") && x.Locked)
                                             );
            }

            Log.Information("Getting count of all the users after filter");
            int userCount = usersQuery.Count();


            Log.Information("Getting list of users by adding the pagination as Offset: {@Offset} and Limit {@Limit}", model.Offset, model.Limit);
            List<Model.User> users = usersQuery.Skip(model.Offset * model.Limit).Take(model.Limit).Select(user => new Model.User
            {
                IsActive = user.IsActive,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,    
                FirstName = user.FirstName,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLegacy = user.IsLegacy,
                UserAttributes = new List<UserAttributeDto>(), // Assuming this will be filled later
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.IsPasswordExpiryEnable,
                IsRetired = user.Retired,
                Language = user.Language,
                LastLogin = user.LastLoginDate,
                LastName = user.LastName,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Username = user.UserName,
                Roles = user.Roles.Select(r => new Model.RoleModel
                {
                    Description = r.Description,
                    Id = r.Id,
                    IsLoggedOutType = r.IsLoggedOutType,
                    Name = r.Name,
                    LinkUsers = r.Users.Select(u => u.Id).ToList()
                }).ToList()
            }).ToList();

            Log.Information("Completed retriving all the users based on the filters {@Filters}", model);
            return new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = userCount, Users = users });
        }
        public async Task<ServiceResponse<User>> AddUserAsync(CreateUser model)
        {
            Log.Information("Starting to add user. User Name: {@Username}", model.Username);

            var user = await _unitOfWork.IUserRepository.GetUserByUsername(model.Username);
            if (user is not null)
            {
                Log.Information("User with username {@Username} already present", model.Username);
                return new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict);
            }

            List<RoleModel> roles = await _unitOfWork.IUserRepository.GetRolesByRoleId(model.Roles);
            if (roles.Count != model.Roles.Count)
            {
                Log.Information("Some of the roles in {@Roles} is not present in db", model.Roles);
                return new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest);
            }

            model.Password = _encryptionDecryption.EncryptPassword(model.Password, model.IsLegacy);

            Log.Information("Adding new User to database. User Name: {@Username}", model.Username);

            var res = await _unitOfWork.IUserRepository.AddUserAsync(model);
           

            Log.Information("User creation completed. User ID: {@UserId}", user);

            return new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, res);
        }

        public async Task<ServiceResponse<Model.User>> GetUserByIdAsync(ulong userId)
        {
            Log.Information("Starting to retrieve user by id, User Id {@UserId}", userId);

            var user = await _unitOfWork.IUserRepository.GetById(userId,r=>r.Roles);
            if (user is null)
                return new ServiceResponse<Model.User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest);

            var newUser = new Model.User
            {
                IsActive = user.IsActive,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,
                FirstName = user.FirstName,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLegacy = user.IsLegacy,
                UserAttributes = new List<UserAttributeDto>(),
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.IsPasswordExpiryEnable,
                IsRetired = user.Retired,
                Language = user.Language,
                LastLogin = user.LastLoginDate,
                LastName = user.LastName,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Username = user.UserName,
                Roles = user.Roles.Select(role => new Model.RoleModel
                {
                    Description = role.Description,
                    Id = role.Id,
                    IsLoggedOutType = role.IsLoggedOutType,
                    Name = role.Name,
                    LinkUsers = role.Users.Select(x=>role.Id).ToList()
                }).ToList()
            };

            Log.Information("Validating for invalid user id");
           

            Log.Information("Completed retriving user by id, {@UserId}", userId);

            return new ServiceResponse<Model.User>(ResponseConstants.Success, HttpStatusCode.OK, newUser);
        }

        public async Task<ServiceResponse<User>> UpdateUserAsync(UpdateUser model, ulong userId)
        {
            Log.Information("Starting to update user with id : {@UserId}", userId);

            var user = await _unitOfWork.IUserRepository.GetById(userId);

            Log.Information("Validating for invalid user id");
            if (user is null)
                return new ServiceResponse<User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest);

            List<RoleModel> roles = await _unitOfWork.IUserRepository.GetRolesByRoleId(model.Roles);
            if (roles.Count != model.Roles.Count)
            {
                Log.Information("Some of the roles in {@Roles} is not present in db", model.Roles);
                return new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest);
            }

            var res = await _unitOfWork.IUserRepository.UpdateUserAsync(model, userId);
                       
            Log.Information("User Updation completed. User ID: {@UserId}", userId);

            return new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, res);
        }

        public async Task<ApiResponse> DeleteUserAsync(ulong userId)
        {
            Log.Information("Starting to Delete user with id : {@UserId}", userId);
            var result = await _unitOfWork.IUserRepository.Delete(userId);
            Log.Information("User deletion status", result);
            if(!result)
            {
                return new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest);
            }

           _unitOfWork.SaveChanges();
            Log.Information("Deleted user from db with user id {@UserId}", userId);
            return new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK);
        }


        public async Task<ServiceResponse<LoginServiceResponse>> LoginAsync(LoginRequest model)
        {
            Log.Information("Starting BAL's LoginAsync Method");

            var user = await _unitOfWork.IUserRepository.GetUserByUsername(model.Username);
            if (user is null)
            {
                Log.Information("User Object From db is null, so returning UserDoesNotExists message with Unauthorized Status code");
                return new(ResponseConstants.UserDoesNotExists, HttpStatusCode.Unauthorized);
            }

            var userStatus = CheckUserStatus(user);

            if (userStatus != default)
            {
                Log.Information("Completed BAL's LoginAsync Method");
                return userStatus;
            }

            if (await VarifyUserPassword(user, model.Password))
            {
                Log.Information("Varified user input password is equal to password stored in db");

                var userDetails = await _unitOfWork.IUserRepository.GetUserDetails(user.Id);
                Log.Information("successfully fetched user details to store in token payload, details are : {@Details}", userDetails);

                var token = GenerateAccessToken(userDetails.Item1, userDetails.Item2);
                Log.Information("Access Token is generated successfully");

                var ssoSessionId = RandomStringGenerator.Generate(12);
                Log.Information("Session Id is generated successfully, Session Id : {@SessionId}", ssoSessionId);

                await _unitOfWork.IUserRepository.UpdateUserSessionDetails(user.Id, ssoSessionId, _jwt.TokenExpireTime, true);
                Log.Information("Updated Session details into db WRT to user with expiry");

                await _unitOfWork.IUserRepository.LogLoginAttempts(user.Id, true);

                Log.Information("Completed BAL's LoginAsync Method");
                return new ServiceResponse<LoginServiceResponse>(ResponseConstants.Success, HttpStatusCode.OK, new LoginServiceResponse
                {
                    Token = token,
                    SSOSessionId = ssoSessionId,
                    ExpiresIn = _jwt.TokenExpireTime
                });
            }
            Log.Information("Input Password : {@Password} does not match with the password saved in db", model.Password);

            await _unitOfWork.IUserRepository.LogLoginAttempts(user.Id, false);

            Log.Information("returning InvalidPassword message with Unauthorized status code");
            Log.Information("Completed BAL's LoginAsync Method");
            return new(ResponseConstants.InvalidPassword, HttpStatusCode.Unauthorized);
        }


        private static ServiceResponse<LoginServiceResponse>? CheckUserStatus(SecMan.Data.SQLCipher.User user)
        {
            Log.Information("Checking if User is locked,retired or password is expired");
            if (user.Locked)
            {
                return new(ResponseConstants.AccountLocked, HttpStatusCode.Unauthorized);
            }
            if (user.Retired)
            {
                return new(ResponseConstants.AccountRetired, HttpStatusCode.Unauthorized);
            }
            if (user.PasswordExpiryEnable && user.PasswordExpiryDate < DateTime.Now)
            {
                return new(ResponseConstants.PasswordExpired, HttpStatusCode.Unauthorized);
            }
            return default;
        }



        private async Task<bool> VarifyUserPassword(SecMan.Data.SQLCipher.User user, string password)
        {
            Log.Information("Starting VarifyUserPassword Method with user object : {@User} and password to verify is: {@Password}", user, password);

            var encryptedPassword = user.Password;
            Log.Information("Fetched User's Encrypted Password from db");

            Log.Information("Validating if the Password is null or white space");
            if (!string.IsNullOrWhiteSpace(encryptedPassword))
            {
                Log.Information("Verified that the Encrypted password is not null or white space");

                if (user.IsLegacy)
                {
                    Log.Information("Decrypting Encrypted password to check if the input password matchs with decrypted password since the user is Legacy User");
                    var decryptedPassword = _encryptionDecryption.DecryptPasswordAES256(encryptedPassword);

                    Log.Information("Completed VarifyUserPassword Method");
                    return decryptedPassword == password;
                }
                else
                {
                    Log.Information("Verifying the password hash for input password and encryped password to check if both the passwords match since its an Non Legacy User");
                    Log.Information("Completed VarifyUserPassword Method");
                    return _encryptionDecryption.VerifyHashPassword(encryptedPassword, password);
                }
            }

            Log.Information("if encrypted password is null or white space, then returning false");
            Log.Information("Completed VarifyUserPassword Method");
            return false;
        }


        private string GenerateAccessToken(UserDetails? userDetails, List<AppPermissions>? appPermissions)
        {
            var key = Encoding.ASCII.GetBytes(_jwt.SecretKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim("user_attributes", JsonConvert.SerializeObject(userDetails)),
                new Claim("apps", JsonConvert.SerializeObject(appPermissions)),
                new Claim("sub", userDetails?.Username ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, Convert.ToString(userDetails?.Id) ?? string.Empty),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwt.TokenExpireTime),
                Issuer = _jwt.ValidIssuer,
                Audience = _jwt.ValidAudience,  
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<ServiceResponse<LoginServiceResponse>> ValidateSessionAsync(string ssoSessionId)
        {
            Log.Information("Starting BAL's ValidateSessionAsync Method");

            var userId = await _unitOfWork.IUserRepository.GetUserBySessionId(ssoSessionId);
            Log.Information("Checking if the userId from db is null");

            if (userId == null)
            {
                Log.Information("returning InvalidSessionId message with Unauthorized Status code since UserId is null");
                Log.Information("Completed BAL's ValidateSessionAsync Method");
                return new(ResponseConstants.InvalidSessionId, HttpStatusCode.Unauthorized);
            }

            var userDetails = await _unitOfWork.IUserRepository.GetUserDetails(userId.Value);
            Log.Information("successfully fetched user details to store in token payload, details are : {@Details}", userDetails);

            var token = GenerateAccessToken(userDetails.Item1, userDetails.Item2);
            Log.Information("Access Token is generated successfully");

            await _unitOfWork.IUserRepository.UpdateUserSessionDetails(userId.Value, ssoSessionId, _jwt.TokenExpireTime, true);
            Log.Information("Updated Session details into db WRT to user with expiry");

            Log.Information("Returning session id and new token with expiry time");
            Log.Information("Completed BAL's ValidateSessionAsync Method");
            return new ServiceResponse<LoginServiceResponse>(ResponseConstants.Success, HttpStatusCode.OK, new LoginServiceResponse
            {
                Token = token,
                SSOSessionId = ssoSessionId,
                ExpiresIn = _jwt.TokenExpireTime
            });

        }
    }
}
