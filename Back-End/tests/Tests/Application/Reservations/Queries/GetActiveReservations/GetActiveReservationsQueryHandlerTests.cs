using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Reservations.Queries.GetActiveReservations;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Reservations.Queries.GetActiveReservations
{
    public class GetActiveReservationsQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetActiveReservationsQueryHandler _handler;

        public GetActiveReservationsQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetActiveReservationsQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnActiveReservations_WhenValid()
        {
            var book = new Book { Title = "Test Book", Author = "Author" };
            var member = new Member { FirstName = "John", LastName = "Doe" };
            var reservations = new List<Reservation>
            {
                new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Pending, Book = book, Member = member, ReservedAt = DateTime.UtcNow },
                new Reservation { Id = Guid.NewGuid(), Status = ReservationStatus.Fulfilled, Book = book, Member = member, ReservedAt = DateTime.UtcNow }
            };
            
            SetupContext(reservations);

            var query = new GetActiveReservationsQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Status.Should().Be(ReservationStatus.Pending.ToString());
        }
    }
}
