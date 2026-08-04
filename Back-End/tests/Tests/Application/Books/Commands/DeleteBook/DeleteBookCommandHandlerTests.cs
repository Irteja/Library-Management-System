using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands.DeleteBook;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.DeleteBook
{
    public class DeleteBookCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly DeleteBookCommandHandler _handler;

        public DeleteBookCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new DeleteBookCommandHandler(_contextMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingBook_RemovesAndSaves()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId };
            var command = new DeleteBookCommand(bookId);

            _contextMock.SetupGet(c => c.Books)
                .Returns(new TestAsyncEnumerable<Book>(new List<Book> { book }));

            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _contextMock.Verify(c => c.Remove(It.Is<Book>(b => b.Id == bookId)), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistingBook_ThrowsNotFoundException()
        {
            // Arrange
            var command = new DeleteBookCommand(Guid.NewGuid());

            _contextMock.SetupGet(c => c.Books)
                .Returns(new TestAsyncEnumerable<Book>(new List<Book>()));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
