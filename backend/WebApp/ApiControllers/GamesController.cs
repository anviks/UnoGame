using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(GameService gameService, UserService userService, IMapper mapper) : ControllerBase
{
    // GET: api/<GameController>
    [HttpGet]
    public async Task<List<GameDto>> Get()
    {
        var allGames = await gameService.GetAllGames();
        return mapper.Map<List<GameDto>>(allGames);
    }

    // GET api/<GameController>/5
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<GameDto?>> Get(int id)
    {
        User? user = await userService.GetCurrentUser();
        Game? game = await gameService.GetGameDto(id);
        if (game == null) return NotFound();
        return mapper.Map<GameDto>(game, opts => opts.Items["requestingUserId"] = user?.Id);
    }

    // POST api/<GameController>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] CreateGameRequest gameRequest)
    {
        var result = await gameService.CreateGame(gameRequest.GameName, gameRequest.Players, gameRequest.IncludedCards);

        if (result.IsFailed) return BadRequest(result.Errors.First());

        return CreatedAtAction(nameof(Get), new { id = result.Value.Id }, mapper.Map<GameDto>(result.Value));
    }

    // DELETE api/<GameController>/5
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task Delete(int id)
    {
        await gameService.DeleteGame(id);
    }
}