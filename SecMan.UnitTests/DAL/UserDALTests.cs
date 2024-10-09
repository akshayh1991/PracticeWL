//using AutoFixture;
//using AutoFixture.AutoMoq;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using SecMan.Data.SQLCipher;
//using SecMan.Model;
//using SecMan.UnitTests.FaultyDbConfig;
//using SecMan.UnitTests.Logger;
//using Serilog;


//namespace SecMan.UnitTests.DAL;
//public class UserDALTests : IDisposable
//{
//    private DbContextOptions<Db> _dbContextOptions;
//    private readonly Db _db;
//    private readonly Data.User _user;
//    private readonly IFixture _fixture;


//    public UserDALTests()
//    {
//        _dbContextOptions = new DbContextOptionsBuilder<Db>()
//        .UseSqlite("DataSource=:memory:")
//        .Options;

//        _db = new Db(_dbContextOptions, string.Empty);
//        _db.Database.OpenConnection();
//        _db.Database.EnsureCreated();

//        List<Data.SQLCipher.Role> roles = new List<Data.SQLCipher.Role>
//        {
//                new Data.SQLCipher.Role { Id = 1, Name = "Admin", Description = "Administrator role", IsLoggedOutType = false },
//                new Data.SQLCipher.Role { Id = 2, Name = "User", Description = "User role", IsLoggedOutType = false }
//        };

//        _db.Roles.AddRange(roles);
//        _db.Users.AddRange(
//                new Data.SQLCipher.User { Id = 1, UserName = "Admin", Description = "Administrator role", Roles = new List<Data.SQLCipher.Role> { roles[0] } },
//                new Data.SQLCipher.User { Id = 2, UserName = "User", Description = "User role", Roles = new List<Data.SQLCipher.Role> { roles[0] } }
//            );
//        _db.SysFeatProps.AddRange(new SysFeatProp
//        {
//            Name = "Max Login Attempts",
//            ValMax = 3ul
//        });
//        _db.SaveChanges();
//        _user = new Data.User(_db);
//        _fixture = new Fixture().Customize(new AutoMoqCustomization() { ConfigureMembers = true });

//        LoggerSetup.Initialize();
//    }


//    // AddUserAsync
//    [Fact]
//    public async Task AddUserAsync_ShouldReturnInsertedUserId()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(AddUserAsync_ShouldReturnInsertedUserId));

//        // Arrange
//        CreateUser addUserDto = _fixture.Build<CreateUser>().With(x => x.IsPasswordExpiryEnabled, true).Create();
//        addUserDto.Roles = [1, 2];
//        Log.Information("Completed Moqing dependencies");

//        // Act
//        Model.User user = await _user.AddUserAsync(addUserDto);
//        List<Model.Role>? result = _user.GetUser().Where(x => x.Id == user.Id).Select(x => x.Roles).FirstOrDefault();
//        Log.Information("Test result: {@Result}", result);

//        // Assert
//        Assert.NotEqual(0UL, user.Id);
//        Log.Information("Verified UserId: {@userId} is not equal to 0", user.Id);
//        Assert.NotNull(_db.Users.Where(x => x.Id == user.Id).ToList());
//        Log.Information("Verified user with userId: {@userId} present in db", user.Id);
//        Assert.NotNull(result);
//        Log.Information("Verified result: {@Result} is not null", result);
//        Assert.Equal(result.Count, addUserDto.Roles.Count);
//        Log.Information("Verified if all the roles : {@roles} are added and mapped to user", result);
//        Log.Information("Test completed successfully");
//    }

//    [Fact]
//    public async Task AddUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(AddUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        CreateUser addUserDto = _fixture.Create<CreateUser>();
//        addUserDto.Roles = _fixture.CreateMany<ulong>(2).ToList();

//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        Log.Information("Completed Moqing dependencies");

//        // Act & Assert
//        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.AddUserAsync(addUserDto));
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task AddUserAsync_ShouldThrowInternalServerError_WhenNullReferenceExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(AddUserAsync_ShouldThrowInternalServerError_WhenNullReferenceExceptionOccurs));

//        // Arrange
//        CreateUser? addUserDto = null;


