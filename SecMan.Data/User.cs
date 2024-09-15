using Microsoft.EntityFrameworkCore;
using SecMan.Interfaces.DAL;
using Serilog;
using static SecMan.Model.User;

namespace SecMan.Data
{
    public class User : IUserDAL
    {
        private enum Property
        {
            Domain,
            UserName,
            Password,
            PasswordDate,
            ChangePassword,
            PasswordExpiryEnable,
            PasswordExpiryDate,
            LastLoginDate,
            RFI,
            RFIDate,
            Biometric,
            BiometricDate,
            FirstName,
            LastName,
            Description,
            Language,
            Email,
            Enabled,
            EnabledDate,
            Retired,
            RetiredDate,
            Locked,
            LockedReason,
            LockedDate,
            LastLogoutDate,
            SessionId,
            SessionExpiry,
            LegacySupport
        }

        internal User(SQLCipher.User sqlCipherUser, bool includeRoles)
        {
            Id = sqlCipherUser.Id;
            _domain = sqlCipherUser.Domain;
            _userName = sqlCipherUser.UserName;
            _password = sqlCipherUser.Password;
            _passwordDate = sqlCipherUser.PasswordDate;
            _changePassword = sqlCipherUser.ChangePassword;
            _passwordExpiryEnable = sqlCipherUser.PasswordExpiryEnable;
            _passwordExpiryDate = sqlCipherUser.PasswordExpiryDate;
            _lastLoginDate = sqlCipherUser.LastLoginDate;
            _rfi = sqlCipherUser.RFI;
            _rfiDate = sqlCipherUser.RFIDate;
            _biometric = sqlCipherUser.Biometric;
            _biometricDate = sqlCipherUser.BiometricDate;
            _firstName = sqlCipherUser.FirstName;
            _lastName = sqlCipherUser.LastName;
            _description = sqlCipherUser.Description;
            _language = sqlCipherUser.Language;
            _email = sqlCipherUser.Email;
            _enabled = sqlCipherUser.Enabled;
            _enabledDate = sqlCipherUser.EnabledDate;
            _retired = sqlCipherUser.Retired;
            _retiredDate = sqlCipherUser.RetiredDate;
            _locked = sqlCipherUser.Locked;
            _lockedReason = sqlCipherUser.LockedReason;
            _lockedDate = sqlCipherUser.LockedDate;
            _lastLogoutDate = sqlCipherUser.LastLogoutDate;
            _sessionId = sqlCipherUser.SessionId;
            _sessionExpiry = sqlCipherUser.SessionExpiry;
            _legacySupport = sqlCipherUser.LegacySupport;

            if (includeRoles)
            {
                if (Roles == null) Roles = new List<Role>();
                sqlCipherUser.Roles
                    .ToList()
                    .ForEach(o => Roles.Add(new(o, false)));
            }
            else
            {
                Roles = null;
            }

        }

        public User()
        {

        }


