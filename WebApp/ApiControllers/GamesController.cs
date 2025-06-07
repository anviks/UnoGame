using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnoGame.Core.Entities;
using UnoGame.Core.Entities.Enums;
using UnoGame.Core.Services;
using WebApp.DTO;

namespace WebApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(GameService gameService, IMapper mapper) : ControllerBase
{
    // GET: api/<GameController>
    [HttpGet]
    public async Task<List<Game>> Get()
    {
        return await gameService.GetAllGames();
    }

    // GET api/<GameController>/5
    [HttpGet("{id:int}")]
    public async Task<GameDto?> Get(int id)
    {
        Game? game = await gameService.GetGame(id);
        return game == null ? null : mapper.Map<GameDto>(game);
    }

    // POST api/<GameController>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromBody] CreateGameRequest gameRequest)
    {
        var game = new Game
        {
            Name = gameRequest.GameName,
            PileCards = gameRequest.Deck.Select(card => new PileCard
                { Card = new Card { Color = card.Color, Value = card.Value }, PileType = PileType.DrawPile }).ToList(),
            Players = gameRequest.Players.Select(p => new Player { Name = p.Name, Type = p.Type }).ToList(),
        };
        var result = await gameService.CreateGame(game);

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