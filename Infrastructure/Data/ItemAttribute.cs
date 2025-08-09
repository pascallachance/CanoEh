namespace Infrastructure.Data
{
    public class ItemAttribute
    {
        public Guid Id { get; set; }
        public Guid ItemID { get; set; }
        public string AttributeName_en { get; set; } = string.Empty;
        public string? AttributeName_fr { get; set; }
        public string Attributes_en { get; set; } = string.Empty;
        public string? Attributes_fr { get; set; }
    }
}