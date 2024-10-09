//using AutoFixture;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Data.Sqlite;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;
//using Moq;
//using SecMan.BL;
//using SecMan.Data.Repository;
//using SecMan.Data.SQLCipher;
//using SecMan.Interfaces.BL;
//using SecMan.Model;
//using SecMan.UnitTests.Logger;
//using Serilog;
//using System.Net;

//namespace SecMan.UnitTests.UserAccessManagement
//{
//    public class UserTests : IDisposable
//    {
//        private DbContextOptions<Db> _dbContextOptions;
//        private readonly Db _db;
//        private readonly IFixture _fixture;
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly Mock<IEncryptionDecryption> _mockEncryptionDecryption;
//        public readonly Mock<IOptionsSnapshot<JwtTokenOptions>> _mockJwtTokenOptions;
//        private UserBL _userBL;

//        public UserTests()
//        {
//            _dbContextOptions = new DbContextOptionsBuilder<Db>()
//            .UseSqlite("DataSource=:memory:")
//            .Options;

//            _db = new Db(_dbContextOptions, string.Empty);
//            _db.Database.OpenConnection();
//            _db.Database.EnsureCreated();
//            List<Data.SQLCipher.Role> roles = new List<Data.SQLCipher.Role>()
//                    {
//                        new Data.SQLCipher.Role { Id = 1, Name = "Admin", Description = "Administrator role", IsLoggedOutType = false },
//                        new Data.SQLCipher.Role { Id = 2, Name = "User", Description = "User role", IsLoggedOutType = false }
//                    };
//            _db.Roles.AddRange(roles);
//            _db.Users.AddRange(
//                    new Data.SQLCipher.User { Id = 1, UserName = "Admin", IsActive = true, Description = "Administrator role", Roles = new List<Data.SQLCipher.Role> { roles[0] } },
//                    new Data.SQLCipher.User { Id = 2, UserName = "User", Locked = true, Description = "User role", Roles = new List<Data.SQLCipher.Role> { roles[1] } }
//                );
//            _db.SaveChanges();
//            _fixture = new Fixture();

//            _fixture.Behaviors
//                .OfType<ThrowingRecursionBehavior>()
//                .ToList()
//                .ForEach(b => _fixture.Behaviors.Remove(b));
//            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

//            _unitOfWork = new UnitOfWork(_db);
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockEncryptionDecryption = new Mock<IEncryptionDecryption>();
//            _mockJwtTokenOptions = new Mock<IOptionsSnapshot<JwtTokenOptions>>();
//            JwtTokenOptions jwtOpts = _fixture.Create<JwtTokenOptions>();
//            _mockJwtTokenOptions.Setup(x => x.Value).Returns(jwtOpts);
//            _userBL = new UserBL(_mockEncryptionDecryption.Object, _mockJwtTokenOptions.Object, _mockUnitOfWork.Object);
//            LoggerSetup.Initialize();
//        }


//        // AddUserAsync
//        [Fact]
//        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs));

//            // Arrange
//            CreateUser? addUserDto = null;

//            // Act & Assert
//            await Assert.ThrowsAsync<NullReferenceException>(() => _userBL.AddUserAsync(addUserDto));
//            Log.Information("Verified if the method throwing NullReferenceException for null input value");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_UserAlreadyExists_ReturnsConflict()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_UserAlreadyExists_ReturnsConflict));

//            // Arrange
//            CreateUser model = _fixture.Create<CreateUser>();
//            Data.SQLCipher.User existingUser = _fixture.Create<SecMan.Data.SQLCipher.User>();

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(model.Username))
//                        .ReturnsAsync(existingUser);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(model);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Conflict:409");
//            Assert.Equal(ResponseConstants.UserAlreadyExists, result.Message);
//            Log.Information("Verified if the result object's message is UserAlreadyExists");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(model.Username), Times.Once);
//            Log.Information("Verified if the GetUserByUsername method triggered only once");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Never);
//            Log.Information("Verified if the AddUserAsync method never triggered");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_InvalidRoles_ReturnsBadRequest()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_InvalidRoles_ReturnsBadRequest));