        public ulong Id { get; set; }
        private string _domain = string.Empty;
        public string GetDomain() { return _domain; }
        public bool SetDomain(string val) { return SetProperty(Property.Domain, val); }
        private string _userName = string.Empty;
        public string GetUserName() { return _userName; }
        public bool SetUserName(string val) { return SetProperty(Property.UserName, val); }
        private string _password = string.Empty;
        public string GetPassword() { return _password; }
        public bool SetPassword(string val) { return SetProperty(Property.Password, val); }
        private DateTime _passwordDate = DateTime.MinValue;
        public DateTime GetPasswordDate() { return _passwordDate; }
        public bool SetPasswordDate(string val) { return SetProperty(Property.PasswordDate, val); }
        private bool _changePassword = false;
        public bool GetChangePassword() { return _changePassword; }
        public bool SetChangePassword(string val) { return SetProperty(Property.ChangePassword, val); }
        private bool _passwordExpiryEnable = false;
        public bool GetPasswordExpiryEnable() { return _passwordExpiryEnable; }
        public bool SetPasswordExpiryEnable(string val) { return SetProperty(Property.PasswordExpiryEnable, val); }
        private DateTime _passwordExpiryDate = DateTime.MinValue;
        public DateTime GetPasswordExpiryDate() { return _passwordExpiryDate; }
        public bool SetPasswordExpiryDate(string val) { return SetProperty(Property.PasswordExpiryDate, val); }
        private DateTime _lastLoginDate = DateTime.MinValue;
        public DateTime GetLastLoginDate() { return _lastLoginDate; }
        public bool SetLastLoginDate(string val) { return SetProperty(Property.LastLoginDate, val); }
        private string _rfi = string.Empty;
        public string GetRFI() { return _rfi; }
        public bool SetRFI(string val) { return SetProperty(Property.RFI, val); }
        private DateTime _rfiDate = DateTime.MinValue;
        public DateTime GetRfiDate() { return _rfiDate; }
        public bool SetRfiDate(string val) { return SetProperty(Property.RFIDate, val); }
        private string _biometric = string.Empty;
        public string GetBiometric() { return _biometric; }
        public bool SetBiometric(string val) { return SetProperty(Property.Biometric, val); }
        private DateTime _biometricDate = DateTime.MinValue;
        public DateTime GetBiometricDate() { return _biometricDate; }
        public bool SetbiometricDate(string val) { return SetProperty(Property.BiometricDate, val); }
        private string _firstName = string.Empty;
        public string GetFirstName() { return _firstName; }
        public bool SetFirstName(string val) { return SetProperty(Property.FirstName, val); }
        private string _lastName = string.Empty;
        public string GetLastName() { return _lastName; }
        public bool SetLastName(string val) { return SetProperty(Property.LastName, val); }
        private string _description = string.Empty;
        public string GetDescription() { return _description; }
        public bool SetDescription(string val) { return SetProperty(Property.Description, val); }
        private string _language = string.Empty;
        public string GetLanguage() { return _language; }
        public bool Setlanguage(string val) { return SetProperty(Property.Language, val); }
        private string _email = string.Empty;
        public string GetEmail() { return _email; }
        public bool SetEmail(string val) { return SetProperty(Property.Email, val); }
        private bool _enabled = false;
        public bool GetEnabled() { return _enabled; }
        public bool SetEnabled(string val) { return SetProperty(Property.Enabled, val); }
        private DateTime _enabledDate = DateTime.MinValue;
        public DateTime GetEnabledDate() { return _enabledDate; }
        public bool SetEnabledDate(string val) { return SetProperty(Property.EnabledDate, val); }
        private bool _retired = false;
        public bool GetRetired() { return _retired; }
        public bool SetRetired(string val) { return SetProperty(Property.Retired, val); }
        private DateTime _retiredDate = DateTime.MinValue;
        public DateTime GetRetiredDate() { return _retiredDate; }
        public bool SetretiredDate(string val) { return SetProperty(Property.RetiredDate, val); }
        private bool _locked = false;
        public bool GetLocked() { return _locked; }
        public bool SetLocked(string val) { return SetProperty(Property.Locked, val); }
        private DateTime _lockedDate = DateTime.MinValue;
        private string _lockedReason = string.Empty;
        public string GetLockedReason() { return _lockedReason; }
        public bool SetLockedReason(string val) { return SetProperty(Property.LockedReason, val); }
        public DateTime GetLockedDate() { return _lockedDate; }
        public bool SetLockedDate(string val) { return SetProperty(Property.LockedDate, val); }
        private bool _legacySupport = false;
        public bool GetLegacySupport() { return _legacySupport; }
        public bool SetLegacySupport(string val) { return SetProperty(Property.LegacySupport, val); }
        private DateTime _lastLogoutDate { get; set; } = DateTime.MinValue;
        public DateTime GetLastLogoutDate() { return _lastLogoutDate; }
        public bool SetLastLogoutDate(string val) { return SetProperty(Property.LastLogoutDate, val); }
        private string _sessionId { get; set; } = string.Empty;
        public string GetSessionId() { return _sessionId; }
        public bool SetSessionId(string val) { return SetProperty(Property.SessionId, val); }
        private DateTime _sessionExpiry { get; set; } = DateTime.MinValue;
        public DateTime GetSessionExpiry() { return _sessionExpiry; }
        public bool SetSessionExpiry(string val) { return SetProperty(Property.SessionExpiry, val); }

