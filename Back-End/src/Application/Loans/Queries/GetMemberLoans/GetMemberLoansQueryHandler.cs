using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Loans.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Loans.Queries.GetMemberLoans;

public class GetMemberLoansQueryHandler : IRequestHandler<GetMemberLoansQuery, List<LoanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMemberLoansQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LoanDto>> Handle(GetMemberLoansQuery request, CancellationToken cancellationToken)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == request.MemberId)
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
