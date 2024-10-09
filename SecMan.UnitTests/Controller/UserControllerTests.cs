//using AutoFixture;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.Sqlite;
//using Moq;
//using SecMan.Interfaces.BL;
//using SecMan.Model;
//using SecMan.UnitTests.Logger;
//using Serilog;
//using System.Net;
//using UserAccessManagement.Controllers;

//namespace SecMan.UnitTests.Controller
//{
//    public class UserControllerTests
//    {
//        private readonly IFixture _fixture;
//        private readonly Mock<IUserBL> _mockUserBl;
//        private readonly UsersController _usersController;
//        private readonly DefaultHttpContext _httpContext;


//        public UserControllerTests()
//        {
//            _fixture = new Fixture();
//            _mockUserBl = new Mock<IUserBL>();
//            _usersController = new UsersController(_mockUserBl.Object);
//            _httpContext = new DefaultHttpContext();
//            _usersController.ControllerContext = new ControllerContext
//            {
//                HttpContext = _httpContext
//            };
//            LoggerSetup.Initialize();
//        }


//        // AddUserAsync
//        [Fact]
//        public async Task AddUserAsync_ShouldReturnConflict_WhenUsernameAlreadyExists()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(AddUserAsync_ShouldReturnConflict_WhenUsernameAlreadyExists));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserAlreadyExists, HttpStatusCode.Conflict));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.AddUserAsync(addUserDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            ConflictObjectResult conflictResult = Assert.IsType<ConflictObjectResult>(result);
//            Log.Information("Verified if the response is of type ConflictObjectResult");
//            CommonResponse response = Assert.IsType<Conflict>(conflictResult.Value);
//            Log.Information("Verified if the response object is of type Conflict");
//            Assert.Equal(HttpStatusCode.Conflict, response.Status);
//            Log.Information("Verified if the response objects status is Conflict:409");
//            Assert.Equal(ResponseConstants.Conflict, response.Title);
//            Log.Information("Verified if the response object's title is Conflict");
//            _mockUserBl.Verify(x => x.AddUserAsync(addUserDto), Times.Once());
//            Log.Information("Verified if the AddUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_ShouldReturnBadRequest_WhenRequestIsInvalid()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(AddUserAsync_ShouldReturnBadRequest_WhenRequestIsInvalid));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.InvalidRequest, HttpStatusCode.BadRequest));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.AddUserAsync(addUserDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            BadRequestObjectResult badrequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            Log.Information("Verified if the response is of type BadRequestObjectResult");
//            BadRequest response = Assert.IsType<BadRequest>(badrequestResult.Value);
//            Log.Information("Verified if the response object is of type BadRequest");
//            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
//            Log.Information("Verified if the response objects status is BadRequest:400");
//            Assert.Equal(ResponseConstants.InvalidRequest, response.Title);
//            Log.Information("Verified if the response object's title is InvalidRequest");
//            _mockUserBl.Verify(x => x.AddUserAsync(addUserDto), Times.Once());
//            Log.Information("Verified if the AddUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_ShouldReturnCreated_WhenUserIsCreated()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(AddUserAsync_ShouldReturnCreated_WhenUserIsCreated));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            User user = _fixture.Create<User>();
//            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, user));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.AddUserAsync(addUserDto);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            CreatedResult createdResult = Assert.IsType<CreatedResult>(result);
//            Log.Information("Verified if the response is of type CreatedResult");
//            Assert.Equal(user, createdResult.Value);
//            Log.Information("Verified the mocked response object is equal to result object's value");
//            _mockUserBl.Verify(x => x.AddUserAsync(addUserDto), Times.Once());
//            Log.Information("Verified if the AddUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task AddUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(AddUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));

//            // Arrange
//            CreateUser addUserDto = _fixture.Create<CreateUser>();
//            _mockUserBl.Setup(x => x.AddUserAsync(It.IsAny<CreateUser>()))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _usersController.AddUserAsync(addUserDto));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // GetUsersAsync
//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnNoContent_NoUsersExists()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(GetUsersAsync_ShouldReturnNoContent_NoUsersExists));
//            // Arrange
//            UsersFilterDto usersFilter = new UsersFilterDto
//            {
//                Limit = 1,
//                Offset = 0,
//                Username = "Test",
//            };
//            _mockUserBl.Setup(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()))
//                .ReturnsAsync(new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = 0, Users = new List<User>() }));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.GetUsersAsync(usersFilter);
//            Log.Information("Test result: {@Result}", result);

