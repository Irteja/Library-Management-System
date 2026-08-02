using LibraryManagementSystem.Application.Common.Services;
using LibraryManagementSystem.Application.Members.Commands.CreateMember;
using LibraryManagementSystem.Application.Members.Commands.DeleteMember;
using LibraryManagementSystem.Application.Members.Commands.UpdateMember;
using LibraryManagementSystem.Application.Members.Queries.GetAllMembers;
using LibraryManagementSystem.Application.Members.Queries.GetMemberById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMemberAccessService _memberAccess;

    public MembersController(IMediator mediator, IMemberAccessService memberAccess)
    {
        _mediator = mediator;
        _memberAccess = memberAccess;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var members = await _mediator.Send(new GetAllMembersQuery(search, page, size));
        return Ok(members);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _mediator.Send(new GetMemberByIdQuery(id));
        return Ok(member);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Member")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var memberId = await _memberAccess.GetCurrentMemberIdAsync(cancellationToken);
        var member = await _mediator.Send(new GetMemberByIdQuery(memberId!.Value), cancellationToken);
        return Ok(member);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Create(CreateMemberCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Librarian")]
    public async Task<IActionResult> Update(Guid id, UpdateMemberCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteMemberCommand(id));
        return NoContent();
    }
}
