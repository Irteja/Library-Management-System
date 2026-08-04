using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Branches.Commands.DeleteBranch;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Branches.Commands.DeleteBranch
{
    public class DeleteBranchCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly DeleteBranchCommandHandler _handler;

        public DeleteBranchCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new DeleteBranchCommandHandler(_contextMock.Object);
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
        public async Task Handle_ShouldDeleteBranch_WhenBranchExists()
        {
            // Arrange
            var branchId = Guid.NewGuid();
            var branches = new List<Branch> { new Branch { Id = branchId } };
            SetupContextWithBranches(branches);
            
            _contextMock.Setup(c => c.Remove(It.IsAny<Branch>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new DeleteBranchCommand(branchId);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _contextMock.Verify(c => c.Remove(It.Is<Branch>(b => b.Id == branchId)), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenBranchDoesNotExist()
        {
            // Arrange
            SetupContextWithBranches(new List<Branch>());
            var command = new DeleteBranchCommand(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
            _contextMock.Verify(c => c.Remove(It.IsAny<Branch>()), Times.Never);
        }
    }
}
