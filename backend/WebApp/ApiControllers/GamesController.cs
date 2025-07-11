using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;
using UnoGame.Core.State;
using WebApp.DTO;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(GameService gameService, UserService userService) : ControllerBase
{
    // GET: api/<GameController>
    [HttpGet]
    public async Task<List<GameDto>> Get()
    {
        var allGames = await gameService.GetAllGames();
        List<GameDto> allGameDtos = [];

        foreach (Game game in allGames)
        {
            GameState state = gameService.GetGameStateByGame(game);
            allGameDtos.Add(new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                CreatedAt = game.CreatedAt,
                UpdatedAt = game.UpdatedAt,
                PlayerNames = state.Players.Select(p => p.Name).ToList(),
            });
        }

        return allGameDtos;
    }

    // GET api/<GameController>/5
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<GameStateDto?>> Get(int id)
    {
        User? user = await userService.GetCurrentUser();
        GameStateDto? state = await gameService.GetGameState(id, user!.Id);
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
            return BadRequest(result.Errors);
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