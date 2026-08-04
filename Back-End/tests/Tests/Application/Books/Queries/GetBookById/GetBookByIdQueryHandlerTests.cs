using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Queries.GetBookById;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Books.Queries.GetBookById
{
    public class GetBookByIdQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetBookByIdQueryHandler _handler;

        public GetBookByIdQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetBookByIdQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnBook_WhenBookExists()
        {
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, Title = "Book 1" };
            SetupContextWithBooks(new List<Book> { book });
            
            var query = new GetBookByIdQuery(bookId);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(bookId);
            result.Title.Should().Be("Book 1");
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenBookDoesNotExist()
        {
            SetupContextWithBooks(new List<Book>());
            var query = new GetBookByIdQuery(Guid.NewGuid());

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
