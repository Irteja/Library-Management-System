using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Reservations.Queries.GetBookReservationQueue;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Reservations.Queries.GetBookReservationQueue
{
    public class GetBookReservationQueueQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetBookReservationQueueQueryHandler _handler;

        public GetBookReservationQueueQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetBookReservationQueueQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnBookQueue_WhenValid()
        {
            var bookId = Guid.NewGuid();
            var book = new Book { Title = "Test Book", Author = "Author" };
            var member = new Member { FirstName = "John", LastName = "Doe" };
            var reservations = new List<Reservation>
            {
                new Reservation { Id = Guid.NewGuid(), BookId = bookId, Status = ReservationStatus.Pending, QueuePosition = 2, Book = book, Member = member, ReservedAt = DateTime.UtcNow },
                new Reservation { Id = Guid.NewGuid(), BookId = bookId, Status = ReservationStatus.Pending, QueuePosition = 1, Book = book, Member = member, ReservedAt = DateTime.UtcNow },
                new Reservation { Id = Guid.NewGuid(), BookId = Guid.NewGuid(), Status = ReservationStatus.Pending, Book = book, Member = member, ReservedAt = DateTime.UtcNow }
            };
            
            SetupContext(reservations);

            var query = new GetBookReservationQueueQuery(bookId);
            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].QueuePosition.Should().Be(1);
            result[1].QueuePosition.Should().Be(2);
        }
    }
}
