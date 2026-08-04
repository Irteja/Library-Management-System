using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Reservations.Commands.CancelReservation;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Reservations.Commands.CancelReservation
{
    public class CancelReservationCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly Mock<IMemberAccessService> _memberAccessMock;
        private readonly CancelReservationCommandHandler _handler;

        public CancelReservationCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _memberAccessMock = new Mock<IMemberAccessService>();
            _handler = new CancelReservationCommandHandler(_contextMock.Object, _memberAccessMock.Object);
        }

        private void SetupContext(List<Reservation> reservations)
        {
            var queryable = new TestAsyncEnumerable<Reservation>(reservations.AsQueryable());
            var mockSet = new Mock<DbSet<Reservation>>();
            
            mockSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Reservation>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Reservations).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldCancelReservation_WhenValid()
        {
            var resId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var reservation = new Reservation { Id = resId, Status = ReservationStatus.Pending, MemberId = memberId };
            
            SetupContext(new List<Reservation> { reservation });
            
            _memberAccessMock.Setup(m => m.GetCurrentMemberIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(memberId);
                
            _contextMock.Setup(c => c.Update(It.IsAny<Reservation>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new CancelReservationCommand(resId);
            await _handler.Handle(command, CancellationToken.None);

            reservation.Status.Should().Be(ReservationStatus.Cancelled);
            _contextMock.Verify(c => c.Update(reservation), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowInvalidOperation_WhenNotPending()
        {
            var resId = Guid.NewGuid();
            var reservation = new Reservation { Id = resId, Status = ReservationStatus.Fulfilled };
            
            SetupContext(new List<Reservation> { reservation });

            var command = new CancelReservationCommand(resId);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowForbidden_WhenWrongMember()
        {
            var resId = Guid.NewGuid();
            var reservation = new Reservation { Id = resId, Status = ReservationStatus.Pending, MemberId = Guid.NewGuid() };
            
            SetupContext(new List<Reservation> { reservation });
            
            _memberAccessMock.Setup(m => m.GetCurrentMemberIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());

            var command = new CancelReservationCommand(resId);
            await Assert.ThrowsAsync<ForbiddenAccessException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
