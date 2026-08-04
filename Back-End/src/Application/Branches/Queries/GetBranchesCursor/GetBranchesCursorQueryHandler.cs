using System.Text;
using LibraryManagementSystem.Application.Branches.DTOs;
using LibraryManagementSystem.Application.Common.Interfaces;
using LibraryManagementSystem.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Application.Branches.Queries.GetBranchesCursor;

public class GetBranchesCursorQueryHandler : IRequestHandler<GetBranchesCursorQuery, CursorPaginatedList<BranchDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBranchesCursorQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<CursorPaginatedList<BranchDto>> Handle(GetBranchesCursorQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Branches.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var cursorParts = DecodeCursor(request.Cursor);
            if (cursorParts != null)
            {
                var cursorDate = cursorParts.Value.CreatedAt;
                var cursorId = cursorParts.Value.Id;
                
                query = query.Where(b => b.CreatedAt > cursorDate || (b.CreatedAt == cursorDate && b.Id.CompareTo(cursorId) > 0));
            }
        }

        query = query.OrderBy(b => b.CreatedAt).ThenBy(b => b.Id);

        var items = await query.Take(request.Limit + 1).ToListAsync(cancellationToken);
        
        bool hasNextPage = items.Count > request.Limit;
        if (hasNextPage)
        {
            items.RemoveAt(items.Count - 1);
        }

        string? nextCursor = null;
        if (items.Any())
        {
            var lastItem = items.Last();
            nextCursor = EncodeCursor(lastItem.CreatedAt, lastItem.Id);
        }

        var dtos = items.Select(b => new BranchDto(b.Id, b.Name, b.Address, b.Phone, b.Email, b.CreatedAt, b.IsActive)).ToList();

        return new CursorPaginatedList<BranchDto>(dtos, nextCursor, hasNextPage);
    }

    private static string EncodeCursor(DateTime createdAt, Guid id)
    {
        var cursorStr = $"{createdAt.ToString("o")}|{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(cursorStr));
    }

    private static (DateTime CreatedAt, Guid Id)? DecodeCursor(string cursor)
    {
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var parts = decoded.Split('|');
            if (parts.Length == 2 && DateTime.TryParse(parts[0], null, System.Globalization.DateTimeStyles.RoundtripKind, out var date) && Guid.TryParse(parts[1], out var id))
            {
                return (date, id);
            }
        }
        catch { }
        return null;
    }
}
