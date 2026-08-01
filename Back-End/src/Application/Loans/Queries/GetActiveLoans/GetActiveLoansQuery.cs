using LibraryManagementSystem.Application.Loans.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Loans.Queries.GetActiveLoans;

public record GetActiveLoansQuery : IRequest<List<LoanDto>>;
