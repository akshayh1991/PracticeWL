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

namespace NewTestProject
{
    public class RoleTests
    {
        private readonly Mock<IRoleDAL> _mockRoleDAL;
        private readonly RoleBL _roleBL;
        private readonly Mock<ILogger<SecMan.Data.Role>> _mockLogger;


        private readonly IRoleDAL _roleDAL;
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
            // Set up mock for IRoleDAL
            _mockRoleDAL = new Mock<IRoleDAL>();
            // Inject the mock into RoleBL
            _roleBL = new RoleBL(_mockRoleDAL.Object);
            _mockLogger = new Mock<ILogger<SecMan.Data.Role>>();

            _mockRoleBAL = new Mock<IRoleBL>();
            _mockLoggerBL = new Mock<ILogger<RoleController>>();
            _roleController = new RoleController(_mockRoleBAL.Object, _mockLoggerBL.Object);

            var connection = new SqliteConnection("Data Source=C:\\Users\\akshay_huded\\Desktop\\New folder\\nextgen_sw\\SecurityManager\\SecMan.Db\\SecMan.Db");
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
        [Fact]
        public async Task GetAllRoles_ShouldReturnRolesFromMockDAL()
        {
            // Arrange
            var mockRoleDAL = new Mock<IRoleDAL>();
            var roleBL = new RoleBL(mockRoleDAL.Object);

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
            var returnedRoles = Assert.IsType<List<GetRoleDto>>(okResult.Value);
            Assert.Equal(2, returnedRoles.Count);
            Assert.Contains(returnedRoles, r => r.Name == "Administrator");

            // Log verification
            Log.Information("Verified that role list contains 'Administrator'");

            // Verify that BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

            // Log verification
            Log.Information("Verified that BL method was called once");

            // Comment below lines if integration not required
            var dalresult = _roleDAL.GetAllRolesAsync();
            var dalValue = dalresult.Result;

            // Log DAL results
            Log.Information("DAL Result: {@DalValue}", dalValue);

            Assert.NotNull(dalresult);
            Assert.Contains(dalValue, r => r.Name == "Administrator");

            // Log success
            Log.Information("Test completed successfully");
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
            var returnedRoles = Assert.IsType<List<GetRoleDto>>(okResult.Value);
            Assert.Empty(returnedRoles); // Verify that the returned list is empty

            // Log verification
            Log.Information("Verified that the returned role list is empty");

            // Verify that BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

            // Log method verification
            Log.Information("Verified that BL method GetAllRolesAsync was called once");
        }

