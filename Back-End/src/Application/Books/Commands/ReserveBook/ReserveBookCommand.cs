using MediatR;

namespace LibraryManagementSystem.Application.Books.Commands.ReserveBook;

public record ReserveBookCommand(Guid BookId, Guid MemberId, Guid BranchId) : IRequest<Guid>;