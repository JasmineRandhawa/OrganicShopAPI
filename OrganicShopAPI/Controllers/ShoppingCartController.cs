using System;
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
    public class ShoppingCartController : ControllerBase
    {
        #region "Declarations and Constructor"

        private readonly OrganicShopDbContext _context;
        private readonly IRepository<ShoppingCart> _cartRepository;

        public ShoppingCartController(OrganicShopDbContext context, IRepository<ShoppingCart> shoppingCartRepository)
        {
            _context = context;
            _cartRepository = shoppingCartRepository;
        }

        #endregion

        #region "ShoppingCart API Public Methods"

        [HttpGet]
        [Route(Routes.UserId)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCartDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUser(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(nameof(Id) + ErrorMessages.LessThenZero);

                var shoppingCart = await GetFilteredCart((cart) => cart.AppUserId == Id);
                if (shoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)} with {nameof(AppUser)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                return Ok(ConstructShoppingCartResponse(shoppingCart));
                                        
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet(Routes.Id)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCartDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(nameof(Id) + ErrorMessages.LessThenZero);

                var shoppingCart = await GetFilteredCart((cart) => cart.Id == Id);

                if (shoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                return Ok(ConstructShoppingCartResponse(shoppingCart));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost(Routes.Add)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ShoppingCartDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(ShoppingCart shoppingCart)
        {
            try
            {
                var checkInputErrorMessage = ValidateShoppingCartInput(shoppingCart, Constants.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                shoppingCart.DateCreated = DateTime.UtcNow.ToString();
                await _cartRepository.Add(shoppingCart);

                await _context.SaveChangesAsync();
                var dbShoppingCart = await GetFilteredCart((cart) => cart.Id == shoppingCart.Id);
                return Created("", ConstructShoppingCartResponse(dbShoppingCart));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Update)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCartDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(ShoppingCart shoppingCart)
        {
            try
            {
                var checkInputErrorMessage = ValidateShoppingCartInput(shoppingCart, Constants.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbShoppingCart = await GetFilteredCart((cart) => cart.Id == shoppingCart.Id);

                if (dbShoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(shoppingCart.Id)} '{shoppingCart.Id}' {ErrorMessages.DoesNotExist}");

                dbShoppingCart.Items = shoppingCart.Items;
                dbShoppingCart.DateModified = DateTime.UtcNow.ToString();

                await _context.SaveChangesAsync();
                return Ok(ConstructShoppingCartResponse(dbShoppingCart));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete(Routes.Id)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(ShoppingCart)}{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbShoppingCart = await _cartRepository.Get(Id);
                if (dbShoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                _cartRepository.Delete(dbShoppingCart);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region "Validation Methods"
        private string ValidateShoppingCartInput(ShoppingCart shoppingCart, Constants.Action action)
        {
            string errorMessage = string.Empty;

            // shoppingCart parameter validation
            if (shoppingCart==null)
                return nameof(shoppingCart) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (shoppingCart.Id <= 0 && action == Constants.Action.Update)
                return nameof(shoppingCart.Id) + ErrorMessages.LessThenZero;

            // Items validation
            if (shoppingCart.Items==null || (shoppingCart.Items == null && shoppingCart.Items.Count() == 0))
                errorMessage += ErrorMessages.ShoppingCartItemsMissing;

            return errorMessage;
        }

        #endregion

        #region "Shopping Cart Private Methods"
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
                ImageURL = product.ImageURL
            };
        }

        private async Task<ShoppingCart> GetFilteredCart(Expression<Func<ShoppingCart,bool>> filter)
        {
            return await _cartRepository.GetAll()
                                        .Where(filter)
                                        .Include(cart => cart.AppUser)
                                        .Include(cart => cart.Items)
                                        .ThenInclude(item => item.Product)
                                        .ThenInclude(product => product.Category)
                                        .FirstOrDefaultAsync();
        }

        #endregion
    }
}
