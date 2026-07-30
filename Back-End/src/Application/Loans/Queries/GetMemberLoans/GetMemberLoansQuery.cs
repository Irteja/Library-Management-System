using LibraryManagementSystem.Application.Loans.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Loans.Queries.GetMemberLoans;

public record GetMemberLoansQuery(Guid MemberId) : IRequest<List<LoanDto>>;
