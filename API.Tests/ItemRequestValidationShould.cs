using Domain.Models.Requests;
using Helpers.Common;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace API.Tests
{
    public class ItemRequestValidationShould
    {
        // ---------------------------------------------------------------
        // CreateItemRequest
        // ---------------------------------------------------------------

        [Fact]
        public void CreateItemRequest_ReturnFailure_WhenNameEnIsEmpty()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid()
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("English name is required.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateItemRequest_ReturnSuccess_WhenNameEnExceeds255Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = new string('A', 256),
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid()
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateItemRequest_ReturnSuccess_WhenNameFrExceeds255Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = new string('A', 256),
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid()
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateItemRequest_ReturnSuccess_WhenNameEnIsExactly255Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = new string('A', 255),
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid()
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateItemRequest_ReturnFailure_WhenVariantSkuExceeds100Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>
                {
                    new CreateItemVariantRequest
                    {
                        Sku = new string('X', 101),
                        Price = 10.0m
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("SKU cannot exceed 100 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateItemRequest_ReturnFailure_WhenVariantProductIdentifierValueExceeds100Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>
                {
                    new CreateItemVariantRequest
                    {
                        Sku = "SKU-001",
                        Price = 10.0m,
                        ProductIdentifierValue = new string('X', 101)
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Product identifier value cannot exceed 100 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void CreateItemRequest_ReturnFailure_WhenVariantAttributeNameEnExceeds255Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<CreateItemVariantRequest>
                {
                    new CreateItemVariantRequest
                    {
                        Sku = "SKU-001",
                        Price = 10.0m,
                        ItemVariantAttributes = new List<CreateItemVariantAttributeRequest>
                        {
                            new CreateItemVariantAttributeRequest
                            {
                                AttributeName_en = new string('A', 256),
                                Attributes_en = "Value"
                            }
                        }
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Attribute name (English) cannot exceed 255 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        // ---------------------------------------------------------------
        // UpdateItemRequest
        // ---------------------------------------------------------------

        [Fact]
        public void UpdateItemRequest_ReturnSuccess_WhenNameEnExceeds255Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = new string('A', 256),
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid()
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateItemRequest_ReturnSuccess_WhenNameFrExceeds255Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = new string('A', 256),
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid()
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenVariantSkuExceeds100Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant { Sku = new string('X', 101) }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("SKU cannot exceed 100 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenVariantProductIdentifierTypeExceeds50Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ProductIdentifierType = new string('X', 51)
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Product identifier type cannot exceed 50 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenVariantProductIdentifierValueExceeds100Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ProductIdentifierValue = new string('X', 101)
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Product identifier value cannot exceed 100 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenVariantNameEnExceeds255Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ItemVariantName_en = new string('A', 256)
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Variant name (English) cannot exceed 255 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenVariantAttributeNameEnExceeds255Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ItemVariantAttributes = new List<ItemVariantAttribute>
                        {
                            new ItemVariantAttribute
                            {
                                AttributeName_en = new string('A', 256),
                                Attributes_en = "Value"
                            }
                        }
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Attribute name (English) cannot exceed 255 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenVariantFeatureNameEnExceeds255Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ItemVariantFeatures = new List<ItemVariantFeatures>
                        {
                            new ItemVariantFeatures
                            {
                                AttributeName_en = new string('A', 256),
                                Attributes_en = "Value"
                            }
                        }
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Feature name (English) cannot exceed 255 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnSuccess_WhenAllFieldsAreValid()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ItemVariantName_en = new string('A', 255),
                        ItemVariantAttributes = new List<ItemVariantAttribute>
                        {
                            new ItemVariantAttribute
                            {
                                AttributeName_en = new string('B', 255),
                                Attributes_en = "Value"
                            }
                        }
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        // ---------------------------------------------------------------
        // CreateItemVariantRequest
        // ---------------------------------------------------------------

        [Fact]
        public void CreateItemRequest_ReturnSuccess_WhenVariantsIsNull()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = null!
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateItemRequest_ReturnFailure_WhenTopLevelFeatureNameEnExceeds255Characters()
        {
            var request = new CreateItemRequest
            {
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                ItemVariantFeatures = new List<CreateItemVariantFeaturesRequest>
                {
                    new CreateItemVariantFeaturesRequest
                    {
                        AttributeName_en = new string('A', 256),
                        Attributes_en = "Value"
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Feature name (English) cannot exceed 255 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnSuccess_WhenVariantsIsNull()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = null!
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateItemRequest_ReturnSuccess_WhenVariantAttributesAndFeaturesAreNull()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                Variants = new List<ItemVariant>
                {
                    new ItemVariant
                    {
                        Sku = "SKU-001",
                        ItemVariantAttributes = null!,
                        ItemVariantFeatures = null!
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void UpdateItemRequest_ReturnFailure_WhenTopLevelFeatureNameEnExceeds255Characters()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                ItemVariantFeatures = new List<ItemVariantFeatures>
                {
                    new ItemVariantFeatures
                    {
                        AttributeName_en = new string('A', 256),
                        Attributes_en = "Value"
                    }
                }
            };

            var result = request.Validate();

            Assert.True(result.IsFailure);
            Assert.Equal("Feature name (English) cannot exceed 255 characters.", result.Error);
            Assert.Equal(StatusCodes.Status400BadRequest, result.ErrorCode);
        }

        [Fact]
        public void UpdateItemRequest_ReturnSuccess_WhenTopLevelFeaturesIsNull()
        {
            var request = new UpdateItemRequest
            {
                Id = Guid.NewGuid(),
                SellerID = Guid.NewGuid(),
                Name_en = "Name EN",
                Name_fr = "Nom FR",
                Description_en = "Desc EN",
                Description_fr = "Desc FR",
                CategoryNodeID = Guid.NewGuid(),
                ItemVariantFeatures = null!
            };

            var result = request.Validate();

            Assert.True(result.IsSuccess);
        }


        [Fact]
        public void CreateItemVariantRequest_ReturnFailure_WhenSkuIsEmpty()
        {
            var request = new CreateItemVariantRequest { Sku = "", Price = 10.0m };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("SKU is required for each variant.", result.Error);
        }

        [Fact]
        public void CreateItemVariantRequest_ReturnFailure_WhenSkuExceeds100Characters()
        {
            var request = new CreateItemVariantRequest { Sku = new string('X', 101), Price = 10.0m };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("SKU cannot exceed 100 characters.", result.Error);
        }

        [Fact]
        public void CreateItemVariantRequest_ReturnSuccess_WhenSkuIsExactly100Characters()
        {
            var request = new CreateItemVariantRequest { Sku = new string('X', 100), Price = 10.0m };
            var result = request.Validate();
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CreateItemVariantRequest_ReturnFailure_WhenVariantNameFrExceeds255Characters()
        {
            var request = new CreateItemVariantRequest
            {
                Sku = "SKU-001",
                Price = 10.0m,
                ItemVariantName_fr = new string('A', 256)
            };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("Variant name (French) cannot exceed 255 characters.", result.Error);
        }

        // ---------------------------------------------------------------
        // CreateItemVariantAttributeRequest
        // ---------------------------------------------------------------

        [Fact]
        public void CreateItemVariantAttributeRequest_ReturnFailure_WhenNameEnExceeds255Characters()
        {
            var request = new CreateItemVariantAttributeRequest
            {
                AttributeName_en = new string('A', 256),
                Attributes_en = "Value"
            };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("Attribute name (English) cannot exceed 255 characters.", result.Error);
        }

        [Fact]
        public void CreateItemVariantAttributeRequest_ReturnFailure_WhenNameFrExceeds255Characters()
        {
            var request = new CreateItemVariantAttributeRequest
            {
                AttributeName_en = "Name EN",
                AttributeName_fr = new string('A', 256),
                Attributes_en = "Value"
            };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("Attribute name (French) cannot exceed 255 characters.", result.Error);
        }

        [Fact]
        public void CreateItemVariantAttributeRequest_ReturnSuccess_WhenNamesAreWithinLimits()
        {
            var request = new CreateItemVariantAttributeRequest
            {
                AttributeName_en = new string('A', 255),
                AttributeName_fr = new string('B', 255),
                Attributes_en = "Value"
            };
            var result = request.Validate();
            Assert.True(result.IsSuccess);
        }

        // ---------------------------------------------------------------
        // CreateItemVariantFeaturesRequest
        // ---------------------------------------------------------------

        [Fact]
        public void CreateItemVariantFeaturesRequest_ReturnFailure_WhenNameEnExceeds255Characters()
        {
            var request = new CreateItemVariantFeaturesRequest
            {
                AttributeName_en = new string('A', 256),
                Attributes_en = "Value"
            };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("Feature name (English) cannot exceed 255 characters.", result.Error);
        }

        [Fact]
        public void CreateItemVariantFeaturesRequest_ReturnFailure_WhenNameFrExceeds255Characters()
        {
            var request = new CreateItemVariantFeaturesRequest
            {
                AttributeName_en = "Feature EN",
                AttributeName_fr = new string('A', 256),
                Attributes_en = "Value"
            };
            var result = request.Validate();
            Assert.True(result.IsFailure);
            Assert.Equal("Feature name (French) cannot exceed 255 characters.", result.Error);
        }

        [Fact]
        public void CreateItemVariantFeaturesRequest_ReturnSuccess_WhenNamesAreWithinLimits()
        {
            var request = new CreateItemVariantFeaturesRequest
            {
                AttributeName_en = new string('A', 255),
                AttributeName_fr = new string('B', 255),
                Attributes_en = "Value"
            };
            var result = request.Validate();
            Assert.True(result.IsSuccess);
        }
    }
}
