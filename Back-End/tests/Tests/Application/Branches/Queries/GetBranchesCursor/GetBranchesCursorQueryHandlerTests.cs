using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Branches.Queries.GetBranchesCursor;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LibraryManagementSystem.Tests.Application.Branches.Queries.GetBranchesCursor
{
    public class GetBranchesCursorQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetBranchesCursorQueryHandler _handler;

        public GetBranchesCursorQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetBranchesCursorQueryHandler(_contextMock.Object);
        }

        private void SetupContextWithBranches(List<Branch> branches)
        {
            var queryable = new TestAsyncEnumerable<Branch>(branches.AsQueryable());
            var mockSet = new Mock<DbSet<Branch>>();
            
            mockSet.As<IQueryable<Branch>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Branch>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Branch>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Branch>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Branch>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Branches).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnLimitedBranchesAndHasNextPage_WhenNoCursor()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch { Id = Guid.NewGuid(), Name = "B1", CreatedAt = DateTime.UtcNow.AddMinutes(1) },
                new Branch { Id = Guid.NewGuid(), Name = "B2", CreatedAt = DateTime.UtcNow.AddMinutes(2) },
                new Branch { Id = Guid.NewGuid(), Name = "B3", CreatedAt = DateTime.UtcNow.AddMinutes(3) }
            };
            SetupContextWithBranches(branches);
            var query = new GetBranchesCursorQuery(Cursor: null, Limit: 2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items[0].Name.Should().Be("B1");
            result.Items[1].Name.Should().Be("B2");
            result.HasNextPage.Should().BeTrue();
            result.NextCursor.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnRemainingBranches_WhenCursorIsProvided()
        {
            // Arrange
            var time1 = DateTime.UtcNow.AddMinutes(1);
            var id1 = Guid.NewGuid();
            var time2 = DateTime.UtcNow.AddMinutes(2);
            var id2 = Guid.NewGuid();

            var branches = new List<Branch>
            {
                new Branch { Id = id1, Name = "B1", CreatedAt = time1 },
                new Branch { Id = id2, Name = "B2", CreatedAt = time2 }
            };
            SetupContextWithBranches(branches);
            
            var cursorStr = $"{time1.ToString("o")}|{id1}";
            var cursorEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(cursorStr));
            
            var query = new GetBranchesCursorQuery(Cursor: cursorEncoded, Limit: 2);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("B2");
            result.HasNextPage.Should().BeFalse();
            result.NextCursor.Should().NotBeNull(); // Still returns cursor of the last item
        }
    }
}
