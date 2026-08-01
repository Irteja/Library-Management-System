using LibraryManagementSystem.Application.Branches.Commands.CreateBranch;
using LibraryManagementSystem.Application.Branches.Commands.DeleteBranch;
using LibraryManagementSystem.Application.Branches.Commands.UpdateBranch;
using LibraryManagementSystem.Application.Branches.Queries.GetAllBranches;
using LibraryManagementSystem.Application.Branches.Queries.GetBranchById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _mediator.Send(new GetAllBranchesQuery());
        return Ok(branches);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var branch = await _mediator.Send(new GetBranchByIdQuery(id));
        return Ok(branch);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBranchCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBranchCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteBranchCommand(id));
        return NoContent();
    }
}