//            // Arrange
//            Assert.IsType<NoContentResult>(result);
//            Log.Information("Verified if the response is of type NoContentResult");
//            _mockUserBl.Verify(x => x.GetUsersAsync(usersFilter), Times.Once());
//            Log.Information("Verified if the GetUsersAsync method triggered only once"); 
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnOk_WithListOfUsers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(GetUsersAsync_ShouldReturnOk_WithListOfUsers));
//            // Arrange
//            UsersFilterDto usersFilter = new UsersFilterDto
//            {
//                Limit = 10,
//                Offset = 0,
//                Username = "Test",
//            };
//            IEnumerable<User> users = _fixture.CreateMany<User>(10);
//            _mockUserBl.Setup(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()))
//                .ReturnsAsync(new ServiceResponse<UsersWithCountDto>(ResponseConstants.Success, HttpStatusCode.OK, new UsersWithCountDto { UserCount = users.Count(), Users = users.ToList() }));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.GetUsersAsync(usersFilter);
//            Log.Information("Test result: {@Result}", result);

//            // Arrange
//            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
//            Log.Information("Verified if the response is of type OkObjectResult");
//            List<User> returneredUsers = Assert.IsType<List<User>>(okResult.Value);
//            Log.Information("Verified if the response object is of type List<User>");
//            Assert.Equal(10, returneredUsers.Count);
//            Log.Information("verified if the returneredUsers Count matchs input filter value");
//            Assert.Equal("10", _httpContext.Response.Headers[ResponseHeaders.TotalCount]);
//            Log.Information("verified if the x-total-count response header had 10 as count");
//            _mockUserBl.Verify(x => x.GetUsersAsync(usersFilter), Times.Once());
//            Log.Information("Verified if the GetUsersAsync method triggered only once"); 
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUsersAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(GetUsersAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));

//            // Arrange
//            UsersFilterDto usersFilter = new UsersFilterDto
//            {
//                Limit = 10,
//                Offset = 0,
//                Username = "Test",
//            };
//            IEnumerable<User> users = _fixture.CreateMany<User>(10);
//            _mockUserBl.Setup(x => x.GetUsersAsync(It.IsAny<UsersFilterDto>()))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _usersController.GetUsersAsync(usersFilter));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // GetUserByIdAsync
//        [Fact]
//        public async Task GetUserByIdAsync_ShouldReturnBadRequest_WhenInvalidUserIdPassed()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(GetUserByIdAsync_ShouldReturnBadRequest_WhenInvalidUserIdPassed));

//            // Arrange
//            ulong userId = _fixture.Create<ulong>();
//            _mockUserBl.Setup(x => x.GetUserByIdAsync(It.IsAny<ulong>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.GetUserByIdAsync(userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            BadRequestObjectResult badRequestResponse = Assert.IsType<BadRequestObjectResult>(result);
//            Log.Information("Verified if the response is of type BadRequestObjectResult");
//            BadRequest responseData = Assert.IsType<BadRequest>(badRequestResponse.Value);
//            Log.Information("Verified if the response object is of type BadRequest");
//            Assert.Equal(ResponseConstants.InvalidRequest, responseData.Title);
//            Log.Information("Verified if the response object's title is InvalidRequest");
//            _mockUserBl.Verify(x => x.GetUserByIdAsync(userId), Times.Once());
//            Log.Information("Verified if the GetUserByIdAsync method triggered only once"); 
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUserByIdAsync_ShouldReturnOk_WithUserObject()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(GetUserByIdAsync_ShouldReturnOk_WithUserObject));

//            // Arrange
//            User user = _fixture.Create<User>();
//            _mockUserBl.Setup(x => x.GetUserByIdAsync(It.IsAny<ulong>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, user));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.GetUserByIdAsync(user.Id);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            OkObjectResult okResponse = Assert.IsType<OkObjectResult>(result);
//            Log.Information("Verified if the response is of type OkObjectResult");
//            User responseData = Assert.IsType<User>(okResponse.Value);
//            Log.Information("Verified if the response is of type User");
//            Assert.Equal(user, responseData);
//            Log.Information("Verified if the mocked result object matchs with result object");
//            _mockUserBl.Verify(x => x.GetUserByIdAsync(user.Id), Times.Once());
//            Log.Information("Verified if the GetUserByIdAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task GetUserByIdAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(GetUserByIdAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));

//            // Arrange
//            User user = _fixture.Create<User>();
//            _mockUserBl.Setup(x => x.GetUserByIdAsync(It.IsAny<ulong>()))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _usersController.GetUserByIdAsync(user.Id));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // UpdateUserAsync
//        [Fact]
//        public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenInvalidUserIdIsPassed()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(UpdateUserAsync_ShouldReturnBadRequest_WhenInvalidUserIdIsPassed));

//            // Arrange
//            UpdateUser user = _fixture.Create<UpdateUser>();
//            ulong userId = _fixture.Create<ulong>();
//            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.UpdateUserAsync(user, userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            BadRequestObjectResult badRequestResponse = Assert.IsType<BadRequestObjectResult>(result);
//            Log.Information("Verified if the response is of type BadRequestObjectResult");
//            BadRequest responseData = Assert.IsType<BadRequest>(badRequestResponse.Value);
//            Log.Information("Verified if the response object is of type BadRequest");
//            Assert.Equal(ResponseConstants.InvalidRequest, responseData.Title);
//            Log.Information("Verified if the response object's title is InvalidRequest");
//            Assert.Equal(ResponseConstants.UserDoesNotExists, responseData.Detail);
//            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
//            _mockUserBl.Verify(x => x.UpdateUserAsync(user, userId), Times.Once());
//            Log.Information("Verified if the UpdateUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenInvalidRoleIdIsPassed()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(UpdateUserAsync_ShouldReturnBadRequest_WhenInvalidRoleIdIsPassed));

