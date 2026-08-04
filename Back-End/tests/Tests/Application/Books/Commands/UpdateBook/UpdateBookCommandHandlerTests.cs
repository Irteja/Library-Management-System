using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands;
using LibraryManagementSystem.Application.Books.Commands.UpdateBook;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.UpdateBook
{
    public class UpdateBookCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly UpdateBookCommandHandler _handler;

        public UpdateBookCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new UpdateBookCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUpdate_UpdatesBookAndSaves()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, Title = "Old Title" };
            var command = new UpdateBookCommand(
                Id: bookId,
                ISBN: "1111111111",
                Title: "Updated Title",
                Author: "Author",
                Publisher: "Pub",
                PublicationYear: 2024,
                Category: "Fiction",
                TotalCopies: 5,
                AvailableCopies: 5
            );

            _contextMock.SetupGet(c => c.Books)
                .Returns(new TestAsyncEnumerable<Book>(new List<Book> { book }));

            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _contextMock.Verify(c => c.Update(It.Is<Book>(b => b.Id == bookId && b.Title == "Updated Title")), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            var command = new UpdateBookCommand(
                Id: Guid.NewGuid(),
                ISBN: "1111111111",
                Title: "Updated Title",
                Author: "Author",
                Publisher: "Pub",
                PublicationYear: 2024,
                Category: "Fiction",
                TotalCopies: 5,
                AvailableCopies: 5
            );

            _contextMock.SetupGet(c => c.Books)
                .Returns(new TestAsyncEnumerable<Book>(new List<Book>()));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
