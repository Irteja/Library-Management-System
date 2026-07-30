
using LibraryManagementSystem.Application.Branches.Commands.CreateBranch;
using LibraryManagementSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public IActionResult GetAll()
    {
        return Ok("List all branches");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Librarian,Member")]
    public IActionResult GetById(Guid id)
    {
        return Ok($"Branch {id}");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateBranchCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(Guid id)
    {
        return Ok($"Branch {id} updated");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(Guid id)
    {
        return Ok($"Branch {id} deleted");
    }
}