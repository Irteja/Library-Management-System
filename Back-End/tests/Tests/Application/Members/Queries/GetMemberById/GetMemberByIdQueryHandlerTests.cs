using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Members.Queries.GetMemberById;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Members.Queries.GetMemberById
{
    public class GetMemberByIdQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetMemberByIdQueryHandler _handler;

        public GetMemberByIdQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetMemberByIdQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnMember_WhenValid()
        {
            var memberId = Guid.NewGuid();
            var members = new List<Member>
            {
                new Member { Id = memberId, FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" }
            };
            
            SetupContext(members);

            var query = new GetMemberByIdQuery(memberId);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(memberId);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
        {
            SetupContext(new List<Member>());

            var query = new GetMemberByIdQuery(Guid.NewGuid());
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
