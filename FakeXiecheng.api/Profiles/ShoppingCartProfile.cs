using AutoMapper;
using FakeXiecheng.api.Dtos;
using FakeXiecheng.api.Models;

namespace FakeXiecheng.api.Profiles
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {
            CreateMap<ShoppingCart, ShoppingCartDto>();
            CreateMap<LineItem, LineItemDto>();
        }
    }
}
