using System;
using System.Linq;
using System.Security.Claims;
using LibraryManagementSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LibraryManagementSystem.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Username => User?.Identity?.Name;

    public string? Role => User?.FindFirstValue(ClaimTypes.Role)
                           ?? User?.Claims
                               .Where(c => c.Type.EndsWith("role", StringComparison.OrdinalIgnoreCase))
                               .Select(c => c.Value)
                               .FirstOrDefault();
}
