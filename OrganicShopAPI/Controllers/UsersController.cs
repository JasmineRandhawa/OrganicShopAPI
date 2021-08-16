using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrganicShopAPI.DataAccess;
using OrganicShopAPI.Models;
using OrganicShopAPI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace OrganicShopAPI.Controllers
{
    [ApiController]
    [Route(Routes.Controller)]
    public class UsersController : ControllerBase
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

        [HttpGet]
        [Route(Routes.All)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AppUser>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            try
            {
                var users = _userRepository.GetAll();

                return (users != null && users.Count() > 0) ? Ok(users.ToList()) : NoContent();
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet(Routes.Id)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AppUser))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(nameof(Id) + ErrorMessages.LessThenZero);

                var user = await _userRepository.Get(Id);
                if(user != null)
                    return Ok(user);

                return NotFound($"{nameof(AppUser)} {nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");
            }
            catch (Exception)
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
                var checkInputErrorMessage = ValidateAppUserInput(user,Utility.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                await _userRepository.Add(user);
                await _context.SaveChangesAsync();
                var dbAppUser = await _userRepository.Get(user.Id);
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
                var checkInputErrorMessage = ValidateAppUserInput(user, Utility.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbAppUser = await _userRepository.Get(user.Id);
                if (dbAppUser == null)
                    return NotFound($"{nameof(AppUser)} {nameof(user.Id)} '{user.Id}' {ErrorMessages.DoesNotExist}");

                dbAppUser.Name = user.Name;
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
        public async Task<IActionResult> Activate(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(AppUser)}{nameof(Id) + ErrorMessages.LessThenZero}");

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
        public async Task<IActionResult> Deactivate(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(AppUser)} {nameof(Id) + ErrorMessages.LessThenZero}");

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
        private string ValidateAppUserInput(AppUser user, Utility.Action action)
        {
            string errorMessage = string.Empty;

            // user parameter validation
            if (user == null)
                return nameof(user) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (user.Id <= 0 && action == Utility.Action.Update)
                return nameof(user.Id) + ErrorMessages.LessThenZero;

            // Name validation
            if (string.IsNullOrWhiteSpace(user.Name))
                errorMessage += nameof(user.Name) + ErrorMessages.EmptyOrWhiteSpace;

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
