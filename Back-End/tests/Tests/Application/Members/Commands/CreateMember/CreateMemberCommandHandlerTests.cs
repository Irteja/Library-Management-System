using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Members.Commands.CreateMember;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ValidationException = LibraryManagementSystem.Application.Common.Exceptions.ValidationException;

namespace LibraryManagementSystem.Tests.Application.Members.Commands.CreateMember
{
    public class CreateMemberCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly CreateMemberCommandHandler _handler;

        public CreateMemberCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new CreateMemberCommandHandler(_contextMock.Object);
        }

        private void SetupContext(List<User> users)
        {
            var userQ = new TestAsyncEnumerable<User>(users.AsQueryable());
            var userMock = new Mock<DbSet<User>>();
            userMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userQ.AsQueryable().Provider);
            userMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userQ.AsQueryable().Expression);
            userMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userQ.AsQueryable().ElementType);
            userMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => userQ.AsQueryable().GetEnumerator());
            userMock.As<IAsyncEnumerable<User>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(userQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Users).Returns(userMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateMember_WhenValid()
        {
            SetupContext(new List<User>());

            var command = new CreateMemberCommand(
                FirstName: "John",
                LastName: "Doe",
                Email: "john@test.com",
                Phone: "123456789",
                MembershipExpiryDate: DateTime.UtcNow.AddYears(1),
                Username: "johndoe",
                Password: "password"
            );

            _contextMock.Setup(c => c.Add(It.IsAny<User>()));
            _contextMock.Setup(c => c.Add(It.IsAny<Member>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _contextMock.Verify(c => c.Add(It.IsAny<User>()), Times.Once);
            _contextMock.Verify(c => c.Add(It.IsAny<Member>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenUsernameExists()
        {
            SetupContext(new List<User> { new User { Username = "johndoe" } });

            var command = new CreateMemberCommand(
                FirstName: "John",
                LastName: "Doe",
                Email: "john@test.com",
                Phone: "123456789",
                MembershipExpiryDate: DateTime.UtcNow.AddYears(1),
                Username: "johndoe",
                Password: "password"
            );

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            ex.Errors.Should().ContainKey("Username");
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenEmailExists()
        {
            SetupContext(new List<User> { new User { Email = "john@test.com" } });

            var command = new CreateMemberCommand(
                FirstName: "John",
                LastName: "Doe",
                Email: "john@test.com",
                Phone: "123456789",
                MembershipExpiryDate: DateTime.UtcNow.AddYears(1),
                Username: "newuser",
                Password: "password"
            );

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            ex.Errors.Should().ContainKey("Email");
        }
    }
}