        [Fact]
        public async Task AddRole_ShouldReturnCreated_WhenRoleIsSuccessfullyAdded()
        {
            // Arrange
            Log.Information("Starting test: AddRole_ShouldReturnCreated_WhenRoleIsSuccessfullyAdded");

            var fixture = new Fixture();
            var newRole = new AddRoleDto
            {
                Name = "Akshay",
                Description = "Test Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

            var createdRole = new GetRoleDto
            {
                Id = 1UL,
                Name = "Akshay",
                Description = "Test Description",
                IsLoggedOutType = "true",
                NoOfUsers = 5
            };

            // Mock the DAL method to return the created role
            _mockRoleDAL.Setup(dal => dal.AddRoleAsync(It.IsAny<AddRoleDto>())).ReturnsAsync(createdRole);

            // Act
            Log.Information("Adding role: {@NewRole}", newRole);
            var result = await _roleBL.AddRoleAsync(newRole);

            // Log the result
            Log.Information("Result from AddRoleAsync: {@Result}", result);

            // Assert
            _mockRoleDAL.Verify(dal => dal.AddRoleAsync(It.IsAny<AddRoleDto>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(createdRole.Id, result.Id);
            Assert.Equal(createdRole.Name, result.Name);
            Assert.Equal(createdRole.Description, result.Description);
            Assert.Equal(createdRole.IsLoggedOutType, result.IsLoggedOutType);
            Assert.Equal(createdRole.NoOfUsers, result.NoOfUsers);

            // Log verification
            Log.Information("Verified that the role was added successfully with Id: {Id}", createdRole.Id);
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnOk_WhenRoleIsSuccessfullyUpdated()
        {
            // Arrange
            ulong id = 12; // Assuming the ID for the role being updated is 12

            var updateRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

            var updatedRole = new GetRoleDto
            {
                Id = id,
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = "true", // Ensure this matches your DTO type
                NoOfUsers = 5
            };

            var mockRoleDAL = new Mock<IRoleDAL>();
            var mockLogger = new Mock<ILogger<RoleController>>();
            var roleBL = new RoleBL(mockRoleDAL.Object);
            var mockRoleController = new RoleController(roleBL, mockLogger.Object);

            // Mock UpdateRoleAsync to return the updated role
            mockRoleDAL.Setup(dal => dal.UpdateRoleAsync(id, It.IsAny<AddRoleDto>())).ReturnsAsync(updatedRole);

            // Log the arrangement
            mockLogger.Object.LogInformation("Testing UpdateRole with ID: {RoleId}", id);
            mockLogger.Object.LogInformation("UpdateRoleDto: {@UpdateRoleDto}", updateRoleDto);

            // Act
            var result = await mockRoleController.UpdateRole(id, updateRoleDto);

            // Log the result
            mockLogger.Object.LogInformation("Result received from UpdateRole: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedRole = Assert.IsType<GetRoleDto>(okResult.Value);

            Assert.Equal(updatedRole.Id, returnedRole.Id);
            Assert.Equal(updatedRole.Name, returnedRole.Name);
            Assert.Equal(updatedRole.Description, returnedRole.Description);
            Assert.Equal(updatedRole.IsLoggedOutType, returnedRole.IsLoggedOutType);
            Assert.Equal(updatedRole.NoOfUsers, returnedRole.NoOfUsers);

            // Verify that the DAL method was called once with the expected parameters
            mockRoleDAL.Verify(dal => dal.UpdateRoleAsync(id, It.Is<AddRoleDto>(dto =>
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

            // Verify that BL method was called once with the correct role ID
            mockRoleBL.Verify(bl => bl.DeleteRoleAsync(roleId), Times.Once);

            // Log verification completion
            mockLogger.Object.LogInformation("BL method DeleteRoleAsync was called with RoleId: {RoleId} and verified.");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnRole_WhenRoleExists()
        {
            // Arrange
            var mockRoleDAL = new Mock<IRoleDAL>();
            var mockLogger = new Mock<ILogger<RoleBL>>(); // Create a mock logger for RoleBL
            var roleBL = new RoleBL(mockRoleDAL.Object);
            ulong id = 10; // Use the same id consistently
            var fixture = new Fixture();

            // Create the expected role object
            var expectedRole = fixture.Build<GetRoleDto>()
                .With(r => r.Id, id) // Set the Id to 10
                .With(r => r.Name, "Administrator") // Set the Name to "Administrator"
                .Create();

            // Mock the DAL method to return the expected role
            mockRoleDAL.Setup(dal => dal.GetRoleByIdAsync(id)).ReturnsAsync(expectedRole);

            // Log the arrangement
            mockLogger.Object.LogInformation("Arranging test for GetRoleById with RoleId: {RoleId}", id);

            // Act
            var result = await roleBL.GetRoleByIdAsync(id);

            // Log the result
            mockLogger.Object.LogInformation("Result received from GetRoleById: {@Result}", result);

            // Assert
            var okResult = Assert.IsType<GetRoleDto>(result); // Assuming the method returns GetRoleDto directly
            Assert.Equal(expectedRole.Id, okResult.Id);
            Assert.Equal(expectedRole.Name, okResult.Name);

            // Verify that the DAL method was called once with the expected parameter
            mockRoleDAL.Verify(dal => dal.GetRoleByIdAsync(id), Times.Once);

            // Log verification
            mockLogger.Object.LogInformation("DAL method GetRoleByIdAsync was called with RoleId: {RoleId} and verified.", id);
        }

        //************************************** Negetive Scenario's **********************************************


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

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
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

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetAllRolesAsync(), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            ulong Id = 10;
            var exceptionMessage = "An unexpected error occurred while retrieving the role by ID.";
            var expectedException = new Exception(exceptionMessage);

            // Setup the mock to throw an exception when GetRoleByIdAsync is called
            _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(Id)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetRoleById_ShouldReturnInternalServerError_WhenExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _roleController.GetRoleById(Id));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(Id), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs()
        {
            // Arrange
            ulong Id = 10;
            var exceptionMessage = "An unexpected error occurred while retrieving the role by ID.";
            var expectedException = new DbUpdateException(exceptionMessage);

            // Setup the mock to throw an exception when GetRoleByIdAsync is called
            _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(Id)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetRoleById_ShouldReturnDbUpdateException_WhenDbUpdateExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _roleController.GetRoleById(Id));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(Id), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task GetRoleById_ShouldReturnCommonBadRequestForRoleException_WhenCommonBadRequestForRoleExceptionOccurs()
        {
            // Arrange
            ulong Id = 10;
            var exceptionMessage = "Provided input request parameter is not valid.";
            var expectedException = new CommonBadRequestForRole(exceptionMessage);

            // Setup the mock to throw an exception when GetRoleByIdAsync is called
            _mockRoleBAL.Setup(bal => bal.GetRoleByIdAsync(Id)).ThrowsAsync(expectedException);

            // Log the test start
            Log.Information("Starting test: GetRoleById_ShouldReturnCommonBadRequestForRoleException_WhenCommonBadRequestForRoleExceptionOccurs");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CommonBadRequestForRole>(() => _roleController.GetRoleById(Id));

            // Verify that the exception message matches
            Assert.Equal(exceptionMessage, exception.Message);

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.GetRoleByIdAsync(Id), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            ulong roleId = 10;
            var updateRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnDbUpdate_WhenDbUpdateOccurs()
        {
            // Arrange
            ulong roleId = 10;
            var updateRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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

            // Verify that the BL method was called once
            _mockRoleBAL.Verify(bl => bl.UpdateRoleAsync(roleId, updateRoleDto), Times.Once);

            // Log verification
            Log.Information("Verified that the exception was thrown and BL method was called once");

            // Log success
            Log.Information("Test completed successfully");
        }

        [Fact]
        public async Task UpdateRole_ShouldReturnBadRequestForLinkUsersNotExits_WhenBadRequestForLinkUsersNotExitsOccurs()
        {
            // Arrange
            ulong roleId = 10;
            var updateRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            ulong roleId = 10;
            var updateRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            ulong roleId = 10;
            var updateRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            var addRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            var addRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            var addRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            var addRoleDto = new AddRoleDto
            {
                Name = "Updated Akshay",
                Description = "Updated Description",
                IsLoggedOutType = true,
                LinkUsers = new List<ulong> { 1, 2, 3, 4, 5 }
            };

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
            ulong roleId = 10;

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
            ulong roleId = 10;

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
            ulong roleId = 10;

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