//        // Act & Assert
//        await Assert.ThrowsAsync<NullReferenceException>(() => _user.AddUserAsync(addUserDto));
//        Log.Information("Verified if the method throwing NullReferenceException for null input value");
//        Log.Information("Test completed successfully");
//    }



//    // UpdateUserAsync
//    [Fact]
//    public async Task UpdateUserAsync_ShouldReturnUpdatedUserId()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(UpdateUserAsync_ShouldReturnUpdatedUserId));

//        // Arrange
//        CreateUser addUserDto = _fixture.Create<CreateUser>();
//        Model.UpdateUser updateUserDto = _fixture.Create<Model.UpdateUser>();
//        addUserDto.Roles = [1, 2];
//        updateUserDto.Roles = addUserDto.Roles;
//        Log.Information("Completed Moqing dependencies");

//        // Act
//        Model.User user = await _user.AddUserAsync(addUserDto);
//        Model.User updatedUser = await _user.UpdateUserAsync(updateUserDto, user.Id);
//        Log.Information("Test result: {@Result}", updatedUser);

//        // Assert
//        Assert.NotNull(updatedUser);
//        Log.Information("Verified if the updated user object : {@updatedUser} is not null", updatedUser);
//        Assert.Equal(user.Id, updatedUser.Id);
//        Log.Information("Verified if the user id passed and updated user object's id are same({@userid}={@updatedUserId})", user.Id, updatedUser.Id);
//        Assert.Equal(updatedUser.Roles.Count, updateUserDto.Roles.Count);
//        Log.Information("Verified if all the roles : {@roles} are added and mapped to user", updateUserDto.Roles);
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task UpdateUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(UpdateUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        CreateUser addUserDto = _fixture.Create<CreateUser>();
//        Model.User updateUserDto = _fixture.Create<Model.User>();
//        addUserDto.Roles = _fixture.CreateMany<ulong>(2).ToList();

//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));

//        Log.Information("Completed Moqing dependencies");

//        // Act
//        Model.User user = await _user.AddUserAsync(addUserDto);
//        Log.Information("Test result: {@Result}", user);

//        // Assert
//        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.UpdateUserAsync(updateUserDto, user.Id));
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task UpdateUserAsync_ShouldThrowInternalServerError_WhenArgumentNullExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(UpdateUserAsync_ShouldThrowInternalServerError_WhenArgumentNullExceptionOccurs));

//        // Arrange
//        Model.User? updateUserDto = _fixture.Create<Model.User>();
//        ulong userId = 3ul;
//        Log.Information("Completed Moqing dependencies");


//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentNullException>(() => _user.UpdateUserAsync(updateUserDto, userId));
//        Log.Information("Verified if the method throwing ArgumentNullException if the updated requested userId : {@userId} doesnot exists", userId);
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task UpdateUserAsync_ShouldThrowInternalServerError_WhenNullReferenceExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(UpdateUserAsync_ShouldThrowInternalServerError_WhenNullReferenceExceptionOccurs));

//        // Arrange
//        Model.User? updateUserDto = null;
//        ulong userId = 1;
//        Log.Information("Completed Moqing dependencies");

//        // Act

//        // Act & Assert
//        await Assert.ThrowsAsync<NullReferenceException>(() => _user.UpdateUserAsync(updateUserDto, userId));
//        Log.Information("Verified if the method throwing NullReferenceException for null input value");
//        Log.Information("Test completed successfully");
//    }



//    // GetRolesByRoleId
//    [Fact]
//    public async Task GetRolesByRoleId_ReturnsRoles_WhenRoleIdsAreValid()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRolesByRoleId_ReturnsRoles_WhenRoleIdsAreValid));

//        // Arrange
//        List<ulong> roleIds = new List<ulong> { 1 };

//        // Act
//        List<Model.Role> roles = await _user.GetRolesByRoleId(roleIds);
//        Log.Information("Test result: {@Result}", roles);


//        // Assert
//        Assert.Single(roles);
//        Assert.Equal("Admin", roles.First().Name);
//        Log.Information("Verified if the returned role objects Name matchs with input role id related name");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetRolesByRoleId_ReturnsEmpty_WhenNoRoleIdsMatch()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRolesByRoleId_ReturnsEmpty_WhenNoRoleIdsMatch));

//        // Arrange
//        List<ulong> roleIds = new List<ulong> { 999 };

