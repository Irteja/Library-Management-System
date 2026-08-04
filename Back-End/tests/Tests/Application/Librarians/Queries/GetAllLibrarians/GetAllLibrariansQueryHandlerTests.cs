using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Librarians.Queries.GetAllLibrarians;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Librarians.Queries.GetAllLibrarians
{
    public class GetAllLibrariansQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetAllLibrariansQueryHandler _handler;

        public GetAllLibrariansQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetAllLibrariansQueryHandler(_contextMock.Object);
        }

        private void SetupContext(List<Librarian> librarians)
        {
            var queryable = new TestAsyncEnumerable<Librarian>(librarians.AsQueryable());
            var mockSet = new Mock<DbSet<Librarian>>();
            
            mockSet.As<IQueryable<Librarian>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Librarian>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Librarian>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Librarian>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Librarian>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Librarians).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllLibrarians_WhenNoSearchTermIsProvided()
        {
            var branch = new Branch { Id = Guid.NewGuid(), Name = "Main Branch" };
            var user1 = new User { Id = Guid.NewGuid(), Username = "lib1" };
            var user2 = new User { Id = Guid.NewGuid(), Username = "lib2" };
            
            var librarians = new List<Librarian>
            {
                new Librarian { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", Branch = branch, User = user1 },
                new Librarian { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", Branch = branch, User = user2 }
            };
            
            SetupContext(librarians);

            var query = new GetAllLibrariansQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredLibrarians_WhenSearchTermIsProvided()
        {
            var branch = new Branch { Id = Guid.NewGuid(), Name = "Main Branch" };
            var user1 = new User { Id = Guid.NewGuid(), Username = "lib1" };
            var user2 = new User { Id = Guid.NewGuid(), Username = "lib2" };
            
            var librarians = new List<Librarian>
            {
                new Librarian { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", Branch = branch, User = user1 },
                new Librarian { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", Branch = branch, User = user2 }
            };
            
            SetupContext(librarians);

            var query = new GetAllLibrariansQuery(SearchTerm: "Alice", PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].FirstName.Should().Be("Alice");
        }
    }
}
