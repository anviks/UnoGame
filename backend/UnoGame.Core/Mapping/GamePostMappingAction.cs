using AutoMapper;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;
using UnoGame.Core.State;

namespace UnoGame.Core.Mapping;

public class GamePostMappingAction(GameService gameService) : IMappingAction<Game, GameDto>
{
    public void Process(Game source, GameDto destination, ResolutionContext context)
    {
        GameState state = gameService.GetGameStateByGame(source);
        destination.PlayerNames = state.Players.Select(p => p.Name).ToList();
    }
}