//            // Arrange
//            CreateUser model = _fixture.Create<CreateUser>();
//            List<RoleModel> partialRoles = _fixture.CreateMany<RoleModel>(model.Roles.Count - 1).ToList();

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(model.Username))
//                        .ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                        .ReturnsAsync(partialRoles);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(model);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//            Log.Information("Verified if the result object's status code is BadRequest:400");
//            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, result.Message);
//            Log.Information("Verified if the result object's message is SomeOfTheRoleNotPresent");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(model.Username), Times.Once);
//            Log.Information("Verified if the GetUserByUsername method triggered only once");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Never);
//            Log.Information("Verified if the AddUserAsync method never triggered");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenPasswordEncryption_ThrowsInvalidOperationException()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_ShouldReturnInternalServerError_WhenPasswordEncryption_ThrowsInvalidOperationException));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            List<RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
//            Model.User user = _fixture.Create<Model.User>();
//            string encryptedPassword = _fixture.Create<string>();

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                    .ReturnsAsync(roles);
//            _mockEncryptionDecryption.Setup(enc => enc.EncryptPassword(It.IsAny<string>(), It.IsAny<bool>()))
//                      .Throws<InvalidOperationException>();
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>())).ReturnsAsync(user);
//            Log.Information("Completed Moqing dependencies");


//            // Act & Assert
//            await Assert.ThrowsAsync<InvalidOperationException>(() => _userBL.AddUserAsync(addUserDto));
//            Log.Information("Verified if the method throwing InvalidOperationException for any password encryption issues");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
//            ulong userId = _fixture.Create<ulong>();
//            string encryptedPassword = _fixture.Create<string>();

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                    .ReturnsAsync(roles);
//            _mockEncryptionDecryption.Setup(enc => enc.EncryptPassword(It.IsAny<string>(), It.IsAny<bool>()))
//                      .Returns(encryptedPassword);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()))
//                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");


//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.AddUserAsync(addUserDto));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_SuccessfulCreation_ReturnsOk()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_SuccessfulCreation_ReturnsOk));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
//            Model.User user = _fixture.Create<Model.User>();
//            string encryptedPassword = _fixture.Create<string>();

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username)).ReturnsAsync((SecMan.Data.SQLCipher.User?)null);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                    .ReturnsAsync(roles);
//            _mockEncryptionDecryption.Setup(enc => enc.EncryptPassword(It.IsAny<string>(), It.IsAny<bool>()))
//                      .Returns(encryptedPassword);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>())).ReturnsAsync(user);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.AddUserAsync(addUserDto);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Assert.Equal(user.Id, result.Data?.Id);
//            Log.Information("Verified of expected mocked object's : {@mockedObject} userid mathces with created objects : {@createdObject} userid", user, result);

//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetUserByUsername(addUserDto.Username), Times.Once);
//            Log.Information("Verified if the GetUserByUsername method triggered only once");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.AddUserAsync(It.IsAny<CreateUser>()), Times.Once);
//            Log.Information("Verified if the AddUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }



//        // GetUsersAsync
//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUsersAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs));

//            // Arrange
//            UsersFilterDto? filterDto = null;
//            _userBL = new UserBL(_mockEncryptionDecryption.Object, _mockJwtTokenOptions.Object, _unitOfWork);
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<NullReferenceException>(() => _userBL.GetUsersAsync(filterDto));
//            Log.Information("Verified if the method throwing NullReferenceException for null input value");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUsersAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccurs));

//            // Arrange
//            UsersFilterDto filterDto = new UsersFilterDto
//            {
//                Limit = 1,
//                Offset = 0,
//                Username = "Admin",
//            };
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetAll(r => r.Roles))
//                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.GetUsersAsync(filterDto));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithUsernameExist()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithUsernameExist));

