using FakeXiecheng.api.Models;
using System;

namespace FakeXiecheng.api.Dtos
{
    public class LineItemDto
    {
        public int Id { get; set; }
        public Guid TouristRouteId { get; set; }
        public TouristRoute TouristRoute { get; set; }
        public Guid? ShoppingCartId { get; set; }
        public Guid? OrderId { get; set; }
        public decimal OriginalPrice { get; set; }
    }
}