        private bool SetProperty(Property property, string val)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Users != null))
                {
                    SQLCipher.User sqlCipherUser = db.Users.Where(o => o.Id == Id).FirstOrDefault();
                    if (sqlCipherUser != null)
                    {
                        switch (property)
                        {
                            case Property.Domain:
                                _domain = val;
                                sqlCipherUser.Domain = val;
                                break;
                            case Property.UserName:
                                _userName = val;
                                sqlCipherUser.UserName = val;
                                break;
                            case Property.Password:
                                _password = val;
                                sqlCipherUser.Password = val;
                                break;
                            case Property.PasswordDate:
                                _passwordDate = DateTime.Parse(val);
                                sqlCipherUser.PasswordDate = _passwordDate;
                                break;
                            case Property.ChangePassword:
                                _changePassword = bool.Parse(val);
                                sqlCipherUser.ChangePassword = _changePassword;
                                break;
                            case Property.PasswordExpiryEnable:
                                _passwordExpiryEnable = bool.Parse(val);
                                sqlCipherUser.PasswordExpiryEnable = _passwordExpiryEnable;
                                break;
                            case Property.PasswordExpiryDate:
                                _passwordExpiryDate = DateTime.Parse(val);
                                sqlCipherUser.PasswordExpiryDate = _passwordExpiryDate;
                                break;
                            case Property.LastLoginDate:
                                _lastLoginDate = DateTime.Parse(val);
                                sqlCipherUser.LastLoginDate = _lastLoginDate;
                                break;
                            case Property.RFI:
                                _rfi = val;
                                sqlCipherUser.RFI = _rfi;
                                break;
                            case Property.RFIDate:
                                _rfiDate = DateTime.Parse(val);
                                sqlCipherUser.RFIDate = _rfiDate;
                                break;
                            case Property.Biometric:
                                _biometric = val;
                                sqlCipherUser.Biometric = val;
                                break;
                            case Property.BiometricDate:
                                _biometricDate = DateTime.Parse(val);
                                sqlCipherUser.BiometricDate = _biometricDate;
                                break;
                            case Property.FirstName:
                                _firstName = val;
                                sqlCipherUser.FirstName = val;
                                break;
                            case Property.LastName:
                                _lastName = val;
                                sqlCipherUser.LastName = val;
                                break;
                            case Property.Description:
                                _description = val;
                                sqlCipherUser.Description = val;
                                break;
                            case Property.Language:
                                _language = val;
                                sqlCipherUser.Language = val;
                                break;
                            case Property.Email:
                                _email = val;
                                sqlCipherUser.Email = val;
                                break;
                            case Property.Enabled:
                                _enabled = bool.Parse(val);
                                sqlCipherUser.Enabled = _enabled;
                                break;
                            case Property.EnabledDate:
                                _enabledDate = DateTime.Parse(val);
                                sqlCipherUser.EnabledDate = _enabledDate;
                                break;
                            case Property.Retired:
                                _retired = bool.Parse(val);
                                sqlCipherUser.Retired = _retired;
                                break;
                            case Property.RetiredDate:
                                _retiredDate = DateTime.Parse(val);
                                sqlCipherUser.RetiredDate = _retiredDate;
                                break;
                            case Property.Locked:
                                _locked = bool.Parse(val);
                                sqlCipherUser.Locked = _locked;
                                break;
                            case Property.LockedReason:
                                _lockedReason = val;
                                sqlCipherUser.LockedReason = _lockedReason;
                                break;
                            case Property.LockedDate:
                                _lockedDate = DateTime.Parse(val);
                                sqlCipherUser.LockedDate = _lockedDate;
                                break;
                            case Property.LastLogoutDate:
                                _lastLogoutDate = DateTime.Parse(val);
                                sqlCipherUser.LastLogoutDate = _lastLogoutDate;
                                break;
                            case Property.SessionId:
                                _sessionId = val;
                                sqlCipherUser.SessionId = _sessionId;
                                break;
                            case Property.SessionExpiry:
                                _sessionExpiry = DateTime.Parse(val);
                                sqlCipherUser.SessionExpiry = _sessionExpiry;
                                break;
                            default:
                                break;
                        }
                        db.SaveChanges();
                        ok = true;
                    }
                }
            }
            catch
            {

                ok = false;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }
        public bool SetProperty(bool val)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                if ((db != null) && (db.Users != null))
                {
                    SQLCipher.User sqlCipherUser = db.Users.Where(o => o.Id == Id).FirstOrDefault();
                    if (sqlCipherUser != null)
                    {
                        //_language = language;
                        //sqlCipherUser.Description = _language;
                        //db.SaveChanges();
                        ok = true;
                    }
                }
            }
            catch { }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }
        public List<Role> Roles { get; set; } = [];

        public bool AddRole(ulong roleId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                using SQLCipher.Db db = new();

                // Get the Role
                SQLCipher.Role? sqlCipherRole = null;
                if (db.Roles != null)
                {
                    sqlCipherRole = db.Roles
                      .Where(x => x.Id == roleId)
                      .FirstOrDefault();
                }

                // Get the User
                if ((sqlCipherRole != null) && (db.Users != null))
                {
                    SQLCipher.User? sqlCipherUser = db.Users
                       .Include(o => o.Roles)
                       .Where(x => x.Id == Id)
                      .FirstOrDefault();

                    if (sqlCipherUser != null)
                    {
                        if (!sqlCipherUser.Roles.Contains(sqlCipherRole))
                        {
                            sqlCipherUser.Roles.Add(sqlCipherRole);
                            db.SaveChanges();
                            Roles.Add(new(sqlCipherRole, false));
                            ok = true;
                        }
                    }
                }
            }
            catch
            {
                ok = false;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }

        public bool RemRole(ulong roleId)
        {
            bool ok = false;
            SecManDb.dbLock.EnterWriteLock();
            try
            {
                Role role = Roles.Where(o => o.Id == roleId).FirstOrDefault();
                if (role != null)
                {
                    using SQLCipher.Db db = new();

                    // Get the Role
                    SQLCipher.Role? sqlCipherRole = null;
                    if (db.Roles != null)
                    {
                        sqlCipherRole = db.Roles
                          .Where(x => x.Id == roleId)
                          .FirstOrDefault();
                    }

                    // Get the User
                    if ((sqlCipherRole != null) && (db.Users != null))
                    {
                        SQLCipher.User? sqlCipherUser = db.Users
                           .Include(o => o.Roles)
                           .Where(x => x.Id == Id)
                          .FirstOrDefault();

                        if (sqlCipherUser != null)
                        {
                            if (sqlCipherUser.Roles.Contains(sqlCipherRole))
                            {
                                sqlCipherUser.Roles.Remove(sqlCipherRole);
                                db.SaveChanges();
                                Roles.Remove(role);
                                ok = true;
                            }
                        }
                    }
                }
            }
            catch
            {
                ok = false;
            }
            finally
            {
                SecManDb.dbLock.ExitWriteLock();
            }
            return ok;
        }
        public Dictionary<string, List<string>> GetAppPerms()
        {
            Dictionary<string, List<string>> userAppPerms = new();

            SecManDb.dbLock.EnterReadLock();
            try
            {
                // Get the user and roles
                using SQLCipher.Db db = new();
                SQLCipher.User sqlCipherUser = db.Users
                    .Include(x => x.Roles)
                    .Where(o => o.Id == Id)
                    .FirstOrDefault();
                if (sqlCipherUser != null)
                {
                    // Get the Zones the Role is allocated to
                    if (sqlCipherUser.Roles != null)
                    {
                        foreach (SQLCipher.Role sqlCipheRole in sqlCipherUser.Roles)
                        {
                            List<SQLCipher.Zone>? sqlCipherZones = null;
                            if (db.Zones != null)
                            {
                                sqlCipherZones = db.Zones.Where(o => o.Roles.Contains(sqlCipheRole)).ToList();
                                if (sqlCipherZones != null)
                                {
                                    List<SQLCipher.Dev>? sqlCipherDevs = null;
                                    foreach (SQLCipher.Zone sqlCipherZone in sqlCipherZones)
                                    {
                                        // Get the App Devices allocated to the Zone
                                        sqlCipherDevs = db.Devs
                                            .Include(o => o.DevDef)
                                            .Include(o => o.Zone)
                                            .Where(o => o.DevDef.App && o.Zone == sqlCipherZone).ToList();
                                        foreach (SQLCipher.Dev sqlCipherDev in sqlCipherDevs)
                                        {
                                            List<SQLCipher.DevPermVal>? devPermVals = db.DevPermVals
                                                .Include(o => o.DevPermDef)
                                                .Where(o => o.Zone == sqlCipherZone && o.DevDef == sqlCipherDev.DevDef && o.Role == sqlCipheRole).ToList();

                                            if (devPermVals != null)
                                            {
                                                if (userAppPerms.ContainsKey(sqlCipherDev.Name))
                                                {
                                                    foreach (SQLCipher.DevPermVal devPermVal in devPermVals)
                                                    {
                                                        if ((devPermVal.Val) && !userAppPerms[sqlCipherDev.Name].Contains(devPermVal.DevPermDef.Name))
                                                        {
                                                            userAppPerms[sqlCipherDev.Name].Add(devPermVal.DevPermDef.Name);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    userAppPerms.Add(sqlCipherDev.Name, new());

                                                    foreach (SQLCipher.DevPermVal devPermVal in devPermVals)
                                                    {
                                                        if (devPermVal.Val)
                                                        {
                                                            userAppPerms[sqlCipherDev.Name].Add(devPermVal.DevPermDef.Name);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                userAppPerms = new();
            }
            finally
            {
                SecManDb.dbLock.ExitReadLock();
            }
            return userAppPerms;
        }

        public List<string> GetDevPerms(ulong devId)
        {
            List<string> userDevPerms = new();

            SecManDb.dbLock.EnterReadLock();
            try
            {
                // Get the user and roles
                using SQLCipher.Db db = new();
                SQLCipher.User sqlCipherUser = db.Users
                    .Include(x => x.Roles)
                    .Where(o => o.Id == Id)
                    .FirstOrDefault();
                if (sqlCipherUser != null)
                {
                    // Get the Zone the Device is allocated to
                    SQLCipher.Dev? sqlCipherDev = db.Devs
                        .Include(o => o.DevDef)
                        .Include(o => o.Zone)
                        .Include(o => o.Zone.Roles)
                        .Where(o => o.Id == devId)
                        .FirstOrDefault();

                    // Which User roles are allocated to the devices zone
                    if (sqlCipherDev.Zone != null)
                    {
                        foreach (SQLCipher.Role sqlCipherRole in sqlCipherUser.Roles)
                        {
                            if (sqlCipherDev.Zone.Roles.Contains(sqlCipherRole))
                            {
                                List<SQLCipher.DevPermVal>? devPermVals = db.DevPermVals
                                    .Include(o => o.DevPermDef)
                                    .Where(o => o.Zone == sqlCipherDev.Zone && o.DevDef == sqlCipherDev.DevDef && o.Role == sqlCipherRole).ToList();

                                if (devPermVals != null)
                                {
                                    foreach (SQLCipher.DevPermVal devPermVal in devPermVals)
                                    {
                                        if ((devPermVal.Val) && !userDevPerms.Contains(devPermVal.DevPermDef.Name))
                                        {
                                            userDevPerms.Add(devPermVal.DevPermDef.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                userDevPerms = new();
            }
            finally
            {
                SecManDb.dbLock.ExitReadLock();
            }
            return userDevPerms;
        }

        #region Pavan's code

        public async Task<ulong> AddUserAsync(AddUserDto model)
        {
            Log.Information("Creating new DB Object");
            SQLCipher.Db db = new();

            Log.Information("Mapping data to dbset user object");
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
                Retired = model.IsRetired,
                RetiredDate = model.RetiredDate,
                UserName = model.Username,
                IsActive = model.IsActive,
                InActiveDate = model.InactiveDate,
                Locked = model.IsLocked,
                LockedDate = model.LockedDate,
                LockedReason = model.LockedReason,
                LastLoginDate = model.LastLogin,
            };

            Log.Information("fetching all the roles which are need to be associated with user");
            var roles = db.Roles.Where(x => model.Roles.Select(x => x.Id).Contains(x.Id)).ToList();

            Log.Information("assigning roles to user object");
            user.Roles = roles;

            Log.Information("adding user to db");
            await db.AddRangeAsync(user);
            await db.SaveChangesAsync();
            Log.Information("completed adding user to db");
            return user.Id;
        }


        public async Task<ulong> UpdateUserAsync(AddUserDto model, ulong userId)
        {
            Log.Information("Creating new DB Object");
            SQLCipher.Db db = new();

            Log.Information("Fetch user object by user Id : {userId} to update", userId);
            var user = await db.Users
                                     .Where(x => x.Id == userId)
                                     .Include(x => x.Roles)
                                     .FirstOrDefaultAsync();
            Log.Information("assigning new values to user object");
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
            user.Retired = model.IsRetired;
            user.RetiredDate = model.RetiredDate;
            user.UserName = model.Username;
            user.IsActive = model.IsActive;
            user.InActiveDate = model.InactiveDate;
            user.Locked = model.IsLocked;
            user.LockedDate = model.LockedDate;
            user.LockedReason = model.LockedReason;
            user.LastLoginDate = model.LastLogin;


            Log.Information("fetching all the roles which are need to be associated with user");
            var roles = db.Roles.Where(x => model.Roles.Select(x => x.Id).Contains(x.Id)).ToList();

            Log.Information("assigning roles to user object");
            user.Roles = roles;

            Log.Information("Updating user object to db");
            db.UpdateRange(user);
            await db.SaveChangesAsync();
            Log.Information("Completed Updating user object to db");
            return user.Id;
        }



        public IQueryable<RoleDto> GetRoles()
        {
            Log.Information("Creating new DB Object");
            SQLCipher.Db db = new();

            Log.Information("Feting all roles from db projecting to custom model as Queryable");
            var roles = db.Roles
                                .Select(x => new RoleDto
                                {
                                    Description = x.Description,
                                    Id = x.Id,
                                    IsLoggedOutType = x.IsLoggedOutType,
                                    Name = x.Name
                                })
                                .AsQueryable();
            Log.Information("Completed fetching roles from db");
            return roles;
        }


        public IQueryable<UserDto> GetUser()
        {
            Log.Information("Creating new DB Object");
            SQLCipher.Db db = new();

            Log.Information("Feting all users from db projecting to custom model as Queryable");
            var users = db.Users
                                .Select(x => new UserDto
                                {
                                    IsActive = x.IsActive,
                                    Description = x.Description,
                                    Domain = x.Domain,
                                    Email = x.Email,
                                    FirstName = x.FirstName,
                                    Id = x.Id,
                                    InactiveDate = x.InActiveDate,
                                    IsLegacy = x.IsLegacy,
                                    UserAttributes = new List<UserAttributeDto>(),
                                    IsLocked = x.Locked,
                                    IsPasswordExpiryEnabled = x.IsPasswordExpiryEnable,
                                    IsRetired = x.Retired,
                                    Language = x.Language,
                                    LastLogin = x.LastLoginDate,
                                    LastName = x.LastName,
                                    LockedDate = x.LockedDate,
                                    LockedReason = x.LockedReason,
                                    PasswordExpiryDate = x.PasswordExpiryDate,
                                    ResetPassword = x.ResetPassword,
                                    RetiredDate = x.RetiredDate,
                                    Username = x.UserName,
                                    Roles = x.Roles.Select(x => new RoleDto
                                    {
                                        Description = x.Description,
                                        Id = x.Id,
                                        IsLoggedOutType = x.IsLoggedOutType,
                                        Name = x.Name,
                                        LinkUsers = x.Users.Select(x => x.Id).ToList()
                                    }).ToList(),
                                })
                                .AsQueryable();
            Log.Information("Completed fetching users from db");
            return users;
        }


        public async Task DeleteUserAsync(ulong userId)
        {
            Log.Information("Creating new DB Object");
            SQLCipher.Db db = new();

            Log.Information("fetching user from db by user id : {userId}", userId);
            var user = await db.Users
                                     .Where(x => x.Id == userId)
                                     .FirstOrDefaultAsync();
            Log.Information("removing user from db");
            if (user is null)
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            db.Remove(user);
            await db.SaveChangesAsync();
            Log.Information("removed user from db");
        }

        #endregion
    }
}

