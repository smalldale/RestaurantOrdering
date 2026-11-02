using AutoMapper;
using RestaurantOrdering.API.DTOs.Category;
using RestaurantOrdering.API.Models;

namespace RestaurantOrdering.API.MappingProfiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore());

        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.MenuItems, opt => opt.Ignore());

        CreateMap<Category, CategoryResponseDto>();
    }
}