//            // Arrange
//            User user = _fixture.Create<User>();
//            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.SomeOfTheRoleNotPresent, HttpStatusCode.BadRequest));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.UpdateUserAsync(user, user.Id);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            BadRequestObjectResult badRequestResponse = Assert.IsType<BadRequestObjectResult>(result);
//            Log.Information("Verified if the response is of type BadRequestObjectResult");
//            BadRequest responseData = Assert.IsType<BadRequest>(badRequestResponse.Value);
//            Log.Information("Verified if the response object is of type BadRequest");
//            Assert.Equal(ResponseConstants.InvalidRequest, responseData.Title);
//            Log.Information("Verified if the response object's title is InvalidRequest");
//            Assert.Equal(ResponseConstants.SomeOfTheRoleNotPresent, responseData.Detail);
//            Log.Information("Verified if the response object's Detail is SomeOfTheRoleNotPresent");
//            _mockUserBl.Verify(x => x.UpdateUserAsync(user, user.Id), Times.Once());
//            Log.Information("Verified if the UpdateUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_ShouldReturnOk_WithUserObject()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(UpdateUserAsync_ShouldReturnOk_WithUserObject));

//            // Arrange
//            User user = _fixture.Create<User>();
//            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()))
//                .ReturnsAsync(new ServiceResponse<User>(ResponseConstants.Success, HttpStatusCode.OK, user));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.UpdateUserAsync(user, user.Id);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            OkObjectResult okResponse = Assert.IsType<OkObjectResult>(result);
//            Log.Information("Verified if the response is of type OkObjectResult");
//            User responseData = Assert.IsType<User>(okResponse.Value);
//            Log.Information("Verified if the response is of type User");
//            Assert.Equal(user, responseData);
//            Log.Information("Verified the mocked response object is equal to result object's value");
//            _mockUserBl.Verify(x => x.UpdateUserAsync(user, user.Id), Times.Once());
//            Log.Information("Verified if the UpdateUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task UpdateUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(UpdateUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));
//            // Arrange
//            User user = _fixture.Create<User>();
//            _mockUserBl.Setup(x => x.UpdateUserAsync(It.IsAny<UpdateUser>(), It.IsAny<ulong>()))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");

//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _usersController.UpdateUserAsync(user, user.Id));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // DeleteUserAsync
//        [Fact]
//        public async Task DeleteUserAsync_ShouldReturnBadRequest_WhenInvalidUserIdIsPassed()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnBadRequest_WhenInvalidUserIdIsPassed));

//            // Arrange
//            ulong userId = _fixture.Create<ulong>();
//            _mockUserBl.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>()))
//                .ReturnsAsync(new ApiResponse(ResponseConstants.UserDoesNotExists, HttpStatusCode.BadRequest));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.DeleteUserAsync(userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            BadRequestObjectResult badRequestResponse = Assert.IsType<BadRequestObjectResult>(result);
//            Log.Information("Verified if the response is of type BadRequestObjectResult");
//            BadRequest responseData = Assert.IsType<BadRequest>(badRequestResponse.Value);
//            Log.Information("Verified if the response is of type BadRequest");
//            Assert.Equal(ResponseConstants.UserDoesNotExists, responseData.Detail);
//            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
//            _mockUserBl.Verify(x => x.DeleteUserAsync(userId), Times.Once());
//            Log.Information("Verified if the DeleteUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task DeleteUserAsync_ShouldReturnNoContent_WhenUserIsDeleted()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnNoContent_WhenUserIsDeleted));

//            // Arrange
//            ulong userId = _fixture.Create<ulong>();
//            _mockUserBl.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>()))
//                .ReturnsAsync(new ApiResponse(ResponseConstants.Success, HttpStatusCode.OK));
//            Log.Information("Completed Moqing dependencies");

//            // Act
//            ActionResult result = await _usersController.DeleteUserAsync(userId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            NoContentResult badRequestResponse = Assert.IsType<NoContentResult>(result);
//            Log.Information("Verified if the response is of type NoContentResult");
//            _mockUserBl.Verify(x => x.DeleteUserAsync(userId), Times.Once());
//            Log.Information("Verified if the DeleteUserAsync method triggered only once");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task DeleteUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(DeleteUserAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));

//            // Arrange
//            ulong userId = _fixture.Create<ulong>();
//            _mockUserBl.Setup(x => x.DeleteUserAsync(It.IsAny<ulong>()))
//                       .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");


//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _usersController.DeleteUserAsync(userId));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }
//    }
//}
