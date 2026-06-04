using Microsoft.AspNetCore.Mvc;
using UserService.Application.Dtos;
using UserService.Application.Services;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;

namespace UserServiceWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(IUserService m_userService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] UserDto dto,
        CancellationToken cancellation)
    {
        User user = await m_userService.CreateUserAsync(dto, cancellation);
        return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, user);
    }

    [HttpPatch("{userId:guid}/balance")]
    public async Task<IActionResult> UpdateBalance(
        Guid userId,
        [FromBody] decimal delta,
        CancellationToken cancellation)
    {
        try
        {
            await m_userService.UpdateBalanceAsync(new UpdateBalanceDto(userId, delta), cancellation);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("balance/bulk")]
    public async Task<IActionResult> UpdateBalanceBulk(
        [FromBody] List<UpdateBalanceDto> dtos,
        CancellationToken cancellation)
    {
        try
        {
            await m_userService.UpdateBalanceBulkAsync(dtos, cancellation);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}