using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Loans.Queries.GetActiveLoans;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Loans.Queries.GetActiveLoans
{
    public class GetActiveLoansQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetActiveLoansQueryHandler _handler;

        public GetActiveLoansQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetActiveLoansQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnOnlyActiveLoans_WhenNoSearchTermIsProvided()
        {
            var member = new Member { FirstName = "John", LastName = "Doe" };
            var book = new Book { Title = "Test Book", Author = "Author" };
            
            var loans = new List<Loan>
            {
                new Loan { Id = Guid.NewGuid(), Status = LoanStatus.Active, Member = member, Book = book, LoanDate = DateTime.UtcNow },
                new Loan { Id = Guid.NewGuid(), Status = LoanStatus.Returned, Member = member, Book = book, LoanDate = DateTime.UtcNow }
            };
            
            SetupContext(loans);

            var query = new GetActiveLoansQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Status.Should().Be(LoanStatus.Active.ToString());
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredActiveLoans_WhenSearchTermIsProvided()
        {
            var member1 = new Member { FirstName = "John", LastName = "Doe" };
            var member2 = new Member { FirstName = "Alice", LastName = "Smith" };
            var book = new Book { Title = "Test Book", Author = "Author" };
            
            var loans = new List<Loan>
            {
                new Loan { Id = Guid.NewGuid(), Status = LoanStatus.Active, Member = member1, Book = book, LoanDate = DateTime.UtcNow },
                new Loan { Id = Guid.NewGuid(), Status = LoanStatus.Active, Member = member2, Book = book, LoanDate = DateTime.UtcNow }
            };
            
            SetupContext(loans);

            var query = new GetActiveLoansQuery(SearchTerm: "Alice", PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].MemberName.Should().Contain("Alice");
        }
    }
}
