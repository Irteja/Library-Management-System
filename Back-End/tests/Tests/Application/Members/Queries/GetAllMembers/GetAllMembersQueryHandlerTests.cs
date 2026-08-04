using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Members.Queries.GetAllMembers;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Members.Queries.GetAllMembers
{
    public class GetAllMembersQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetAllMembersQueryHandler _handler;

        public GetAllMembersQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetAllMembersQueryHandler(_contextMock.Object);
        }

        private void SetupContext(List<Member> members)
        {
            var queryable = new TestAsyncEnumerable<Member>(members.AsQueryable());
            var mockSet = new Mock<DbSet<Member>>();
            
            mockSet.As<IQueryable<Member>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Member>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Member>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Member>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Member>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Members).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllMembers_WhenNoSearchTerm()
        {
            var members = new List<Member>
            {
                new Member { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" },
                new Member { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", Email = "bob@test.com" }
            };
            
            SetupContext(members);

            var query = new GetAllMembersQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredMembers_WhenSearchTermProvided()
        {
            var members = new List<Member>
            {
                new Member { Id = Guid.NewGuid(), FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" },
                new Member { Id = Guid.NewGuid(), FirstName = "Bob", LastName = "Jones", Email = "bob@test.com" }
            };
            
            SetupContext(members);

            var query = new GetAllMembersQuery(SearchTerm: "Alice", PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].FirstName.Should().Be("Alice");
        }
    }
}
