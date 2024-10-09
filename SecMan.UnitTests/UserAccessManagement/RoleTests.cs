using Moq;
using SecMan.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using UserAccessManagement.Controllers;
using SecMan.Interfaces.BL;
using Microsoft.AspNetCore.Mvc;
using AutoFixture;
using SecMan.UnitTests.Logger;
using Serilog;
using Microsoft.AspNetCore.Http;
using SecMan.Data.Exceptions;
using SecMan.Data.Repository;
using SecMan.Data.DAL;

namespace NewTestProject
{
    public class RoleTests
    {
        private readonly Mock<IRoleDal> _mockRoleDAL;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RoleBL _roleBL;
        private readonly Mock<ILogger<SecMan.Data.Role>> _mockLogger;


        private readonly IRoleDal _roleDAL;
        private readonly DbContextOptions<DbContext> _dbContextOptions;
        private readonly RoleController _roleController;
        private readonly Mock<IRoleBL> _mockRoleBAL;
        private readonly Mock<ILogger<RoleController>> _mockLoggerBL;

        static RoleTests()
        {
            // Initialize Serilog
            LoggerSetup.Initialize();
        }

        public RoleTests()
        {
            // Set up mock for IRoleDal
            _mockRoleDAL = new Mock<IRoleDal>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            // Inject the mock into RoleBL
            _roleBL = new RoleBL(_mockUnitOfWork.Object);
            _mockLogger = new Mock<ILogger<SecMan.Data.Role>>();

            _mockRoleBAL = new Mock<IRoleBL>();
            _mockLoggerBL = new Mock<ILogger<RoleController>>();
            _roleController = new RoleController(_mockRoleBAL.Object, _mockLoggerBL.Object);

            var connection = new SqliteConnection("Data Source=C:\\Users\\akshay_huded\\OneDrive - Torry Harris Business Solutions Pvt Ltd\\Desktop\\New folder\\nextgen_sw\\SecurityManager\\SecMan.Db\\SecMan.Db");
            connection.Open();

            _dbContextOptions = new DbContextOptionsBuilder<DbContext>()
                                .UseSqlite(connection)
                                .Options;

            // Ensure the database schema is created
            using (var context = new DbContext(_dbContextOptions))
            {
                context.Database.EnsureCreated();
            }

            // Instantiate the real DAL and inject it into RoleBL
            _roleDAL = new SecMan.Data.Role(_mockLogger.Object);
        }






        //************************************** Positive Scenario's **********************************************
        //[Fact]
        //public async Task GetAllRoles_ShouldReturnRolesFromMockDAL()
        //{
        //    // Arrange
        //    var mockRoleDAL = new Mock<IRoleDal>();
        //    var roleBL = new RoleBL(mockRoleDAL.Object);

        //    var fixture = new Fixture(); // AutoFixture instance
        //    var expectedRoles = fixture.Build<GetRoleDto>()
        //        .With(r => r.Id, 1UL) // Set the Id to 1 for the first item
        //        .With(r => r.Name, "Administrator") // Set the Name to "Administrator" for the first item
        //        .CreateMany(1) // Create the first item
        //        .ToList();

        //    expectedRoles.Add(fixture.Build<GetRoleDto>()
        //        .With(r => r.Id, 2UL) // Set the Id to 2
        //        .With(r => r.Name, "User") // Set the Name to "User"
        //        .Create()); // Create the second item

        //    // Mock GetAllRolesAsync to return roles
        //    _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ReturnsAsync(expectedRoles);

        //    // Log the test start
        //    Log.Information("Starting test: GetAllRoles_ShouldReturnRolesFromMockDAL");

        //    // Act
        //    var result = await _roleController.GetAllRoles();

        //    // Log the result
        //    Log.Information("Result from GetAllRoles: {@Result}", result);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnedRoles = Assert.IsType<List<GetRoleDto>>(okResult.Value);
        //    Assert.Equal(2, returnedRoles.Count);
        //    Assert.Contains(returnedRoles, r => r.Name == "Administrator");

