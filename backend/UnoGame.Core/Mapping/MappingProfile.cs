using AutoMapper;
using UnoGame.Core.DTO;
using UnoGame.Core.Entities;
using UnoGame.Core.State;

namespace UnoGame.Core.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Player, PlayerDto>()
            .ForMember(
                dest => dest.HandSize,
                opt => opt.MapFrom(src => src.Cards.Count)
            )
            .ForMember(
                dest => dest.Cards,
                opt => opt.MapFrom((src, _, _, context) =>
                {
                    var requestingUserId = context.Items["requestingUserId"] as int? ?? 0;
                    var isSelf = src.UserId == requestingUserId;
                    return isSelf ? src.Cards : null;
                })
            );

        CreateMap<GameState, GameStateDto>()
            .ForMember(
                dest => dest.DrawPileSize,
                opt => opt.MapFrom(src => src.DrawPile.Count)
            );

        CreateMap<Game, GameDto>()
            .AfterMap<GamePostMappingAction>();

        CreateMap<User, UserDto>();
    }
}