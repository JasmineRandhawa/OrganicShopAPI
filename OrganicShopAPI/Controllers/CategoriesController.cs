using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using OrganicShopAPI.DataAccess;
using OrganicShopAPI.Models;
using OrganicShopAPI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicShopAPI.Controllers
{
    [ApiController]
    [Route(Routes.Controller)]
    public class CategoriesController : ODataController
    {
        #region "Declarations and Constructor"

        private readonly OrganicShopDbContext _context;
        private readonly IRepository<Category> _categoryRepository;

        public CategoriesController(OrganicShopDbContext context,IRepository<Category> categoryRepository)
        {
            _context = context;
            _categoryRepository = categoryRepository;
        }

        #endregion

        #region "Categories API Public Methods"

        [EnableQuery]
        [Route(Routes.All)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Category>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_categoryRepository.GetAll());
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost(Routes.Add)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Category))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(Category category)
        {
            try
            {
                var checkInputErrorMessage = ValidateCategoryInput(category,Utility.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                await _categoryRepository.Add(category);
                await _context.SaveChangesAsync();
                var dbCategory = await _categoryRepository.Get(category.Id);
                return Created("", dbCategory);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut(Routes.Update)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Category))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Category category)
        {
            try
            {
                var checkInputErrorMessage = ValidateCategoryInput(category, Utility.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbCategory = await _categoryRepository.Get(category.Id);
                if (dbCategory == null)
                    return NotFound($"{nameof(Category)}{nameof(category.Id)} '{category.Id}' {ErrorMessages.DoesNotExist}");

                dbCategory.Name = category.Name;
                dbCategory.IsActive = category.IsActive;

                await _context.SaveChangesAsync();
                return Ok(dbCategory);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Activate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Category))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Activate(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(Category)}{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbCategory = await _categoryRepository.Get(Id);
                if (dbCategory == null)
                    return NotFound($"{nameof(Category)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                dbCategory.IsActive = true;

                await _context.SaveChangesAsync();
                return Ok(dbCategory);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Deactivate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Category))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deactivate(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(Category)}{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbCategory = await _categoryRepository.Get(Id);
                if (dbCategory == null)
                    return NotFound($"{nameof(Category)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                dbCategory.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(dbCategory);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region "Validation Methods"
        private string ValidateCategoryInput(Category category, Utility.Action action)
        {
            string errorMessage = string.Empty;

            // category parameter validation
            if (category == null)
                return nameof(category) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (category.Id <= 0 && action == Utility.Action.Update)
                return nameof(category.Id) + ErrorMessages.LessThenZero;

            // Name validation
            if (string.IsNullOrWhiteSpace(category.Name))
                errorMessage += nameof(category.Name) + ErrorMessages.EmptyOrWhiteSpace;

            return errorMessage;
        }

        #endregion
    }
}
