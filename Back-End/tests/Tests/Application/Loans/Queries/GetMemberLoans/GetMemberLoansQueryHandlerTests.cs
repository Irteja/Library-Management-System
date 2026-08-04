using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Loans.Queries.GetMemberLoans;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Loans.Queries.GetMemberLoans
{
    public class GetMemberLoansQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IMemberAccessService> _memberAccessMock;
        private readonly GetMemberLoansQueryHandler _handler;

        public GetMemberLoansQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _memberAccessMock = new Mock<IMemberAccessService>();
            _handler = new GetMemberLoansQueryHandler(_contextMock.Object, _memberAccessMock.Object);
        }

        private void SetupContext(List<Loan> loans)
        {
            var queryable = new TestAsyncEnumerable<Loan>(loans.AsQueryable());
            var mockSet = new Mock<DbSet<Loan>>();
            
            mockSet.As<IQueryable<Loan>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Loan>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Loan>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Loan>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Loan>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Loans).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnMemberLoans_WhenValid()
        {
            var requestedMemberId = Guid.NewGuid();
            var authorizedMemberId = Guid.NewGuid();
            
            var member = new Member { FirstName = "John", LastName = "Doe" };
            var book = new Book { Title = "Test Book", Author = "Author" };
            
            var loans = new List<Loan>
            {
                new Loan { Id = Guid.NewGuid(), MemberId = authorizedMemberId, Member = member, Book = book, LoanDate = DateTime.UtcNow, Status = LoanStatus.Active },
                new Loan { Id = Guid.NewGuid(), MemberId = Guid.NewGuid(), Member = member, Book = book, LoanDate = DateTime.UtcNow, Status = LoanStatus.Active }
            };
            
            SetupContext(loans);
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(requestedMemberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(authorizedMemberId);

            var query = new GetMemberLoansQuery(requestedMemberId);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].MemberId.Should().Be(authorizedMemberId);
        }
    }
}