//        // Act
//        List<Model.Role> roles = await _user.GetRolesByRoleId(roleIds);
//        Log.Information("Test result: {@Result}", roles);


//        // Assert
//        Assert.Empty(roles);
//        Log.Information("Verified if the roles are empty if id passed in filter is invalid");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetRolesByRoleId_GetRolesByRoleId_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRolesByRoleId_GetRolesByRoleId_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        List<ulong> roleIds = new List<ulong> { 1 };
//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        Log.Information("Completed Moqing dependencies");

//        // Act & Assert
//        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetRolesByRoleId(roleIds));
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }


//    // GetRoles
//    [Fact]
//    public async Task GetRoles_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRoles_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        IQueryable<Model.Role> rolesQuery = userWithFaultyDb.GetRoles();
//        Log.Information("Completed Moqing dependencies");

//        // Act & Assert
//        await Assert.ThrowsAsync<SqliteException>(() => rolesQuery.ToListAsync());
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetRoles_ReturnsAllRoles_AsRoleDto()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRoles_ReturnsAllRoles_AsRoleDto));

//        // Arrange

//        // Act
//        IQueryable<Model.Role> rolesQuery = _user.GetRoles();
//        List<Model.Role> roles = await rolesQuery.ToListAsync();
//        Log.Information("Test result: {@Result}", roles);

//        // Assert
//        Assert.Equal(2, roles.Count);
//        Log.Information("Verified if role count returned matchs with seeded role count: {@roleCount}", roles.Count);
//        Assert.Contains(roles, r => r.Id == 1 && r.Name == "Admin");
//        Log.Information("Verified the first role is Admin");
//        Assert.Contains(roles, r => r.Id == 2 && r.Name == "User");
//        Log.Information("Verified the last role is User");
//        Log.Information("Test completed successfully");
//    }


//    // GetUserByUsername
//    [Fact]
//    public async Task GetUserByUsername_ShouldReturn_MatchingUser()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetUserByUsername_ShouldReturn_MatchingUser));

//        // Arrange
//        string username = "Admin";

//        // Act
//        Model.User? user = await _user.GetUserByUsername(username);
//        Log.Information("Test result: {@Result}", user);


//        // Assert
//        Assert.NotNull(user);
//        Log.Information("Verified if the returned user is not null");
//        Assert.Equal(username, user.Username);
//        Log.Information("Verified if the returened user object's username matchs with input username");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserByUsername_ShouldReturn_Null()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetUserByUsername_ShouldReturn_Null));

//        // Arrange
//        string username = "test user";

//        // Act
//        Model.User? user = await _user.GetUserByUsername(username);
//        Log.Information("Test result: {@Result}", user);


//        // Assert
//        Assert.Null(user);
//        Log.Information("Verified if the returned user object is null for invalid username");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserByUsername_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRoles_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        string username = "test user";
//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        Log.Information("Completed Moqing dependencies");

//        // Act & Assert
//        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetUserByUsername(username));
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }



//    // GetUserById
//    [Fact]
//    public async Task GetUserById_ShouldReturn_MatchingUser()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetUserById_ShouldReturn_MatchingUser));

//        // Arrange
//        ulong userId = 1ul;

//        // Act
//        Model.User? user = await _user.GetUserById(userId);
//        Log.Information("Test result: {@Result}", user);

//        // Assert
//        Assert.NotNull(user);
//        Log.Information("Verified if the returned user is not null");
//        Assert.Equal(userId, user.Id);
//        Log.Information("Verified if the returened user object's id matchs with input id");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserById_ShouldReturn_Null()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetUserById_ShouldReturn_Null));

//        // Arrange
//        ulong userId = 100ul;

//        // Act
//        Model.User? user = await _user.GetUserById(userId);
//        Log.Information("Test result: {@Result}", user);

//        // Assert
//        Assert.Null(user);
//        Log.Information("Verified if the returned user object is null for invalid id");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserById_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetRoles_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        ulong userId = 1ul;
//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        Log.Information("Completed Moqing dependencies");

//        // Act & Assert
//        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.GetUserById(userId));
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }



//    // GetUsers
//    [Fact]
//    public async Task GetUsers_ReturnsAllUsers_AsUserDto()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetUsers_ReturnsAllUsers_AsUserDto));

