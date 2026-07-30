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
[Authorize(Roles = "Admin,Librarian")]
public class MembersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MembersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var members = await _mediator.Send(new GetAllMembersQuery());
        return Ok(members);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var member = await _mediator.Send(new GetMemberByIdQuery(id));
        return Ok(member);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMemberCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
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
