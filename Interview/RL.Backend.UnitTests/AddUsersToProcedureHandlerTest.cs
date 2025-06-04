// filepath: RL.Backend/Commands/Handlers/User/AddUsersToProcedureHandlerTest.cs
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Commands.Handlers.User;
using RL.Data;
using RL.Data.DataModels;
using Xunit;

namespace RL.Backend.Tests.Commands.Handlers.User
{
    public class AddUsersToProcedureHandlerTest
    {
        private readonly Mock<RLContext> _mockContext = new Mock<RLContext>();
        private readonly Mock<IValidator<AddUserToProducer>> _mockValidator;
        private readonly AddUsersToProcedureHandler _handler;

        public AddUsersToProcedureHandlerTest()
        {
            _mockValidator = new Mock<IValidator<AddUserToProducer>>();
            _handler = new AddUsersToProcedureHandler(_mockContext.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentNullException_WhenUserIdIsNull()
        {
            // Arrange
            var request = new AddUserToProducer
            {
                UserId = null,
                ProcedureId = 1
            };

            // Act & Assert
            await Xunit.Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldRemoveExistingAssignments_WhenAssignmentsExist()
        {
            // Arrange
            var existingAssignments = new List<UserProcedureAssignment>
            {
                new UserProcedureAssignment { ProcedureId = 1, UserId = 1 },
                new UserProcedureAssignment { ProcedureId = 1, UserId = 2 }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<UserProcedureAssignment>>();
            mockDbSet.As<IQueryable<UserProcedureAssignment>>().Setup(m => m.Provider).Returns(existingAssignments.Provider);
            mockDbSet.As<IQueryable<UserProcedureAssignment>>().Setup(m => m.Expression).Returns(existingAssignments.Expression);
            mockDbSet.As<IQueryable<UserProcedureAssignment>>().Setup(m => m.ElementType).Returns(existingAssignments.ElementType);
            mockDbSet.As<IQueryable<UserProcedureAssignment>>().Setup(m => m.GetEnumerator()).Returns(existingAssignments.GetEnumerator());

            _mockContext.Setup(c => c.UserProcedureAssignment).Returns(mockDbSet.Object);

            var request = new AddUserToProducer
            {
                UserId = new List<int> { 3, 4 },
                ProcedureId = 1
            };

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            mockDbSet.Verify(m => m.RemoveRange(existingAssignments), Times.Once);
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Handle_ShouldAddNewAssignments_WhenValidRequest()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<UserProcedureAssignment>>();
            _mockContext.Setup(c => c.UserProcedureAssignment).Returns(mockDbSet.Object);

            var request = new AddUserToProducer
            {
                UserId = new List<int> { 3, 4 },
                ProcedureId = 1
            };

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            mockDbSet.Verify(m => m.Add(It.IsAny<UserProcedureAssignment>()), Times.Exactly(2));
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResponse_WhenExceptionOccurs()
        {
            // Arrange
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

            var request = new AddUserToProducer
            {
                UserId = new List<int> { 3, 4 },
                ProcedureId = 1
            };

            // Act
            var response = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Xunit.Assert.False(response.Succeeded);
            Xunit.Assert.NotNull(response.Exception);
        }
    }
}