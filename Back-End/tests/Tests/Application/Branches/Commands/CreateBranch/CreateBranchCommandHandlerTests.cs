using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Branches.Commands.CreateBranch;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests.Application.Branches.Commands.CreateBranch
{
    public class CreateBranchCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly CreateBranchCommandHandler _handler;

        public CreateBranchCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new CreateBranchCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateBranchAndReturnId()
        {
            // Arrange
            var command = new CreateBranchCommand(
                Name: "Main Branch",
                Address: "123 Library St",
                Phone: "555-0100",
                Email: "main@library.com");

            Branch captured = null;
            _contextMock.Setup(c => c.Add(It.IsAny<Branch>()))
                .Callback<Branch>(b => captured = b);
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            captured.Should().NotBeNull();
            captured.Name.Should().Be(command.Name);
            captured.Address.Should().Be(command.Address);
            captured.Phone.Should().Be(command.Phone);
            captured.Email.Should().Be(command.Email);
            captured.IsActive.Should().BeTrue();
            _contextMock.Verify(c => c.Add(It.IsAny<Branch>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
