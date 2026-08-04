using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands.UpdateBook;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

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

        private void SetupContextWithBooks(List<Book> books)
        {
            var queryable = new TestAsyncEnumerable<Book>(books.AsQueryable());
            var mockSet = new Mock<DbSet<Book>>();
            
            mockSet.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Book>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Books).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateBook_WhenBookExists()
        {
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, Title = "Old Title" };
            SetupContextWithBooks(new List<Book> { book });
            
            _contextMock.Setup(c => c.Update(It.IsAny<Book>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new UpdateBookCommand(
                Id: bookId,
                ISBN: "1234567890",
                Title: "New Title",
                Author: "New Author",
                Publisher: "New Publisher",
                PublicationYear: 2024,
                Category: "Fiction",
                TotalCopies: 5,
                AvailableCopies: 5);

            await _handler.Handle(command, CancellationToken.None);

            book.Title.Should().Be(command.Title);
            book.Author.Should().Be(command.Author);
            
            _contextMock.Verify(c => c.Update(It.Is<Book>(b => b.Id == bookId)), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenBookDoesNotExist()
        {
            SetupContextWithBooks(new List<Book>());
            var command = new UpdateBookCommand(
                Id: Guid.NewGuid(),
                ISBN: "1234567890",
                Title: "New Title",
                Author: "New Author",
                Publisher: "New Publisher",
                PublicationYear: 2024,
                Category: "Fiction",
                TotalCopies: 5,
                AvailableCopies: 5);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            _contextMock.Verify(c => c.Update(It.IsAny<Book>()), Times.Never);
        }
    }
}
