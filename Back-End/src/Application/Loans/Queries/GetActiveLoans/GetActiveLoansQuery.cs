using LibraryManagementSystem.Application.Common.Models;
using LibraryManagementSystem.Application.Loans.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Loans.Queries.GetActiveLoans;

public record GetActiveLoansQuery(string? SearchTerm = null, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<LoanDto>>;
