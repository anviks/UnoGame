using AutoMapper;
using UnoGame.Core.Entities;
using WebApp.DTO;

namespace WebApp.Mapping;

public class GameMappingProfile : Profile
{
    public GameMappingProfile()
    {
        CreateMap<Game, GameDto>();
    }
}