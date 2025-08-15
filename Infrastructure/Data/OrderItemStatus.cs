namespace Infrastructure.Data
{
    public class OrderItemStatus
    {
        public int ID { get; set; }
        public string StatusCode { get; set; } = string.Empty;
        public string Name_en { get; set; } = string.Empty;
        public string Name_fr { get; set; } = string.Empty;
    }
}