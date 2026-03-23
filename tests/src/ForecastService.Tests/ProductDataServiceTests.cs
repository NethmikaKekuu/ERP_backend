using Xunit;
using Moq;
using ForecastService.Models;
using ForecastService.Services;
using ForecastService.Repositories;
using Microsoft.Extensions.Logging;

namespace ForecastService.Tests.Services;

public class ProductDataServiceTests
{
    private readonly Mock<ISalesRepository> _mockRepository;
    private readonly Mock<ITimeSeriesAnalyzer> _mockAnalyzer;
    private readonly Mock<ILogger<ProductDataService>> _mockLogger;
    private readonly ProductDataService _service;

    public ProductDataServiceTests()
    {
        _mockRepository = new Mock<ISalesRepository>();
        _mockAnalyzer = new Mock<ITimeSeriesAnalyzer>();
        _mockLogger = new Mock<ILogger<ProductDataService>>();
        _service = new ProductDataService(
            _mockRepository.Object,
            _mockAnalyzer.Object,
            _mockLogger.Object);
    }

    #region GetProductMetricsAsync Tests

    [Fact]
    public async Task GetProductMetricsAsync_WithValidProduct_ReturnsMetricsWithAnalysis()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product",
            SKU = "SKU123",
            CurrentPrice = 100m,
            TotalUnitsSold = 1000,
            TotalRevenue = 100000m,
            OrderCount = 50
        };

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.15m);

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.25m);

        // Act
        var result = await _service.GetProductMetricsAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal("Test Product", result.ProductName);
        Assert.True(result.TrendDirection >= 0);
        Assert.True(result.Volatility >= 0);
        Assert.True(result.SeasonalityIndex >= 0);
    }

    [Fact]
    public async Task GetProductMetricsAsync_WithNonexistentProduct_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync((ProductMetrics?)null);

        // Act
        var result = await _service.GetProductMetricsAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductMetricsAsync_CalculatesTrendDirection()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product"
        };

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.5m); // Positive trend

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.1m);

        // Act
        var result = await _service.GetProductMetricsAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TrendDirection > 0);
    }

    #endregion

    #region GetProductSalesHistoryAsync Tests

    [Fact]
    public async Task GetProductSalesHistoryAsync_WithValidProduct_ReturnsSalesHistory()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var salesHistory = GenerateSalesHistory(productId, 30);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(salesHistory);

        // Act
        var result = await _service.GetProductSalesHistoryAsync(productId, days: 30);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(30, result.Count);
        Assert.All(result, r => Assert.Equal(productId, r.ProductId));
    }

    [Fact]
    public async Task GetProductSalesHistoryAsync_WithEmptyHistory_ReturnsEmptyList()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(new List<SalesData>());

        // Act
        var result = await _service.GetProductSalesHistoryAsync(productId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductSalesHistoryAsync_DefaultDaysIs365()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var salesHistory = GenerateSalesHistory(productId, 365);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, 365))
            .ReturnsAsync(salesHistory);

        // Act
        var result = await _service.GetProductSalesHistoryAsync(productId); // No days specified

        // Assert
        Assert.NotEmpty(result);
    }

    #endregion

    #region GetAllProductMetricsAsync Tests

    [Fact]
    public async Task GetAllProductMetricsAsync_WithMultipleProducts_ReturnsAllMetrics()
    {
        // Arrange
        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var allMetrics = productIds.Select(id => new ProductMetrics
        {
            ProductId = id,
            ProductName = $"Product {id}",
            TotalUnitsSold = 100
        }).ToList();

        _mockRepository
            .Setup(r => r.GetAllProductMetricsAsync())
            .ReturnsAsync(allMetrics);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.1m);

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.2m);

        // Act
        var result = await _service.GetAllProductMetricsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, m => Assert.True(m.TrendDirection >= 0));
    }

    [Fact]
    public async Task GetAllProductMetricsAsync_WithNoProducts_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetAllProductMetricsAsync())
            .ReturnsAsync(new List<ProductMetrics>());

        // Act
        var result = await _service.GetAllProductMetricsAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region AnalyzeProductSalesAsync Tests

    [Fact]
    public async Task AnalyzeProductSalesAsync_WithValidData_ReturnsAnalytics()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product",
            TotalUnitsSold = 1000,
            TotalRevenue = 100000m
        };

        var salesHistory = GenerateSalesHistory(productId, 365);

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(salesHistory);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.1m);

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.3m);

        // Act
        var result = await _service.AnalyzeProductSalesAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.ProductId);
        Assert.Equal("Test Product", result.ProductName);
        Assert.True(result.AvgDailySales > 0);
        Assert.True(result.StandardDeviation >= 0);
        Assert.NotEmpty(result.Trend);
    }

    [Fact]
    public async Task AnalyzeProductSalesAsync_WithNullMetrics_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync((ProductMetrics?)null);

        // Act
        var result = await _service.AnalyzeProductSalesAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyzeProductSalesAsync_WithEmptyHistory_ReturnsNull()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product"
        };

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(new List<SalesData>());

        // Act
        var result = await _service.AnalyzeProductSalesAsync(productId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AnalyzeProductSalesAsync_DetectsSeasonalPattern()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product",
            SeasonalityIndex = 0.35m // Seasonal
        };

        var salesHistory = GenerateSalesHistory(productId, 365);

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(salesHistory);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.1m);

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.35m);

        // Act
        var result = await _service.AnalyzeProductSalesAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SEASONAL", result.SeasonalPattern);
    }

    [Fact]
    public async Task AnalyzeProductSalesAsync_DetectsUpwardTrend()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product"
        };

        var salesHistory = GenerateSalesHistory(productId, 365);

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(salesHistory);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.15m); // Upward trend

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.2m);

        // Act
        var result = await _service.AnalyzeProductSalesAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UPWARD", result.Trend);
        Assert.True(result.GrowthRate > 0 || result.GrowthRate == 0); // May be 0 if not enough data
    }

    [Fact]
    public async Task AnalyzeProductSalesAsync_CalculatesCorrectDateRange()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var metrics = new ProductMetrics
        {
            ProductId = productId,
            ProductName = "Test Product"
        };

        var salesHistory = GenerateSalesHistory(productId, 100);

        _mockRepository
            .Setup(r => r.GetProductMetricsAsync(productId))
            .ReturnsAsync(metrics);

        _mockRepository
            .Setup(r => r.GetProductSalesHistoryAsync(productId, It.IsAny<int>()))
            .ReturnsAsync(salesHistory);

        _mockAnalyzer
            .Setup(a => a.CalculateTrend(It.IsAny<decimal[]>()))
            .Returns(0.1m);

        _mockAnalyzer
            .Setup(a => a.CalculateSeasonality(It.IsAny<decimal[]>(), It.IsAny<int>()))
            .Returns(0.2m);

        // Act
        var result = await _service.AnalyzeProductSalesAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.DaysWithData);
        Assert.True(result.FirstSaleDate <= result.LastSaleDate);
    }

    #endregion

    #region Helper Methods

    private List<SalesData> GenerateSalesHistory(Guid productId, int days)
    {
        var history = new List<SalesData>();
        var random = new Random();
        var baseDate = DateTime.UtcNow.AddDays(-days);

        for (int i = 0; i < days; i++)
        {
            // Add some seasonality - higher sales on weekends
            var dayOfWeek = baseDate.AddDays(i).DayOfWeek;
            var multiplier = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) ? 1.5 : 1.0;

            history.Add(new SalesData
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Date = baseDate.AddDays(i),
                UnitsSold = (int)(random.Next(10, 100) * multiplier),
                Revenue = (decimal)(random.Next(1000, 10000) * multiplier),
                AveragePrice = 100m,
                OrderCount = random.Next(1, 10)
            });
        }

        return history;
    }

    #endregion
}