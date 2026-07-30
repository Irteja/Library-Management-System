using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Reports.DTOs;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Reports.Queries.GetReports;

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, LibraryReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetReportsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<LibraryReportDto> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var activeLoans = await _context.Loans
            .CountAsync(l => l.Status == LoanStatus.Active, cancellationToken);

        var overdueLoans = await _context.Loans
            .CountAsync(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow, cancellationToken);

        var totalMembers = await _context.Members.CountAsync(cancellationToken);
        var totalBooks = await _context.Books.CountAsync(cancellationToken);

        var topBooks = await _context.Loans
            .GroupBy(l => new { l.Book.Title, l.Book.Author, l.Book.ISBN })
            .Select(g => new PopularBookDto
            {
                Title = g.Key.Title,
                Author = g.Key.Author,
                ISBN = g.Key.ISBN,
                BorrowCount = g.Count()
            })
            .OrderByDescending(b => b.BorrowCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new LibraryReportDto
        {
            ActiveLoansCount = activeLoans,
            OverdueLoansCount = overdueLoans,
            TotalMembers = totalMembers,
            TotalBooks = totalBooks,
            TopBorrowedBooks = topBooks
        };
    }
}