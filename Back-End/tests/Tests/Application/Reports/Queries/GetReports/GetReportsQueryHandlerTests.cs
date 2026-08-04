using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Reports.Queries.GetReports;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Reports.Queries.GetReports
{
    public class GetReportsQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetReportsQueryHandler _handler;

        public GetReportsQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetReportsQueryHandler(_contextMock.Object);
        }

        private void SetupContext(List<Loan> loans, List<Member> members, List<Book> books)
        {
            var loanQ = new TestAsyncEnumerable<Loan>(loans.AsQueryable());
            var loanMock = new Mock<DbSet<Loan>>();
            loanMock.As<IQueryable<Loan>>().Setup(m => m.Provider).Returns(loanQ.AsQueryable().Provider);
            loanMock.As<IQueryable<Loan>>().Setup(m => m.Expression).Returns(loanQ.AsQueryable().Expression);
            loanMock.As<IQueryable<Loan>>().Setup(m => m.ElementType).Returns(loanQ.AsQueryable().ElementType);
            loanMock.As<IQueryable<Loan>>().Setup(m => m.GetEnumerator()).Returns(() => loanQ.AsQueryable().GetEnumerator());
            loanMock.As<IAsyncEnumerable<Loan>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(loanQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Loans).Returns(loanMock.Object);

            var memberQ = new TestAsyncEnumerable<Member>(members.AsQueryable());
            var memberMock = new Mock<DbSet<Member>>();
            memberMock.As<IQueryable<Member>>().Setup(m => m.Provider).Returns(memberQ.AsQueryable().Provider);
            memberMock.As<IQueryable<Member>>().Setup(m => m.Expression).Returns(memberQ.AsQueryable().Expression);
            memberMock.As<IQueryable<Member>>().Setup(m => m.ElementType).Returns(memberQ.AsQueryable().ElementType);
            memberMock.As<IQueryable<Member>>().Setup(m => m.GetEnumerator()).Returns(() => memberQ.AsQueryable().GetEnumerator());
            memberMock.As<IAsyncEnumerable<Member>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(memberQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Members).Returns(memberMock.Object);

            var bookQ = new TestAsyncEnumerable<Book>(books.AsQueryable());
            var bookMock = new Mock<DbSet<Book>>();
            bookMock.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(bookQ.AsQueryable().Provider);
            bookMock.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(bookQ.AsQueryable().Expression);
            bookMock.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(bookQ.AsQueryable().ElementType);
            bookMock.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => bookQ.AsQueryable().GetEnumerator());
            bookMock.As<IAsyncEnumerable<Book>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(bookQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Books).Returns(bookMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnReport_WhenValid()
        {
            var book = new Book { Title = "Book", Author = "Author", ISBN = "123" };
            var loans = new List<Loan>
            {
                new Loan { Status = LoanStatus.Active, DueDate = DateTime.UtcNow.AddDays(1), Book = book },
                new Loan { Status = LoanStatus.Active, DueDate = DateTime.UtcNow.AddDays(-1), Book = book }
            };
            var members = new List<Member> { new Member(), new Member() };
            var books = new List<Book> { book };
            
            SetupContext(loans, members, books);

            var query = new GetReportsQuery();
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.ActiveLoansCount.Should().Be(2);
            result.OverdueLoansCount.Should().Be(1);
            result.TotalMembers.Should().Be(2);
            result.TotalBooks.Should().Be(1);
            result.TopBorrowedBooks.Should().HaveCount(1);
            result.TopBorrowedBooks.First().BorrowCount.Should().Be(2);
        }
    }
}
