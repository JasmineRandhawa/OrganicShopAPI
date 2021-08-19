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
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;

        public ShoppingCartController(OrganicShopDbContext context, IRepository<ShoppingCartItem> shoppingCartItemRepository,
            IRepository<ShoppingCart> shoppingCartRepository,IRepository<AppUser> userRepository)
        {
            _context = context;
            _cartRepository = shoppingCartRepository;
            _userRepository = userRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
        }

        #endregion

        #region "ShoppingCart API Public Methods"

        [HttpGet]
        [Route(Routes.UserId)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCartDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUser(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return BadRequest($"{nameof(AppUser)}{nameof(Id) + ErrorMessages.EmptyOrWhiteSpace}");

                var shoppingCart = await GetFilteredCart((cart) => cart.AppUserId.Equals(Id));
                if (shoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)} with {nameof(AppUser)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                return Ok(ConstructShoppingCartResponseAsync(shoppingCart));
                                        
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
                    return BadRequest(nameof(Id) + ErrorMessages.LessThanEqualToZero);

                var shoppingCart = await GetFilteredCart((cart) => cart.Id == Id);

                if (shoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                return Ok(ConstructShoppingCartResponseAsync(shoppingCart));
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

                var appUser = await _userRepository.Get(shoppingCart.AppUserId);
                if (appUser != null)
                    shoppingCart.AppUserName = appUser.AppUserName;
                shoppingCart.DateCreated = DateTime.UtcNow.ToString();
                await _cartRepository.Add(shoppingCart);

                await _context.SaveChangesAsync();
                var dbShoppingCart = await GetFilteredCart((cart) => cart.Id == shoppingCart.Id);
                return Created("", dbShoppingCart);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost(Routes.ItemAdd)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddItem(ShoppingCartItem shoppingCartItem)
        {
            try
            {
                var checkInputErrorMessage = ValidateShoppingCartItemInput(shoppingCartItem, Constants.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbShoppingCart = await _cartRepository.Get(shoppingCartItem.ShoppingCartId);

                if (dbShoppingCart == null)
                    return NotFound($"{nameof(shoppingCartItem.ShoppingCartId)} '{shoppingCartItem.ShoppingCartId}' {ErrorMessages.DoesNotExist}");

                await _shoppingCartItemRepository.Add(shoppingCartItem);

                await _context.SaveChangesAsync();
                return Created("",shoppingCartItem.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPatch(Routes.ItemUpdate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ShoppingCartItem))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateItem(ShoppingCartItem shoppingCartItem)
        {
            try
            {
                var checkInputErrorMessage = ValidateShoppingCartItemInput(shoppingCartItem, Constants.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbShoppingCartItem = await GetFilteredCartItem((cart) => cart.Id == shoppingCartItem.Id);

                if (dbShoppingCartItem == null)
                    return NotFound($"{nameof(ShoppingCartItem)}{nameof(shoppingCartItem.Id)} '{shoppingCartItem.Id}' {ErrorMessages.DoesNotExist}");

                var dbShoppingCart = await _cartRepository.Get(dbShoppingCartItem.ShoppingCartId);
                if(dbShoppingCart!=null)
                    dbShoppingCart.DateModified = DateTime.UtcNow.ToString();

                dbShoppingCartItem.Quantity = shoppingCartItem.Quantity;

                await _context.SaveChangesAsync();
                return Ok(dbShoppingCartItem);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete(Routes.ItemId)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItem(int ItemId)
        {
            try
            {
                if (ItemId <= 0)
                    return BadRequest($"{nameof(ShoppingCart)}{nameof(ItemId) + ErrorMessages.LessThanEqualToZero}");

                var dbShoppingCartItem = await _shoppingCartItemRepository.Get(ItemId);
                if (dbShoppingCartItem == null)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(ItemId)} '{ItemId}' {ErrorMessages.DoesNotExist}");

                _shoppingCartItemRepository.Delete(dbShoppingCartItem);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete(Routes.AllId)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAll(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(ShoppingCart)}{nameof(Id) + ErrorMessages.LessThanEqualToZero}");

                var dbShoppingCart = await GetFilteredCart((cart) => cart.Id ==Id);
                if (dbShoppingCart == null)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                if(dbShoppingCart.Items!=null && dbShoppingCart.Items.Count() == 0)
                    return NotFound($"{nameof(ShoppingCart)}{nameof(Id)} '{Id}' {ErrorMessages.EmptyCart}");

                foreach (var item in dbShoppingCart.Items)
                {
                    _shoppingCartItemRepository.Delete(item);
                }
                await _context.SaveChangesAsync();
                return NoContent();
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
                    return BadRequest($"{nameof(ShoppingCart)}{nameof(Id) + ErrorMessages.LessThanEqualToZero}");

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
                return nameof(shoppingCart.Id) + ErrorMessages.LessThanEqualToZero;

            // Items validation
            if (shoppingCart.Items==null || (shoppingCart.Items != null && shoppingCart.Items.Count() == 0))
                errorMessage += ErrorMessages.ShoppingCartItemsMissing;

            return errorMessage;
        }

        private string ValidateShoppingCartItemInput(ShoppingCartItem shoppingCartItem, Constants.Action action)
        {
            string errorMessage = string.Empty;

            // shoppingCartItem parameter validation
            if (shoppingCartItem == null)
                return nameof(shoppingCartItem) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (shoppingCartItem.Id <= 0 && action == Constants.Action.Update)
                return nameof(shoppingCartItem.Id) + ErrorMessages.LessThanEqualToZero;

            // ShoppingCartId validation on update operation
            if (shoppingCartItem.ShoppingCartId <= 0 && action == Constants.Action.Add)
                return nameof(shoppingCartItem.ShoppingCartId) + ErrorMessages.LessThanEqualToZero;

            // ProductId validation on update operation
            if (shoppingCartItem.ProductId <= 0 )
                return nameof(shoppingCartItem.ProductId) + ErrorMessages.LessThanEqualToZero;

            // Quantity validation on update operation
            if (shoppingCartItem.Quantity <= 0)
                return nameof(shoppingCartItem.Quantity) + ErrorMessages.LessThanEqualToZero;

            return errorMessage;
        }

        #endregion

        #region "Shopping Cart Private Methods"
        private ShoppingCartDto ConstructShoppingCartResponseAsync(ShoppingCart shoppingCart)
        {
            ShoppingCartDto shoppingCartResponse = new();
            shoppingCartResponse.Id = shoppingCart.Id;
            shoppingCartResponse.AppUserId = shoppingCart.AppUserId;
            shoppingCartResponse.AppUserName = shoppingCart.AppUserName;
            foreach (var item in shoppingCart.Items)
            {
                ShoppingCartItemDto shoppingCartItem = new ShoppingCartItemDto();
                shoppingCartItem.ShoppingCartId = shoppingCart.Id;
                shoppingCartItem.Id = item.Id;
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
                Id = product.Id,
                Title = product.Title,
                Category = product.Category.Name,
                Price = product.Price,
                ImageURL = product.ImageURL
            };
        }

        private async Task<ShoppingCart> GetFilteredCart(Expression<Func<ShoppingCart,bool>> filter)
        {
            return await _cartRepository.GetAll()
                                        .Where(filter)
                                        .Include(cart => cart.Items)
                                        .ThenInclude(item => item.Product)
                                        .ThenInclude(product => product.Category)
                                        .FirstOrDefaultAsync();
        }

        private async Task<ShoppingCartItem> GetFilteredCartItem(Expression<Func<ShoppingCartItem, bool>> filter)
        {
            return await _shoppingCartItemRepository.GetAll()
                                        .Where(filter)
                                        .Include(item => item.Product)
                                        .FirstOrDefaultAsync();
        }

        #endregion
    }
}
