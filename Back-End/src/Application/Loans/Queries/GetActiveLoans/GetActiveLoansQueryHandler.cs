using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Loans.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Loans.Queries.GetActiveLoans;

public class GetActiveLoansQueryHandler : IRequestHandler<GetActiveLoansQuery, List<LoanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveLoansQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LoanDto>> Handle(GetActiveLoansQuery request, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.Status == LoanStatus.Active)
            .OrderByDescending(l => l.LoanDate)
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
            ))
            .ToListAsync(cancellationToken);
    }
}
