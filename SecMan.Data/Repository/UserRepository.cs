using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace SecMan.Data.DAL
{
    public class UserRepository : GenericRepository<SQLCipher.User>, IUserRepository
    {
        private readonly Serilog.ILogger _logger = Log.ForContext<UserRepository>();
        private Db _context { get; }
        public UserRepository(Db context) : base(context)
        {
            _context = context;
        }

        public async Task<Model.User> UpdateUserAsync(UpdateUser model, ulong userId)
        {
            _logger.Information("Fetch user object by user Id : {@UserId} to update", userId);
           
            var user = await _context.Users
                                     .Where(x => x.Id == userId)
                                     .Include(x => x.Roles)
                                     .FirstOrDefaultAsync();

            if (user is null)
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            user.Description = model.Description;
            user.Domain = model.Domain;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.Language = model.Language;
            user.LastName = model.LastName;
            user.IsLegacy = model.IsLegacy;
            user.PasswordExpiryDate = model.PasswordExpiryDate;
            user.IsPasswordExpiryEnable = model.IsPasswordExpiryEnabled;
            user.UserName = model.Username;
            user.IsActive = model.IsActive;

            var roles = _context.Roles.Where(x => model.Roles.Contains(x.Id)).ToListAsync();

            user.Roles = roles.Result;

            _logger.Information("Validating Add User Data");
            user = ValidateAddUserDto(user);

            _context.Update(user);
            await _context.SaveChangesAsync();

            return new Model.User()
            {
                IsActive = user.IsActive,
                UserAttributes = model.UserAttributes,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,
                FirstName = user.FirstName,
                Language = user.Language,
                LastName = user.LastName,
                IsLegacy = user.IsLegacy,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                IsRetired = user.Retired,
                LastLogin = user.LastLoginDate,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Roles = user.Roles.Select(x => new Model.RoleModel
                {
                    Description = x.Description,
                    Id = x.Id,
                    IsLoggedOutType = x.IsLoggedOutType,
                    Name = x.Name,
                }).ToList(),
                Username = user.UserName,
            };
        }

        private static SQLCipher.User ValidateAddUserDto(SQLCipher.User model)
        {
            // need to update password enpiry based on config in future
            if (model.PasswordExpiryEnable)
                model.PasswordExpiryDate = DateTime.Now.AddDays(30);

            if (!model.IsActive)
                model.InActiveDate = DateTime.Now;

            if (model.Locked)
                model.LockedDate = DateTime.Now;

            if (model.Retired)
                model.RetiredDate = DateTime.Now;

            return model;
        }


        public async Task<List<RoleModel>> GetRolesByRoleId(List<ulong> roleIds)
        {
            var result = _context.Roles.Where(x => roleIds.Contains(x.Id));

            return await result.Select(r => new RoleModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsLoggedOutType = r.IsLoggedOutType,
                LinkUsers = r.Users.Select(x => x.Id).ToList()
            }).ToListAsync();
        }


        public async Task<Model.User> AddUserAsync(CreateUser model)
        {
            _logger.Information("Mapping data to dbset user object");
            var user = new SQLCipher.User
            {
                Description = model.Description,
                Domain = model.Domain,
                Email = model.Email,
                FirstName = model.FirstName,
                Language = model.Language,
                LastName = model.LastName,
                IsLegacy = model.IsLegacy,
                Password = model.Password,
                PasswordExpiryDate = model.PasswordExpiryDate,
                IsPasswordExpiryEnable = model.IsPasswordExpiryEnabled,
                UserName = model.Username,
                IsActive = model.IsActive,
                FirstLogin = model.FirstLogin,
                ResetPassword = model.ResetPassword,
            };

            _logger.Information("fetching all the roles which are need to be associated with user");
            var roles = await _context.Roles.Where(x => model.Roles.Contains(x.Id)).ToListAsync();

            user.Roles = roles;

            _logger.Information("Validating Boolean values and assigning its related dates");
            user = ValidateAddUserDto(user);
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();

            return new Model.User()
            {
                IsActive = user.IsActive,
                UserAttributes = model.UserAttributes,
                Description = user.Description,
                Domain = user.Domain,
                Email = user.Email,
                FirstName = user.FirstName,
                Language = user.Language,
                LastName = user.LastName,
                IsLegacy = user.IsLegacy,
                Id = user.Id,
                InactiveDate = user.InActiveDate,
                IsLocked = user.Locked,
                IsPasswordExpiryEnabled = user.PasswordExpiryEnable,
                IsRetired = user.Retired,
                LastLogin = user.LastLoginDate,
                LockedDate = user.LockedDate,
                LockedReason = user.LockedReason,
                PasswordExpiryDate = user.PasswordExpiryDate,
                ResetPassword = user.ResetPassword,
                RetiredDate = user.RetiredDate,
                Roles = user.Roles.Select(x => new Model.RoleModel
                {
                    Description = x.Description,
                    Id = x.Id,
                    IsLoggedOutType = x.IsLoggedOutType,
                    Name = x.Name,
                }).ToList(),
                Username = user.UserName,
            };
        }

        public async Task<SecMan.Data.SQLCipher.User> GetUserByUsername(string? username)
        {
            if (username != null)
            {
                return await _context.Users.Where(x => x.UserName != null &&
                                                  x.UserName.ToLower().Equals(username.ToLower()))
                                                  .FirstOrDefaultAsync();
            }
            return null;
        }


        public async Task UpdateUserSessionDetails(ulong userId, string sessionId, double sessionExpiryTime, bool isLogin)
        {
            var user = await _context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            {
                user.SessionId = sessionId;
                user.LastLoginDate = isLogin ? DateTime.Now : DateTime.MinValue;
                user.LastLogoutDate = isLogin ? DateTime.MinValue : DateTime.Now;
                user.SessionExpiry = isLogin ? DateTime.Now.AddMinutes(sessionExpiryTime) : DateTime.MinValue;
                if (user.FirstLogin)
                {
                    user.FirstLogin = false;
                }
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<Tuple<UserDetails?, List<AppPermissions>?>> GetUserDetails(ulong userId)
        {
            var userDetails = new UserDetails();
            var permissions = new List<AppPermissions>();
            var user = await _context.Users
                                .Include(x => x.Roles)
                                .Where(x => x.Id == userId)
                                .FirstOrDefaultAsync();
            if (user is not null)
            {
                var userDetailsObject = new User(user, true);
                userDetails = new UserDetails
                {
                    IsActive = user.IsActive,
                    Description = user.Description,
                    Domain = user.Domain,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Id = user.Id,
                    IsLegacy = user.IsLegacy,
                    IsLocked = user.Locked,
                    IsRetired = user.Retired,
                    Language = user.Language,
                    LastName = user.LastName,
                    Roles = user.Roles.Select(x => x.Name).ToList(),
                    Username = user.UserName,
                };
                if (user.PasswordExpiryEnable && user.PasswordExpiryDate < DateTime.Now)
                {
                    userDetails.IsExpired = true;
                }
                else
                {
                    userDetails.IsExpired = false;
                }

                foreach (var p in userDetailsObject.GetAppPerms())
                {
                    var permission = new AppPermissions
                    {
                        Name = p.Key,
                        Permission = p.Value
                    };
                    permissions.Add(permission);
                }
            }
            else
            {
                userDetails = default;
                permissions = default;
            }
            return new Tuple<UserDetails?, List<AppPermissions>?>(userDetails, permissions);
        }


        public async Task<SecMan.Data.SQLCipher.User> GetUserById(ulong id)
        {
            return await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
        }


        public async Task<string?> GetUserPasswordByUserId(ulong userId)
        {
            var password = await _context.Users.Where(x => x.Id == userId).Select(x => x.Password).FirstOrDefaultAsync();
            return password;
        }

        public async Task<ulong?> GetUserBySessionId(string sessionId)
        {
            ulong? user = await _context.Users.Where(x => x.SessionId == sessionId &&
                                                      x.SessionExpiry > DateTime.Now)
                                          .Select(x => x.Id)
                                          .FirstOrDefaultAsync();
            return user;
        }


        public async Task LogLoginAttempts(ulong userId, bool isLoginSuccess)
        {
            var count = await _context.SysFeatProps.Where(x => x.Name == "Max Login Attempts")
                                              .Select(x => x.ValMax)
                                              .FirstOrDefaultAsync();
            var user = await _context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
            if (user is not null)
            {
                var loginLog = new LoginLogs
                {
                    ActionDate = DateTime.Now,
                    IsSuccessfullyLoggedIn = isLoginSuccess,
                    User = user
                };
                await _context.AddAsync(loginLog);
                await _context.SaveChangesAsync();

                if (isLoginSuccess)
                {
                    Log.Information("Removing old login logs since this is a successfull login");
                    var userLogs = await _context.LoginLogs.Where(x => x.User == user).ToListAsync();
                    _context.RemoveRange(userLogs);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var userLogs = await _context.LoginLogs
                                            .Where(x => x.User == user)
                                            .OrderByDescending(x => x.ActionDate)
                                            .Take((int)count)
                                            .ToListAsync();
                    Log.Information("User has made {@Count} failed login attempts", userLogs.Count);
                    if (userLogs.Count >= (int)count && userLogs.All(x => !x.IsSuccessfullyLoggedIn))
                    {
                        Log.Information("User is locked due to Exceeding Max Failed login Attempts");
                        user.Locked = true;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
