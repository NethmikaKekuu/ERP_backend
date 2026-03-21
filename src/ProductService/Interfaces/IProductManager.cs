using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductService.DTOs;
using ProductService.Common;

namespace ProductService.Interfaces
{
    public interface IProductManager
    {
        // ── Read ──────────────────────────────────────────────────────────────
        Task<PaginatedResponse<ProductResponseDto>> GetProductsAsync(Guid? userId, int pageNumber, int pageSize, string? categoryName, string? name);
        Task<ProductResponseDto?> GetProductByIdAsync(Guid? userId, Guid id);

        // ── Write ─────────────────────────────────────────────────────────────
<<<<<<< Updated upstream
        Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto, Guid createdByUserId);
        Task<ProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(Guid id);
=======
        Task<ProductResponseDto> CreateProductAsync(Guid? userId, CreateProductDto dto);
        Task<ProductResponseDto?> UpdateProductAsync(Guid? userId, Guid id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(Guid? userId, Guid id);
>>>>>>> Stashed changes

        // ── Inventory ─────────────────────────────────────────────────────────
        /// <summary>Returns stock information for all active products (with QuantityAvailable count).</summary>
        Task<IEnumerable<StockResponseDto>> GetStockAsync(Guid? userId);

        /// <summary>Returns stock information for a single product.</summary>
        Task<StockResponseDto?> GetStockByProductIdAsync(Guid? userId, Guid productId);

        /// <summary>
        /// Deducts <paramref name="quantity"/> units from a product's available stock when
        /// an order is placed. Creates an InventoryReservation record and fires a low-stock
        /// alert if the threshold is crossed. Returns false when stock is insufficient.
        /// </summary>
        Task<(bool Success, string Message)> DeductStockAsync(Guid? userId, DeductStockDto dto);
    }
}
