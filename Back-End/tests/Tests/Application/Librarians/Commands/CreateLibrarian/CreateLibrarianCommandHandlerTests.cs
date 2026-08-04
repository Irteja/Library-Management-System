using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LibraryManagementSystem.Application.Librarians.Commands.CreateLibrarian;
using LibraryManagementSystem.Application.Common.Exceptions;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Tests.Common;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using ValidationException = LibraryManagementSystem.Application.Common.Exceptions.ValidationException;

namespace LibraryManagementSystem.Tests.Application.Librarians.Commands.CreateLibrarian
{
    public class CreateLibrarianCommandHandlerTests
    {
        private readonly Mock<IApplicationDbContext> _contextMock;
        private readonly CreateLibrarianCommandHandler _handler;

        public CreateLibrarianCommandHandlerTests()
        {
            _contextMock = new Mock<IApplicationDbContext>();
            _handler = new CreateLibrarianCommandHandler(_contextMock.Object);
        }

        private void SetupContext(List<User> users, List<Branch> branches)
        {
            var userQ = new TestAsyncEnumerable<User>(users.AsQueryable());
            var userMock = new Mock<DbSet<User>>();
            userMock.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userQ.AsQueryable().Provider);
            userMock.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userQ.AsQueryable().Expression);
            userMock.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userQ.AsQueryable().ElementType);
            userMock.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => userQ.AsQueryable().GetEnumerator());
            userMock.As<IAsyncEnumerable<User>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(userQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Users).Returns(userMock.Object);

            var branchQ = new TestAsyncEnumerable<Branch>(branches.AsQueryable());
            var branchMock = new Mock<DbSet<Branch>>();
            branchMock.As<IQueryable<Branch>>().Setup(m => m.Provider).Returns(branchQ.AsQueryable().Provider);
            branchMock.As<IQueryable<Branch>>().Setup(m => m.Expression).Returns(branchQ.AsQueryable().Expression);
            branchMock.As<IQueryable<Branch>>().Setup(m => m.ElementType).Returns(branchQ.AsQueryable().ElementType);
            branchMock.As<IQueryable<Branch>>().Setup(m => m.GetEnumerator()).Returns(() => branchQ.AsQueryable().GetEnumerator());
            branchMock.As<IAsyncEnumerable<Branch>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(branchQ.GetAsyncEnumerator());
            _contextMock.Setup(c => c.Branches).Returns(branchMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateLibrarian_WhenValid()
        {
            var branchId = Guid.NewGuid();
            SetupContext(new List<User>(), new List<Branch> { new Branch { Id = branchId } });

            var command = new CreateLibrarianCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                Phone = "123456789",
                Username = "johndoe",
                Password = "password",
                BranchId = branchId
            };

            _contextMock.Setup(c => c.Add(It.IsAny<User>()));
            _contextMock.Setup(c => c.Add(It.IsAny<Librarian>()));
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeEmpty();
            _contextMock.Verify(c => c.Add(It.IsAny<User>()), Times.Once);
            _contextMock.Verify(c => c.Add(It.IsAny<Librarian>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenUsernameExists()
        {
            var branchId = Guid.NewGuid();
            SetupContext(new List<User> { new User { Username = "johndoe" } }, new List<Branch> { new Branch { Id = branchId } });

            var command = new CreateLibrarianCommand
            {
                Username = "johndoe"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            ex.Errors.Should().ContainKey("Username");
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenEmailExists()
        {
            var branchId = Guid.NewGuid();
            SetupContext(new List<User> { new User { Email = "john@test.com" } }, new List<Branch> { new Branch { Id = branchId } });

            var command = new CreateLibrarianCommand
            {
                Username = "newuser",
                Email = "john@test.com"
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            ex.Errors.Should().ContainKey("Email");
        }
        
        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenBranchDoesNotExist()
        {
            SetupContext(new List<User>(), new List<Branch>());

            var command = new CreateLibrarianCommand
            {
                Username = "newuser",
                Email = "new@test.com",
                BranchId = Guid.NewGuid()
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(command, CancellationToken.None));
            ex.Errors.Should().ContainKey("BranchId");
        }
    }
}