//        // Arrange

//        // Act
//        IQueryable<Model.User> usersQuery = _user.GetUser();
//        List<Model.User> users = await usersQuery.ToListAsync();
//        Log.Information("Test result: {@Result}", users);

//        // Assert
//        Assert.Equal(2, users.Count);
//        Log.Information("Verified if user count returned matchs with seeded user count: {@userCount}", users.Count);
//        Assert.Contains(users, r => r.Id == 1 && r.Username == "Admin");
//        Log.Information("Verified the first user is Admin");
//        Assert.Contains(users, r => r.Id == 2 && r.Username == "User");
//        Log.Information("Verified the last user is User");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUsers_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(GetUsers_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        Log.Information("Completed Moqing dependencies");

//        // Act
//        IQueryable<Model.User> usersQuery = userWithFaultyDb.GetUser();

//        // Assert
//        await Assert.ThrowsAsync<SqliteException>(() => usersQuery.ToListAsync());
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task DeleteUserAsync_ShouldRemove_UserFromDb()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldRemove_UserFromDb));

//        // Arrange
//        ulong userId = 1ul;

//        // Act
//        await _user.DeleteUserAsync(userId);
//        List<Model.User> users = await _user.GetUser().ToListAsync();
//        Model.User? deletedUser = users.Where(x => x.Id == userId).FirstOrDefault();
//        Log.Information("Test result: {@Result}", deletedUser);


//        // Assert
//        Assert.DoesNotContain(users, x => x.Id == userId);
//        Log.Information("Verified if the users object : {@users} does not contain deleted user", users);
//        Assert.Null(deletedUser);
//        Log.Information("Verified if the object is null when deleted user id is queried");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task DeleteUserAsync_ShouldThrowInternalServerError_WhenArgumentNullExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenArgumentNullExceptionOccurs));

//        // Arrange
//        ulong userId = 100ul;

//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentNullException>(() => _user.DeleteUserAsync(userId));
//        Log.Information("Verified if the method throwing ArgumentNullException if the updated requested userId : {@userId} doesnot exists", userId);
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));

//        // Arrange
//        ulong userId = 1ul;
//        Data.User userWithFaultyDb = new Data.User(new FaultyDbContext(_dbContextOptions));
//        Log.Information("Completed Moqing dependencies");

//        // Act & Assert
//        await Assert.ThrowsAsync<SqliteException>(() => userWithFaultyDb.DeleteUserAsync(userId));
//        Log.Information("Verified if the method throwing SqliteException for any db related issues");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task LogLoginAttempts_ShouldReturn_CompledTask_IfUserDoesNotExists()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 100;
//        bool isSuccess = true;

//        // Act
//        await _user.LogLoginAttempts(userId, isSuccess);
//        List<LoginLogs> logs = await _db.LoginLogs.Where(x => x.User.Id == userId).ToListAsync();
//        Log.Information("Test result: {@Result}", logs);

//        // Assert
//        Assert.Empty(logs);
//        Log.Information("Verified If the Login Logs are empty");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task LogLoginAttempts_ShouldReturn_CompledTask_WithZero_Logs_SinceIts_SuccessLogin()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 1;
//        bool isSuccess = true;

//        // Act
//        await _user.LogLoginAttempts(userId, isSuccess);
//        List<LoginLogs> logs = await _db.LoginLogs.Where(x => x.User.Id == userId && x.IsSuccessfullyLoggedIn).ToListAsync();
//        Log.Information("Test result: {@Result}", logs);

//        // Assert
//        Assert.Empty(logs);
//        Log.Information("Verified If the Login Logs are empty");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task LogLoginAttempts_ShouldReturn_CompledTask_WithInserting_LogInto_DB()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 1;
//        bool isSuccess = false;

//        // Act
//        await _user.LogLoginAttempts(userId, isSuccess);
//        List<LoginLogs> logs = await _db.LoginLogs.Where(x => x.User.Id == userId)
//            .Include(x => x.User).ToListAsync();
//        Log.Information("Test result: {@Result}", logs);

