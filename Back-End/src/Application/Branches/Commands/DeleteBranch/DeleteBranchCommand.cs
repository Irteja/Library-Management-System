using MediatR;

namespace LibraryManagementSystem.Application.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest;
