using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OrganicShopAPI.Constants;
using OrganicShopAPI.DataAccess;
using OrganicShopAPI.DataTransferObjects;
using OrganicShopAPI.Models;

namespace OrganicShopAPI.Controllers
{
    [ApiController]
    [Route(Routes.Controller)]
    public class OrdersController : ControllerBase
    {
        #region "Declarations and Constructor"

        private readonly OrganicShopDbContext _context;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<ShoppingCart> _cartRepository;
        private readonly IRepository<AppUser> _userRepository;

        public OrdersController(OrganicShopDbContext context, IRepository<Order> orderRepository,
                                IRepository<ShoppingCart> cartRepository, IRepository<AppUser> userRepository)
        {
            _context = context;
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _userRepository = userRepository;
        }

        #endregion

        #region "Order API Public Methods"

        [HttpGet]
        [Route(Routes.UserId)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUser(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(nameof(Id) + ErrorMessages.LessThanEqualToZero);

                var orders = await GetFilteredOrders((cart) => cart.ShoppingCart.AppUserId == Id);
                if (orders == null)
                    return NotFound($"{nameof(Order)} with {nameof(AppUser)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                if (orders.Count() == 0)
                    return NoContent();


                return Ok(await ConstructOrdersResponse(orders.ToList()));
                                        
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet(Routes.Id)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(nameof(Id) + ErrorMessages.LessThanEqualToZero);

                var order = (await GetFilteredOrders((cart) => cart.Id == Id)).FirstOrDefault(); ;

                if (order == null)
                    return NotFound($"{nameof(Order)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                return Ok(await ConstructOrderResponse(order));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost(Routes.Add)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(Order order)
        {
            try
            {
                if (order.ShoppingCartId <= 0)
                    return BadRequest(nameof(order.ShoppingCartId) + ErrorMessages.LessThanEqualToZero);

                var shoppingCart = await _cartRepository.Get(order.ShoppingCartId);

                if(shoppingCart==null)
                    return BadRequest($"{nameof(order.ShoppingCartId)} '{order.ShoppingCartId}' {ErrorMessages.DoesNotExist}");

                order.DateCreated = DateTime.UtcNow.ToString();
                await _orderRepository.Add(order);

                await _context.SaveChangesAsync();
                var dbOrder = (await GetFilteredOrders((cart) => cart.Id == order.Id)).FirstOrDefault();
                return Created("", await ConstructOrderResponse(dbOrder));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region "Shopping Cart Private Methods"

        private async Task<List<OrderDto>> ConstructOrdersResponse(List<Order> orders)
        {
            List<OrderDto> ordersResponse = new();
            foreach (var order in orders)
            {
                var orderObj = order;
                orderObj.ShoppingCart.AppUser = await _userRepository.Get(order.ShoppingCart.AppUserId);

                OrderDto orderResponse = new();
                orderResponse.Id = order.Id;
                orderResponse.UserName = order.UserName;
                orderResponse.Address = order.Address;
                orderResponse.ShoppingCart = ConstructShoppingCartResponse(orderObj.ShoppingCart);
                orderResponse.DateCreated = order.DateCreated;
                ordersResponse.Add(orderResponse);
            }
            return ordersResponse;
        }

        private async Task<OrderDto> ConstructOrderResponse(Order order)
        {
            order.ShoppingCart.AppUser = await _userRepository.Get(order.ShoppingCart.AppUserId);

            OrderDto orderResponse = new();
            orderResponse.Id = order.Id;
            orderResponse.UserName = order.UserName;
            orderResponse.Address = order.Address;
            orderResponse.ShoppingCart = ConstructShoppingCartResponse(order.ShoppingCart);
            orderResponse.DateCreated = order.DateCreated;
            return orderResponse;
        }

        private ShoppingCartDto ConstructShoppingCartResponse(ShoppingCart shoppingCart)
        {
            ShoppingCartDto shoppingCartResponse = new();
            shoppingCartResponse.Id = shoppingCart.Id;
            shoppingCartResponse.AppUserId = shoppingCart.AppUserId;
            shoppingCartResponse.AppUserName = shoppingCart.AppUser.Name;
            foreach (var item in shoppingCart.Items)
            {
                ShoppingCartItemDto shoppingCartItem = new ShoppingCartItemDto();
                shoppingCartItem.Product = ConstructProductResponse(item.Product);
                shoppingCartItem.Quantity = item.Quantity;
                shoppingCartResponse.Items.Add(shoppingCartItem);
            }
            return shoppingCartResponse;
        }

        private ProductDto ConstructProductResponse(Product product)
        {
            return new ProductDto
            {
                Title = product.Title,
                Category = product.Category.Name,
                Price = product.Price,
                ImageURL = product.ImageURL
            };
        }

        private async Task<IEnumerable<Order>> GetFilteredOrders(Expression<Func<Order,bool>> filter)
        {
            return await _orderRepository.GetAll()
                                        .Where(filter)
                                        .Include(cart => cart.ShoppingCart)
                                        .ThenInclude(cart => cart.Items)
                                        .ThenInclude(item => item.Product)
                                        .ThenInclude(product => product.Category)
                                        .ToListAsync();
        }

        #endregion
    }
}