//        // Assert
//        Assert.Single(logs);
//        Log.Information("Verified if only one log in added");
//        Assert.False(logs.Select(x => x.User.Locked).FirstOrDefault());
//        Log.Information("Verified if the user is not locked since this was an 1st attempt");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task LogLoginAttempts_ShouldReturn_CompledTask_ByLockingUser()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 1;
//        bool isSuccess = false;
//        await _user.LogLoginAttempts(userId, isSuccess);
//        await _user.LogLoginAttempts(userId, isSuccess);
//        Log.Information("Completed Moqing dependencies");

//        // Act
//        await _user.LogLoginAttempts(userId, isSuccess);
//        List<LoginLogs> logs = await _db.LoginLogs.Where(x => x.User.Id == userId)
//            .Include(x => x.User).ToListAsync();
//        Log.Information("Test result: {@Result}", logs);


//        // Assert
//        Assert.Equal(3, logs.Count);
//        Log.Information("verified there a total 3 logs for the same user");
//        Assert.True(logs.Select(x => x.User.Locked).FirstOrDefault());
//        Log.Information("verfied that the user is locked since 3 max wrong attempts allowed");
//        Log.Information("Test completed successfully");
//    }

//    [Fact]
//    public async Task GetUserPasswordByUserId_ShouldReturn_Password_ForValidUser()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 1;

//        // Act
//        string? result = await _user.GetUserPasswordByUserId(userId);
//        Log.Information("Test result: {@Result}", result);

//        Assert.NotNull(result);
//        Log.Information("Verified if the response password is not null");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserPasswordByUserId_ShouldReturn_Null_ForInValidUser()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 100;

//        // Act
//        string? result = await _user.GetUserPasswordByUserId(userId);
//        Log.Information("Test result: {@Result}", result);

//        Assert.Null(result);
//        Log.Information("Verified if the response password is null");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserBySessionId_ShouldReturn_Null_ForInvalidSessionId()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        string sessionId = _fixture.Create<string>();

//        // Act
//        ulong? result = await _user.GetUserBySessionId(sessionId);
//        Log.Information("Test result: {@Result}", result);

//        // Assert
//        Assert.Equal(0ul, result);
//        Log.Information("Verfied if the response is null for invalid session id");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserBySessionId_ShouldReturn_UserId_ForValidSessionId()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 1;
//        string sessionId = _fixture.Create<string>();
//        await _user.UpdateUserSessionDetails(userId, sessionId, 5, true);
//        Log.Information("Completed Moqing dependencies");

//        // Act
//        ulong? result = await _user.GetUserBySessionId(sessionId);
//        Log.Information("Test result: {@Result}", result);

//        // Assert
//        Assert.NotNull(result);
//        Log.Information("verified that the user details are not null since user id is valid");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserDetails_ShouldReturn_DefaultValues_IfUserId_IsInvalid()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 100;

//        // Act
//        Tuple<UserDetails?, List<AppPermissions>?> result = await _user.GetUserDetails(userId);
//        Log.Information("Test result: {@Result}", result);

//        // Assert
//        Assert.Equal(default, result.Item1);
//        Log.Information("Verified if the response of the user details are null or default for invalid user id");
//        Assert.Equal(default, result.Item2);
//        Log.Information("Verified if the response of the permissions are null or default for invalid user id");
//        Log.Information("Test completed successfully");
//    }


//    [Fact]
//    public async Task GetUserDetails_ShouldReturn_UserDetails_IfUserId_IsValid()
//    {
//        Log.Information("Starting DAL test: {@TestName}", nameof(DeleteUserAsync_ShouldThrowInternalServerError_WhenSqliteExceptionOccurs));
//        // Arrange
//        ulong userId = 1;

//        // Act
//        Tuple<UserDetails?, List<AppPermissions>?> result = await _user.GetUserDetails(userId);
//        Log.Information("Test result: {@Result}", result);

//        // Assert
//        Assert.NotEqual(default, result.Item1);
//        Log.Information("Verified if the response of the user details are not null or default for valid user id");
//        Assert.NotEqual(default, result.Item2);
//        Log.Information("Verified if the response of the permissions are not null or default for valid user id");
//        Assert.Single(result.Item1.Roles);
//        Log.Information("Verified if the roles assigned to the user is 1");
//        Log.Information("Test completed successfully");
//    }


//    public void Dispose()
//    {
//        _db.Database.EnsureDeleted();
//        _db.Dispose();
//    }
//}
