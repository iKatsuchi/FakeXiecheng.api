using AutoMapper;
using FakeXiecheng.api.Dtos;
using FakeXiecheng.api.Models;

namespace FakeXiecheng.api.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(
                dest => dest.State,
                opt =>
                {
                    opt.MapFrom(src => src.State.ToString());
                }
                );

        }
    }
}
