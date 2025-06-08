using AutoMapper;
using UnoGame.Core.Entities;
using WebApp.DTO;

namespace WebApp.Mapping;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        CreateMap<Game, GameDto>();
        CreateMap<Card, CardDto>().ReverseMap();
        CreateMap<Player, PlayerDto>()
            .ForMember(dest => dest.Cards,
                opt => opt.MapFrom(src =>
                    src.PlayerCards.Select(pc => pc.Card)
                ));
    }
}