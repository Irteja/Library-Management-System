using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Members.Commands.UpdateMember;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Application.Members.Commands.UpdateMember
{
    public class UpdateMemberCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly UpdateMemberCommandHandler _handler;

        public UpdateMemberCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new UpdateMemberCommandHandler(_contextMock.Object);
        }

        private void SetupContext(List<Member> members)
        {
            var queryable = new TestAsyncEnumerable<Member>(members.AsQueryable());
            var mockSet = new Mock<DbSet<Member>>();
            
            mockSet.As<IQueryable<Member>>().Setup(m => m.Provider).Returns(queryable.AsQueryable().Provider);
            mockSet.As<IQueryable<Member>>().Setup(m => m.Expression).Returns(queryable.AsQueryable().Expression);
            mockSet.As<IQueryable<Member>>().Setup(m => m.ElementType).Returns(queryable.AsQueryable().ElementType);
            mockSet.As<IQueryable<Member>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.AsQueryable().GetEnumerator());
            mockSet.As<IAsyncEnumerable<Member>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(queryable.GetAsyncEnumerator());

            _contextMock.Setup(c => c.Members).Returns(mockSet.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateMember_WhenValid()
        {
            var memberId = Guid.NewGuid();
            var member = new Member { Id = memberId, FirstName = "Old" };
            SetupContext(new List<Member> { member });

            var command = new UpdateMemberCommand(
                Id: memberId,
                FirstName: "New",
                LastName: "Last",
                Email: "new@test.com",
                Phone: "123",
                MembershipExpiryDate: DateTime.UtcNow,
                IsActive: true
            );

            _contextMock.Setup(c => c.Update(It.IsAny<Member>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _handler.Handle(command, CancellationToken.None);

            member.FirstName.Should().Be("New");
            _contextMock.Verify(c => c.Update(member), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenMemberDoesNotExist()
        {
            SetupContext(new List<Member>());

            var command = new UpdateMemberCommand(
                Id: Guid.NewGuid(),
                FirstName: "New",
                LastName: "Last",
                Email: "new@test.com",
                Phone: "123",
                MembershipExpiryDate: DateTime.UtcNow,
                IsActive: true
            );

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
