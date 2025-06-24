using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;
using UnoGame.Core.State;
using WebApp.DTO;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(GameService gameService) : ControllerBase
{
    // GET: api/<GameController>
    [HttpGet]
    public async Task<List<GameDto>> Get()
    {
        var allGames = await gameService.GetAllGames();
        List<GameDto> allGameDtos = [];

        foreach (Game game in allGames)
        {
            var state = gameService.GetGameStateByGame(game);
            allGameDtos.Add(new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                CreatedAt = game.CreatedAt,
                UpdatedAt = game.UpdatedAt,
                Players = state.Players,
            });
        }

        return allGameDtos;
    }

    // GET api/<GameController>/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameState?>> Get(int id)
    {
        GameState? state = await gameService.GetGameState(id);
        if (state == null) return NotFound();
        return state;
    }

    // POST api/<GameController>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] CreateGameRequest gameRequest)
    {
        var gameState = new GameState
        {
            DrawPile = gameRequest.Deck,
            Players = gameRequest.Players,
        };

        var result = await gameService.CreateGame(gameRequest.GameName, gameState);

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