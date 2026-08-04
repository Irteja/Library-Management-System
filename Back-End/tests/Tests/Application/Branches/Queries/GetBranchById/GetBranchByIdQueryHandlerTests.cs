using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Branches.Queries.GetBranchById;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Branches.Queries.GetBranchById
{
    public class GetBranchByIdQueryHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly GetBranchByIdQueryHandler _handler;

        public GetBranchByIdQueryHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new GetBranchByIdQueryHandler(_contextMock.Object);
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
        public async Task Handle_ShouldReturnBranch_WhenBranchExists()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var branch = new Branch { Id = branchId, Name = "Branch 1", IsActive = true };
            SetupContextWithBranches(new List<Branch> { branch });
            var query = new GetBranchByIdQuery(branchId);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(branchId);
            result.Name.Should().Be("Branch 1");
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenBranchDoesNotExist()
        {
            // Arrange
            SetupContextWithBranches(new List<Branch>());
            var query = new GetBranchByIdQuery(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(query, CancellationToken.None));
        }
    }
}
