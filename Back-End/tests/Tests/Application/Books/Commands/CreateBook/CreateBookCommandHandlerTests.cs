using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.CreateBook
{
    public class CreateBookCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly CreateBookCommandHandler _handler;

        public CreateBookCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new CreateBookCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateBookAndReturnId()
        {
            // Arrange
            var command = new CreateBookCommand(
                ISBN: "1234567890",
                Title: "New Book",
                Author: "Author",
                Publisher: "Publisher",
                PublicationYear: 2023,
                Category: "Fiction",
                TotalCopies: 5,
                BranchId: Guid.NewGuid());

            Book captured = null;
            _contextMock.Setup(c => c.Add(It.IsAny<object>()))
                .Callback<object>(b => captured = b as Book);
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            captured.Should().NotBeNull();
            captured.Title.Should().Be(command.Title);
            captured.AvailableCopies.Should().Be(command.TotalCopies);
            _contextMock.Verify(c => c.Add(It.IsAny<Book>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
