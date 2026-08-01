using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Application.Loans.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Loans.Queries.GetMemberLoans;

public class GetMemberLoansQueryHandler : IRequestHandler<GetMemberLoansQuery, List<LoanDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemberAccessService _memberAccess;

    public GetMemberLoansQueryHandler(IApplicationDbContext context, IMemberAccessService memberAccess)
    {
        _context = context;
        _memberAccess = memberAccess;
    }

    public async Task<List<LoanDto>> Handle(GetMemberLoansQuery request, CancellationToken cancellationToken)
    {
        var memberId = await _memberAccess.GetAccessibleMemberIdAsync(request.MemberId, cancellationToken);

        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == memberId)
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
