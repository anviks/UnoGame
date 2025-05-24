using Domain;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.DTO;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(GameService gameService) : ControllerBase
{
    // GET: api/<GameController>
    [HttpGet]
    public async Task<List<Game>> Get()
    {
        return await gameService.GetAllGames();
    }

    // GET api/<GameController>/5
    [HttpGet("{id:int}")]
    public async Task<Game?> Get(int id)
    {
        return await gameService.GetGame(id);
    }

    // POST api/<GameController>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Game>> Post([FromBody] CreateGameRequest gameRequest)
    {
        var game = new Game
        {
            GameName = gameRequest.GameName,
            Deck = new CardDeck(gameRequest.Deck),
            Players = gameRequest.Players.Select(p => new Player { Name = p.Name, Type = p.Type }).ToList(),
        };
        var result = await gameService.CreateGame(game);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        return result.Value;
    }

    // DELETE api/<GameController>/5
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task Delete(int id)
    {
        await gameService.DeleteGame(id);
    }
}