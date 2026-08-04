using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands.ReturnBook;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.ReturnBook
{
    public class ReturnBookCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly ReturnBookCommandHandler _handler;

        public ReturnBookCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new ReturnBookCommandHandler(_contextMock.Object);
        }

        private void SetupContext(List<Loan> loans, List<Reservation> reservations)
        {
            var loanSet = new Mock<DbSet<Loan>>();
            var loanQ = new TestAsyncEnumerable<Loan>(loans.AsQueryable());
            loanSet.As<IQueryable<Loan>>().Setup(m => m.Provider).Returns(loanQ.AsQueryable().Provider);
            loanSet.As<IQueryable<Loan>>().Setup(m => m.Expression).Returns(loanQ.AsQueryable().Expression);
            loanSet.As<IQueryable<Loan>>().Setup(m => m.ElementType).Returns(loanQ.AsQueryable().ElementType);
            loanSet.As<IQueryable<Loan>>().Setup(m => m.GetEnumerator()).Returns(() => loanQ.AsQueryable().GetEnumerator());
            loanSet.As<IAsyncEnumerable<Loan>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(loanQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Loans).Returns(loanSet.Object);

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
        public async Task Handle_ShouldReturnBookAndFulfillReservation_WhenReservationExists()
        {
            var loanId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, AvailableCopies = 0 };
            var loan = new Loan { Id = loanId, BookId = bookId, Book = book, Status = LoanStatus.Active };
            var reservation = new Reservation { BookId = bookId, Status = ReservationStatus.Pending, QueuePosition = 1 };
            
            SetupContext(new List<Loan> { loan }, new List<Reservation> { reservation });
            
            _contextMock.Setup(c => c.Update(It.IsAny<Loan>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new ReturnBookCommand(loanId);

            await _handler.Handle(command, CancellationToken.None);

            loan.Status.Should().Be(LoanStatus.Returned);
            loan.ReturnDate.Should().NotBeNull();
            book.AvailableCopies.Should().Be(1);
            reservation.Status.Should().Be(ReservationStatus.Fulfilled);
            
            _contextMock.Verify(c => c.Update(loan), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenAlreadyReturned()
        {
            var loanId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var book = new Book { Id = bookId, AvailableCopies = 0 };
            var loan = new Loan { Id = loanId, BookId = bookId, Book = book, Status = LoanStatus.Returned };
            
            SetupContext(new List<Loan> { loan }, new List<Reservation>());
            
            var command = new ReturnBookCommand(loanId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
