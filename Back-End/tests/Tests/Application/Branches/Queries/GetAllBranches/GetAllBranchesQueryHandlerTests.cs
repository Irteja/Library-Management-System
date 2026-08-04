using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Branches.Queries.GetAllBranches;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Branches.Queries.GetAllBranches
{
    public class GetAllBranchesQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetAllBranchesQueryHandler _handler;

        public GetAllBranchesQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetAllBranchesQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnAllBranches_WhenSearchTermIsNull()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch { Id = Guid.NewGuid(), Name = "Branch 1", Address = "Address 1", IsActive = true },
                new Branch { Id = Guid.NewGuid(), Name = "Branch 2", Address = "Address 2", IsActive = true }
            };
            SetupContextWithBranches(branches);
            var query = new GetAllBranchesQuery(SearchTerm: null, PageNumber: 1, PageSize: 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ShouldReturnFilteredBranches_WhenSearchTermIsProvided()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch { Id = Guid.NewGuid(), Name = "Central Branch", Address = "123 Main St", IsActive = true },
                new Branch { Id = Guid.NewGuid(), Name = "North Branch", Address = "456 North St", IsActive = true }
            };
            SetupContextWithBranches(branches);
            var query = new GetAllBranchesQuery(SearchTerm: "Central", PageNumber: 1, PageSize: 10);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items[0].Name.Should().Be("Central Branch");
        }
    }
}
