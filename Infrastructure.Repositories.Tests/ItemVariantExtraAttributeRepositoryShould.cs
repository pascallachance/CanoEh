using Infrastructure.Data;
using Infrastructure.Repositories.Tests.Common;

namespace Infrastructure.Repositories.Tests
{
    public class ItemVariantExtraAttributeRepositoryShould : BaseRepositoryShould<ItemVariantExtraAttribute>
    {
        protected override ItemVariantExtraAttribute CreateValidEntity()
        {
            return new ItemVariantExtraAttribute
            {
                Id = Guid.NewGuid(),
                ItemVariantId = Guid.NewGuid(),
                Name_en = "Serial Number",
                Name_fr = "Numéro de série",
                Value_en = "SN-123456",
                Value_fr = "SN-123456"
            };
        }
    }
}
