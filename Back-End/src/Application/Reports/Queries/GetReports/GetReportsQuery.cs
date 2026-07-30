using LibraryManagementSystem.Application.Reports.DTOs;
using MediatR;

namespace LibraryManagementSystem.Application.Reports.Queries.GetReports;

public record GetReportsQuery : IRequest<LibraryReportDto>;