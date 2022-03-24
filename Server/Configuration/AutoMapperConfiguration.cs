using AutoMapper;
using traderui.Server.Commands;
using traderui.Shared;
using traderui.Shared.Requests;

namespace traderui.Server.Configuration
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<PlaceOrderRequest, PlaceOrderCommand>().ReverseMap();
            CreateMap<PlaceOrderCommand, WebOrder>().ReverseMap();
        }
    }
}
