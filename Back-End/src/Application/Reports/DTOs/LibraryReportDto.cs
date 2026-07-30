namespace LibraryManagementSystem.Application.Reports.DTOs;

public class LibraryReportDto
{
    public int ActiveLoansCount { get; set; }
    public int OverdueLoansCount { get; set; }
    public int TotalMembers { get; set; }
    public int TotalBooks { get; set; }
    public List<PopularBookDto> TopBorrowedBooks { get; set; } = new();
}

public class PopularBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
}