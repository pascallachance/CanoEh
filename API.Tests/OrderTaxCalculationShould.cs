using Domain.Models.Requests;
using Domain.Models.Responses;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Helpers.Common;
using Infrastructure.Data;
using Infrastructure.Repositories.Interfaces;
using Moq;

namespace API.Tests
{
    public class OrderTaxCalculationShould
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderItemRepository> _mockOrderItemRepository;
        private readonly Mock<IOrderAddressRepository> _mockOrderAddressRepository;
        private readonly Mock<IOrderPaymentRepository> _mockOrderPaymentRepository;
        private readonly Mock<IOrderStatusRepository> _mockOrderStatusRepository;
        private readonly Mock<IItemRepository> _mockItemRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITaxRatesService> _mockTaxRatesService;
        private readonly string _connectionString;

        public OrderTaxCalculationShould()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockOrderItemRepository = new Mock<IOrderItemRepository>();
            _mockOrderAddressRepository = new Mock<IOrderAddressRepository>();
            _mockOrderPaymentRepository = new Mock<IOrderPaymentRepository>();
            _mockOrderStatusRepository = new Mock<IOrderStatusRepository>();
            _mockItemRepository = new Mock<IItemRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTaxRatesService = new Mock<ITaxRatesService>();
            _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CanoEhTest;Trusted_Connection=true;TrustServerCertificate=true;";
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldUseCorrectTaxRates_WhenTaxRatesFoundForBillingAddress()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var orderStatus = new OrderStatus
            {
                ID = 1,
                StatusCode = "Pending",
                Name_en = "Pending",
                Name_fr = "En attente"
            };

            var item = new Item
            {
                Id = itemId,
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Id = variantId,
                        ItemVariantName_en = "Test Variant",
                        ItemVariantName_fr = "Variante de test",
                        Price = 100.00m,
                        StockQuantity = 5
                    }
                }
            };

            var createRequest = new CreateOrderRequest
            {
                OrderItems = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ItemID = itemId,
                        ItemVariantID = variantId,
                        Quantity = 1
                    }
                },
                ShippingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Toronto",
                    ProvinceState = "ON",
                    PostalCode = "M5V 3A8",
                    Country = "Canada"
                },
                BillingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Toronto",
                    ProvinceState = "ON", 
                    PostalCode = "M5V 3A8",
                    Country = "Canada"
                },
                Payment = new CreateOrderPaymentRequest
                {
                    PaymentMethodID = Guid.NewGuid(),
                    Provider = "Credit Card"
                }
            };

            // Setup mock tax rates (HST: 5% GST + 8% PST = 13% total)
            var taxRates = new List<GetTaxRateResponse>
            {
                new GetTaxRateResponse
                {
                    ID = Guid.NewGuid(),
                    Name_en = "GST",
                    Name_fr = "TPS",
                    Country = "Canada",
                    ProvinceState = "ON",
                    Rate = 0.05m,
                    IsActive = true
                },
                new GetTaxRateResponse
                {
                    ID = Guid.NewGuid(),
                    Name_en = "PST",
                    Name_fr = "TVP",
                    Country = "Canada",
                    ProvinceState = "ON",
                    Rate = 0.08m,
                    IsActive = true
                }
            };

            // Setup mocks
            _mockUserRepository.Setup(x => x.ExistsAsync(userId)).ReturnsAsync(true);
            _mockOrderStatusRepository.Setup(x => x.FindByStatusCodeAsync("Pending")).ReturnsAsync(orderStatus);
            _mockItemRepository.Setup(x => x.GetByIdAsync(itemId)).ReturnsAsync(item);
            _mockTaxRatesService.Setup(x => x.GetTaxRatesByLocationAsync("Canada", "ON"))
                .ReturnsAsync(Result.Success(taxRates.AsEnumerable()));

            var orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockOrderItemRepository.Object,
                _mockOrderAddressRepository.Object,
                _mockOrderPaymentRepository.Object,
                _mockOrderStatusRepository.Object,
                _mockItemRepository.Object,
                _mockUserRepository.Object,
                _mockTaxRatesService.Object,
                _connectionString);

            // Act
            var result = await orderService.CreateOrderAsync(userId, createRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            
            // Verify tax calculation: $100 * 13% = $13
            Assert.Equal(100.00m, result.Value.Subtotal);
            Assert.Equal(13.00m, result.Value.TaxTotal);
            Assert.Equal(123.00m, result.Value.GrandTotal); // $100 + $13 + $10 shipping - $10 (free shipping over $50) = $113
            
            // Verify the tax service was called with the correct billing address
            _mockTaxRatesService.Verify(x => x.GetTaxRatesByLocationAsync("Canada", "ON"), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldUseZeroTax_WhenNoTaxRatesFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();

            var orderStatus = new OrderStatus
            {
                ID = 1,
                StatusCode = "Pending",
                Name_en = "Pending",
                Name_fr = "En attente"
            };

            var item = new Item
            {
                Id = itemId,
                Name_en = "Test Item",
                Name_fr = "Article de test",
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Id = variantId,
                        ItemVariantName_en = "Test Variant",
                        ItemVariantName_fr = "Variante de test",
                        Price = 100.00m,
                        StockQuantity = 5
                    }
                }
            };

            var createRequest = new CreateOrderRequest
            {
                OrderItems = new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest
                    {
                        ItemID = itemId,
                        ItemVariantID = variantId,
                        Quantity = 1
                    }
                },
                ShippingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Unknown City",
                    ProvinceState = "Unknown State",
                    PostalCode = "12345",
                    Country = "Unknown Country"
                },
                BillingAddress = new CreateOrderAddressRequest
                {
                    FullName = "John Doe",
                    AddressLine1 = "123 Main St",
                    City = "Unknown City",
                    ProvinceState = "Unknown State",
                    PostalCode = "12345",
                    Country = "Unknown Country"
                },
                Payment = new CreateOrderPaymentRequest
                {
                    PaymentMethodID = Guid.NewGuid(),
                    Provider = "Credit Card"
                }
            };

            // Setup mocks - return empty tax rates
            _mockUserRepository.Setup(x => x.ExistsAsync(userId)).ReturnsAsync(true);
            _mockOrderStatusRepository.Setup(x => x.FindByStatusCodeAsync("Pending")).ReturnsAsync(orderStatus);
            _mockItemRepository.Setup(x => x.GetByIdAsync(itemId)).ReturnsAsync(item);
            _mockTaxRatesService.Setup(x => x.GetTaxRatesByLocationAsync("Unknown Country", "Unknown State"))
                .ReturnsAsync(Result.Success(Enumerable.Empty<GetTaxRateResponse>()));

            var orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockOrderItemRepository.Object,
                _mockOrderAddressRepository.Object,
                _mockOrderPaymentRepository.Object,
                _mockOrderStatusRepository.Object,
                _mockItemRepository.Object,
                _mockUserRepository.Object,
                _mockTaxRatesService.Object,
                _connectionString);

            // Act
            var result = await orderService.CreateOrderAsync(userId, createRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            
            // Verify no tax applied when no tax rates found
            Assert.Equal(100.00m, result.Value.Subtotal);
            Assert.Equal(0.00m, result.Value.TaxTotal);
            Assert.Equal(100.00m, result.Value.GrandTotal); // $100 + $0 tax + $0 shipping (free over $50)
            
            // Verify the tax service was called with the correct billing address
            _mockTaxRatesService.Verify(x => x.GetTaxRatesByLocationAsync("Unknown Country", "Unknown State"), Times.Once);
        }
    }
}