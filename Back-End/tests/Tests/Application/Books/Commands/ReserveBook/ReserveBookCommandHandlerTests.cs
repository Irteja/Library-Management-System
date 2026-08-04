using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands.ReserveBook;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.ReserveBook
{
    public class ReserveBookCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IMemberAccessService> _memberAccessMock;
        private readonly ReserveBookCommandHandler _handler;

        public ReserveBookCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _memberAccessMock = new Mock<IMemberAccessService>();
            _handler = new ReserveBookCommandHandler(_contextMock.Object, _memberAccessMock.Object);
        }

        private void SetupContext(List<Book> books, List<Member> members, List<Reservation> reservations)
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

            var resSet = new Mock<DbSet<Reservation>>();
            var resQ = new TestAsyncEnumerable<Reservation>(reservations.AsQueryable());
            resSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(resQ.AsQueryable().Provider);
            resSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(resQ.AsQueryable().Expression);
            resSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(resQ.AsQueryable().ElementType);
            resSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(() => resQ.AsQueryable().GetEnumerator());
            resSet.As<IAsyncEnumerable<Reservation>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(resQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Reservations).Returns(resSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldReserveBook_WhenValid()
        {
            var bookId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            
            var book = new Book { Id = bookId, BranchId = branchId, AvailableCopies = 0 };
            var member = new Member { Id = memberId, IsActive = true };
            
            SetupContext(new List<Book> { book }, new List<Member> { member }, new List<Reservation>());
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
            
            _contextMock.Setup(c => c.Add(It.IsAny<Reservation>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new ReserveBookCommand(bookId, memberId, branchId);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _contextMock.Verify(c => c.Add(It.IsAny<Reservation>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenBookHasAvailableCopies()
        {
            var bookId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            
            var book = new Book { Id = bookId, BranchId = branchId, AvailableCopies = 1 };
            var member = new Member { Id = memberId, IsActive = true };
            
            SetupContext(new List<Book> { book }, new List<Member> { member }, new List<Reservation>());
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
            
            var command = new ReserveBookCommand(bookId, memberId, branchId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenAlreadyReserved()
        {
            var bookId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var branchId = Guid.NewGuid();
            
            var book = new Book { Id = bookId, BranchId = branchId, AvailableCopies = 0 };
            var member = new Member { Id = memberId, IsActive = true };
            var res = new Reservation { BookId = bookId, MemberId = memberId, Status = ReservationStatus.Pending };
            
            SetupContext(new List<Book> { book }, new List<Member> { member }, new List<Reservation> { res });
            
            _memberAccessMock.Setup(m => m.GetAccessibleMemberIdAsync(memberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
            
            var command = new ReserveBookCommand(bookId, memberId, branchId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