//            // Arrange
//            _userBL = new UserBL(_mockEncryptionDecryption.Object, _mockJwtTokenOptions.Object, _unitOfWork);
//            UsersFilterDto filterDto = new UsersFilterDto
//            {
//                Limit = 1,
//                Offset = 0,
//                Username = "Admin",
//            };
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the result's data object which contains user objects is not null");
//            Assert.Equal(1, result.Data.UserCount);
//            Log.Information("Verified if the result contains one object based on filter and seeded data");
//            Assert.Equal(filterDto.Limit, result.Data.Users.Count);
//            Log.Information("Verified if the User Count Returned matchs limit that has been queried");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithRoleExist()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithRoleExist));

//            // Arrange
//            _userBL = new UserBL(_mockEncryptionDecryption.Object, _mockJwtTokenOptions.Object, _unitOfWork);
//            UsersFilterDto filterDto = new UsersFilterDto
//            {
//                Limit = 1,
//                Offset = 0,
//                Role = new List<string> { "Admin" },
//                Status = []
//            };
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the result's data object which contains user objects is not null");
//            Assert.Equal(1, result.Data.UserCount);
//            Log.Information("Verified if the result contains one object based on filter and seeded data");
//            Assert.Equal(filterDto.Limit, result.Data.Users.Count);
//            Log.Information("Verified if the User Count Returned matchs limit that has been queried");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithStatusExist()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUsersAsync_ShouldReturnUsersWithCount_WhenUsersWithStatusExist));
//            // Arrange
//            _userBL = new UserBL(_mockEncryptionDecryption.Object, _mockJwtTokenOptions.Object, _unitOfWork);
//            UsersFilterDto filterDto = new UsersFilterDto
//            {
//                Limit = 2,
//                Offset = 0,
//                Status = new List<string> { "active", "locked" },
//                Role = []
//            };
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the result's data object which contains user objects is not null");
//            Assert.Equal(2, result.Data.UserCount);
//            Log.Information("Verified if the result contains one object based on filter and seeded data");
//            Assert.Equal(2, result.Data.Users.Count);
//            Log.Information("Verified if the User Count Returned matchs limit that has been queried");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnNoUsers_WhenUsersWithFilterDoesNotExist()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUsersAsync_ShouldReturnNoUsers_WhenUsersWithFilterDoesNotExist));

//            // Arrange
//            _userBL = new UserBL(_mockEncryptionDecryption.Object, _mockJwtTokenOptions.Object, _unitOfWork);
//            UsersFilterDto filterDto = new UsersFilterDto
//            {
//                Limit = 2,
//                Offset = 0,
//                Username = "test"
//            };
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<UsersWithCountDto> result = await _userBL.GetUsersAsync(filterDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the result's data object which contains user objects is not null");
//            Assert.Equal(0, result.Data.UserCount);
//            Log.Information("Verified if the result contains 0 object based on filter and seeded data");
//            Assert.Empty(result.Data.Users);
//            Log.Information("Verified user object response is empty based on user filers");
//            Log.Information("Test completed successfully");
//        }



//        // GetUserByIdAsync
//        [Fact]
//        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserIdIsInvalid()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUserByIdAsync_ShouldReturnNull_WhenUserIdIsInvalid));

//            // Arrange
//            ulong userId = 100ul;
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>()))
//                .ReturnsAsync((Data.SQLCipher.User?)null);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.GetUserByIdAsync(userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the result's data object which contains user object is not null");
//            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//            Log.Information("Verified if the result object's status code is BadRequest:400");
//            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
//            Log.Information("Verified if the result object's message is UserDoesNotExists");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserIdIsValid()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUserByIdAsync_ShouldReturnUser_WhenUserIdIsValid));

//            // Arrange
//            ulong userId = 1ul;
//            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
//            user.Id = userId;
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>(), r => r.Roles))
//                .ReturnsAsync(user);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<Model.User> result = await _userBL.GetUserByIdAsync(userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the result's data object which contains user object is not null");
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUserByIdAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(GetUserByIdAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccurs));

