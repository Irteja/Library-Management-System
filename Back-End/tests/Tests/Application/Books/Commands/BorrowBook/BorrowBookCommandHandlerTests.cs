using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands.BorrowBook;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.BorrowBook
{
    public class BorrowBookCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IMemberAccessService> _memberAccessMock;
        private readonly Mock<ICurrentUserService> _currentUserMock;
        private readonly BorrowBookCommandHandler _handler;

        public BorrowBookCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _memberAccessMock = new Mock<IMemberAccessService>();
            _currentUserMock = new Mock<ICurrentUserService>();
            _handler = new BorrowBookCommandHandler(_contextMock.Object, _memberAccessMock.Object, _currentUserMock.Object);
        }

        private void SetupContext(List<Book> books, List<Member> members, List<Loan> loans, List<Librarian> librarians = null)
        {
            var bookSet = new Mock<DbSet<Book>>();
            var bookQ = new TestAsyncEnumerable<Book>(books.AsQueryable());
            bookSet.As<IQueryable<Book>>().Setup(m => m.Provider).Returns(bookQ.AsQueryable().Provider);
            bookSet.As<IQueryable<Book>>().Setup(m => m.Expression).Returns(bookQ.AsQueryable().Expression);
            bookSet.As<IQueryable<Book>>().Setup(m => m.ElementType).Returns(bookQ.AsQueryable().ElementType);
            bookSet.As<IQueryable<Book>>().Setup(m => m.GetEnumerator()).Returns(() => bookQ.AsQueryable().GetEnumerator());
            bookSet.As<IAsyncEnumerable<Book>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(bookQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Books).Returns(bookSet.Object);

            var memberSet = new Mock<DbSet<Member>>();
            var memberQ = new TestAsyncEnumerable<Member>(members.AsQueryable());
            memberSet.As<IQueryable<Member>>().Setup(m => m.Provider).Returns(memberQ.AsQueryable().Provider);
            memberSet.As<IQueryable<Member>>().Setup(m => m.Expression).Returns(memberQ.AsQueryable().Expression);
            memberSet.As<IQueryable<Member>>().Setup(m => m.ElementType).Returns(memberQ.AsQueryable().ElementType);
            memberSet.As<IQueryable<Member>>().Setup(m => m.GetEnumerator()).Returns(() => memberQ.AsQueryable().GetEnumerator());
            memberSet.As<IAsyncEnumerable<Member>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(memberQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Members).Returns(memberSet.Object);

            var loanSet = new Mock<DbSet<Loan>>();
            var loanQ = new TestAsyncEnumerable<Loan>(loans.AsQueryable());
            loanSet.As<IQueryable<Loan>>().Setup(m => m.Provider).Returns(loanQ.AsQueryable().Provider);
            loanSet.As<IQueryable<Loan>>().Setup(m => m.Expression).Returns(loanQ.AsQueryable().Expression);
            loanSet.As<IQueryable<Loan>>().Setup(m => m.ElementType).Returns(loanQ.AsQueryable().ElementType);
            loanSet.As<IQueryable<Loan>>().Setup(m => m.GetEnumerator()).Returns(() => loanQ.AsQueryable().GetEnumerator());
            loanSet.As<IAsyncEnumerable<Loan>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(loanQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Loans).Returns(loanSet.Object);

            if (librarians != null)
            {
                var libSet = new Mock<DbSet<Librarian>>();
                var libQ = new TestAsyncEnumerable<Librarian>(librarians.AsQueryable());
                libSet.As<IQueryable<Librarian>>().Setup(m => m.Provider).Returns(libQ.AsQueryable().Provider);
                libSet.As<IQueryable<Librarian>>().Setup(m => m.Expression).Returns(libQ.AsQueryable().Expression);
                libSet.As<IQueryable<Librarian>>().Setup(m => m.ElementType).Returns(libQ.AsQueryable().ElementType);
                libSet.As<IQueryable<Librarian>>().Setup(m => m.GetEnumerator()).Returns(() => libQ.AsQueryable().GetEnumerator());
                libSet.As<IAsyncEnumerable<Librarian>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(libQ.GetAsyncEnumerator());
                _contextMock.Setup(c => c.Librarians).Returns(libSet.Object);
            }
        }

        [Fact]
        public async Task Handle_ShouldBorrowBook_WhenValid()
        {
            var bookId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            
            var book = new Book { Id = bookId, BranchId = branchId, AvailableCopies = 2 };
            var member = new Member { Id = memberId, IsActive = true };
            
            SetupContext(new List<Book> { book }, new List<Member> { member }, new List<Loan>());
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
            _currentUserMock.Setup(c => c.Role).Returns("Member");
            
            _contextMock.Setup(c => c.Add(It.IsAny<Loan>()));
            _contextMock.Setup(c => c.Update(It.IsAny<Book>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new BorrowBookCommand(bookId, memberId, branchId, null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            book.AvailableCopies.Should().Be(1);
            _contextMock.Verify(c => c.Add(It.IsAny<Loan>()), Times.Once);
            _contextMock.Verify(c => c.Update(book), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenBookNotAvailable()
        {
            var bookId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            
            var book = new Book { Id = bookId, BranchId = branchId, AvailableCopies = 0 };
            var member = new Member { Id = memberId, IsActive = true };
            
            SetupContext(new List<Book> { book }, new List<Member> { member }, new List<Loan>());
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
            _currentUserMock.Setup(c => c.Role).Returns("Member");
            
            var command = new BorrowBookCommand(bookId, memberId, branchId, null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenMaxLoansReached()
        {
            var bookId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            
            var book = new Book { Id = bookId, BranchId = branchId, AvailableCopies = 2 };
            var member = new Member { Id = memberId, IsActive = true };
            
            var loans = Enumerable.Range(0, 5).Select(_ => new Loan { MemberId = memberId, Status = LoanStatus.Active }).ToList();
            
            SetupContext(new List<Book> { book }, new List<Member> { member }, loans);
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
            _currentUserMock.Setup(c => c.Role).Returns("Member");
            
            var command = new BorrowBookCommand(bookId, memberId, branchId, null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
            ex.Message.Should().Contain("maximum borrowing limit");
        }
    }
}
