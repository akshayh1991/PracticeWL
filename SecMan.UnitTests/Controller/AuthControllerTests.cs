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
//    public class AuthControllerTests
//    {
//        private readonly IFixture _fixture;
//        private readonly Mock<IAuthBL> _mockAuthBl;
//        private readonly AuthController _authController;
//        private readonly DefaultHttpContext _httpContext;

//        public AuthControllerTests()
//        {
//            _fixture = new Fixture();
//            _mockAuthBl = new Mock<IAuthBL>();
//            _authController = new AuthController(_mockAuthBl.Object);
//            _httpContext = new DefaultHttpContext();
//            _authController.ControllerContext = new ControllerContext
//            {
//                HttpContext = _httpContext
//            };
//            LoggerSetup.Initialize();
//        }


//        // LoginAsync

//        [Fact]
//        public async Task LoginAsync_ShouldReturn_OkResponse_WithToken()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(LoginAsync_ShouldReturn_OkResponse_WithToken));

//            // Arrange
//            var loginModel = _fixture.Create<LoginRequest>();
//            var loginResponse = _fixture.Create<LoginServiceResponse>();
//            _mockAuthBl.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
//                .ReturnsAsync(new ServiceResponse<LoginServiceResponse>(ResponseConstants.Success, HttpStatusCode.OK, loginResponse));
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            var result = await _authController.LoginAsync(loginModel);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
//            Log.Information("Verified if the result is of OkResponse");
//            LoginResponse loginResponseReturned = Assert.IsType<LoginResponse>(okResult.Value);
//            Log.Information("Verified it the result body is of LoginResponse Type");
//            Assert.Equal(loginResponseReturned.Token, loginResponse.Token);
//            Log.Information("Verified if the mocked token is returned from action method invocation");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnUnauthorizedResponse_WhenUserDoesNotExists()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(LoginAsync_ShouldReturn_UnUnauthorizedResponse_WhenUserDoesNotExists));

//            // Arrange
//            var loginModel = _fixture.Create<LoginRequest>();
//            _mockAuthBl.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
//                .ReturnsAsync(new ServiceResponse<LoginServiceResponse>(ResponseConstants.UserDoesNotExists, HttpStatusCode.Unauthorized));
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            var result = await _authController.LoginAsync(loginModel);
//            Log.Information("Test result: {@Result}", result);


//            // Assert
//            UnauthorizedObjectResult unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
//            Log.Information("Verified if the response is of type UnauthorizedObjectResult");
//            Unauthorized loginResponseReturned = Assert.IsType<Unauthorized>(unauthorizedResult.Value);
//            Log.Information("Verified if the response object is of type Unauthorized");
//            Assert.Equal(ResponseConstants.UserDoesNotExists, loginResponseReturned.Detail);
//            Log.Information("Verified if the response object's Detail is UserDoesNotExists");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturn_UnUnauthorizedResponse_WhenPasswordIsIncorrect()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(LoginAsync_ShouldReturn_UnUnauthorizedResponse_WhenPasswordIsIncorrect));

//            // Arrange
//            var loginModel = _fixture.Create<LoginRequest>();
//            _mockAuthBl.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
//                .ReturnsAsync(new ServiceResponse<LoginServiceResponse>(ResponseConstants.InvalidPassword, HttpStatusCode.Unauthorized));
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            var result = await _authController.LoginAsync(loginModel);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            UnauthorizedObjectResult conflictResult = Assert.IsType<UnauthorizedObjectResult>(result);
//            Log.Information("Verified if the response is of type UnauthorizedObjectResult");
//            Unauthorized loginResponseReturned = Assert.IsType<Unauthorized>(conflictResult.Value);
//            Log.Information("Verified if the response object is of type Unauthorized");
//            Assert.Equal(ResponseConstants.InvalidPassword, loginResponseReturned.Detail);
//            Log.Information("Verified if the response object's Detail is InvalidPassword");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task LoginAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(LoginAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));

//            // Arrange
//            LoginRequest loginModel = _fixture.Create<LoginRequest>();
//            _mockAuthBl.Setup(x => x.LoginAsync(It.IsAny<LoginRequest>()))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");


//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _authController.LoginAsync(loginModel));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }



//        // ValidateSessionAsync
//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturnUnauthorized_WhenSessionId_IsNullOrWhiteSpace()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(ValidateSessionAsync_ShouldReturnUnauthorized_WhenSessionId_IsNullOrWhiteSpace));

//            // Arrange
//            string? sessionId = string.Empty;

//            // Act
//            var result = await _authController.ValidateSessionAsync(sessionId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            UnauthorizedObjectResult unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
//            Log.Information("Verified if the response is of type UnauthorizedObjectResult");
//            Unauthorized validateSessionResponseReturned = Assert.IsType<Unauthorized>(unauthorizedResult.Value);
//            Log.Information("Verified if the response object is of type Unauthorized");
//            Assert.Equal(ResponseConstants.InvalidSessionId, validateSessionResponseReturned.Detail);
//            Log.Information("Verified if the response object's Detail is InvalidSessionId");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturn_OkResponse_WithToken()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(ValidateSessionAsync_ShouldReturn_OkResponse_WithToken));

//            // Arrange
//            string sessionId = _fixture.Create<string>();
//            var loginResponse = _fixture.Create<LoginServiceResponse>();
//            _mockAuthBl.Setup(x => x.ValidateSessionAsync(It.IsAny<string>()))
//                .ReturnsAsync(new ServiceResponse<LoginServiceResponse>(ResponseConstants.Success, HttpStatusCode.OK, loginResponse));
//            Log.Information("Completed Moqing dependencies");


//            // Act
//            var result = await _authController.ValidateSessionAsync(sessionId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            OkObjectResult unauthorizedResult = Assert.IsType<OkObjectResult>(result);
//            Log.Information("Verified if the response is of type OkObjectResult");
//            LoginResponse validateSessionResponseReturned = Assert.IsType<LoginResponse>(unauthorizedResult.Value);
//            Assert.Equal(loginResponse.Token, validateSessionResponseReturned.Token);
//            Log.Information("Verified if the mocked token is returned from action method invocation");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturn_UnauthorizedResponse_If_SessionId_IsInvalid()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(ValidateSessionAsync_ShouldReturn_UnauthorizedResponse_If_SessionId_IsInvalid));

//            // Arrange
//            string sessionId = _fixture.Create<string>();
//            _mockAuthBl.Setup(x => x.ValidateSessionAsync(It.IsAny<string>()))
//                .ReturnsAsync(new ServiceResponse<LoginServiceResponse>(ResponseConstants.InvalidSessionId, HttpStatusCode.Unauthorized));

//            Log.Information("Completed Moqing dependencies");

//            // Act
//            var result = await _authController.ValidateSessionAsync(sessionId);
//            Log.Information("Test result: {@Result}", result);

//            // Assert
//            UnauthorizedObjectResult unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
//            Log.Information("Verified if the response is of type UnauthorizedObjectResult");
//            Unauthorized validateSessionResponseReturned = Assert.IsType<Unauthorized>(unauthorizedResult.Value);
//            Log.Information("Verified if the response object is of type Unauthorized");
//            Assert.Equal(ResponseConstants.InvalidSessionId, validateSessionResponseReturned.Detail);
//            Log.Information("Verified if the response object's Detail is InvalidSessionId");
//            Log.Information("Test completed successfully");
//        }


//        [Fact]
//        public async Task ValidateSessionAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers()
//        {
//            Log.Information("Starting controller test: {@TestName}", nameof(ValidateSessionAsync_ShouldReturnInternalServerError_WhenSqliteExceptionOccers));

//            // Arrange
//            string sessionId = _fixture.Create<string>();
//            _mockAuthBl.Setup(x => x.ValidateSessionAsync(It.IsAny<string>()))
//                .ThrowsAsync(new SqliteException(nameof(StatusCodes.Status500InternalServerError), StatusCodes.Status500InternalServerError));
//            Log.Information("Completed Moqing dependencies");


//            // Act & Assert
//            await Assert.ThrowsAsync<SqliteException>(() => _authController.ValidateSessionAsync(sessionId));
//            Log.Information("Verified if the method throwing SqliteException for any db related issues");
//            Log.Information("Test completed successfully");
//        }
//    }
//}