//            // Arrange
//            ulong userId = 1ul;
//            Model.User user = _fixture.Create<Model.User>();
//            user.Id = userId;
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>(), r => r.Roles))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.GetUserByIdAsync(userId));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // UpdateUserAsync
//        [Fact]
//        public async Task UpdateUserAsync_UserDoesNotExists_ReturnsBadRequest()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_UserAlreadyExists_ReturnsConflict));

//            // Arrange
//            CreateUser model = _fixture.Create<CreateUser>();
//            ulong userId = 1ul;

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>()))
//                        .ReturnsAsync((Data.SQLCipher.User?)null);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//            Log.Information("Verified if the result object's status code is BadRequest:400");
//            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
//            Log.Information("Verified if the result object's message is UserDoesNotExists");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(userId), Times.Once);
//            Log.Information("Verified if the GetUserById method triggered only once");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<CreateUser>(), It.IsAny<ulong>()), Times.Never);
//            Log.Information("Verified if the UpdateUserAsync method never triggered");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(UpdateUserAsync_ShouldReturnInternalServerError_WhenNullReferenceExceptionOccurs));

//            // Arrange
//            CreateUser? addUserDto = null;
//            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetById(It.IsAny<ulong>()))
//                .ReturnsAsync(user);
//            ulong userId = 1ul;
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<NullReferenceException>(() => _userBL.UpdateUserAsync(addUserDto, userId));
//            Log.Information("Verified if the method throwing NullReferenceException for null input value");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_InvalidRoles_ReturnsBadRequest()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_InvalidRoles_ReturnsBadRequest));

//            // Arrange
//            CreateUser model = _fixture.Create<CreateUser>();
//            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
//            ulong userId = user.Id;
//            List<Model.RoleModel> partialRoles = _fixture.CreateMany<Model.RoleModel>(model.Roles.Count - 1).ToList();

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(It.IsAny<ulong>()))
//                        .ReturnsAsync(user);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                        .ReturnsAsync(partialRoles);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(model, userId);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//            Log.Information("Verified if the result object's status code is BadRequest:400");
//            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, result.Message);
//            Log.Information("Verified if the result object's message is SomeOfTheRoleNotPresent");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(userId), Times.Once);
//            Log.Information("Verified if the GetUserById method triggered only once");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<CreateUser>(), It.IsAny<ulong>()), Times.Never);
//            Log.Information("Verified if the UpdateUserAsync method never triggered");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            Model.User addUserDto = _fixture.Create<Model.User>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Id, addUserDto.Id)
//                .Create();
//            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
//            ulong userId = addUserDto.Id;

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(userId))
//                .ReturnsAsync(user);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                    .ReturnsAsync(roles);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()))
//                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");


//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.UpdateUserAsync(addUserDto, userId));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_SuccessfulCreation_ReturnsOk()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(AddUserAsync_SuccessfulCreation_ReturnsOk));

//            // Arrange
//            Model.User addUserDto = _fixture.Create<Model.User>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                        .With(x => x.Id, addUserDto.Id)
//                        .Create();
//            List<Model.RoleModel> roles = _fixture.CreateMany<Model.RoleModel>(addUserDto.Roles.Count).ToList();
//            ulong userId = addUserDto.Id;

//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetById(userId))
//                .ReturnsAsync(user);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.GetRolesByRoleId(It.IsAny<List<ulong>>()))
//                    .ReturnsAsync(roles);
//            _mockUnitOfWork.Setup(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()))
//                .ReturnsAsync(addUserDto);
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            ServiceResponse<Model.User> result = await _userBL.UpdateUserAsync(addUserDto, userId);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Assert.Equal(userId, result?.Data?.Id);
//            Log.Information("Verified if the user id passed and updated user object's id are same({@userid}={@updatedUserId})", userId, result?.Data?.Id);
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.GetById(It.IsAny<ulong>()), Times.Once);
//            Log.Information("Verified if the GetUserById method triggered only once");
//            _mockUnitOfWork.Verify(dal => dal.IUserRepository.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()), Times.Once);
//            Log.Information("Verified if the UpdateUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }



