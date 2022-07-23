using AutoMapper;
using FakeXiecheng.api.Dtos;
using FakeXiecheng.api.Helper;
using FakeXiecheng.api.Models;
using FakeXiecheng.api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FakeXiecheng.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController:ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        public ShoppingCartController(
            IHttpContextAccessor httpContextAccessor
,           ITouristRouteRepository touristRouteRepository,
            IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetShoppingCart")]
        [Authorize]
        public async Task<IActionResult> GetShoppingCartAsync()
        {
            //1、获得当前用户
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //2、使用uesrId获得购物车
            var shoppingCart = await _touristRouteRepository.GetShoppingCartByUserIdAsync(userId);

            return Ok(_mapper.Map<ShoppingCartDto>(shoppingCart));
        }
        [HttpPost("items")]
        [Authorize(AuthenticationSchemes ="Bearer")]
        public async Task<IActionResult> AddShoppingCartItemsAsync([FromBody] AddShoppingCartItemDto addShoppingCartItemDto)
        {
            //1、获得当前用户
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //2、使用uesrId获得购物车
            var shoppingCart = await _touristRouteRepository.GetShoppingCartByUserIdAsync(userId);

            //3、创建Lineitem
            var touristRoute = await _touristRouteRepository.GetTouristRouteAsync(addShoppingCartItemDto.TouristRouteId);
            if(touristRoute == null)
            {
                return NotFound("旅游路线不存在");
            }

            var lineItem = new LineItem()
            {
                TouristRouteId = addShoppingCartItemDto.TouristRouteId,
                ShoppingCartId = shoppingCart.Id,
                OriginalPrice = touristRoute.OriginalPrice,
                DiscountPresent = touristRoute.DiscountPresent
            };

            //4、添加lineitem,并保存数据库
            await _touristRouteRepository.AddShoppingCartItemAsync(lineItem);
            await _touristRouteRepository.SaveAsync();

            return Ok(_mapper.Map<ShoppingCartDto>(shoppingCart));
        }
        [HttpDelete("items/{itemId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteShoppingCrtItem([FromRoute] int itemId)
        {
            //1、获取lineitem数据
            var lineItem = await _touristRouteRepository
                .GetShoppingCartItemByItemIdAsync(itemId);
            if(lineItem == null)
            {
                return NotFound("购物车商品找不到");
            }

            _touristRouteRepository.DeleteShoppingCartItem(lineItem);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("items/({itemIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveShoppingCartItems(
            [ModelBinder(BinderType =typeof(ArrayModelBinder))][FromRoute] IEnumerable<int> itemIDs
            )
        {
            var lineitems = await _touristRouteRepository
                .GetshoppingCartsByIdListAsync(itemIDs);

            _touristRouteRepository.DeleteShoppingCartItems(lineitems);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes ="Bearer")]
        public async Task<IActionResult> Checkout()
        {
            //1、获得当前用户
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //2、使用uesrId获得购物车
            var shoppingCart = await _touristRouteRepository.GetShoppingCartByUserIdAsync(userId);

            //3、创建订单
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                State = OrderStateEnum.Pending,
                ShoppingCartItems = shoppingCart.ShoppingCartItems,
                CreateDateUTC = DateTime.UtcNow
            };
            shoppingCart.ShoppingCartItems = null;
            //4、保存订单
            await _touristRouteRepository.AddOrderAsync(order);
            await _touristRouteRepository.SaveAsync();
            //5、return
            return Ok(_mapper.Map<OrderDto>(order));
        }
    }
}
