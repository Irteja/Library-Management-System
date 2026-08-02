using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Models;
using LibraryManagementSystem.Application.Loans.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Loans.Queries.GetActiveLoans;

public class GetActiveLoansQueryHandler : IRequestHandler<GetActiveLoansQuery, PaginatedList<LoanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveLoansQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<LoanDto>> Handle(GetActiveLoansQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.Status == LoanStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(l => l.Book.Title.Contains(request.SearchTerm) || l.Member.FirstName.Contains(request.SearchTerm) || l.Member.LastName.Contains(request.SearchTerm));
        }

        var projectedQuery = query.OrderByDescending(l => l.LoanDate)
            .Select(l => new LoanDto(
                l.Id,
                l.BookId,
                l.Book.Title,
                l.Book.Author,
                l.MemberId,
                l.Member.FirstName + " " + l.Member.LastName,
                l.BranchId,
                l.LoanDate,
                l.DueDate,
                l.ReturnDate,
                l.Status.ToString()
            ));

        return await PaginatedList<LoanDto>.CreateAsync(projectedQuery, request.PageNumber, request.PageSize);
    }
}