//        // DeleteUserAsync
//        [Fact]
//        public async Task DeleteUserAsync_UserDoesNotExists_ReturnsBadRequest()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_UserDoesNotExists_ReturnsBadRequest));

//            // Arrange
//            ulong userId = _fixture.Create<ulong>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.Delete(It.IsAny<ulong>()))
//                .ReturnsAsync(false);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ApiResponse result = await _userBL.DeleteUserAsync(userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
//            Log.Information("Verified if the result object's message is UserDoesNotExists");
//            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
//            Log.Information("Verified if the result object's status code is BadRequest:400");
//            _mockUnitOfWork.Verify(x => x.IUserRepository.Delete(It.IsAny<ulong>()), Times.Once);
//            Log.Information("Verified if the DeleteUserAsync method never triggered");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task DeleteUserAsync_Successful_ReturnOk()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_Successful_ReturnOk));

//            // Arrange
//            Data.SQLCipher.User user = _fixture.Create<Data.SQLCipher.User>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.Delete(It.IsAny<ulong>()))
//                .ReturnsAsync(true);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ApiResponse result = await _userBL.DeleteUserAsync(user.Id);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            _mockUnitOfWork.Verify(x => x.IUserRepository.Delete(It.IsAny<ulong>()), Times.Once);
//            Log.Information("Verified if the DeleteUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));
//            // Arrange
//            Model.User user = _fixture.Create<Model.User>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.Delete(It.IsAny<ulong>()))
//                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.DeleteUserAsync(user.Id));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }


//        // LoginAsync
//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserDoesNotExists()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync((Data.SQLCipher.User?)null);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.UserDoesNotExists, result.Message);
//            Log.Information("Verified if the result object's message is UserDoesNotExists");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserIsLocked()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, true)
//                .With(x => x.Retired, false)
//                .With(x => x.PasswordExpiryEnable, false)
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.AccountLocked, result.Message);
//            Log.Information("Verified if the result object's message is AccountLocked");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserIsRetired()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, true)
//                .With(x => x.PasswordExpiryEnable, false)
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.AccountRetired, result.Message);
//            Log.Information("Verified if the result object's message is AccountRetired");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_UserPassword_IsExpired()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, false)
//                .With(x => x.PasswordExpiryEnable, true)
//                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.PasswordExpired, result.Message);
//            Log.Information("Verified if the result object's message is PasswordExpired");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_OkResponse_ForLegacyUser()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, false)
//                .With(x => x.IsLegacy, true)
//                .With(x => x.PasswordExpiryEnable, false)
//                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
//                .Returns(loginRequest.Password);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
//                    .ReturnsAsync(userDetails);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<bool>()))
//                .Returns(Task.CompletedTask);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>()))
//                 .Returns(Task.CompletedTask);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the response data object is not null");
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_ForLegacyUsers_IfPasswordIsIncorrect()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            string dbPassword = _fixture.Create<string>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, false)
//                .With(x => x.IsLegacy, true)
//                .With(x => x.PasswordExpiryEnable, false)
//                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            _mockEncryptionDecryption.Setup(x => x.DecryptPasswordAES256(It.IsAny<string>()))
//                .Returns(dbPassword);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>()))
//                 .Returns(Task.CompletedTask);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
//            Log.Information("Verified if the result object's message is InvalidPassword");
//            Log.Information("Test completed successfully");
//        }



