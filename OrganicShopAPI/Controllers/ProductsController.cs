using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class ProductsController : ControllerBase
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

        [HttpGet]
        [Route(Routes.All)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetAll()
        {
            try
            {
                var products = _productRepository.GetAll()
                                                 .ToList();
                                                 
                if (products != null)
                    return products.Count > 0 ? Ok(products.OrderByDescending(product => product.IsActive)) 
                                              : NoContent();

                return NotFound();
            }
            catch(Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpGet(Routes.Id)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAsync(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest(nameof(Id) + ErrorMessages.LessThenZero);

                var product = await _productRepository.Get(Id);
                if(product!=null)
                    return Ok(product);

                return NotFound($" Product{nameof(Id)} : {Id + ErrorMessages.DoesNotExist}");
            }
            catch (Exception)
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
                var checkInputErrorMessage = await ValidateProductInputAsync(product,Utility.Action.Add);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                await _productRepository.Add(product);
                await _context.SaveChangesAsync();
                var dbProduct = await _productRepository.Get(product.Id);
                return Created("", dbProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut(Routes.Update)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(Product product)
        {
            try
            {
                var checkInputErrorMessage = await ValidateProductInputAsync(product, Utility.Action.Update);

                if (!string.IsNullOrWhiteSpace(checkInputErrorMessage))
                    return BadRequest(checkInputErrorMessage);

                var dbProduct = await _productRepository.Get(product.Id);
                if (dbProduct == null)
                    return NotFound($" Product{nameof(product.Id)} : {product.Id + ErrorMessages.DoesNotExist}");

                dbProduct.Title = product.Title;
                dbProduct.ImageURL = product.ImageURL;
                dbProduct.CategoryId = product.CategoryId;
                dbProduct.IsActive = product.IsActive;

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
        public async Task<IActionResult> ActivateAsync(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"Product{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbProduct = await _productRepository.Get(Id);

                dbProduct.IsActive = true;

                if (dbProduct == null)
                    return NotFound($"Product{nameof(Id)} : {Id + ErrorMessages.DoesNotExist}");

                dbProduct.CategoryId = dbProduct.CategoryId;
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
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest($"Product{nameof(Id) + ErrorMessages.LessThenZero}");

                var dbProduct = await _productRepository.Get(Id);

                dbProduct.IsActive = false;

                if (dbProduct == null)
                    return NotFound($"Product{nameof(Id)} : {Id + ErrorMessages.DoesNotExist}");

                dbProduct.CategoryId = dbProduct.CategoryId;
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
        private async Task<string> ValidateProductInputAsync(Product product, Utility.Action action)
        {
            string errorMessage = string.Empty;

            // product parameter validation
            if (product==null)
                return nameof(product) + ErrorMessages.NullParameter;

            // Id validation on update operation
            if (product.Id <= 0 && action == Utility.Action.Update)
                return nameof(product.Id) + ErrorMessages.LessThenZero;

            // Title validation
            if (string.IsNullOrWhiteSpace(product.Title))
                errorMessage += nameof(product.Title) + ErrorMessages.EmptyOrWhiteSpace;

            // ImageURL validations
            if (string.IsNullOrWhiteSpace(product.ImageURL))
                errorMessage += nameof(product.ImageURL) + ErrorMessages.EmptyOrWhiteSpace;
            if (product.ImageURL.CheckURLValid())
                errorMessage += nameof(product.ImageURL) + ErrorMessages.InvalidURL;

            // CategoryId validations
            if (product.CategoryId < 0 )
                errorMessage += nameof(product.CategoryId) + ErrorMessages.EmptyOrWhiteSpace;;
            if (product.CategoryId > 0 && await _categoryRepository.Get(product.CategoryId) == null)
                errorMessage += $" {nameof(product.CategoryId)} : {product.CategoryId + ErrorMessages.DoesNotExist}";

            return errorMessage;
        }

        #endregion
    }
}
