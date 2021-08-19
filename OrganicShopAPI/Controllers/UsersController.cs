using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

using OrganicShopAPI.Constants;
using OrganicShopAPI.DataAccess;
using OrganicShopAPI.Models;
using OrganicShopAPI.Utility;

namespace OrganicShopAPI.Controllers
{
    [ApiController]
    [Route(Routes.Controller)]
    public class UsersController : ODataController
    {
        #region "Declarations and Constructor"

        private readonly OrganicShopDbContext _context;
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<ShoppingCart> _cartRepository;

        public UsersController(OrganicShopDbContext context,IRepository<AppUser> userRepository, IRepository<ShoppingCart> shoppingCartRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _cartRepository = shoppingCartRepository;
        }

        #endregion

        #region "users API Public Methods"

        [EnableQuery]
        [Route(Routes.All)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IQueryable<AppUser>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Get()
        {
            try
            {
                return Ok(_userRepository.GetAll());
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost(Routes.Add)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AppUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(AppUser user)
        {
            try
            {
                var checkInputErrorMessage = ValidateAppUserInput(user, Constants.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                await _userRepository.Add(user);
                await _context.SaveChangesAsync();
                var dbAppUser = await _userRepository.Get(user.AppUserId);
                return Created("", dbAppUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Update)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AppUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(AppUser user)
        {
            try
            {
                var checkInputErrorMessage = ValidateAppUserInput(user, Constants.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbAppUser = await _userRepository.Get(user.AppUserId);
                if (dbAppUser == null)
                    return NotFound($"{nameof(AppUser)} {nameof(user.AppUserId)} '{user.AppUserId}' {ErrorMessages.DoesNotExist}");

                dbAppUser.AppUserName = user.AppUserName;
                dbAppUser.Email = user.Email;
                dbAppUser.IsAdmin = user.IsAdmin;

                await _context.SaveChangesAsync();
                return Ok(dbAppUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Activate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AppUser))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Activate(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return BadRequest($"{nameof(AppUser)}{nameof(Id) + ErrorMessages.EmptyOrWhiteSpace}");

                var dbAppUser = await _userRepository.Get(Id);
                if (dbAppUser == null)
                    return NotFound($"{nameof(AppUser)} {nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                dbAppUser.IsActive = true;

                await _context.SaveChangesAsync();
                return Ok(dbAppUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Deactivate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AppUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deactivate(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return BadRequest($"{nameof(AppUser)}{nameof(Id) + ErrorMessages.EmptyOrWhiteSpace}");

                var dbAppUser = await _userRepository.Get(Id);
                if (dbAppUser == null)
                    return NotFound($"{nameof(AppUser)} {nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                var shoppingCart = _cartRepository.GetAll()
                                                  .Where(cart => cart.AppUserId == Id)
                                                  .FirstOrDefault();
                if (shoppingCart != null)
                    return BadRequest($"{nameof(AppUser)} with {nameof(Id)} : {Id} {ErrorMessages.PendingItemsInCart}");

                dbAppUser.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(dbAppUser);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region "Validation Methods"
        private string ValidateAppUserInput(AppUser user, Constants.Action action)
        {
            string errorMessage = string.Empty;

            // user parameter validation
            if (user == null)
                return nameof(user) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (!string.IsNullOrWhiteSpace(user.AppUserId) && action == Constants.Action.Update)
                return nameof(user.AppUserId) + ErrorMessages.LessThanEqualToZero;

            // Name validation
            if (string.IsNullOrWhiteSpace(user.AppUserName))
                errorMessage += nameof(user.AppUserName) + ErrorMessages.EmptyOrWhiteSpace;

            // Email validation
            if (string.IsNullOrWhiteSpace(user.Email))
                errorMessage += nameof(user.Email) + ErrorMessages.EmptyOrWhiteSpace;
            // Email validation
            if (!string.IsNullOrWhiteSpace(user.Email) && !user.Email.IsValidEmail())
                errorMessage += $"{nameof(user.Email)} : '{user.Email}' {ErrorMessages.InvalidEmail}";


            return errorMessage;
        }

        #endregion
    }
}
