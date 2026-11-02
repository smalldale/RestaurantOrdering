using AutoMapper;
using RestaurantOrdering.API.DTOs.MenuItem;
using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.Profiles;

public class MenuItemProfile : Profile
{
    public MenuItemProfile()
    {
        CreateMap<CreateMenuItemDto, MenuItem>();

        CreateMap<UpdateMenuItemDto, MenuItem>();

        CreateMap<MenuItem, MenuItemResponseDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.Name));
    }
}
