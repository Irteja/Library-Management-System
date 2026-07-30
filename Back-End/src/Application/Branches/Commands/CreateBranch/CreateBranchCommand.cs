using MediatR;

namespace LibraryManagementSystem.Application.Branches.Commands.CreateBranch;

public record CreateBranchCommand(string Name, string Address, string Phone, string Email) : IRequest<Guid>;
