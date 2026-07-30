using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<Branch> Branches { get; }
    IQueryable<Book> Books { get; }
    IQueryable<Member> Members { get; }
    IQueryable<Loan> Loans { get; }
    IQueryable<User> Users { get; }
    IQueryable<Reservation> Reservations { get; }

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Update<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}