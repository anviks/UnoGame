using AutoMapper;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.Services;
using UnoGame.Core.State;

namespace UnoGame.Core.Mapping;

public class GamePostMappingAction(GameService gameService, IMapper mapper) : IMappingAction<Game, GameDto>
{
    public void Process(Game source, GameDto destination, ResolutionContext context)
    {
        GameState state = gameService.GetGameStateByGame(source);
        
        // Pass the requestingUserId from parent context to nested mapping
        var requestingUserId = context.Items.TryGetValue("requestingUserId", out var userId) ? userId : null;
        destination.State = mapper.Map<GameStateDto>(state, opts =>
        {
            if (requestingUserId != null)
            {
                opts.Items["requestingUserId"] = requestingUserId;
            }
        });
    }
}