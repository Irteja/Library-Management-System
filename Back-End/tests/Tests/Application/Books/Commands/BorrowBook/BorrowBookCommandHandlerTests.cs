using FluentAssertions;
using LibraryManagementSystem.Application.Books.Commands.BorrowBook;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Domain.Enums;
using LibraryManagementSystem.Tests.Common;
using Moq;

namespace LibraryManagementSystem.Tests.Application.Books.Commands.BorrowBook;

public class BorrowBookCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly BorrowBookCommandHandler _handler;

    private readonly List<Book> _books;
    private readonly List<Member> _members;
    private readonly List<Loan> _loans;
    private readonly List<object> _addedEntities;

    public BorrowBookCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _addedEntities = new List<object>();

        var branchId = Guid.NewGuid();

        _members =
        [
            new Member
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                IsActive = true
            }
        ];

        _books =
        [
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                AvailableCopies = 3,
                TotalCopies = 3,
                BranchId = branchId
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Unavailable Book",
                Author = "Test Author",
                ISBN = "0987654321",
                AvailableCopies = 0,
                TotalCopies = 2,
                BranchId = branchId
            }
        ];

        _loans = [];

        _contextMock.SetupGet(m => m.Members).Returns(new TestAsyncEnumerable<Member>(_members));
        _contextMock.SetupGet(m => m.Books).Returns(new TestAsyncEnumerable<Book>(_books));
        _contextMock.SetupGet(m => m.Loans).Returns(new TestAsyncEnumerable<Loan>(_loans));

        _contextMock
            .Setup(m => m.Add(It.IsAny<object>()))
            .Callback<object>(entity =>
            {
                _addedEntities.Add(entity);
                if (entity is Loan loan)
                    _loans.Add(loan);
            });

        _contextMock
            .Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var memberAccessMock = new Mock<LibraryManagementSystem.Application.Common.Services.IMemberAccessService>();
        _handler = new BorrowBookCommandHandler(_contextMock.Object, memberAccessMock.Object);
    }

    [Fact]
    public async Task Handle_WithAvailableCopies_ShouldCreateLoanAndDecrementCopies()
    {
        var book = _books[0];
        var member = _members[0];
        var command = new BorrowBookCommand(book.Id, member.Id, book.BranchId);

        var loanId = await _handler.Handle(command, CancellationToken.None);

        loanId.Should().NotBeEmpty();
        var addedLoan = _addedEntities.OfType<Loan>().Should().ContainSingle().Subject;
        addedLoan.BookId.Should().Be(book.Id);
        addedLoan.MemberId.Should().Be(member.Id);
        addedLoan.Status.Should().Be(LoanStatus.Active);
        book.AvailableCopies.Should().Be(2);

        _contextMock.Verify(m => m.Add(It.IsAny<Loan>()), Times.Once);
        _contextMock.Verify(m => m.Update(book), Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoAvailableCopies_ShouldThrowInvalidOperationException()
    {
        var book = _books[1];
        var member = _members[0];
        var command = new BorrowBookCommand(book.Id, member.Id, book.BranchId);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No copies available. Please place a reservation instead.");

        _addedEntities.Should().BeEmpty();
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
