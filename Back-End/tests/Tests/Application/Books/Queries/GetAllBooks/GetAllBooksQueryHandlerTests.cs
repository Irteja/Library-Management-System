using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Queries.GetAllBooks;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Books.Queries.GetAllBooks
{
    public class GetAllBooksQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly GetAllBooksQueryHandler _handler;

        public GetAllBooksQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _handler = new GetAllBooksQueryHandler(_contextMock.Object, _currentUserMock.Object);
        }

        private void SetupContext(List<Book> books, List<Librarian> librarians = null)
        {
            var bookQueryable = new TestAsyncEnumerable<Book>(books.AsQueryable());
            var bookMockSet = new Mock<DbSet<Book>>();
            
            bookMockSet.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(bookQueryable.AsQueryable().Provider);
            bookMockSet.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(bookQueryable.AsQueryable().Expression);
            bookMockSet.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(bookQueryable.AsQueryable().ElementType);
            bookMockSet.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => bookQueryable.AsQueryable().GetEnumerator());
            bookMockSet.As<IAsyncEnumerable<Book>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(bookQueryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Books).Returns(bookMockSet.Object);

            if (librarians != null)
            {
                var librarianQueryable = new TestAsyncEnumerable<Librarian>(librarians.AsQueryable());
                var librarianMockSet = new Mock<DbSet<Librarian>>();
                
                librarianMockSet.As<IQueryable<Librarian>>().Setup(m => m.Provider).Returns(librarianQueryable.AsQueryable().Provider);
                librarianMockSet.As<IQueryable<Librarian>>().Setup(m => m.Expression).Returns(librarianQueryable.AsQueryable().Expression);
                librarianMockSet.As<IQueryable<Librarian>>().Setup(m => m.ElementType).Returns(librarianQueryable.AsQueryable().ElementType);
                librarianMockSet.As<IQueryable<Librarian>>().Setup(m => m.GetEnumerator()).Returns(() => librarianQueryable.AsQueryable().GetEnumerator());
                librarianMockSet.As<IAsyncEnumerable<Librarian>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(librarianQueryable.GetAsyncEnumerator());

                _contextMock.Setup(c => c.Librarians).Returns(librarianMockSet.Object);
            }
        }

        [Fact]
        public async Task Handle_ShouldReturnAllBooks_WhenNoSearchTerm_AndUserIsNotLibrarian()
        {
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", BranchId = Guid.NewGuid() },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", BranchId = Guid.NewGuid() }
            };
            SetupContext(books);
            
            _currentUserMock.Setup(c => c.Role).Returns("Member");
            
            var query = new GetAllBooksQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredBooks_WhenSearchTermIsProvided()
        {
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "C# Programming", Author = "John Doe", BranchId = Guid.NewGuid() },
                new Book { Id = Guid.NewGuid(), Title = "Java Basics", Author = "Jane Doe", BranchId = Guid.NewGuid() }
            };
            SetupContext(books);
            
            _currentUserMock.Setup(c => c.Role).Returns("Member");
            
            var query = new GetAllBooksQuery(SearchTerm: "C#", PageNumber: 1, PageSize: 10);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Title.Should().Be("C# Programming");
        }

        [Fact]
        public async Task Handle_ShouldReturnBranchBooks_WhenUserIsLibrarian()
        {
            var branchId = Guid.NewGuid();
            var otherBranchId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var books = new List<Book>
            {
                new Book { Id = Guid.NewGuid(), Title = "Book 1", BranchId = branchId },
                new Book { Id = Guid.NewGuid(), Title = "Book 2", BranchId = otherBranchId }
            };
            var librarians = new List<Librarian>
            {
                new Librarian { UserId = userId, BranchId = branchId }
            };
            
            SetupContext(books, librarians);
            
            _currentUserMock.Setup(c => c.Role).Returns("Librarian");
            _currentUserMock.Setup(c => c.UserId).Returns(userId);
            
            var query = new GetAllBooksQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].BranchId.Should().Be(branchId);
        }
    }
}