        //    // Log verification
        //    Log.Information("Verified that role list contains 'Administrator'");

        //    // Verify that BL method was called once
        //    _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

        //    // Log verification
        //    Log.Information("Verified that BL method was called once");

        //    // Comment below lines if integration not required
        //    var dalresult = _roleDAL.GetAllRolesAsync();
        //    var dalValue = dalresult.Result;

        //    // Log DAL results
        //    Log.Information("DAL Result: {@DalValue}", dalValue);

        //    Assert.NotNull(dalresult);
        //    Assert.Contains(dalValue, r => r.Name == "Administrator");

        //    // Log success
        //    Log.Information("Test completed successfully");
        //}

        [Fact]
        public async Task GetAllRoles_ShouldReturnRolesFromMockDAL()
        {
            // Arrange
            var mockRoleDAL = new Mock<IRoleDal>();
            var roleBL = new RoleBL(_mockUnitOfWork.Object);
            var fixture = new Fixture(); // AutoFixture instance

            var expectedRoles = fixture.Build<GetRoleDto>()
                .With(r => r.Id, 1UL) // Set the Id to 1 for the first item
                .With(r => r.Name, "Administrator") // Set the Name to "Administrator" for the first item
                .CreateMany(1) // Create the first item
                .ToList();

            expectedRoles.Add(fixture.Build<GetRoleDto>()
                .With(r => r.Id, 2UL) // Set the Id to 2
                .With(r => r.Name, "User") // Set the Name to "User"
                .Create()); // Create the second item

            // Mock GetAllRolesAsync to return roles
            _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ReturnsAsync(expectedRoles);

            // Log the test start
            Log.Information("Starting test: GetAllRoles_ShouldReturnRolesFromMockDAL");

            // Act
            var result = await _roleController.GetAllRoles();

            // Log the result
            Log.Information("Result from GetAllRoles: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Assertion passed: Result is OkObjectResult.");

            var returnedRoles = Assert.IsType<List<GetRoleDto>>(okResult.Value);
            Log.Information("Assertion passed: Result value is a list of GetRoleDto.");

            Assert.Equal(2, returnedRoles.Count);
            Log.Information("Assertion passed: Returned roles count is 2.");

            Assert.Contains(returnedRoles, r => r.Name == "Administrator");
            Log.Information("Verified that role list contains 'Administrator'");

            // Verify that BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);
            Log.Information("Verified that BL method was called once.");

            // Comment below lines if integration not required
            var dalresult = await _roleDAL.GetAllRolesAsync();
            var dalValue = dalresult;

            // Log DAL results
            Log.Information("DAL Result: {@DalValue}", dalValue);

            Assert.NotNull(dalresult);
            Log.Information("Assertion passed: DAL result is not null.");

            Assert.Contains(dalValue, r => r.Name == "Administrator");
            Log.Information("Assertion passed: DAL value contains 'Administrator'.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnEmptyList_WhenNoRolesPresent()
        {
            // Arrange
            Log.Information("Starting test: GetAllRoles_ShouldReturnEmptyList_WhenNoRolesPresent");

            var expectedRoles = new List<GetRoleDto>(); // Empty list

            _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ReturnsAsync(expectedRoles);

            // Act
            var result = await _roleController.GetAllRoles();

            // Log the result
            Log.Information("Result from GetAllRoles: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Assertion passed: Result is OkObjectResult.");

            var returnedRoles = Assert.IsType<List<GetRoleDto>>(okResult.Value);
            Log.Information("Assertion passed: Result value is a list of GetRoleDto.");

            Assert.Empty(returnedRoles); // Verify that the returned list is empty
            Log.Information("Verified that the returned role list is empty.");

            // Verify that BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);
            Log.Information("Verified that BL method GetAllRolesAsync was called once.");
        }

        [Fact]
        public async Task AddRole_ShouldReturnCreated_WhenRoleIsSuccessfullyAdded()
        {
            // Arrange
            Log.Information("Starting test: AddRole_ShouldReturnCreated_WhenRoleIsSuccessfullyAdded");

            var fixture = new Fixture();

            // Use fixture to create a new role
            var newRole = fixture.Create<CreateRole>();

            // Create a corresponding created role DTO
            var createdRole = fixture.Build<SecMan.Data.SQLCipher.Role>()
                .With(r => r.Id, 1UL)
                //.With(r => r.NoOfUsers, 5)
                .Create();

            // Mock the DAL method to return the created role
            _mockUnitOfWork.Setup(dal => dal.IRoleRepository.AddRoleAsync(It.IsAny<CreateRole>()))
                .ReturnsAsync(createdRole);

            // Act
            Log.Information("Adding role: {@NewRole}", newRole);
            var result = await _roleBL.AddRoleAsync(newRole);

            // Log the result
            Log.Information("Result from AddRoleAsync: {@Result}", result);

            // Assert
            _mockUnitOfWork.Verify(dal => dal.IRoleRepository.AddRoleAsync(It.IsAny<CreateRole>()), Times.Once);
            Log.Information("Verified that AddRoleAsync was called once.");

            Assert.NotNull(result);
            Log.Information("Assertion passed: Result is not null.");

            Assert.Equal(createdRole.Id, result.Id);
            Log.Information("Assertion passed: Role ID matches.");

            Assert.Equal(createdRole.Name, result.Name);
            Log.Information("Assertion passed: Role name matches.");

            Assert.Equal(createdRole.Description, result.Description);
            Log.Information("Assertion passed: Role description matches.");

            Assert.Equal(createdRole.IsLoggedOutType, result.IsLoggedOutType);
            Log.Information("Assertion passed: IsLoggedOutType matches.");

            //Assert.Equal(createdRole.NoOfUsers, result.NoOfUsers);
            //Log.Information("Assertion passed: Number of users matches.");

            // Log verification
            Log.Information("Verified that the role was added successfully with Id: {Id}", createdRole.Id);
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnOk_WhenRoleIsSuccessfullyUpdated()
        {
            // Arrange
            ulong id = 12; // Assuming the ID for the role being updated is 12

            var fixture = new Fixture();

            // Use fixture to create an update role DTO
            var updateRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var updatedRole = fixture.Build<GetRoleDto>()
                .With(r => r.Id, id)
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                //.With(r => r.IsLoggedOutType, "true") // Ensure this matches your DTO type
                .With(r => r.NoOfUsers, 5)
                .Create();

            var mockRoleDAL = new Mock<IRoleDal>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var roleBL = new RoleBL(_mockUnitOfWork.Object);
            var mockRoleController = new RoleController(roleBL, mockLogger.Object);

            // Mock UpdateRoleAsync to return the updated role
            _mockUnitOfWork.Setup(dal => dal.IRoleRepository.UpdateRoleAsync(id, It.IsAny<CreateRole>())).ReturnsAsync(updatedRole);

            // Log the arrangement
            mockLogger.Object.LogInformation("Testing UpdateRole with ID: {RoleId}", id);
            mockLogger.Object.LogInformation("UpdateRoleDto: {@UpdateRoleDto}", updateRoleDto);

            // Act
            var result = await mockRoleController.UpdateRole(id, updateRoleDto);

            // Log the result
            mockLogger.Object.LogInformation("Result received from UpdateRole: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Log.Information("Assertion passed: Result is OkObjectResult.");

            var returnedRole = Assert.IsType<GetRoleDto>(okResult.Value);
            Log.Information("Assertion passed: Result value is of type GetRoleDto.");

            Assert.Equal(updatedRole.Id, returnedRole.Id);
            Log.Information("Assertion passed: Role ID matches.");

            Assert.Equal(updatedRole.Name, returnedRole.Name);
            Log.Information("Assertion passed: Role name matches.");

            Assert.Equal(updatedRole.Description, returnedRole.Description);
            Log.Information("Assertion passed: Role description matches.");

            Assert.Equal(updatedRole.IsLoggedOutType, returnedRole.IsLoggedOutType);
            Log.Information("Assertion passed: IsLoggedOutType matches.");

            Assert.Equal(updatedRole.NoOfUsers, returnedRole.NoOfUsers);
            Log.Information("Assertion passed: Number of users matches.");

            // Verify that the DAL method was called once with the expected parameters
            _mockUnitOfWork.Verify(dal => dal.IRoleRepository.UpdateRoleAsync(id, It.Is<CreateRole>(dto =>
                dto.Name == updateRoleDto.Name &&
                dto.Description == updateRoleDto.Description &&
                dto.IsLoggedOutType == updateRoleDto.IsLoggedOutType &&
                dto.LinkUsers.SequenceEqual(updateRoleDto.LinkUsers))), Times.Once);

            // Log verification completion
            mockLogger.Object.LogInformation("DAL method UpdateRoleAsync was called with expected parameters.");
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnNoContent_WhenRoleIsSuccessfullyDeleted()
        {
            // Arrange
            var roleId = 1UL;
            var mockRoleBL = new Mock<IRoleBL>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var roleController = new RoleController(mockRoleBL.Object, mockLogger.Object);

            // Mock DeleteRoleAsync to return true indicating successful deletion
            mockRoleBL.Setup(bl => bl.DeleteRoleAsync(roleId)).ReturnsAsync(true);

            // Log the arrangement
            mockLogger.Object.LogInformation("Testing DeleteRole with RoleId: {RoleId}", roleId);

            // Act
            var result = await roleController.DeleteRole(roleId);

            // Log the result
            mockLogger.Object.LogInformation("Result received from DeleteRole: {@Result}", result);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
            Log.Information("Assertion passed: NoContentResult status code is 204.");

            // Verify that BL method was called once with the correct role ID
            mockRoleBL.Verify(bl => bl.DeleteRoleAsync(roleId), Times.Once);
            Log.Information("Verified that BL method DeleteRoleAsync was called with RoleId: {RoleId}.", roleId);

            // Log verification completion
            mockLogger.Object.LogInformation("BL method DeleteRoleAsync was called with RoleId: {RoleId} and verified.", roleId);
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnRole_WhenRoleExists()
        {
            // Arrange
            var mockRoleDAL = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<RoleBL>>(); // Create a mock logger for RoleBL
            var roleBL = new RoleBL(_mockUnitOfWork.Object);
            ulong id = 10; // Use the same id consistently
            var fixture = new Fixture();

            // Create the expected role object
            var expectedRole = fixture.Build<SecMan.Data.SQLCipher.Role>()
                .With(r => r.Id, id) // Set the Id to 10
                .With(r => r.Name, "Administrator") // Set the Name to "Administrator"
                .Create();

            // Mock the DAL method to return the expected role
            _mockUnitOfWork.Setup(dal => dal.IRoleRepository.GetById(id)).ReturnsAsync(expectedRole);

            // Log the arrangement
            mockLogger.Object.LogInformation("Arranging test for GetRoleById with RoleId: {RoleId}", id);

            // Act
            var result = await roleBL.GetRoleByIdAsync(id);

            // Log the result
            mockLogger.Object.LogInformation("Result received from GetRoleById: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<GetRoleDto>(result); // Assuming the method returns GetRoleDto directly
            Assert.Equal(expectedRole.Id, okResult.Id);
            Log.Information("Assertion passed: Role ID matches.");

            Assert.Equal(expectedRole.Name, okResult.Name);
            Log.Information("Assertion passed: Role name matches.");

            // Verify that the DAL method was called once with the expected parameter
            mockRoleDAL.Verify(dal => dal.IRoleRepository.GetById(id), Times.Once);
            Log.Information("Verified that DAL method GetRoleByIdAsync was called with RoleId: {RoleId}.", id);

            // Log verification
            mockLogger.Object.LogInformation("DAL method GetRoleByIdAsync was called with RoleId: {RoleId} and verified.", id);
        }


        //************************************** Negetive Scenario's **********************************************


        //[Fact]
        //public async Task GetAllRoles_ShouldReturnInternalServerError_WhenExceptionOccurs()
        //{
        //    // Arrange
        //    var exceptionMessage = "An unexpected error occurred while retrieving roles.";
        //    var expectedException = new Exception(exceptionMessage);

        //    // Setup the mock to throw an exception when GetAllRolesAsync is called
        //    _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ThrowsAsync(expectedException);

        //    // Log the test start
        //    Log.Information("Starting test: GetAllRoles_ShouldReturnInternalServerError_WhenExceptionOccurs");

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.GetAllRoles());

        //    // Verify that the exception message matches
        //    Assert.Equal(exceptionMessage, exception.Message);

        //    // Verify that the BL method was called once
        //    _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

        //    // Log verification
        //    Log.Information("Verified that the exception was thrown and BL method was called once");

        //    // Log success
        //    Log.Information("Test completed successfully");
        //}

        //[Fact]
        //public async Task GetAllRoles_ShouldReturnDbUpdateException_WhenDbUpdateException()
        //{
        //    // Arrange
        //    var exceptionMessage = "An error occurred while retrieving roles from the database.";
        //    var expectedException = new DbUpdateException(exceptionMessage);

        //    // Setup the mock to throw an exception when GetAllRolesAsync is called
        //    _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ThrowsAsync(expectedException);

        //    // Log the test start
        //    Log.Information("Starting test: GetAllRoles_ShouldReturnDbUpdateException_WhenDbUpdateException");

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.GetAllRoles());

        //    // Verify that the exception message matches
        //    Assert.Equal(exceptionMessage, exception.Message);

        //    // Verify that the BL method was called once
        //    _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

        //    // Log verification
        //    Log.Information("Verified that the exception was thrown and BL method was called once");

        //    // Log success
        //    Log.Information("Test completed successfully");
        //}

        //[Fact]
        //public async Task GetRoleById_ShouldReturnInternalServerError_WhenExceptionOccurs()
        //{
        //    // Arrange
        //    ulong Id = 10;
        //    var exceptionMessage = "An unexpected error occurred while retrieving the role by ID.";
        //    var expectedException = new Exception(exceptionMessage);

        //    // Setup the mock to throw an exception when GetRoleByIdAsync is called
        //    _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(Id)).ThrowsAsync(expectedException);

        //    // Log the test start
        //    Log.Information("Starting test: GetRoleById_ShouldReturnInternalServerError_WhenExceptionOccurs");

        //    // Act & Assert
        //    var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.GetRoleById(Id));

        //    // Verify that the exception message matches
        //    Assert.Equal(exceptionMessage, exception.Message);

        //    // Verify that the BL method was called once
        //    _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(Id), Times.Once);

        //    // Log verification
        //    Log.Information("Verified that the exception was thrown and BL method was called once");

        //    // Log success
        //    Log.Information("Test completed successfully");
        //}

        [Fact]
        public async Task GetAllRoles_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var exceptionMessage = "An unexpected error occurred while retrieving roles.";
            var expectedException = new Exception(exceptionMessage);

            // Setup the mock to throw an exception when GetAllRolesAsync is called
            _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetAllRoles_ShouldReturnInternalServerError_WhenExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.GetAllRoles());

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task GetAllRoles_ShouldReturnDbUpdateException_WhenDbUpdateException()
        {
            // Arrange
            var exceptionMessage = "An error occurred while retrieving roles from the database.";
            var expectedException = new DbUpdateException(exceptionMessage);

            // Setup the mock to throw an exception when GetAllRolesAsync is called
            _mockRoleBAL.Setup(bal => bal.GetAllRolesAsync()).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetAllRoles_ShouldReturnDbUpdateException_WhenDbUpdateException");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.GetAllRoles());

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            ulong id = 10;
            var exceptionMessage = "An unexpected error occurred while retrieving the role by ID.";
            var expectedException = new Exception(exceptionMessage);

            // Setup the mock to throw an exception when GetRoleByIdAsync is called
            _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(id)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetRoleById_ShouldReturnInternalServerError_WhenExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.GetRoleById(id));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(id), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong id = fixture.Create<ulong>();
            var exceptionMessage = "An unexpected error occurred while retrieving the role by ID.";
            var expectedException = new DbUpdateException(exceptionMessage);

            // Setup the mock to throw an exception when GetRoleByIdAsync is called
            _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(id)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetRoleById_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.GetRoleById(id));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(id), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnCommonBadRequestForRoleException_WhenCommonBadRequestForRoleExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong id = fixture.Create<ulong>();
            var exceptionMessage = "Provided input request parameter is not valid.";
            var expectedException = new CommonBadRequestForRole(exceptionMessage);

            // Setup the mock to throw an exception when GetRoleByIdAsync is called
            _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(id)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetRoleById_ShouldReturnCommonBadRequestForRoleException_WhenCommonBadRequestForRoleExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CommonBadRequestForRole>(() => _roleController.GetRoleById(id));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(id), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();
            var updateRoleDto = fixture.Create<CreateRole>();

            var exceptionMessage = "An unexpected error occurred while updating the role.";
            var expectedException = new Exception(exceptionMessage);

            // Setup the mock to throw an exception when UpdateRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.UpdateRoleAsync(roleId, updateRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: UpdateRole_ShouldReturnInternalServerError_WhenExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.UpdateRole(roleId, updateRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnDbUpdate_WhenDbUpdateOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();
            var updateRoleDto = fixture.Create<CreateRole>();

            var exceptionMessage = "An error occurred while updating the role in the database.";
            var expectedException = new DbUpdateException(exceptionMessage);

            // Setup the mock to throw an exception when UpdateRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.UpdateRoleAsync(roleId, updateRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: UpdateRole_ShouldReturnDbUpdate_WhenDbUpdateOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.UpdateRole(roleId, updateRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);
            Log.Information("Assertion passed: Exception message matches.");

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);
            Log.Information("Verified that the exception was thrown and BL method was called once.");

            // Log success
            Log.Information("Test completed successfully.");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnBadRequestForLinkUsersNotExits_WhenBadRequestForLinkUsersNotExitsOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();
            var updateRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "Some provided user IDs do not exist. Please provide valid user IDs.";
            var expectedException = new BadRequestForLinkUsersNotExits(exceptionMessage);

            // Setup the mock to throw an exception when UpdateRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.UpdateRoleAsync(roleId, updateRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: UpdateRole_ShouldReturnBadRequestForLinkUsersNotExits_WhenBadRequestForLinkUsersNotExitsOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestForLinkUsersNotExits>(() => _roleController.UpdateRole(roleId, updateRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnUpdatingExistingNameException_WhenUpdatingExistingNameExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();
            var updateRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "A role with the same name already exists.";
            var expectedException = new UpdatingExistingNameException(exceptionMessage);

            // Setup the mock to throw an exception when UpdateRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.UpdateRoleAsync(roleId, updateRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: UpdateRole_ShouldReturnUpdatingExistingNameException_WhenUpdatingExistingNameExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UpdatingExistingNameException>(() => _roleController.UpdateRole(roleId, updateRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnCommonBadRequestForRoleException_WhenCommonBadRequestForRoleOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();
            var updateRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "A role with the same name already exists.";
            var expectedException = new CommonBadRequestForRole(exceptionMessage);

            // Setup the mock to throw an exception when UpdateRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.UpdateRoleAsync(roleId, updateRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: UpdateRole_ShouldReturnCommonBadRequestForRoleException_WhenCommonBadRequestForRoleOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CommonBadRequestForRole>(() => _roleController.UpdateRole(roleId, updateRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task AddRole_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            var addRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "An unexpected error occurred while adding the role";
            var expectedException = new Exception(exceptionMessage);

            // Setup the mock to throw an exception when AddRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.AddRoleAsync(addRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: AddRole_ShouldReturnInternalServerError_WhenExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.AddRole(addRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.AddRoleAsync(addRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task AddRole_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            var addRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "An error occurred while updating the database.";
            var expectedException = new DbUpdateException(exceptionMessage);

            // Setup the mock to throw an exception when AddRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.AddRoleAsync(addRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: AddRole_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.AddRole(addRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.AddRoleAsync(addRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task AddRole_ShouldReturnBadRequestForLinkUsersNotExits_WhenBadRequestForLinkUsersNotExitsOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            var addRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "Some provided user IDs do not exist. Please provide valid user IDs.";
            var expectedException = new BadRequestForLinkUsersNotExits(exceptionMessage);

            // Setup the mock to throw an exception when AddRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.AddRoleAsync(addRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: AddRole_ShouldReturnBadRequestForLinkUsersNotExits_WhenBadRequestForLinkUsersNotExitsOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestForLinkUsersNotExits>(() => _roleController.AddRole(addRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.AddRoleAsync(addRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task AddRole_ShouldReturnConflictException_WhenConflictExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            var addRoleDto = fixture.Build<CreateRole>()
                .With(r => r.Name, "Updated Akshay")
                .With(r => r.Description, "Updated Description")
                .With(r => r.IsLoggedOutType, true)
                .With(r => r.LinkUsers, new List<ulong> { 1, 2, 3, 4, 5 })
                .Create();

            var exceptionMessage = "A role with the same name already exists.";
            var expectedException = new ConflictException(exceptionMessage);

            // Setup the mock to throw an exception when AddRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.AddRoleAsync(addRoleDto)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: AddRole_ShouldReturnConflictException_WhenConflictExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _roleController.AddRole(addRoleDto));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.AddRoleAsync(addRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnException_WhenExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();

            var exceptionMessage = "An unexpected error occurred while deleting the role.";
            var expectedException = new Exception(exceptionMessage);

            // Setup the mock to throw an exception when DeleteRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.DeleteRoleAsync(roleId)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: DeleteRole_ShouldReturnException_WhenExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.DeleteRole(roleId));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.DeleteRoleAsync(roleId), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();

            var exceptionMessage = "An error occurred while deleting the role from the database.";
            var expectedException = new DbUpdateException(exceptionMessage);

            // Setup the mock to throw an exception when DeleteRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.DeleteRoleAsync(roleId)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: DeleteRole_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.DeleteRole(roleId));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.DeleteRoleAsync(roleId), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task DeleteRole_ShouldReturnCommonBadRequestForRole_WhenCommonBadRequestForRoleOccurs()
        {
            // Arrange
            var fixture = new Fixture();
            ulong roleId = fixture.Create<ulong>();

            var exceptionMessage = "The role with the specified ID was not found.";
            var expectedException = new CommonBadRequestForRole(exceptionMessage);

            // Setup the mock to throw an exception when DeleteRoleAsync is called
            _mockRoleBAL.Setup(bal => bal.DeleteRoleAsync(roleId)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: DeleteRole_ShouldReturnCommonBadRequestForRole_WhenCommonBadRequestForRoleOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CommonBadRequestForRole>(() => _roleController.DeleteRole(roleId));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.DeleteRoleAsync(roleId), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }


    }
}