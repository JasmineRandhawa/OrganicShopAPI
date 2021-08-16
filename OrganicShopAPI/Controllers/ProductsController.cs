using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

using OrganicShopAPI.Constants;
using OrganicShopAPI.DataAccess;
using OrganicShopAPI.Models;
using OrganicShopAPI.Utility;

namespace OrganicShopAPI.Controllers
{
    [ApiController]
    [Route(Routes.Controller)]
    public class ProductsController : ODataController
    {
        #region "Declarations and Constructor"

        private readonly OrganicShopDbContext _context;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;

        public ProductsController(OrganicShopDbContext context, IRepository<Product> productRepository
                                  , IRepository<Category> categoryRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        #endregion

        #region "Products API Public Methods"

        [EnableQuery]
        [Route(Routes.All)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_productRepository.GetAll());
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost(Routes.Add)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Add(Product product)
        {
            try
            {
                var checkInputErrorMessage = await ValidateProductInputAsync(product, Constants.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                await _productRepository.Add(product);
                await _context.SaveChangesAsync();
                var dbProduct = await GetProductById(product.Id);
                return Created("", dbProduct);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut(Routes.Update)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Product product)
        {
            try
            {
                var checkInputErrorMessage = await ValidateProductInputAsync(product, Constants.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbProduct = await GetProductById(product.Id);
                if (dbProduct == null)
                    return NotFound($"{nameof(Product)}{nameof(product.Id)} '{product.Id}' {ErrorMessages.DoesNotExist}");

                dbProduct.Title = product.Title;
                dbProduct.ImageURL = product.ImageURL;
                dbProduct.CategoryId = product.CategoryId;

                await _context.SaveChangesAsync();
                return Ok(dbProduct);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Activate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Activate(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(Product)}{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbProduct = await GetProductById(Id);
                if (dbProduct == null)
                    return NotFound($"{nameof(Product)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                dbProduct.IsActive = true;
                await _context.SaveChangesAsync();
                return Ok(dbProduct);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch(Routes.Deactivate)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deactivate(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"{nameof(Product)}{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbProduct = await GetProductById(Id);
                if (dbProduct == null)
                    return NotFound($"{nameof(Product)}{nameof(Id)} '{Id}' {ErrorMessages.DoesNotExist}");

                dbProduct.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(dbProduct);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region "Validation Methods"
        private async Task<string> ValidateProductInputAsync(Product product, Constants.Action action)
        {
            string errorMessage = string.Empty;

            // product parameter validation
            if (product==null)
                return nameof(product) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (product.Id <= 0 && action == Constants.Action.Update)
                return nameof(product.Id) + ErrorMessages.LessThenZero;

            // Title validation
            if (string.IsNullOrWhiteSpace(product.Title))
                errorMessage += nameof(product.Title) + ErrorMessages.EmptyOrWhiteSpace;

            // ImageURL validations
            if (string.IsNullOrWhiteSpace(product.ImageURL))
                errorMessage += nameof(product.ImageURL) + ErrorMessages.EmptyOrWhiteSpace;
            if (!string.IsNullOrWhiteSpace(product.ImageURL) && !product.ImageURL.IsURLValid())
                errorMessage += nameof(product.ImageURL) + ErrorMessages.InvalidURL;

            // CategoryId validations
            if (product.CategoryId < 0 )
                errorMessage += nameof(product.CategoryId) + ErrorMessages.EmptyOrWhiteSpace;;
            if (product.CategoryId > 0 && await _categoryRepository.Get(product.CategoryId) == null)
                errorMessage += $" {nameof(product.CategoryId)} '{product.CategoryId}' {ErrorMessages.DoesNotExist}";

            return errorMessage;
        }

        #endregion

        #region "Products Private Methods"

        private async Task<Product> GetProductById(int Id)
        {
            return await _productRepository.GetAll()
                                                 .Where(product => product.Id == Id)
                                                 .Include(product => product.Category)
                                                 .FirstOrDefaultAsync();
        }
        #endregion
    }
}