//        [Fact]
//        public async Task LoginAsync_ShouldReturn_OkResponse_ForNonLegacyUser()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, false)
//                .With(x => x.IsLegacy, false)
//                .With(x => x.PasswordExpiryEnable, false)
//                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            _mockEncryptionDecryption.Setup(x => x.VerifyHashPassword(It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(true);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
//                    .ReturnsAsync(userDetails);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<bool>()))
//                .Returns(Task.CompletedTask);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>()))
//                 .Returns(Task.CompletedTask);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the response data object is not null");
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Log.Information("Test completed successfully");
//        }



//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_ForNonLegacyUsers_IfPasswordIsIncorrect()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, false)
//                .With(x => x.IsLegacy, false)
//                .With(x => x.PasswordExpiryEnable, false)
//                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            _mockEncryptionDecryption.Setup(x => x.VerifyHashPassword(It.IsAny<string>(), It.IsAny<string>()))
//                .Returns(false);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>()))
//                 .Returns(Task.CompletedTask);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
//            Log.Information("Verified if the result object's message is InvalidPassword");
//            Log.Information("Test completed successfully");
//        }

//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnauthorizedResponse_PasswordIsNotSetForUser()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            Data.SQLCipher.User user = _fixture.Build<Data.SQLCipher.User>()
//                .With(x => x.Locked, false)
//                .With(x => x.Retired, false)
//                .With(x => x.IsLegacy, false)
//                .With(x => x.Password, string.Empty)
//                .With(x => x.PasswordExpiryEnable, false)
//                .With(x => x.PasswordExpiryDate, DateTime.Now.AddMinutes(-1))
//                .Create();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .ReturnsAsync(user);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.LogLoginAttempts(It.IsAny<ulong>(), It.IsAny<bool>()))
//                 .Returns(Task.CompletedTask);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.LoginAsync(loginRequest);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.InvalidPassword, result.Message);
//            Log.Information("Verified if the result object's message is InvalidPassword");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            LoginRequest loginRequest = _fixture.Create<LoginRequest>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserByUsername(It.IsAny<string>()))
//                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.LoginAsync(loginRequest));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // ValidateSessionAsync
//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturn_UnauthorizedResponse_SessionIdIsInvalid()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            string sessionId = _fixture.Create<string>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserBySessionId(sessionId))
//                .ReturnsAsync((ulong?)null);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.ValidateSessionAsync(sessionId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.Null(result.Data);
//            Log.Information("Verified if the response data object is null");
//            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Unauthorized:401");
//            Assert.Equal(ResponseConstants.InvalidSessionId, result.Message);
//            Log.Information("Verified if the result object's message is InvalidSessionId");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturn_OkResponse_ValidSessionId()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            string sessionId = _fixture.Create<string>();
//            ulong userId = _fixture.Create<ulong>();
//            Tuple<UserDetails?, List<AppPermissions>?> userDetails = _fixture.Create<Tuple<UserDetails?, List<AppPermissions>?>>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserBySessionId(sessionId))
//                .ReturnsAsync(userId);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserDetails(It.IsAny<ulong>()))
//                .ReturnsAsync(userDetails);
//            _mockUnitOfWork.Setup(x => x.IUserRepository.UpdateUserSessionDetails(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<bool>()))
//                .Returns(Task.CompletedTask);
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ServiceResponse<LoginServiceResponse> result = await _userBL.ValidateSessionAsync(sessionId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            Assert.NotNull(result.Data);
//            Log.Information("Verified if the response data object is not null");
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
//            Log.Information("Verified if the result object's status code is Ok:200");
//            Assert.Equal(ResponseConstants.Success, result.Message);
//            Log.Information("Verified if the result object's message is Success");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs()
//        {
//            Log.Information("Starting BL test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenThrowsSqliteExceptionOccurs));

//            // Arrange
//            string sessionId = _fixture.Create<string>();
//            _mockUnitOfWork.Setup(x => x.IUserRepository.GetUserBySessionId(sessionId))
//                .Throws(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _userBL.ValidateSessionAsync(sessionId));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }


//        public void Dispose()
//        {
//            _db.Database.EnsureDeleted();
//            _db.Dispose();
//        }
//    }
//}
