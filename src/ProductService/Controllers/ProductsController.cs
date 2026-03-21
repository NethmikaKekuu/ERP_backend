using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Interfaces;
using ProductService.Common;

namespace ProductService.Controllers
{
    /// <summary>
    /// Manages products and inventory stock within the ERP system.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductManager _productManager;

        public ProductsController(IProductManager productManager)
        {
            _productManager = productManager;
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out var userId))
            {
                return userId;
            }
            return null;
        }

        // ────────────────────────────────────────────────────────────────────
        //  GET api/products  — paginated product list with optional filters
        // ────────────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(PaginatedResponse<ProductResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize   = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? name     = null)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            var userId = GetUserId();
            var result = await _productManager.GetProductsAsync(userId, pageNumber, pageSize, category, name);
            return Ok(result);
        }

        // ────────────────────────────────────────────────────────────────────
        //  GET api/products/search/{name}
        // ────────────────────────────────────────────────────────────────────

        /// <summary>Searches for products strictly by item name (partial match).</summary>
        /// <param name="name">The name or partial name to search for.</param>
        [HttpGet("search/{name}")]
        [Authorize(Roles = "Admin,Employee")]
        [ProducesResponseType(typeof(PaginatedResponse<ProductResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> SearchProductsByName(string name)
        {
            var result = await _productManager.GetProductsAsync(1, 100, null, name);
            return Ok(result);
        }

        // ────────────────────────────────────────────────────────────────────
        //  GET api/products/{id}
        // ────────────────────────────────────────────────────────────────────

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var userId = GetUserId();
            var product = await _productManager.GetProductByIdAsync(userId, id);
            if (product == null) return NotFound(new { message = $"Product {id} not found." });
            return Ok(product);
        }

        // ────────────────────────────────────────────────────────────────────
        //  POST api/products  — CREATE a new product
        // ────────────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(ProductResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

<<<<<<< Updated upstream
            // Extract the userId from the JWT 'sub' claim (mapped to NameIdentifier by ASP.NET Core)
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var createdByUserId))
                return Unauthorized(new { message = "Unable to identify the requesting user from the token." });

            var created = await _productManager.CreateProductAsync(dto, createdByUserId);
            return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
=======
            var userId = GetUserId();
            try
            {
                var created = await _productManager.CreateProductAsync(userId, dto);
                return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
>>>>>>> Stashed changes
        }

        // ────────────────────────────────────────────────────────────────────
        //  PUT api/products/{id}  — UPDATE a product
        // ────────────────────────────────────────────────────────────────────

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            try
            {
                var updated = await _productManager.UpdateProductAsync(userId, id, dto);
                if (updated == null) return NotFound(new { message = $"Product {id} not found." });
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // ────────────────────────────────────────────────────────────────────
        //  DELETE api/products/{id}  — DELETE a product
        // ────────────────────────────────────────────────────────────────────

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var userId = GetUserId();
            var deleted = await _productManager.DeleteProductAsync(userId, id);
            if (!deleted) return NotFound(new { message = $"Product {id} not found." });
            return NoContent();
        }

        // ────────────────────────────────────────────────────────────────────
        //  GET api/products/stock  — VIEW current stock for all products
        // ────────────────────────────────────────────────────────────────────

        [HttpGet("stock")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<StockResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStock()
        {
            var userId = GetUserId();
            var stocks = await _productManager.GetStockAsync(userId);
            return Ok(stocks);
        }

        // ────────────────────────────────────────────────────────────────────
        //  GET api/products/{id}/stock  — VIEW stock for a single product
        // ────────────────────────────────────────────────────────────────────

        [HttpGet("{id:guid}/stock")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(StockResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStockByProductId(Guid id)
        {
            var userId = GetUserId();
            var stock = await _productManager.GetStockByProductIdAsync(userId, id);
            if (stock == null) return NotFound(new { message = $"Product {id} not found." });
            return Ok(stock);
        }

        // ────────────────────────────────────────────────────────────────────
        //  POST api/products/deduct-stock  — DEDUCT stock on order placement
        // ────────────────────────────────────────────────────────────────────

        [HttpPost("deduct-stock")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> DeductStock([FromBody] DeductStockDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserId();
            var (success, message) = await _productManager.DeductStockAsync(userId, dto);

            if (!success)
                return Conflict(new { message });

            return Ok(new { message });
        }
    }
}
