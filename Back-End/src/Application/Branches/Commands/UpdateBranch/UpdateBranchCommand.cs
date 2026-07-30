using MediatR;

namespace LibraryManagementSystem.Application.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(Guid Id, string Name, string Address, string Phone, string Email) : IRequest;
