using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Interfaces;
using ProductService.Models;
using ProductService.Common;

namespace ProductService.Services
{
    public class ProductManager : IProductManager
    {
        private readonly ProductDbContext _context;

        public ProductManager(ProductDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────────────────
        //  READ
        // ────────────────────────────────────────────────────────────────────

        public async Task<PaginatedResponse<ProductResponseDto>> GetProductsAsync(
            Guid? userId, int pageNumber, int pageSize, string? categoryName, string? name)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(p => p.Category != null && p.Category.Name.ToLower() == categoryName.ToLower());

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));

            var totalRecords = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => MapToProductResponse(p))
                .ToListAsync();

            return new PaginatedResponse<ProductResponseDto>
            {
                Data = products,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid? userId, Guid id)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            var p = await query.FirstOrDefaultAsync(p => p.Id == id);

            return p == null ? null : MapToProductResponse(p);
        }

        // ────────────────────────────────────────────────────────────────────
        //  CREATE
        // ────────────────────────────────────────────────────────────────────

<<<<<<< Updated upstream
        public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto, Guid createdByUserId)
=======
        public async Task<ProductResponseDto> CreateProductAsync(Guid? userId, CreateProductDto dto)
>>>>>>> Stashed changes
        {
            // First check if the SKU already exists
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Sku == dto.Sku);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"A product with SKU '{dto.Sku}' already exists.");
            }

            var product = new Product
            {
<<<<<<< Updated upstream
                Sku             = dto.Sku,
                Name            = dto.Name,
                Description     = dto.Description,
                CategoryId      = dto.CategoryId,
                Price           = dto.Price,
                IsActive        = dto.IsActive,
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow,
                CreatedByUserId = createdByUserId,
                QuantityAvailable = dto.InitialStock
=======
                Sku         = dto.Sku,
                Name        = dto.Name,
                Description = dto.Description,
                CategoryId  = dto.CategoryId,
                Price       = dto.Price,
                IsActive    = dto.IsActive,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow,
                CreatedByUserId = userId
>>>>>>> Stashed changes
            };

            _context.Products.Add(product);

            // Create the corresponding Inventory record in the same transaction
            var inventory = new Inventory
            {
                ProductId          = product.Id,
                QuantityAvailable  = dto.InitialStock,
                QuantityReserved   = 0,
                LowStockThreshold  = dto.LowStockThreshold,
                UpdatedAt          = DateTime.UtcNow
            };

            _context.Inventory.Add(inventory);
            await _context.SaveChangesAsync();

            // Load category for the response
            if (product.CategoryId.HasValue)
                await _context.Entry(product).Reference(x => x.Category).LoadAsync();

            product.Inventory = inventory;

            return MapToProductResponse(product);
        }

        // ────────────────────────────────────────────────────────────────────
        //  UPDATE
        // ────────────────────────────────────────────────────────────────────

        public async Task<ProductResponseDto?> UpdateProductAsync(Guid? userId, Guid id, UpdateProductDto dto)
        {
            var existingProductWithSameSku = await _context.Products.FirstOrDefaultAsync(p => p.Sku == dto.Sku && p.Id != id);
            if (existingProductWithSameSku != null)
            {
                throw new InvalidOperationException($"A product with SKU '{dto.Sku}' already exists.");
            }

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Inventory)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            var p = await query.FirstOrDefaultAsync(p => p.Id == id);

            if (p == null) return null;

            p.Sku         = dto.Sku;
            p.Name        = dto.Name;
            p.Description = dto.Description;
            p.CategoryId  = dto.CategoryId;
            p.Price       = dto.Price;
            p.IsActive    = dto.IsActive;
            p.UpdatedAt   = DateTime.UtcNow;
            p.QuantityAvailable = dto.QuantityAvailable;

            if (p.Inventory != null)
            {
                p.Inventory.QuantityAvailable = dto.QuantityAvailable;
                p.Inventory.LowStockThreshold = dto.LowStockThreshold;
                p.Inventory.UpdatedAt         = DateTime.UtcNow;
            }
            else
            {
                p.Inventory = new Inventory
                {
                    ProductId         = id,
                    QuantityAvailable = dto.QuantityAvailable,
                    LowStockThreshold = dto.LowStockThreshold
                };
                _context.Inventory.Add(p.Inventory);
            }

            // Raise low-stock alert if needed
            if (p.Inventory.IsLowStock)
                await EnsureLowStockAlertAsync(id, p.Inventory.QuantityAvailable);

            await _context.SaveChangesAsync();

            if (p.CategoryId.HasValue)
                await _context.Entry(p).Reference(x => x.Category).LoadAsync();

            return MapToProductResponse(p);
        }

        // ────────────────────────────────────────────────────────────────────
        //  DELETE
        // ────────────────────────────────────────────────────────────────────

        public async Task<bool> DeleteProductAsync(Guid? userId, Guid id)
        {
            var query = _context.Products.AsQueryable();
            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            var product = await query.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        // ────────────────────────────────────────────────────────────────────
        //  INVENTORY / STOCK
        // ────────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<StockResponseDto>> GetStockAsync(Guid? userId)
        {
            var query = _context.Products
                .Include(p => p.Inventory)
                .Where(p => p.IsActive);

            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            var stocks = await query
                .OrderBy(p => p.Name)
                .Select(p => MapToStockResponse(p))
                .ToListAsync();

            return stocks;
        }

        public async Task<StockResponseDto?> GetStockByProductIdAsync(Guid? userId, Guid productId)
        {
            var query = _context.Products
                .Include(p => p.Inventory)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            var p = await query.FirstOrDefaultAsync(p => p.Id == productId);

            return p == null ? null : MapToStockResponse(p);
        }

        public async Task<(bool Success, string Message)> DeductStockAsync(Guid? userId, DeductStockDto dto)
        {
            var query = _context.Products.AsQueryable();
            if (userId.HasValue)
                query = query.Where(p => p.CreatedByUserId == userId.Value);

            var product = await query.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
                return (false, $"Product {dto.ProductId} not found or you do not have permission.");

            // Load inventory
            var inventory = await _context.Inventory
                .FirstOrDefaultAsync(i => i.ProductId == dto.ProductId);

            if (inventory == null)
                return (false, $"No inventory record found for product {dto.ProductId}.");

            if (inventory.QuantityAvailable < dto.Quantity)
                return (false, $"Insufficient stock. Available: {inventory.QuantityAvailable}, Requested: {dto.Quantity}.");

            // Deduct stock in Inventory
            inventory.QuantityAvailable -= dto.Quantity;
            inventory.UpdatedAt          = DateTime.UtcNow;
            
            // Deduct stock in Product table natively
            var productForReduction = await _context.Products.FindAsync(dto.ProductId);
            if (productForReduction != null)
                productForReduction.QuantityAvailable = inventory.QuantityAvailable;

            // Record the reservation / deduction
            var reservation = new InventoryReservation
            {
                ProductId  = dto.ProductId,
                OrderId    = dto.OrderId,
                Quantity   = dto.Quantity,
                Status     = "DEDUCTED",
                ReservedAt = DateTime.UtcNow
            };
            _context.InventoryReservations.Add(reservation);

            // Fire low-stock alert if the threshold is now breached
            if (inventory.IsLowStock)
                await EnsureLowStockAlertAsync(dto.ProductId, inventory.QuantityAvailable);

            await _context.SaveChangesAsync();

            return (true, $"Stock deducted successfully. Remaining: {inventory.QuantityAvailable}.");
        }

        // ────────────────────────────────────────────────────────────────────
        //  PRIVATE HELPERS
        // ────────────────────────────────────────────────────────────────────

        private async Task EnsureLowStockAlertAsync(Guid productId, int currentQty)
        {
            var existing = await _context.LowStockAlerts
                .FirstOrDefaultAsync(a => a.ProductId == productId && !a.IsResolved);

            if (existing == null)
            {
                _context.LowStockAlerts.Add(new LowStockAlert
                {
                    ProductId       = productId,
                    QuantityAtAlert = currentQty
                });
            }
        }

        private static ProductResponseDto MapToProductResponse(Product p) => new()
        {
            Id                = p.Id,
            Sku               = p.Sku,
            Name              = p.Name,
            Description       = p.Description,
            CategoryId        = p.CategoryId,
            CategoryName      = p.Category?.Name,
            Price             = p.Price,
            IsActive          = p.IsActive,
            QuantityAvailable = p.QuantityAvailable,
            QuantityReserved  = p.Inventory?.QuantityReserved  ?? 0,
            IsLowStock        = p.Inventory?.IsLowStock        ?? false,
            CreatedByUserId   = p.CreatedByUserId
        };

        private static StockResponseDto MapToStockResponse(Product p) => new()
        {
            ProductId         = p.Id,
            Sku               = p.Sku,
            Name              = p.Name,
            QuantityAvailable = p.Inventory?.QuantityAvailable ?? 0,
            QuantityReserved  = p.Inventory?.QuantityReserved  ?? 0,
            LowStockThreshold = p.Inventory?.LowStockThreshold ?? 0,
            IsLowStock        = p.Inventory?.IsLowStock        ?? false
        };
    }
}
