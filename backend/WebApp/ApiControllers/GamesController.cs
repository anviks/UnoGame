using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(GameService gameService, UserService userService) : ControllerBase
{
    // GET: api/<GameController>
    [HttpGet]
    public async Task<List<GameDto>> Get()
    {
        return await gameService.GetAllGameDtos();
    }

    // GET api/<GameController>/5
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<GameStateDto?>> Get(int id)
    {
        User? user = await userService.GetCurrentUser();
        GameStateDto? state = await gameService.GetGameStateDto(id, user!.Id);
        if (state == null) return NotFound();
        return state;
    }

    // POST api/<GameController>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] CreateGameRequest gameRequest)
    {
        var result = await gameService.CreateGame(gameRequest.GameName, gameRequest.Players, gameRequest.IncludedCards);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors.First());
        }

        return Created();
    }

    // DELETE api/<GameController>/5
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task Delete(int id)
    {
        await gameService.DeleteGame(id);
    }
}