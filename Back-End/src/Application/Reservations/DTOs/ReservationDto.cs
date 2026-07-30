namespace LibraryManagementSystem.Application.Reservations.DTOs;

public record ReservationDto(
    Guid Id,
    Guid BookId,
    string BookTitle,
    string BookAuthor,
    Guid MemberId,
    string MemberName,
    Guid BranchId,
    DateTime ReservedAt,
    DateTime ExpiresAt,
    int QueuePosition,
    string Status
);
